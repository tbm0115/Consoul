[![Package](https://github.com/tbm0115/Consoul/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/tbm0115/Consoul/actions/workflows/dotnetcore.yml)

# Consoul
Add some life to your console project with a structured toolkit for prompts, tables, progress bars, and navigable views.

![Tiny Text Adventures](/Consoul_1.png)

## Why Consoul?
Consoul wraps the standard `System.Console` API with higher-level concepts—color aware output, validated input, menu-driven views, progress indicators, and automation routines—so that you can prototype console UX quickly. Compared to competitors such as [Spectre.Console](https://spectreconsole.net/), Consoul emphasises:

* **View orchestration** – Static and dynamic view classes let you model a menu hierarchy while keeping actions encapsulated.
* **Scriptable routines** – XML/JSON driven automation lets you replay demos or smoke test your flows without manual input.
* **Drop-in helpers** – Keep working with `Console` while layering in theming, templated output, and input validation as needed.

If you need a lightweight console UI with strong navigation support, Consoul gives you everything in one package.

## Installation
Install via NuGet or the GitHub package registry:

```bash
# NuGet
dotnet add package Consoul

# GitHub packages
dotnet add package Consoul --source "https://nuget.pkg.github.com/tbm0115/index.json"
```

Alternatively, clone the repository, build the solution, and reference the compiled assembly directly.

## Quick start
```csharp
using ConsoulLibrary;

// Configure basic theming
Consoul.RenderOptions.PromptColor = ConsoleColor.Yellow;

// Output
Consoul.Write("Welcome to Consoul!", ConsoleColor.Cyan);

// Input
if (Consoul.Ask("Do you want to continue?"))
{
    // Selection prompt with structured result
    var prompt = new SelectionPrompt("Choose an option");
    prompt.Add("Start a job", ConsoleColor.Green, isDefault: true);
    prompt.Add("Show progress", ConsoleColor.Blue);
    prompt.Add("Exit", ConsoleColor.DarkGray);

    PromptResult choice = prompt.Render();
    if (choice.HasSelection)
    {
        Consoul.Write($"You selected option #{choice.Index + 1}");
    }
    else
    {
        Consoul.Write("Prompt canceled", ConsoleColor.DarkYellow);
    }
}
```

## Core concepts
| Scenario | Key types | Summary |
| --- | --- | --- |
| **Console output** | `Consoul.Write`, `ColorScheme`, `RenderOptions` | Centralise color choices and fall back to plain text if colors are suppressed. |
| **Input & validation** | `Consoul.Input`, `Consoul.Ask`, `Consoul.Read` | Prompt for free-form text or yes/no answers with optional empty-response validation. |
| **Prompts** | `SelectionPrompt`, `PromptResult` | Display numbered options, handle default selections, and detect cancelation without magic constants. |
| **Views** | `StaticView`, `DynamicView`, `ViewOption` | Model multi-step flows with menus that execute bound methods or dynamically generated entries. |
| **Tables** | `TableView`, `TableRenderOptions` | Generate padded, column-aware tables that respect console buffer width. |
| **Progress** | `ProgressBar` | Render textual progress indicators for long running tasks. |
| **Routines** | `Routines`, `RoutineRunner` | Automate keyboard input or scripted demos for repeatable workflows. |

Each concept has dedicated documentation with minimal examples and advanced tips—see the documentation section below.

### Prompt navigation with `PromptResult`
`SelectionPrompt.Render` now returns a `PromptResult` struct rather than a raw index. This removes the need to check sentinel values like `Consoul.EscapeIndex`. Inspect the result via:

* `HasSelection` – whether the user chose an entry.
* `Index` – the zero-based index when a selection was made.
* `IsCanceled` – whether the user dismissed the prompt (typing “back”, “exit”, etc.).

### Writing output consistently
Internal helpers have been renamed to follow .NET conventions. The former `_write` method is now `WriteCore`, and public overloads defer to it after evaluating the configured `RenderOptions.WriteMode`.

## Extensibility
* **Custom themes** – Override `RenderOptions` colors globally or swap schemes on the fly for sections of your UI.
* **Logging integration** – Use `ConsoulLoggerProvider` and `ConsoulLoggerExtensions` to route `Microsoft.Extensions.Logging` output into Consoul styled messages.
* **Window resize awareness** – Subscribe to `Consoul.WindowResized` to redraw content when the console buffer changes size.

## Documentation
The `/docs` folder contains component deep dives:

1. [Core console APIs](docs/core-console.md)
2. [Prompts](docs/prompts.md)
3. [Views](docs/views.md)
4. [Tables](docs/tables.md)
5. [Progress](docs/progress.md)
6. [Routines](docs/routines.md)

Each page covers when to use the feature, the primary types involved, minimal examples, and guidance for advanced scenarios.

## Recipes
* Confirmation dialogs with fallbacks for empty responses.
* Paginated tables driven by dynamic data.
* Long-running jobs that report progress with cancellation handling.
* Automated smoke tests using scripted routines.

Explore the recipes in the documentation to jumpstart common console patterns.

## Troubleshooting
* **Buffer width issues** – Very small console windows can truncate tables or progress bars. Pass explicit dimensions to rendering helpers or resize the console before rendering.
* **Color suppression** – Set `RenderOptions.WriteMode` to `WriteAll` to re-enable color output if running in constrained environments (for example, CI logs).
* **Prompt cancelation** – Check `PromptResult.IsCanceled` instead of comparing indices to detect when the user exits a prompt.

## License
This project is licensed under the GNU Lesser General Public License v3.0 - see the [LICENSE](LICENSE) file for details.
