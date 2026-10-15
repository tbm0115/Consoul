# NuGet packages, builds, and prereleases

The repository is configured for **3.0.0-preview.1**. This is a local release candidate configuration; these instructions do not imply that the version is already available on either registry. See the [preview notes](releases/3.0.0-preview.1.md), [2.x migration guide](migration-3.0.md), and [documentation index](index.md) before upgrading.

## Package and dependency matrix

All five packages share the version in [`Directory.Build.props`](../Directory.Build.props). Install only the integrations the application needs.

| Package | Shipped target frameworks | Direct package dependencies |
| --- | --- | --- |
| `Consoul` | `netstandard2.0`, `netcoreapp3.1`, `net461` | No optional infrastructure packages. Framework support dependencies can be supplied by the SDK. |
| `Consoul.Cli` | `netstandard2.0` | `Consoul` at the same preview version |
| `Consoul.Extensions.Configuration` | `netstandard2.0` | `Consoul` at the same preview version; `Microsoft.Extensions.Configuration.Json` 10.0.9 |
| `Consoul.Extensions.Logging` | `netstandard2.0` | `Consoul` at the same preview version; `Microsoft.Extensions.Logging` 10.0.9 |
| `Consoul.Text.Json` | `netstandard2.0` | `Consoul` at the same preview version; `System.Text.Json` 8.0.5 |

Project references become NuGet dependencies when packing. Their generated minimum versions must match this release; package verification checks that relationship. NuGet also resolves the transitive dependencies of the integration libraries. A package's target framework does not guarantee that all its dependencies support every historical runtime that implements that framework. Check the restored dependency graph in the consuming application, especially for older .NET Framework applications.

The legacy core targets are preserved for compatibility. They are compilation targets, not a claim that the underlying .NET Core 3.1 or .NET Framework 4.6.1 runtimes are still supported. The current automated tests run on .NET 8; they do not establish runtime coverage for those older targets or every terminal/operating system.

## Package contents and version ownership

[`Directory.Build.props`](../Directory.Build.props) owns package version `3.0.0-preview.1`, assembly/file version `3.0.0.0`, repository metadata, release notes, deterministic compilation, portable PDB generation, and XML documentation generation. Assembly and file versions are numeric; the package/informational version carries the prerelease label. The SDK may append the source revision to informational version metadata.

[`Directory.Build.targets`](../Directory.Build.targets) includes each library's own `README.md`, the repository `LICENSE`, and the shared 128 × 128 [Ember icon](assets/brand/ember-icon-128.png) as `consoul-icon.png` in the package root. All five projects inherit `PackageIcon` from `Directory.Build.props`; no remote icon URL is required. See the [brand asset guide](branding.md) for other sizes and formats. License metadata points to the existing LGPL v3 license file. Each `lib/<TFM>/` directory contains its assembly and matching XML IntelliSense documentation. A separate `.snupkg` carries the portable PDBs for each target. XML comments are therefore available to consumers without a source checkout. The SDK supplies repository/source revision metadata during builds when available.

Missing public API documentation and malformed/unresolved XML documentation warnings are errors for packable projects: `CS1591`, `CS1570`, `CS1572`, `CS1573`, `CS1574`, `CS1580`, `CS1581`, `CS1584`, `CS1587`, `CS1711`, and `CS1712`. [`scripts/Verify-Documentation.ps1`](../scripts/Verify-Documentation.ps1) additionally checks generated documentation and local Markdown links. Warnings are not suppressed to make release validation pass.

Only the five library projects opt into `IsPackable`. Tests and the sample are not packages. `GeneratePackageOnBuild` is disabled: an ordinary build compiles the solution, and explicit packing follows successful tests. Update the shared version and release notes together for the next preview; avoid per-project version overrides that would separate integration dependencies from the core release.

## Build prerequisites and restore configuration

Install the .NET SDK selected by [`global.json`](../global.json): **10.0.300**, or a later stable patch in the same `10.0.3xx` feature band. `rollForward: latestPatch` allows servicing patches but does not silently switch feature bands or SDK major versions. Install a .NET 8 runtime as well to execute the existing `net8.0` tests and sample; installing the .NET 8 SDK provides that runtime. CI provisions both through `actions/setup-dotnet`.

