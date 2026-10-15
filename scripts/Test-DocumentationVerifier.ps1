#Requires -Version 7.0
<#
.SYNOPSIS
Regression checks for Markdown code fences in the release documentation verifier.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$testRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/documentation-verifier-tests'))
$fixtureRoot = Join-Path $testRoot ([Guid]::NewGuid().ToString('N'))
$libraryTargets = [ordered]@{
    'Consoul' = @('netstandard2.0', 'netcoreapp3.1', 'net461')
    'Consoul.Cli' = @('netstandard2.0')
    'Consoul.Extensions.Logging' = @('netstandard2.0')
    'Consoul.Extensions.Configuration' = @('netstandard2.0')
    'Consoul.Text.Json' = @('netstandard2.0')
}
$caseCount = 0

function Write-FixtureFile([string] $RelativePath, [string] $Content) {
    $path = Join-Path $fixtureRoot $RelativePath
    [void][IO.Directory]::CreateDirectory((Split-Path -Parent $path))
    [IO.File]::WriteAllText($path, $Content, [Text.UTF8Encoding]::new($false))
}

function Assert-DocumentationCase([string] $Name, [string] $Markdown, [string] $MissingTarget) {
    Write-FixtureFile 'README.md' $Markdown
    $failure = $null
    try {
        # Run the actual checker against a tiny repository with valid XML documentation.
        & (Join-Path $fixtureRoot 'scripts/Verify-Documentation.ps1') 6>$null
    }
    catch {
        $failure = $_.Exception.Message
    }
    if ($MissingTarget) {
        if (-not $failure -or -not $failure.Contains('Broken local link') -or
            -not $failure.Contains(": $MissingTarget")) {
            throw "${Name}: expected rejection of '$MissingTarget', received '$failure'."
        }
    }
    elseif ($failure) {
        throw "${Name}: $failure"
    }
    $script:caseCount++
}

try {
    Write-FixtureFile 'scripts/Verify-Documentation.ps1' (Get-Content (Join-Path $PSScriptRoot 'Verify-Documentation.ps1') -Raw)
    Write-FixtureFile 'docs/guide.md' '# Guide'
    foreach ($library in $libraryTargets.Keys) {
        Write-FixtureFile "$library/README.md" '# Library'
        foreach ($framework in $libraryTargets[$library]) {
            Write-FixtureFile "$library/bin/Release/$framework/$library.xml" '<doc><members><member name="T:Fixture"><summary>Fixture API.</summary></member></members></doc>'
        }
    }

    $cliReadme = Get-Content (Join-Path $repositoryRoot 'Consoul.Cli/README.md') -Raw
    foreach ($lineEnding in @("`n", "`r`n")) {
        $label = if ($lineEnding.Length -eq 1) { 'LF' } else { 'CRLF' }
        $code = @('```csharp', 'console.MarkupLine("[green](default)[/]");', '```') -join $lineEnding
        Assert-DocumentationCase "$label code and following link" ($code + $lineEnding + '[Guide](docs/guide.md)')
        Assert-DocumentationCase "$label closing fence at EOF" $code
        Assert-DocumentationCase "$label trailing spaces" ($code + '  ' + $lineEnding + '[Guide](docs/guide.md)')
        Assert-DocumentationCase "$label multiple fences" ($code + $lineEnding + '[Guide](docs/guide.md)' + $lineEnding + $code + $lineEnding)
        Assert-DocumentationCase "$label missing real target" ($code + $lineEnding + '[Missing](missing.md)') 'missing.md'

        # Exercise the real example that failed on a Windows GitHub Actions checkout.
        Write-FixtureFile 'Consoul.Cli/README.md' ([regex]::Replace($cliReadme, '\r?\n', $lineEnding))
        Assert-DocumentationCase "$label actual CLI README" '# Root'
        Write-FixtureFile 'Consoul.Cli/README.md' '# Library'
    }
    Write-Host "Documentation verifier regression checks passed: $caseCount cases (LF and CRLF)."
}
finally {
    $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
    if (-not $resolvedFixture.StartsWith($testRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove a fixture outside the documentation test directory.'
    }
    if (Test-Path -LiteralPath $resolvedFixture) {
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
}
