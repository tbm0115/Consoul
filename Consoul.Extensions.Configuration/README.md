# Consoul.Extensions.Configuration

`Consoul.Extensions.Configuration` is the optional `Microsoft.Extensions.Configuration` bridge for Consoul routines. It keeps the core `Consoul` package free of `Microsoft.Extensions.*` dependencies while allowing applications with configuration providers to feed routine transforms from configuration.

For **3.0.0**, read the [documentation index](https://github.com/tbm0115/Consoul/blob/v3.0.0/docs/index.md), [migration guide](https://github.com/tbm0115/Consoul/blob/v3.0.0/docs/migration-3.0.md), and [release notes](https://github.com/tbm0115/Consoul/blob/v3.0.0/docs/releases/3.0.0.md). Versioned documentation links become available when the release tag is published.

## Installation

After the package is published to your configured source:

```bash
dotnet add package Consoul.Extensions.Configuration --version 3.0.0
```

See the [NuGet guide](https://github.com/tbm0115/Consoul/blob/v3.0.0/docs/nuget.md) for local builds, NuGet.org, and authenticated GitHub Packages setup. These instructions do not imply that the package is already published.

This package depends on:

| Package | Purpose |
| --- | --- |
| `Consoul` | Core routines and input transforms. |
| `Microsoft.Extensions.Configuration.Json` | JSON configuration provider and configuration abstractions. |

## appsettings.json Shape

Consoul routine transforms read values under the `Consoul:Transforms` section:

```json
{
  "Consoul": {
    "Transforms": {
      "Username": "demo-user",
      "Environment": "staging"
    }
  }
}
```

## Configure Routines From IConfiguration

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

configuration.ConfigureConsoulRoutines();
```

After configuration is applied, routine inputs can replace transform tokens:

```csharp
var input = new RoutineInput
{
    Value = "Deploying {{Environment}} as {{Username}}",
    Transforms = new[]
    {
        new InputTransform { Key = "Environment", UseAppSettings = true },
        new InputTransform { Key = "Username", UseAppSettings = true }
    }
};

Consoul.Write(input.Value);
```

## Initialize A Routine With Configuration

If your startup path receives command-line arguments and configuration, use the convenience helper:

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

configuration.InitializeConsoulRoutine(args);
```

This configures routine transforms and then calls `Routines.InitializeRoutine(args)`.

## Load Settings From A JSON File

```csharp
RoutineSettingsSection settings =
    ConsoulConfigurationExtensions.LoadConsoulRoutineSettingsFromJsonFile("appsettings.json", optional: true);

Routines.ConfigureAppSettings(settings);
```

## Core Alternative Without Microsoft.Extensions.Configuration

The core `Consoul` package includes `RoutineSettingsSection` for simple cases:

```csharp
Routines.ConfigureTransforms(new Dictionary<string, string>
{
    ["Environment"] = "local"
});
```

Use this optional package when your application already uses `IConfiguration` or needs provider-based configuration.

## License

Consoul.Extensions.Configuration is licensed under the GNU Lesser General Public License v3.0.
