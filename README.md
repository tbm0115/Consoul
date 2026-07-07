[![Package](https://github.com/tbm0115/Consoul/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/tbm0115/Consoul/actions/workflows/dotnetcore.yml)

# Consoul Repository

This repository contains the Consoul console UI toolkit and its optional integration packages. The repository is organized so the core package stays dependency-light, while integrations that depend on `Microsoft.Extensions.*` or `System.Text.Json` are packaged separately.

![Tiny Text Adventures](/Consoul_1.png)

## Packages

| Project | NuGet package | Purpose | README |
| --- | --- | --- | --- |
| `Consoul` | `Consoul` | Core MVC-style console UI: typed prompts, validation helpers, tables, views, shortcut bars, progress bars, fixed messages, and routines. | [Consoul README](Consoul/README.md) |
| `Consoul.Cli` | `Consoul.Cli` | Optional command-line app runner for typed commands, delegate commands, help output, aliases, examples, and MVC view commands. | [CLI README](Consoul.Cli/README.md) |
| `Consoul.Extensions.Logging` | `Consoul.Extensions.Logging` | Optional `Microsoft.Extensions.Logging` provider for Consoul output. | [Logging README](Consoul.Extensions.Logging/README.md) |
| `Consoul.Extensions.Configuration` | `Consoul.Extensions.Configuration` | Optional `Microsoft.Extensions.Configuration` bridge for routine transforms. | [Configuration README](Consoul.Extensions.Configuration/README.md) |
| `Consoul.Text.Json` | `Consoul.Text.Json` | Optional `System.Text.Json` serializer hook and syntax-colored JSON rendering helpers. | [Text.Json README](Consoul.Text.Json/README.md) |

## Why The Repository Is Split

Older-targeting applications often have strict dependency graphs. The `Consoul` core project therefore avoids package references to `Microsoft.Extensions.*`, `System.Text.Json`, and similar optional infrastructure libraries. Applications can install only the integration packages they actually need.

For example:

```bash
dotnet add package Consoul

# Optional integrations
dotnet add package Consoul.Cli
dotnet add package Consoul.Extensions.Logging
dotnet add package Consoul.Extensions.Configuration
dotnet add package Consoul.Text.Json
```

## Core Package Quick Start

```csharp
using ConsoulLibrary;

Consoul.MarkupLine("[cyan]Welcome to Consoul[/]");

var prompt = new SelectionPrompt("Choose an option");
prompt.Add("Start", ConsoleColor.Green, isDefault: true);
prompt.Add("Exit", ConsoleColor.DarkGray);

PromptResult result = prompt.Render();
if (result.HasSelection)
{
    Consoul.Write($"Selected option {result.Index + 1}");
}
```

For a fast proof-of-concept menu, start with the dynamic view API:

```csharp
Consoul.View("Tools")
    .Option("Say hello", () => Consoul.Write("Hello"))
    .DescribeLastOption("Writes a friendly test message.")
    .ShortcutLastOption("h")
    .Render();
```

For teams migrating snippets from Spectre.Console, the core package also includes an `AnsiConsole` facade that delegates to Consoul's dependency-light APIs while keeping prompt cancellation explicit:

```csharp
AnsiConsole.MarkupLine("[green]Ready[/]");
PromptResult result = AnsiConsole.Prompt(prompt);
TextPromptResult<int> retries = await AnsiConsole.AskAsync("Retries", 3);
```

Renderable builder aliases such as `Panel`, `Table`, `Metrics`, `Sparkline`, `Timeline`, and `ShortcutBar` keep fast POC screens composable while still flowing through Consoul's MVC-style view and renderable model. Manual tables, panels, rules, text blocks, trees, badges, breadcrumbs, bullet lists, checklists, metrics, sparklines, timelines, property lists, exception diagnostics, rows, columns, grids, layouts, padding, alignment, shortcut bars, prompt text, prompt helper text, prompt disabled reasons, prompt invalid-selection copy, and scalar prompt validation copy, status/spinner/progress messages, progress task labels, bar charts, and breakdown charts accept Spectre-style markup in common text slots, preserving styling while measuring responsive layout by visible text. Prompt feedback distinguishes disabled choices from malformed input so quick MVC menus can explain unavailable actions.

Install `Consoul.Cli` when the console app also needs a command-line entry point with help, aliases, examples, typed settings, arguments, options, and commands that can launch MVC-style Consoul views:

```csharp
using ConsoulLibrary;

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.AddView<DashboardView>("dashboard")
        .WithDescription("Open the interactive dashboard")
        .WithAlias("dash");
});

return await app.RunAsync(args);
```

For command-line parameters, typed command settings use familiar annotations such as `CommandArgument`, `CommandOption`, `[Description]`, and `[DefaultValue]`. Repeated values can bind to arrays or common generic collection properties, boolean short aliases can be clustered as `-vf`, and options can read environment-variable fallbacks without pulling in configuration packages, so POC settings can stay close to normal MVC-style view models. Ported Spectre-style command classes can inherit from `Command<TSettings>` or `AsyncCommand<TSettings>`, while interface-based commands remain available:

```csharp
public sealed class CopyCommand : Command<CopySettings>
{
    protected override int Execute(CommandContext context, CopySettings settings, CancellationToken cancellationToken)
    {
        context.Console.MarkupLineInterpolated($"Copying [cyan]{settings.Source}[/]");
        return 0;
    }
}

public sealed class CopySettings : CommandSettings
{
    [CommandArgument(0, "<source>")]
    public string Source { get; set; } = string.Empty;

    [CommandOption("-f|--force")]
    public bool Force { get; set; }
}
```

`Consoul.Cli` also supports branch-style command groups for tools that grow beyond a flat command list:

```csharp
app.Configure(config =>
{
    config.AddBranch("service", service =>
    {
        service.AddCommand<ListServicesCommand>("list").WithAlias("ls");
        service.AddCommand<RestartServiceCommand>("restart");
    });
});
```

The same command model can provide completion suggestions for shell adapters or embedded command palettes without executing commands:

```csharp
IReadOnlyList<CommandCompletionItem> completions =
    app.GetCompletions(new[] { "service", "" });
```

For responsive POC screens, query `Consoul.Terminal` instead of `System.Console`:

```csharp
int width = Consoul.Terminal.ResolveWidth(100, minimumWidth: 40);
Consoul.Rule("Dashboard", width: width);
```

For MVC-style POCs, use a model-backed view:

```csharp
Consoul.View("Counter", new CounterModel())
    .Section("Summary", model =>
    {
        Consoul.PropertyList(list =>
        {
            list.Add("Count", model.Count, ConsoleColor.Cyan);
            list.Add("State", model.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
        });
    })
    .Separator("Actions")
    .Option("Increment", model => model.Count++)
    .Render();
```

Use `SectionWhen(...)` for validation, warnings, and detail panels that should appear only when the view model reaches the relevant state.
Use `PropertyListSection(...)`, `TableSection(...)`, `MetricsSection(...)`, `RowsSection(...)`, `ColumnsSection(...)`, `GridSection(...)`, `LayoutSection(...)`, `TextBlockSection(...)`, `PanelSection(...)`, `CalloutSection(...)`, `ExceptionSection(...)`, `BreadcrumbSection(...)`, `ShortcutBarSection(...)`, `BulletListSection(...)`, `ChecklistSection(...)`, `CalendarSection(...)`, `SparklineSection(...)`, `BarChartSection(...)`, `BreakdownChartSection(...)`, `TreeSection(...)`, `TimelineSection(...)`, and their `...When(...)` counterparts when a model-backed POC view should generate common read-only summaries, collection tables, KPI blocks, composed dashboard rows, columns, grids, pane layouts, wrapped explanatory text, framed details, status callouts, bounded diagnostics, navigation trails, action hints, notes, readiness checks, schedules, trend lines, charts, nested diagnostics, or workflow/history timelines from the model without custom render loops. Use `AddOptions(...)` with `isEnabled` and `disabledReason` selectors when a model-backed POC menu should project a collection of actions while keeping unavailable actions visible.
Use `ValidationSummarySection(...)` with `OptionWhenValid(...)` when a settings or deployment view should show DataAnnotations-style issues and keep save/apply actions disabled until the model is valid; validation summary palette helpers preserve RGB/hex colors and ANSI decorations on capable terminals.
When the view model owns the palette, model-backed sections and options can use selectors returning `ConsoleColor`, `Style`, style text, `Color`, or `RgbColor`; rich visual helpers preserve RGB/hex and decoration metadata on ANSI-capable terminals while keeping console-color fallbacks.

See [Consoul/README.md](Consoul/README.md) for complete core usage examples, including model-backed dynamic views, spinner/status/live-display/multi-progress helpers, align/badge/bar-chart/breadcrumb/breakdown-chart/bullet-list/calendar/callout/canvas/checklist/columns/exception/figlet-text/grid/headline/layout/metrics/padding/panel/property-list/rows/rule/shortcut-bar/sparkline/text-block/text-path/timeline/tree view helpers, render capture for logs/tests, typed text prompts, result-first single and multi-selection helpers, tables, prompts, and form/object editing.

Use `ConsoulTheme` presets for fast visual polish across prompts, markup, charts, and view helpers:

```csharp
using (Consoul.UseTheme(ConsoulTheme.Modern))
{
    Consoul.MarkupLine("[prompt]Ready[/] [option]Deploy[/]");
}
```

Exploratory settings and plain model objects can be edited from a model-backed `DynamicView<TModel>` by adding the built-in object editor as a view option:

```csharp
Consoul.View("Settings", settings)
    .RenderModel(model => new PropertyListEntry().AddModel(model))
    .EditForm("Edit settings")
    .Render();
```

Use `Consoul.Form(settings).Render()` when a proof-of-concept only needs the generated form editor. The form helper uses the same MVC-style object editor stack as `Consoul.EditObject(...)`, so model metadata, validation, and optional JSON value formatting stay consistent.

## Repository Projects

- `Consoul` is the packable core library.
- `Consoul.Cli` is the packable command-line application helper package.
- `Consoul.Extensions.Logging` is the packable logging integration.
- `Consoul.Extensions.Configuration` is the packable configuration integration.
- `Consoul.Text.Json` is the packable JSON serializer integration for form/object-editor value rendering and syntax-colored JSON snapshots.
- `Consoul.Tests` contains xUnit coverage for input handling, rendering, prompts, and metadata behavior.
- `Consoul.Test` is a sample console application used for manual exploration, including small POC menu, badge, breadcrumb, bullet-list, callout, metrics, shortcut bar, timeline, table, and settings object-editor views.

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