The supported release verification path uses Windows and PowerShell 7, matching CI. Building `net461` requires .NET Framework reference assemblies; the SDK may restore the reference assembly package when the targeting pack is absent. Building all targets does not require installing the .NET Core 3.1 runtime because no tests execute on that runtime.

[`NuGet.Config`](../NuGet.Config) clears inherited package sources and selects only the public NuGet v3 feed, `https://api.nuget.org/v3/index.json`. Repository restore requires no private feed or credential. This prevents a developer's unrelated private feed from becoming an undocumented CI prerequisite. Consumers can use a separate configuration for GitHub Packages or local preview packages; do not put credentials in this repository's configuration.

Inspect installed tools from the repository root:

```powershell
dotnet --version
dotnet --list-runtimes
```

## Validate and pack locally without publishing

Run the same orchestration script used by CI:

```powershell
pwsh -File ./scripts/Verify-Release.ps1
```

It restores the solution from `NuGet.Config`, builds Release, runs the complete xUnit project, packs each of the five libraries, checks all package contents and dependency versions, then checks documentation. Every native command exit code is checked. No publishing command is present in this script.

Outputs are five `.nupkg` and five `.snupkg` files under `artifacts/packages/3.0.0-preview.1/`, plus test results under `artifacts/test-results/`. The package verifier rejects extra versions or missing packages in its selected directory; use a clean output directory for each candidate. The default directory includes the configured version to keep separate previews apart.

The equivalent individual commands, useful while diagnosing a specific stage, are:

```powershell
dotnet restore Consoul.sln --configfile NuGet.Config
dotnet build Consoul.sln --configuration Release --no-restore
dotnet test Consoul.Tests/Consoul.Tests.csproj --configuration Release --no-build --no-restore

$projects = 'Consoul', 'Consoul.Cli', 'Consoul.Extensions.Configuration', 'Consoul.Extensions.Logging', 'Consoul.Text.Json'
foreach ($project in $projects) {
    dotnet pack "$project/$project.csproj" --configuration Release --no-build --no-restore --output artifacts/packages/3.0.0-preview.1
    if ($LASTEXITCODE -ne 0) { throw "Packing failed for $project" }
}
pwsh -File ./scripts/Verify-Packages.ps1
pwsh -File ./scripts/Verify-Documentation.ps1 -Configuration Release
```

`--no-build` and `--no-restore` require the corresponding earlier stages to have succeeded for the same configuration and version. Do not reuse old outputs after a source/version change. Package verification checks the package ID/version, README/license/icon/repository metadata, assemblies and XML files for every expected target, matching core dependency versions, and corresponding symbol packages.

## Install the preview in a consuming application

After publication to NuGet.org, select the version explicitly. Prerelease packages are not selected by an ordinary stable-only update:

```powershell
dotnet add package Consoul --version 3.0.0-preview.1
dotnet add package Consoul.Cli --version 3.0.0-preview.1
```

Optional integration packages use the same `--version 3.0.0-preview.1` argument. In Visual Studio's NuGet manager, enable **Include prerelease**. NuGet.org package restore requires no API key. If a consuming project uses multiple feeds, ensure its configuration can also resolve public integration dependencies from NuGet.org.

Before publication, consume the locally built packages with a configuration in a separate smoke-test application directory. Replace the local path below with the output directory on your machine:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="consoul-preview" value="C:\path\to\Consoul\artifacts\packages\3.0.0-preview.1" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

Then use the same explicit-version install commands in that application. NuGet caches packages by ID and version. If rebuilding a local candidate with the same identity, isolate the consumer's global package cache (for example, set `NUGET_PACKAGES` to a temporary directory for that smoke test) so an older locally cached package cannot masquerade as the latest build. Published package versions should be immutable; use the next preview version for a changed release.

For GitHub Packages, add `https://nuget.pkg.github.com/tbm0115/index.json` to the consuming application's feed configuration alongside NuGet.org. Outside GitHub Actions, authentication uses a GitHub personal access token **(classic)** with `read:packages` and access to the package. Supply the source credential through a user-scoped configuration or a session environment variable, never a committed file. For a source named `github`, NuGet recognizes:

