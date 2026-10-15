---
title: Optional packages
description: Add command-line parsing, logging, configuration, or JSON only when your app needs it.
searchable: true
---
# Optional packages

The core `Consoul` package provides rendering, prompts, views, tables, and routines. Add an integration when your app needs it, keeping all Consoul package versions aligned at **3.0.0**. See [Installation](nuget.md) for package sources and local builds.

| Package | Use it for |
| --- | --- |
| `Consoul.Cli` | Commands, arguments, options, help, and view commands |
| `Consoul.Extensions.Logging` | Send `ILogger` messages through Consoul |
| `Consoul.Extensions.Configuration` | Read routine transforms from `IConfiguration` |
| `Consoul.Text.Json` | Render JSON and serialize object-editor values |

## Command-line apps

Install `Consoul.Cli` and replace `Program.cs` with this complete command app:

```csharp
using ConsoulLibrary;

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.AddCommand("status", context =>
    {
        context.Console.MarkupLine("[green]Ready[/]");
    }).WithDescription("Show deployment status");
});

return await app.RunAsync(args);
```

Run `dotnet run -- status` to invoke the command, or `dotnet run -- --help` for help.

[CLI package reference: typed settings, aliases, completion, and view commands](https://github.com/tbm0115/Consoul/blob/develop/Consoul.Cli/README.md)

## Logging

Install `Consoul.Extensions.Logging`. In an app using a service collection, register the provider:

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsoulLogger());

using var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application ready");
```

[Logging package reference](https://github.com/tbm0115/Consoul/blob/develop/Consoul.Extensions.Logging/README.md)

## Routine configuration

Use `Consoul.Extensions.Configuration` when your routines read settings through `Microsoft.Extensions.Configuration`. The core package also supports its own dependency-light routine settings model.

Start with [Scripted routines](routines.md), then follow the [configuration integration setup](https://github.com/tbm0115/Consoul/blob/develop/Consoul.Extensions.Configuration/README.md) for configuration providers and transforms.

## JSON output

Install `Consoul.Text.Json` to display an object as formatted JSON:

```csharp
using ConsoulLibrary;

ConsoulTextJsonExtensions.JsonValue(new
{
    Service = "API",
    State = "Healthy",
    Retries = 3
});
```

[JSON package reference: rendering and form-value serialization](https://github.com/tbm0115/Consoul/blob/develop/Consoul.Text.Json/README.md)
