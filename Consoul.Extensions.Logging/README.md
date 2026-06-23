# Consoul.Extensions.Logging

`Consoul.Extensions.Logging` is the optional `Microsoft.Extensions.Logging` integration for Consoul. It keeps the core `Consoul` package free of `Microsoft.Extensions.*` dependencies while still allowing host-builder and dependency-injection based applications to route logs to Consoul.

## Installation

Install from NuGet:

```bash
dotnet add package Consoul.Extensions.Logging
```

Or install from the GitHub package registry:

```bash
dotnet add package Consoul.Extensions.Logging --source "https://nuget.pkg.github.com/tbm0115/index.json"
```

This package depends on:

| Package | Purpose |
| --- | --- |
| `Consoul` | Core console rendering. |
| `Microsoft.Extensions.Logging` | Logger abstractions and registration APIs. |

## Basic Usage

Register the Consoul logger provider with an `ILoggingBuilder`:

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddConsoulLogger();
    builder.SetMinimumLevel(LogLevel.Information);
});

using ServiceProvider provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started");
logger.LogWarning("A warning rendered through Consoul");
logger.LogError(new InvalidOperationException("Example failure"), "Task failed");
```

## Host Builder Usage

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsoulLogger();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Ready");
```

## Custom Log Colors

`ConsoulLogger` exposes `LogLevelToColorMap`, which maps each `LogLevel` to a console color.

```csharp
var logger = new ConsoulLogger();

logger.LogLevelToColorMap[LogLevel.Information] = ConsoleColor.Cyan;
logger.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.Yellow;
logger.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.Red;

logger.Log(
    LogLevel.Information,
    new EventId(1, "Startup"),
    "Custom color message",
    exception: null,
    formatter: (state, exception) => state);
```

For most applications, prefer registering `AddConsoulLogger()` and let the logging system create providers and loggers.

## When To Use This Package

Use `Consoul.Extensions.Logging` when:

- Your app already uses `Microsoft.Extensions.Logging`.
- You want `ILogger` messages to render through Consoul colors and output helpers.
- You want to keep `Microsoft.Extensions.*` dependencies out of projects that only need core console UI.

Use only `Consoul` when you do not need `ILogger` integration.

## License

Consoul.Extensions.Logging is licensed under the GNU Lesser General Public License v3.0.
