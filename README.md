[![Package](https://github.com/tbm0115/Consoul/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/tbm0115/Consoul/actions/workflows/dotnetcore.yml)

# Consoul Repository

This repository contains the Consoul console UI toolkit and its optional integration packages. The repository is organized so the core package stays dependency-light, while integrations that depend on `Microsoft.Extensions.*` or `System.Text.Json` are packaged separately.

![Tiny Text Adventures](/Consoul_1.png)

## Packages

| Project | NuGet package | Purpose | README |
| --- | --- | --- | --- |
| `Consoul` | `Consoul` | Core console UI: prompts, tables, views, progress bars, fixed messages, and routines. | [Consoul README](Consoul/README.md) |
| `Consoul.Extensions.Logging` | `Consoul.Extensions.Logging` | Optional `Microsoft.Extensions.Logging` provider for Consoul output. | [Logging README](Consoul.Extensions.Logging/README.md) |
| `Consoul.Extensions.Configuration` | `Consoul.Extensions.Configuration` | Optional `Microsoft.Extensions.Configuration` bridge for routine transforms. | [Configuration README](Consoul.Extensions.Configuration/README.md) |
| `Consoul.Text.Json` | `Consoul.Text.Json` | Optional `System.Text.Json` serializer hook for object-editor value rendering. | [Text.Json README](Consoul.Text.Json/README.md) |

## Why The Repository Is Split

Older-targeting applications often have strict dependency graphs. The `Consoul` core project therefore avoids package references to `Microsoft.Extensions.*`, `System.Text.Json`, and similar optional infrastructure libraries. Applications can install only the integration packages they actually need.

For example:

```bash
dotnet add package Consoul

# Optional integrations
dotnet add package Consoul.Extensions.Logging
dotnet add package Consoul.Extensions.Configuration
dotnet add package Consoul.Text.Json
```

## Core Package Quick Start

```csharp
using ConsoulLibrary;

Consoul.Write("Welcome to Consoul", ConsoleColor.Cyan);

var prompt = new SelectionPrompt("Choose an option");
prompt.Add("Start", ConsoleColor.Green, isDefault: true);
prompt.Add("Exit", ConsoleColor.DarkGray);

PromptResult result = prompt.Render();
if (result.HasSelection)
{
    Consoul.Write($"Selected option {result.Index + 1}");
}
```

For a fast proof-of-concept menu, start with the fluent view API:

```csharp
Consoul.View("Tools")
    .Option("Say hello", () => Consoul.Write("Hello"))
    .Render();
```

See [Consoul/README.md](Consoul/README.md) for complete core usage examples, including fluent POC views, result-first selection helpers, tables, prompts, and object editing.

## Repository Projects

- `Consoul` is the packable core library.
- `Consoul.Extensions.Logging` is the packable logging integration.
- `Consoul.Extensions.Configuration` is the packable configuration integration.
- `Consoul.Text.Json` is the packable JSON integration.
- `Consoul.Tests` contains xUnit coverage for input handling, rendering, prompts, and metadata behavior.
- `Consoul.Test` is a sample console application used for manual exploration, including small POC menu, table, and settings-editor views.

## Documentation

The `/docs` folder contains component deep dives:

1. [Core console APIs](docs/core-console.md)
2. [Prompts](docs/prompts.md)
3. [Views](docs/views.md)
4. [Tables](docs/tables.md)
5. [Progress](docs/progress.md)
6. [Routines](docs/routines.md)

Package-specific NuGet README files live next to each packable project.

## Build And Test

Restore packages:

```bash
dotnet restore Consoul.sln
```

Run tests:

```bash
dotnet test Consoul.Tests\Consoul.Tests.csproj --no-restore -v minimal
```

Build all projects:

```bash
dotnet build Consoul.sln --no-restore -v minimal
```

## License

This project is licensed under the GNU Lesser General Public License v3.0. See [LICENSE](LICENSE) for details.
