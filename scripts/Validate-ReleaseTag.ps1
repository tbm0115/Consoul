[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Tag,
    [switch]$Prerelease
)

$ErrorActionPreference = 'Stop'
[xml]$properties = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../Directory.Build.props') -Raw
$version = [string]$properties.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'Directory.Build.props must declare the release Version.'
}
if (-not [string]::Equals($Tag, "v$version", [StringComparison]::Ordinal)) {
    throw "Release tag '$Tag' does not match configured package version '$version'; expected 'v$version'."
}
$isPreviewVersion = $version.Contains('-')
if ($Prerelease.IsPresent -ne $isPreviewVersion) {
    throw "GitHub release prerelease=$($Prerelease.IsPresent) does not match package version '$version'."
}
Write-Host "Release tag and prerelease flag match $version."