```powershell
# Obtain the token through your local secret manager; do not hard-code it here.
$env:NuGetPackageSourceCredentials_github = "Username=$env:GITHUB_USER;Password=$env:GITHUB_PACKAGES_READ_TOKEN;ValidAuthenticationTypes=Basic"
dotnet add package Consoul --version 3.0.0-preview.1
```

The feed's configured name must match the suffix `github`. Clear session credentials when the restore session ends. [GitHub's registry documentation](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry) describes token scopes and repository/package access rules.

## CI and publication safeguards

[`dotnetcore.yml`](../.github/workflows/dotnetcore.yml) runs validation on pushes, pull requests, manual dispatch, and published GitHub release events. Push, PR, and manual runs only build, test, verify, and upload workflow artifacts. They cannot enter the publication job. The workflow uses explicit action major versions instead of floating `@main`, disables checkout credential persistence, and gives validation only `contents: read` permission.

Actual publication is a separate job that requires a successful validation job **and** a GitHub `release: published` event. Before validation and again before publishing, [`Validate-ReleaseTag.ps1`](../scripts/Validate-ReleaseTag.ps1) requires the exact tag `v3.0.0-preview.1` to match `Directory.Build.props` and requires GitHub's prerelease flag for a version containing a prerelease suffix. A stable version must likewise have a non-prerelease GitHub release. Publishing a GitHub prerelease is a real registry publication; creating a draft or running this workflow manually is not.

The publication job downloads and verifies the same package artifact that passed validation. It pushes all five main packages and their `.snupkg` symbols to the NuGet v3 endpoint, then the five main packages to the repository owner's GitHub Packages feed. Symbols remain available in workflow artifacts and NuGet.org's symbol service. Pushes check exit codes and use `--skip-duplicate` so a partially completed release can resume; duplicate skipping does not compare an existing registry package's contents or replace it. Do not retry a version with different bytes.

Required publisher configuration:

| Setting | Purpose |
| --- | --- |
| Repository secret `NUGET_API_KEY` | NuGet.org API key authorized to push the five package IDs (and create IDs if they do not exist). It is read only in the release publication step. |
| Automatic `GITHUB_TOKEN` with job permission `packages: write` | Publishes packages associated with this repository. Repository/organization policy and existing package access must permit the workflow to write each package. |
| Repository owner | Determines `https://nuget.pkg.github.com/<owner>/index.json`; the current owner is `tbm0115`. No `GPR_URI` or `GPR_USERNAME` secret is needed. |

The GitHub publication step creates a temporary runner configuration containing only a named `github` source URL. Its `NuGetPackageSourceCredentials_github` environment variable supplies the actor and `GITHUB_TOKEN` for Basic authentication when NuGet reads the service index; the same token is passed as the publishing API key. This avoids relying on a push API key alone to authenticate feed discovery. The temporary file contains no token. [NuGet documents this environment credential convention](https://learn.microsoft.com/en-us/nuget/consume-packages/consuming-packages-authenticated-feeds).

Credentials are not needed for validation runs and are not included in package metadata, artifacts, or the checked-in NuGet configuration. Registry account ownership and package permissions must be confirmed by a maintainer before a future publication; local validation cannot prove those permissions or registry availability.

No release, tag, push, or registry publication is performed by preparing these files. Before an authorized publication, review the migration/release notes, test the packages in a fresh consumer, commit the intended source snapshot, obtain green CI for it, and publish the matching GitHub prerelease only when the release is intended to become available to consumers.

## Reference documentation

- [.NET SDK selection and roll-forward](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
- [NuGet MSBuild pack properties](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets)
- [NuGet symbol package creation and publication](https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg)
- [GitHub Packages NuGet authentication](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
- [Setup .NET action](https://github.com/actions/setup-dotnet), [checkout action](https://github.com/actions/checkout), [artifact upload](https://github.com/actions/upload-artifact), and [artifact download](https://github.com/actions/download-artifact)
