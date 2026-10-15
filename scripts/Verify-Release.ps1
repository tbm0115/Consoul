[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
[xml]$properties = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Directory.Build.props') -Raw
$version = [string]$properties.Project.PropertyGroup.Version
$packageDirectory = Join-Path $repositoryRoot "artifacts/packages/$version"
$testResultsDirectory = Join-Path $repositoryRoot 'artifacts/test-results'
$projects = @('Consoul', 'Consoul.Cli', 'Consoul.Extensions.Configuration', 'Consoul.Extensions.Logging', 'Consoul.Text.Json')

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

Push-Location -LiteralPath $repositoryRoot
try {
    Invoke-Dotnet -Arguments @('restore', 'Consoul.sln', '--configfile', 'NuGet.Config')
    Invoke-Dotnet -Arguments @('build', 'Consoul.sln', '--configuration', $Configuration, '--no-restore')
    Invoke-Dotnet -Arguments @('test', 'Consoul.Tests/Consoul.Tests.csproj', '--configuration', $Configuration,
        '--no-build', '--no-restore', '--logger', 'trx', '--results-directory', $testResultsDirectory)
    foreach ($project in $projects) {
        Invoke-Dotnet -Arguments @('pack', "$project/$project.csproj", '--configuration', $Configuration,
            '--no-build', '--no-restore', '--output', $packageDirectory)
    }
    & (Join-Path $PSScriptRoot 'Verify-Packages.ps1') -PackageDirectory $packageDirectory
    & (Join-Path $PSScriptRoot 'Verify-Documentation.ps1') -Configuration $Configuration
    Write-Host "Release verification passed. Packages are in $packageDirectory. Nothing was published."
}
finally { Pop-Location }
