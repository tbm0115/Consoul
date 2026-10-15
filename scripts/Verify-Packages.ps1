[CmdletBinding()]
param(
    [string]$PackageDirectory
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
[xml]$properties = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Directory.Build.props') -Raw
$version = [string]$properties.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
    $PackageDirectory = Join-Path $repositoryRoot "artifacts/packages/$version"
}
$PackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path
$packageFrameworks = [ordered]@{
    'Consoul' = @('netstandard2.0', 'netcoreapp3.1', 'net461')
    'Consoul.Cli' = @('netstandard2.0')
    'Consoul.Extensions.Configuration' = @('netstandard2.0')
    'Consoul.Extensions.Logging' = @('netstandard2.0')
    'Consoul.Text.Json' = @('netstandard2.0')
}
$expectedFiles = foreach ($packageId in $packageFrameworks.Keys) {
    "$packageId.$version.nupkg"
    "$packageId.$version.snupkg"
}
$actualFiles = @(Get-ChildItem -LiteralPath $PackageDirectory -File |
    Where-Object { $_.Extension -in @('.nupkg', '.snupkg') } |
    Select-Object -ExpandProperty Name)
if (@(Compare-Object -ReferenceObject @($expectedFiles) -DifferenceObject $actualFiles).Count -ne 0) {
    throw "Expected exactly the five $version packages and five symbol packages in '$PackageDirectory'. Found: $($actualFiles -join ', ')"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($packageId in $packageFrameworks.Keys) {
    $packagePath = Join-Path $PackageDirectory "$packageId.$version.nupkg"
    $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $nuspec = $archive.GetEntry("$packageId.nuspec")
        if ($null -eq $nuspec) { throw "$packageId is missing its nuspec." }
        $reader = [IO.StreamReader]::new($nuspec.Open())
        try { [xml]$manifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $manifest.package.metadata
        if ($metadata.id -ne $packageId -or $metadata.version -ne $version) {
            throw "$packageId has an unexpected package ID or version."
        }
        if ($metadata.readme -ne 'README.md' -or $metadata.icon -ne 'Consoul_Small.png' -or
            $metadata.license.type -ne 'file' -or $metadata.license.InnerText -ne 'LICENSE' -or
            $metadata.repository.url -ne 'https://github.com/tbm0115/Consoul' -or
            $metadata.repository.type -ne 'git') {
            throw "$packageId has incomplete README, icon, license, or repository metadata."
        }
        foreach ($asset in @('README.md', 'LICENSE', 'Consoul_Small.png')) {
            $entry = $archive.GetEntry($asset)
            if ($null -eq $entry -or $entry.Length -eq 0) { throw "$packageId is missing $asset." }
        }
        foreach ($framework in $packageFrameworks[$packageId]) {
            foreach ($extension in @('dll', 'xml')) {
                $asset = "lib/$framework/$packageId.$extension"
                $entry = $archive.GetEntry($asset)
                if ($null -eq $entry -or $entry.Length -eq 0) { throw "$packageId is missing $asset." }
            }
        }
        if ($packageId -ne 'Consoul') {
            $dependencies = @($metadata.dependencies.group.dependency | Where-Object { $_.id -eq 'Consoul' })
            if ($dependencies.Count -eq 0) { throw "$packageId is missing its Consoul dependency." }
            foreach ($dependency in $dependencies) {
                $minimumVersion = ([string]$dependency.version -replace '[\[\]\(\)\s]', '').Split(',')[0]
                if ($minimumVersion -ne $version) {
                    throw "$packageId must depend on Consoul $version, found '$($dependency.version)'."
                }
            }
        }
    }
    finally { $archive.Dispose() }

    $symbols = [IO.Compression.ZipFile]::OpenRead((Join-Path $PackageDirectory "$packageId.$version.snupkg"))
    try {
        foreach ($framework in $packageFrameworks[$packageId]) {
            $asset = "lib/$framework/$packageId.pdb"
            $entry = $symbols.GetEntry($asset)
            if ($null -eq $entry -or $entry.Length -eq 0) { throw "$packageId symbols are missing $asset." }
        }
    }
    finally { $symbols.Dispose() }
    Write-Host "Verified $packageId $version`: assemblies, IntelliSense XML, README, license, icon, dependencies, and symbols."
}
