#Requires -Version 7.0
<#
.SYNOPSIS
Checks built IntelliSense XML and local links in the GitHub documentation.
.DESCRIPTION
Run after a solution build. Checks every library target's XML documentation for
nonempty summaries or inherited documentation and unresolved compiler references.
Checks Markdown file targets without making network requests or validating anchors.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()
$libraryTargets = [ordered]@{
    'Consoul' = @('netstandard2.0', 'netcoreapp3.1', 'net461')
    'Consoul.Cli' = @('netstandard2.0')
    'Consoul.Extensions.Logging' = @('netstandard2.0')
    'Consoul.Extensions.Configuration' = @('netstandard2.0')
    'Consoul.Text.Json' = @('netstandard2.0')
}

$memberCount = 0
foreach ($library in $libraryTargets.Keys) {
    foreach ($framework in $libraryTargets[$library]) {
        $xmlPath = Join-Path $repositoryRoot "$library/bin/$Configuration/$framework/$library.xml"
        if (-not (Test-Path -LiteralPath $xmlPath -PathType Leaf)) {
            $failures.Add("Missing IntelliSense XML: $xmlPath")
            continue
        }

        [xml] $documentation = Get-Content -LiteralPath $xmlPath -Raw
        $members = @($documentation.SelectNodes('/doc/members/member'))
        if ($members.Count -eq 0) {
            $failures.Add("No documented members: $xmlPath")
        }
        foreach ($member in $members) {
            $summary = $member.SelectSingleNode('summary')
            $inherited = $member.SelectSingleNode('inheritdoc')
            if ($null -eq $inherited -and
                ($null -eq $summary -or [string]::IsNullOrWhiteSpace($summary.InnerXml))) {
                $failures.Add("Missing summary in ${library}/${framework}: $($member.GetAttribute('name'))")
            }
            foreach ($reference in $member.SelectNodes('.//*[@cref]')) {
                if ($reference.GetAttribute('cref').StartsWith('!:')) {
                    $failures.Add("Unresolved XML reference in ${library}/${framework}: $($reference.GetAttribute('cref'))")
                }
            }
        }
        $memberCount += $members.Count
    }
}

$markdownFiles = @(
    Get-Item -LiteralPath (Join-Path $repositoryRoot 'README.md')
    Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'docs') -Filter '*.md' -Recurse -File
    foreach ($library in $libraryTargets.Keys) {
        Get-Item -LiteralPath (Join-Path $repositoryRoot "$library/README.md")
    }
)
$linkCount = 0
foreach ($markdownFile in $markdownFiles) {
    $markdown = Get-Content -LiteralPath $markdownFile.FullName -Raw
    # Ignore fenced examples; brackets in code are not navigation links.
    # Multiline $ stops before LF, so consume CR explicitly for Windows checkouts.
    $markdown = [regex]::Replace($markdown, '(?ms)^```[^\r\n]*\r?\n.*?^```[^\r\n]*\r?$', '')
    $links = [regex]::Matches($markdown, '\[[^\]\r\n]*\]\((?<target><[^>]+>|[^\s)]+)(?:\s+"[^"]*")?\)')
    foreach ($link in $links) {
        $target = $link.Groups['target'].Value.Trim('<', '>')
        if ($target -match '^(?:[a-zA-Z][a-zA-Z0-9+.-]*:|//|#)') {
            continue
        }

        $pathPart = [Uri]::UnescapeDataString(($target -split '[?#]', 2)[0])
        if ([string]::IsNullOrWhiteSpace($pathPart)) {
            continue
        }
        $basePath = if ($pathPart.StartsWith('/')) { $repositoryRoot } else { $markdownFile.DirectoryName }
        $resolvedTarget = [IO.Path]::GetFullPath((Join-Path $basePath $pathPart.TrimStart('/')))
        if (-not $resolvedTarget.StartsWith($repositoryRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
            -not (Test-Path -LiteralPath $resolvedTarget)) {
            $failures.Add("Broken local link in $($markdownFile.FullName): $target")
        }
        $linkCount++
    }
}

if ($failures.Count -gt 0) {
    throw ($failures -join [Environment]::NewLine)
}
Write-Host "Documentation verified: $memberCount XML member entries across all library targets; $linkCount local Markdown links."
