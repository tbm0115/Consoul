# Consoul

`Consoul` is the dependency-light core package for building interactive console applications with prompts, views, tables, progress bars, fixed messages, and scriptable routines.

The core package intentionally avoids `Microsoft.Extensions.*` and `System.Text.Json` package dependencies. Optional integration packages are available when you want those ecosystems:

| Package | Purpose |
| --- | --- |
| `Consoul.Extensions.Logging` | Adds `Microsoft.Extensions.Logging` support. |
| `Consoul.Extensions.Configuration` | Bridges `Microsoft.Extensions.Configuration` into routine transforms. |
| `Consoul.Text.Json` | Uses `System.Text.Json` for form/object-editor value rendering and syntax-colored JSON snapshots. |

## Installation

Install the core package from NuGet:

```bash
dotnet add package Consoul
```

Or install from the GitHub package registry:

```bash
dotnet add package Consoul --source "https://nuget.pkg.github.com/tbm0115/index.json"
```

## Quick Start

```csharp
using ConsoulLibrary;
using ConsoulLibrary.Color;

Consoul.MarkupLine("[cyan]Welcome to Consoul[/]");

if (Consoul.Ask("Continue?"))
{
    var prompt = new SelectionPrompt("Choose a task");
    prompt.Add("Render a table", ConsoleColor.Green, isDefault: true);
    prompt.Add("Show progress", ConsoleColor.Blue);
    prompt.Add("Exit", ConsoleColor.DarkGray);

    PromptResult result = prompt.Render();
    if (result.HasSelection)
    {
        Consoul.Write($"Selected option {result.Index + 1}");
    }
}
```

## Markup

Use lightweight markup when a view needs quick inline color without splitting a sentence into multiple `Write` calls. Color tags use `ConsoleColor` names, common aliases, named Consoul schemes, optional background colors, and nearest-match hex colors such as `[#22c55e]`. Common Spectre-style modifiers such as `bold`, `dim`, `underline`, and `link` are accepted as compatibility tags. Link tags render as OSC-8 hyperlinks when `AnsiConsole.Profile.Capabilities.Links` is enabled and otherwise keep plain visible text. Style-only tags render with the current color scheme.

```csharp
Consoul.MarkupLine("Status: [green]Ready[/]");
Consoul.MarkupLine("[white on darkblue] Deployment [/]");
Consoul.MarkupLine("[#22c55e]Nearest 16-color terminal match[/]");
Consoul.MarkupLine("[bold red]Blocked[/] [dim](approval required)[/]");
Consoul.MarkupLine("[link=https://example.com]Runbook[/]");
Consoul.MarkupLine("[green]:check_mark:[/] Ready [red]:x:[/] Failed");
Consoul.MarkupLine("[prompt]Choose an action[/] [subnote](or type exit)[/]");
Consoul.MarkupLine("Target: [green]{0}[/]", "Production [hot]");
```

Use composite-format overloads, `MarkupLineInterpolated(...)`, or reusable `Markup.FromFormat(...)` widgets when inserted model or user values may contain square brackets. Tags in the template remain active, while formatted or interpolated values are escaped automatically.

```csharp
string target = "Production [red]hot[/]";
Consoul.MarkupLineInterpolated($"Deploying to [cyan]{target}[/]");
AnsiConsole.MarkupLine("Deploying to [cyan]{0}[/]", target);
```

Use doubled brackets when you need literal bracket characters:

```csharp
Consoul.MarkupLine("[[not a tag]]");
Consoul.MarkupLine("[cyan]" + Consoul.EscapeMarkup(target) + "[/]");
```

Reusable markup widgets also include Spectre-style helpers for escaping, stripping, interpolated construction, and composite-format construction:

```csharp
Markup safe = Markup.FromInterpolated($"Target: [cyan]{target}[/]");
Markup reusable = Markup.FromFormat("Target: [cyan]{0}[/]", target);
string escaped = Markup.Escape(target);
string plain = Markup.Remove("[green]Ready[/] [[literal]]");
string samePlain = Consoul.RemoveMarkup("[green]Ready[/] [[literal]]");
```

Both `[/]` and named closing tags such as `[/red]` close the current style, which makes pasted examples more forgiving.

Known emoji shortcodes are replaced in markup strings using the same `:name:` shape as Spectre.Console. Use `Emoji.Replace(...)`, `Emoji.Get(...)`, or `Emoji.Known` constants when a view helper needs the same symbols outside markup rendering.

## Spectre-Style Entry Points

If you are moving examples from Spectre.Console, `AnsiConsole` provides familiar method names over Consoul's existing core. These wrappers are dependency-free and intentionally return Consoul result types for prompts, so cancellation and empty selections stay explicit while your screens can still graduate into MVC-style `StaticView` or `DynamicView<TModel>` classes.

```csharp
AnsiConsole.MarkupLine("[cyan]Deployment[/] [green]ready[/]");
AnsiConsole.Rule("Checks");
AnsiConsole.Panel("All systems ready", new PanelOptions { Title = "Status" });

var prompt = new SelectionPrompt<string>()
    .Title("Choose an action")
    .AddChoices("Preview", "Deploy");

SelectionResult<string> action = AnsiConsole.Prompt(prompt);
if (action.HasSelection)
{
    AnsiConsole.MarkupLineInterpolated($"Selected [green]{action.SelectedItem}[/]");
}
```

For value input, the facade preserves Consoul's result-first prompt model:

```csharp
TextPromptResult<int> port = AnsiConsole.Ask<int>("Port");
TextPromptResult<int> retries = await AnsiConsole.AskAsync("Retries", 3);
ConfirmResult deploy = AnsiConsole.Confirm("Deploy to production?", defaultValue: false);
ConfirmResult preview = await AnsiConsole.ConfirmAsync("Enable preview?", defaultValue: true);
```

For pasted Spectre-style snippets that assign directly to values, prompt results also provide value-style conversions while keeping `HasValue`, `HasSelection`, and `IsCanceled` available:

```csharp
int portValue = AnsiConsole.Ask<int>("Port");
bool shouldDeploy = AnsiConsole.Confirm("Deploy?");
List<string> targets = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>().AddChoices("dev", "prod"));
```

Prompt builders also implement `IPrompt<TResult>`, so reusable services can accept a prompt object, call `AnsiConsole.Prompt(...)` or `AnsiConsole.PromptAsync(...)`, and still receive Consoul's explicit result types instead of sentinel indexes:

```csharp
IPrompt<SelectionResult<DeployTarget>> targetPrompt = new SelectionPrompt<DeployTarget>()
    .Title("Target")
    .UseConverter(target => target.Name)
    .AddChoices(development, production);

SelectionResult<DeployTarget> target = await AnsiConsole.PromptAsync(targetPrompt);
```

When a service or reusable POC component should avoid static calls, depend on `IAnsiConsole` and pass `AnsiConsole.Console` by default. The injected facade tracks the same migration-helper method names as the static `AnsiConsole` facade, including output, prompts, panels, rules, tables, trees, lists, grids, layouts, calendars, canvases, canvas images, validation/property summaries, metrics, charts, status/progress/live scopes, cursor/profile access, and capture. It still uses the active Consoul console driver, so tests can install a virtual driver and production code can use the real terminal. For Spectre-style test setup, `AnsiConsole.Console` is assignable; static facade helpers route through the assigned facade until you restore the previous value:

Primitive, formatted, and renderable output overloads are available on `IAnsiConsole` as extension methods, so injected services can keep simple migrated calls such as `console.Write(42)`, `console.WriteLine(false)`, `console.WriteLine(new Panel("Ready"))`, and `console.Write(CultureInfo.InvariantCulture, "Port {0}", port)` without leaving the same driver-backed rendering path.

```csharp
public sealed class DeploymentScreen
{
    private readonly IAnsiConsole _console;

    public DeploymentScreen(IAnsiConsole console = null)
    {
        _console = console ?? AnsiConsole.Console;
    }

    public void Render()
    {
        _console.MarkupLine("[cyan]Deployment[/]");
        _console.WriteLine(new Panel("Ready").Header("Status"));
        _console.Status("Preparing", status => status.Succeed("Ready"));
    }
}
```

```csharp
IAnsiConsole previous = AnsiConsole.Console;
try
{
    AnsiConsole.Console = testConsole;
    AnsiConsole.MarkupLine("[green]Recorded by the test facade[/]");
}
finally
{
    AnsiConsole.Console = previous;
}
```

Use `AnsiConsole.Create(...)` with `AnsiConsoleSettings` when a test, sample, or embedded POC host should write to a specific `TextWriter` and read from a specific `TextReader` instead of the process console. Created consoles keep their own width/profile, cursor state, input stream, and recording state while still rendering through Consoul's MVC-style components.

```csharp
var writer = new System.IO.StringWriter();
IAnsiConsole previewConsole = AnsiConsole.Create(new AnsiConsoleSettings
{
    Out = new AnsiConsoleOutput(writer, width: 48, height: 20),
    In = new System.IO.StringReader("Production\n")
});

previewConsole.MarkupLine("[green]Preview ready[/]");
previewConsole.Panel("Rendered through a writer", new PanelOptions { Title = "Preview" });
TextPromptResult<string> target = previewConsole.Prompt(
    new TextPrompt<string>("Target").AddChoices("Development", "Production"));

string renderedPreview = writer.ToString();
```

Created consoles also honor Spectre-style profile settings, so tests and embedded POC hosts can make width, interaction, ANSI, and color branches deterministic:

```csharp
IAnsiConsole testConsole = AnsiConsole.Create(new AnsiConsoleSettings
{
    Out = new AnsiConsoleOutput(writer, width: 60, height: 12, isTerminal: false),
    Ansi = AnsiSupport.Yes,
    ColorSystem = ColorSystemSupport.TrueColor,
    Interactive = InteractionSupport.Yes
});

if (testConsole.Profile.Capabilities.ColorSystem == ColorSystem.TrueColor)
{
    testConsole.MarkupLine("[green]True-color branch enabled[/]");
}
```

Typed prompts support migration-friendly aliases such as `Title(...)`, `PromptStyle(...)`, `InstructionsText(...)`, `AddChoice(...)`, `AddChoices(...)`, `AddChoiceGroup(...)`, `AddGroup(...)`, `AddChild(...)`, `UseConverter(...)`, `SearchEnabled(...)`, `EnableSearch(...)`, `UseKeyboardNavigation(...)`, `UseWrapAround(...)`/`WrapAround(...)`, `WithPageSize(...)`/`PageSize(...)`, `WithMoreChoicesText(...)`/`MoreChoicesText(...)`, `WithSearchPlaceholderText(...)`/`SearchPlaceholderText(...)`, `FilterCommandPrefix(...)`, `FilterHelpText(...)`, `ActiveFilterText(...)`, `NoFilterMatchesText(...)`, `KeyboardNavigationHelpText(...)`, `KeyboardFilterHelpText(...)`, `WithInvalidSelectionMessage(...)`/`InvalidSelectionMessage(...)`, `DefaultValue(...)`, `Select(...)`, `SelectAll()`, `OnCancel(...)`, the `CancelResult` property, `DisabledStyle(...)`, `SearchHighlightStyle(...)`, `ParseWith(...)`, `ValidationErrorMessage(...)`, `InvalidChoiceMessage(...)`, `AllowEmpty()`, `NotRequired()`, `NotEmpty(...)`, `MinimumLength(...)`, `MaximumLength(...)`, `LengthBetween(...)`, `Choices(...)`, `ShowChoices(...)`, `HideChoices()`, `ShowDefaultValue(...)`, `HideDefaultValue()`, `EditableDefaultValue(...)`, `ClearOnFinish(...)`, `Secret(...)`, `Mask(...)`, `MaskStyle(...)`, `ChoicesSeparator(...)`, `ChoicesStyle(...)`, `DefaultValueStyle(...)`, and `Culture(...)` while still returning MVC-friendly domain objects. On selection and multi-selection prompts, `InvalidChoiceMessage(...)` aliases invalid-selection feedback for pasted Spectre-style snippets. `TextPrompt<T>.Validate(...)` accepts message, predicate, and `PromptValidationResult.Success()` / `PromptValidationResult.Error(...)` delegates for structured POC form rules. Prompt color helpers accept `ConsoleColor` values, reusable `Style` objects, `Color` / `RgbColor` palette values, and foreground style strings:

```csharp
var development = new DeployTarget("Development", "dev");
var production = new DeployTarget("Production", "prod");

var target = new SelectionPrompt<DeployTarget>()
    .Title("Target")
    .PromptStyle(Color.Aqua)
    .InstructionStyle(RgbColor.DarkGray)
    .SearchHighlightStyle(Color.Yellow)
    .UseConverter(item => item.Name)
    .EnableSearch()
    .UseKeyboardNavigation()
    .PageSize(8)
    .MoreChoicesText("Showing {0}-{1} of {2}.")
    .WrapAround(false)
    .SearchPlaceholderText("Search targets")
    .InvalidSelectionMessage("Choose a listed target.")
    .DefaultValue(production);

target.AddChoice(development);
target.AddChoice(production);

SelectionResult<DeployTarget> selected = AnsiConsole.Prompt(target);
```

Plain output snippets can use Spectre-style value and format overloads without leaving Consoul's console-driver path:

```csharp
AnsiConsole.Write(42);
AnsiConsole.Write((object)jobStatus);
AnsiConsole.Write(System.Globalization.CultureInfo.InvariantCulture, " workers: {0:0.0}", 3.5);
AnsiConsole.WriteLine(true);
AnsiConsole.WriteLine((object)DateTimeOffset.Now);
AnsiConsole.WriteLine(System.Globalization.CultureInfo.InvariantCulture, "elapsed: {0:0.00}s", 12.5);
AnsiConsole.WriteAnsi(() => AnsiConsole.Write("\u001b[2K"));
```

Object overloads use normal `ToString()` formatting for plain values. Boxed Consoul renderables still render through `IConsoulRenderable`; the Spectre-style `IRenderable` alias is also recognized, so visual components do not fall back to their type name when routed through generic helper code.

Renderable snippets can use `new Markup(...)` and `new Rule(...)` while still flowing through Consoul's markup and rule renderers. Rule titles can also contain raw Spectre-style markup, which is measured by visible text and rendered with parsed title colors:

```csharp
AnsiConsole.Write(new Markup("[green]Ready[/]").Append(" for [cyan]prod[/]").NewLine());
AnsiConsole.Render(new Panel("Renderable alias").Header("Render"));
AnsiConsole.Console.Render(new Markup("[cyan]Injected console alias[/]").NewLine());

AnsiConsole.Write(new Rule("Checks")
    .Justify(Justify.Left)
    .RuleStyle("grey")
    .TitleStyle("cyan"));

AnsiConsole.Write(new Rule("Deploy", ConsoleColor.Cyan, Justify.Right)
    .RuleStyle("grey"));

AnsiConsole.Write(new Rule("Deploy", Color.Aqua, Justify.Right)
    .RuleColor(RgbColor.Green)
    .TitleColor(Color.Yellow));

AnsiConsole.Write(new Rule
{
    Title = "Deploy",
    Justification = Justify.Left,
    Style = new Style(ConsoleColor.DarkGray)
}.TitleStyle("aqua"));
```

Common fluent color helpers also accept Spectre-style style strings and reusable `Style` objects, so pasted snippets can use names, aliases, backgrounds, nearest-match hex colors, and decoration words such as `bold`, `dim`, `italic`, `underline`, or `strike` before a screen graduates into MVC-style views. Text path segment colors, canvas drawing helpers, composition rows/cells/panes, model-summary values, and per-item factories for lists, charts, metrics, timelines, shortcut bars, and calendars use the same inputs, which keeps diagnostics and quick diagrams on the shared palette. Builders that align content also accept `Justify.Left`, `Justify.Center`, and `Justify.Right` as migration-friendly aliases over Consoul's existing alignment enums. Use `Style.Parse(...)`, `Style.ParseColor(...)`, `Style.Plain.WithForeground(...)`, or `Style.Bold.WithBackground(...)` when you need the same parsing or immutable style composition outside a builder chain; `Style.Parse(...)` preserves decoration metadata even when the built-in console renderer can only show the foreground/background colors. Add `using ConsoulLibrary.Color;` when ported snippets use `Color.Red`, `Color.FromHex(...)`, `Color.Rgb(...)`, `RgbColor.Red`, `RgbColor.FromHex(...)`, or `Colors.Grey`; `Color` mirrors the pasted-snippet shape while `RgbColor` keeps the underlying RGB channels and maps to the nearest `ConsoleColor` for Consoul's dependency-light renderer.

```csharp
AnsiConsole.Write(new Text("Ready").Color("white on navy"));
AnsiConsole.Write(new Text("Deploy").Color(Color.FromHex("#22c55e")));
AnsiConsole.Write(new Text("Reusable").Color(Style.Plain.WithForeground(Color.Aqua)));
AnsiConsole.Write(new Text("Initializer") { Style = new Style(Color.Aqua) });
AnsiConsole.Write(new Panel("Build complete")
    .Header("Status", Justify.Center)
    .Padding(1, 0, 1, 0)
    .BorderColor(Style.Plain.WithForeground(Colors.Grey)));
AnsiConsole.Write(new Table().Title("Jobs").TitleColor("#22c55e"));
AnsiConsole.Write(new Sparkline(new[] { 12d, 10d, 14d })
    .Label("Latency")
    .LineColor(Style.Plain.WithForeground(RgbColor.Green)));
AnsiConsole.Write(new ShortcutBar()
    .AddShortcut("Esc", "Back")
    .KeyColor(Style.Plain.WithForeground(RgbColor.Aqua)));
```

Cursor snippets can use the same Spectre-style cursor helpers while Consoul clamps movement to the active terminal or fake-console bounds:

```csharp
AnsiConsole.Cursor.Hide();
try
{
    AnsiConsole.Cursor.SetPosition(0, 0);
    AnsiConsole.MarkupLine("[green]Refreshing dashboard[/]");
    AnsiConsole.Cursor.MoveDown(1);
}
finally
{
    AnsiConsole.Cursor.Show();
}
```

Alternate-screen snippets can use `AnsiConsole.AlternateScreen(...)` for temporary dashboards without changing the main terminal scrollback. Injected consoles get the same helper through extension methods, and non-ANSI outputs simply run the action without emitting escape sequences:

```csharp
await AnsiConsole.AlternateScreenAsync(async cancellationToken =>
{
    AnsiConsole.Live(new Panel("Starting").Header("Deploy"))
        .Start(live =>
        {
            live.UpdateTarget(new Panel("[green]Ready[/]").Header("Deploy"));
            live.Refresh();
        });

    await Task.Delay(250, cancellationToken);
});
```

Manual tables can use `new Table()` with fluent column and row builders while still rendering through Consoul's responsive table engine. Add `using ConsoulLibrary.Color;` for `Color.Grey`, `Color.Red`, `Color.FromHex(...)`, `Colors.Grey`, `RgbColor.Red`, and related aliases:

```csharp
var table = new Table()
    .Title("Jobs", Color.Aqua, Justify.Center)
    .Caption("Data refreshed", RgbColor.DarkGray, Justify.Right)
    .Width(42)
    .Border(TableBorder.Rounded)
    .BorderColor(Color.Grey)
    .HeaderColor(RgbColor.Yellow)
    .ContentColor(Color.White)
    .AlternatingRowColor(RgbColor.Cyan)
    .HideFooter()
    .FooterColor(RgbColor.Magenta)
    .AddColumn(new TableColumn("Job").Width(14).Padding(2, 1))
    .AddColumn("State", column => column.Centered())
    .AddColumn(new TableColumn("Retries").RightJustified().Width(8).PadRight(2).Footer("2"))
    .AddRow("Build", "Queued", 0)
    .AddEmptyRow()
    .InsertRow(1, "Deploy", "Waiting", 2)
    .UpdateCell(0, 1, "Ready")
    .RemoveRow(2);

AnsiConsole.Write(table);
```

Manual panels follow the same pattern for Spectre-style snippets while still rendering through Consoul's panel entry. Raw markup in panel content, headers, and footers preserves foreground/background styling while wrapping and alignment use the visible text:

```csharp
var panel = new Panel("Build complete")
    .Header("Status")
    .Footer("Esc to close")
    .Border(BoxBorder.Rounded)
    .Width(36)
    .Padding(1)
    .HeaderAlignment(Justify.Center)
    .FooterAlignment(Justify.Right);

AnsiConsole.Write(panel);

AnsiConsole.Write(new Panel(table)
    .Header("Jobs")
    .Width(48)
    .Border(BoxBorder.Rounded));

AnsiConsole.Write(new Panel(() =>
{
    AnsiConsole.MarkupLine("[green]Ready[/]");
    AnsiConsole.Badge("prod", BadgeKind.Info);
})
    .Header("Status")
    .BorderCharacters('[', ']', '[', ']', '=', '!'));
```

Charts and trees can use builder aliases while remaining reusable renderables for MVC-style views. Chart titles and item labels can contain raw markup, so status colors can stay inline while responsive layout still measures visible text:

```csharp
var tree = new Tree("Deployment")
    .Guide(TreeGuide.Ascii);

tree.AddNodes("Docs", "Scripts");
tree.AddNodes("Build #", 42, new Markup("[green]Ready[/]"));
tree.AddNode("Checks").AddNode("Build");
tree.AddNode(new Markup("[green]Release[/]"));
tree.AddNode(new Panel("Ready").Border(BoxBorder.Ascii).Width(18), "yellow");
tree.AddNode("Archived").Collapse().AddNode("Hidden legacy target");
AnsiConsole.Write(tree);

var services = new[]
{
    new { Name = "API", Color = ConsoleColor.Cyan, Style = "aqua" },
    new { Name = "Worker", Color = ConsoleColor.Magenta, Style = "white on navy" }
};

AnsiConsole.Write(new Tree("Services")
    .Width(40)
    .AddNodes(services, service => service.Name, service => service.Color)
    .AddNodes(services, service => "Styled " + service.Name, service => service.Style));

var chart = new BarChart()
    .Label("Job health")
    .CenterLabel()
    .Width(40)
    .MaximumValue(10)
    .AddItem("Passing", 8, ConsoleColor.Green)
    .AddItem("Failing", 2, ConsoleColor.Red);

AnsiConsole.Write(chart);

var jobs = new[]
{
    new { Name = "Queued", Count = 4d, Color = Color.FromHex("#facc15"), Rgb = RgbColor.Yellow, StyleText = "white on navy" },
    new { Name = "Running", Count = 2d, Color = Color.Green, Rgb = RgbColor.Green, StyleText = "green" },
    new { Name = "Failed", Count = 1d, Color = Color.Red, Rgb = RgbColor.Red, StyleText = "red" }
};

AnsiConsole.Write(new BarChart()
    .Width(40)
    .AddItems(jobs, job => job.Name, job => job.Count, job => job.Color)
    .AddItems(jobs, job => job.Name + " rgb", job => job.Count, job => job.Rgb)
    .AddItems(jobs, job => job.Name + " styled", job => job.Count, job => job.StyleText));

AnsiConsole.Write(new BreakdownChart()
    .Label("Queue mix")
    .CenterLabel()
    .Width(40)
    .ShowPercentage()
    .AddItem("Queued", 4, Color.Yellow)
    .AddItem("Running", 2, RgbColor.Green)
    .AddItem("Failed", 1, Color.Red)
    .AddItems(jobs, job => job.Name + " styled", job => job.Count, job => job.StyleText));
```

Dashboard widgets use the same renderable-builder pattern for quick POC status screens. Metric labels, values, units, and notes, plus sparkline labels, can contain raw markup while still wrapping, padding, and truncating by visible text. Metric color helpers and sparkline label, line, and range colors accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values:

```csharp
AnsiConsole.Write(new Metrics()
    .Width(40)
    .ColumnCount(2)
    .LabelColor(RgbColor.DarkGray)
    .ValueColor(Color.White)
    .SuccessColor(Color.Green)
    .WarningColor(RgbColor.Yellow)
    .AddSuccess("Passing", 8, "jobs")
    .AddWarning("Waiting", 2));

var health = new[]
{
    new { Name = "Passing", Count = 8, Unit = "jobs", Note = "healthy", Kind = MetricKind.Success, LabelColor = Color.Green, Rgb = RgbColor.Green, LabelStyle = "grey", ValueStyle = "white on navy", NoteStyle = "darkgray" },
    new { Name = "Waiting", Count = 2, Unit = "jobs", Note = "approval", Kind = MetricKind.Warning, LabelColor = Color.Yellow, Rgb = RgbColor.Yellow, LabelStyle = "grey", ValueStyle = "yellow", NoteStyle = "aqua" }
};

AnsiConsole.Write(new Metrics()
    .Width(40)
    .AddMetrics(health, item => item.Name, item => item.Count, item => item.Unit, item => item.Note, item => item.Kind)
    .AddMetrics(health, item => item.Name + " color", item => item.Count, item => item.LabelColor, item => item.LabelColor, item => item.LabelColor, unitSelector: item => item.Unit, noteSelector: item => item.Note, kindSelector: item => item.Kind)
    .AddMetrics(health, item => item.Name + " rgb", item => item.Count, item => item.Rgb, item => item.Rgb, item => item.Rgb, unitSelector: item => item.Unit, noteSelector: item => item.Note, kindSelector: item => item.Kind)
    .AddMetricsStyled(health, item => item.Name + " styled", item => item.Count, item => item.Unit, item => item.Note, item => item.Kind, item => item.LabelStyle, item => item.ValueStyle, item => item.NoteStyle));

AnsiConsole.Write(new Sparkline(new[] { 12d, 10d, 11d, 8d, 7d, 9d })
    .Width(40)
    .Label("Duration")
    .ShowRange()
    .LineColor(RgbColor.Green)
    .RangeColor(Color.Aqua));

var durations = new[]
{
    new { Stage = "Build", Minutes = 12d },
    new { Stage = "Test", Minutes = 18d },
    new { Stage = "Deploy", Minutes = 10d }
};

AnsiConsole.Write(new Sparkline()
    .Width(40)
    .Label("Duration")
    .ShowRange()
    .AddValues(durations, point => point.Minutes));

AnsiConsole.Write(new Timeline()
    .Width(40)
    .CompleteColor(Style.Plain.WithForeground(RgbColor.Green))
    .ActiveColor(Style.Plain.WithForeground(RgbColor.Aqua))
    .AddComplete("Build")
    .AddActive("Review", "Waiting for owner approval.")
    .AddStepStyled("Package", TimelineItemState.Warning, "Direct style text", titleStyle: "white on navy", markerStyle: "yellow", descriptionStyle: "grey")
    .AddPending("Publish"));

var workflow = new[]
{
    new { Name = "Build", State = TimelineItemState.Complete, Detail = "Artifacts ready", At = DateTimeOffset.Now, Color = Color.Green, Rgb = RgbColor.Green, TitleStyle = "white on navy", MarkerStyle = "green", DescriptionStyle = "grey" },
    new { Name = "Review", State = TimelineItemState.Active, Detail = "Waiting for owner", At = DateTimeOffset.Now.AddMinutes(15), Color = Color.Aqua, Rgb = RgbColor.Aqua, TitleStyle = "aqua", MarkerStyle = "aqua", DescriptionStyle = "darkgray" }
};

AnsiConsole.Write(new Timeline()
    .Width(40)
    .TimestampFormat("HH:mm")
    .AddSteps(workflow, step => step.Name, step => step.State, step => step.Detail, step => (DateTimeOffset?)step.At)
    .AddSteps(workflow, step => step.Name + " color", step => step.Color, step => step.Color, step => step.Color, stateSelector: step => step.State, descriptionSelector: step => step.Detail, timestampSelector: step => (DateTimeOffset?)step.At)
    .AddSteps(workflow, step => step.Name + " rgb", step => step.Rgb, step => step.Rgb, step => step.Rgb, stateSelector: step => step.State, descriptionSelector: step => step.Detail, timestampSelector: step => (DateTimeOffset?)step.At)
    .AddStepsStyled(workflow, step => step.Name + " styled", step => step.State, step => step.Detail, step => (DateTimeOffset?)step.At, step => step.TitleStyle, step => step.MarkerStyle, step => step.DescriptionStyle));

var actions = new[]
{
    new { Key = "Enter", Description = "Select", Disabled = false, KeyStyle = "aqua", DescriptionStyle = "white" },
    new { Key = "F5", Description = "Refresh", Disabled = true, KeyStyle = "red", DescriptionStyle = "grey" }
};

AnsiConsole.Write(new ShortcutBar()
    .Width(40)
    .AddShortcut("Enter", "Select", "white on navy", "grey")
    .AddShortcut("Esc", "Back")
    .AddShortcuts(actions, action => action.Key, action => action.Description, action => action.Disabled)
    .AddShortcuts(actions, action => action.Key, action => action.Description, action => action.KeyStyle, action => action.DescriptionStyle));
```

The facade also forwards Consoul's helper-style calls for developers adapting procedural Spectre-style snippets:

```csharp
AnsiConsole.Badge("ready", BadgeKind.Success);
AnsiConsole.Metrics(metrics =>
{
    metrics.Add("Passing", 8, "jobs", kind: MetricKind.Success);
    metrics.Add("Waiting", 2, kind: MetricKind.Warning);
});
AnsiConsole.ShortcutBar(bar => bar.Add("Enter", "Select"));
AnsiConsole.ValidationSummary(
    settings,
    new ValidationSummaryOptions { Width = 48 }.InvalidColor(Color.Red).IssueColor(RgbColor.Yellow));
```

Composition builders make quick dashboard layouts readable without leaving the renderable path. Their color helpers accept `ConsoleColor`, reusable `Style` objects, Spectre-style style strings, `Color`, and `RgbColor` values, and item-level row, cell, column, and pane overloads can use the same palette vocabulary so branded POC regions share colors with tables, prompts, and status widgets. `Padder.Color(...)` and `Align.Color(...)` keep wrapper components on that same shared-palette path. `Rows`, `Columns`, and `Grid` can mix plain values, configured row/cell objects, strings, and reusable renderables, which keeps quick MVC-style diagnostic layouts from needing adapter code. Aligned text, padded text, row text, column content, grid headers, grid cell text, layout pane titles, and layout pane content can contain raw markup and still wrap, collapse, pad, align, and render by visible text. `Rows.AddRows(...)`, `Columns.AddColumns(...)`, and `Grid.AddCells(...)` accept text, renderable, and mixed object params as well as enumerables for pasted fluent snippets; selector-backed overloads such as `Rows.AddRows(models, item => item.Summary, item => item.StyleText)`, `Columns.AddColumns(models, item => item.Panel, item => item.Title, item => item.StyleText)`, `Grid.AddCells(models, item => item.Value, item => item.Label, item => item.StyleText)`, and `Layout.AddPanes(models, item => item.Content, item => item.Title, item => item.StyleText)` keep model-backed dashboard sections out of hand-written adapter loops. Those row, column, cell, and pane color selectors can also return `Color` or `RgbColor` directly when the view model owns palette objects. `Layout.AddPanes(...)` selectors can return plain values, reusable renderables, render actions, configured `LayoutPane` instances, or nested `Layout` regions. `Align` accepts `LeftJustified()` and `RightJustified()` aliases alongside `LeftAligned()`, `Centered()`, and `RightAligned()`, so dashboard snippets use the same alignment vocabulary as text, rules, and figlet titles. Captured `Align` and `Padder` content stays plain text after capture, while direct text parses raw markup. `GridColumn` accepts `LeftAligned()`, `Centered()`, and `RightAligned()` helpers alongside `Justify(...)`, so compact model-state grids use the same fluent alignment names as table columns. Named layouts can use `Size(...)`, `Ratio(...)`, `MinimumSize(...)`, `Visible(...)`, and `Invisible()` hints while still being updated as MVC-style view regions.

```csharp
AnsiConsole.Rows(
    new RowsOptions { Width = 36, Spacing = 0 },
    "Summary",
    new Markup("[green]Ready[/]"),
    2);

AnsiConsole.Columns(
    new ColumnsOptions { Width = 48, Columns = 3 },
    "API",
    new Markup("[green]Ready[/]"),
    3);

AnsiConsole.Grid(
    new GridOptions { Width = 48, Columns = 3 },
    "Worker",
    new Markup("[yellow]Queued[/]"),
    4);

AnsiConsole.Write(new Rows()
    .Spacing(0)
    .AddRow(new Markup("[green]Ready[/]"))
    .AddRow(new Rule("Next"))
    .AddRow(2));

var checklistRows = new List<IConsoulRenderable>
{
    new Markup("[green]Build[/]"),
    new Markup("[yellow]Review[/]")
};
AnsiConsole.Write(new Rows(checklistRows).Collapse().Spacing(0));

var statuses = new[]
{
    new { Name = "API", State = "Ready", Warning = false },
    new { Name = "Worker", State = "Waiting", Warning = true }
};

AnsiConsole.Write(new Rows()
    .Spacing(0)
    .AddRows(
        statuses,
        status => new Markup(status.Warning ? "[yellow]" + status.Name + ": " + status.State + "[/]" : "[green]" + status.Name + ": " + status.State + "[/]")));

AnsiConsole.Write(new Columns()
    .ColumnCount(2)
    .AddColumn(new Panel("Ready").Header("API"))
    .AddColumn("Retries: 2"));

var serviceColumns = new List<IConsoulRenderable>
{
    new Text("API").Color("green"),
    new Text("Worker").Color("yellow")
};
AnsiConsole.Write(new Columns(serviceColumns).Collapse());

AnsiConsole.Write(new Columns()
    .ColumnCount(2)
    .AddColumns(
        statuses,
        status => new Markup(status.Warning ? "[yellow]" + status.State + "[/]" : "[green]" + status.State + "[/]"),
        status => status.Name));

AnsiConsole.Write(new Grid()
    .AddColumns(
        new GridColumn().Width(14).Padding(1, 1).NoWrap(),
        new GridColumn().RightAligned())
    .Collapse()
    .AddRow("Build", new Markup("[green]Ready[/]"))
    .AddEmptyRow()
    .AddRow("Deploy", 2));

AnsiConsole.Write(new Grid()
    .ColumnCount(3)
    .AddCells(
        statuses,
        status => new Markup(status.Warning ? "[yellow]" + status.State + "[/]" : "[green]" + status.State + "[/]"),
        status => status.Name));

AnsiConsole.Write(new Padder(new Panel("Ready").Header("Status"))
{
    Padding = new Padding(2, 1),
    Expand = true
}.Color(RgbColor.Green));

AnsiConsole.Write(new Align(new Panel("Centered summary").Header("Overview"), HorizontalAlignment.Center)
    .Width(48)
    .Color(Color.Aqua));

var target = new Tree("Deploy");
target.AddNode("Production");

var layout = new Layout("Dashboard")
    .SplitRows(
        new Layout("Header").Size(3),
        new Layout("Body").SplitColumns(
            new Layout("Tests").Ratio(2).MinimumSize(18),
            new Layout("Target").Ratio(1),
            new Layout("Debug").Invisible()));

layout["Header"].Update(new Panel("POC deployment view").Header("Dashboard"));
layout["Tests"].Update(new BarChart().AddItem("Passing", 8));
layout["Target"].Update(target);

AnsiConsole.Write(layout);
```

Text and terminal-art builders cover quick labels, splash text, schedules, and small diagrams. `Text` and `Paragraph` color helpers accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values. Their content can include raw markup such as `[green]Ready[/]`; the text-block renderer wraps, aligns, crops, and ellipsizes by visible text while preserving parsed colors:

```csharp
AnsiConsole.Write(new Text("Ready", ConsoleColor.Green));
AnsiConsole.Write(new Text("Deploy", Color.Aqua));
AnsiConsole.Write(new Text("Owner approval", "white on navy"));

AnsiConsole.Write(new Text("Ready for deployment")
    .Centered()
    .Width(32)
    .Color(RgbColor.Green));

AnsiConsole.Write(new Paragraph("Status: ", Color.Aqua)
    .Append("Queued", RgbColor.Yellow)
    .AppendLine()
    .AppendLineStyled("Ready", "green")
    .AppendMarkup("Next [yellow]review[/]")
    .Width(40)
    .Ellipsis());

AnsiConsole.Write(new FigletText("Consoul")
    .Width(48)
    .Character('#')
    .Fitted()
    .Pad(false)
    .Color(ConsoleColor.Cyan));

AnsiConsole.Write(new Calendar(new DateTime(2026, 6, 1))
    .Culture(System.Globalization.CultureInfo.InvariantCulture)
    .HeaderStyle("cyan")
    .HighlightStyle("yellow")
    .AddCalendarEvent("Release", 2026, 6, 15, new Style(ConsoleColor.Green))
    .AddCalendarEvent("Freeze", new DateTime(2026, 6, 20), new Style(ConsoleColor.Magenta)));

var schedule = new[]
{
    new { Name = "Release", Date = new DateTime(2026, 6, 15), Color = ConsoleColor.Green, StyleText = "white on navy" },
    new { Name = "Freeze", Date = new DateTime(2026, 6, 20), Color = ConsoleColor.Magenta, StyleText = "green" }
};

AnsiConsole.Write(new Calendar(new DateTime(2026, 6, 1))
    .Culture(System.Globalization.CultureInfo.InvariantCulture)
    .AddCalendarEvents(schedule, item => item.Date, item => item.Name, item => item.Color)
    .AddCalendarEventsStyled(schedule, item => item.Date, item => item.Name, item => item.StyleText));

AnsiConsole.Write(new Canvas(16, 4)
    .Rectangle(0, 0, 15, 3, '#', Color.Grey)
    .Text(3, 1, "API", RgbColor.Aqua));

AnsiConsole.Write(new Canvas(4, 2)
    .SetPixel(0, 0, Color.Red)
    .SetPixel(0, 1, RgbColor.Green)
    .SetPixel(1, 0, RgbColor.Blue)
    .SetPixel(1, 1, Color.Yellow)
    .MaxWidth(4));

var thumbnail = new[,]
{
    { ConsoleColor.DarkBlue, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.White },
    { ConsoleColor.DarkGreen, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.White },
    { ConsoleColor.DarkRed, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.White },
    { ConsoleColor.Black, ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.White }
};

AnsiConsole.Write(new CanvasImage(thumbnail)
    .PixelWidth(8)
    .MaxWidth(8)
    .BicubicResampler());
```

`Canvas` is still the best fit for MVC-style console diagrams with labels, lines, and boxes. It also accepts Spectre-style `SetPixel(...)` calls for compact half-block pixel sketches; call `Scale(false)` when you want a clipped top-left preview instead of scaled sampling.

`CanvasImage` renders in-memory pixels or `ConsoleColor` matrices as terminal half-blocks without adding image-decoding dependencies to the core package. Keep PNG/JPEG loading in an optional companion package or application adapter, then pass the decoded pixels into `CanvasImage` when the screen needs a quick thumbnail or status image.

Status and list builders keep dashboards scannable. Their marker, label, content, border, separator, key, and disabled color helpers accept `ConsoleColor`, style strings, reusable `Style` objects, `Color`, and `RgbColor` values. Badge labels, breadcrumb labels, bullet-list item text, checklist labels/descriptions, timeline titles/descriptions, property-list labels/values, and shortcut-bar keys/descriptions can also contain raw markup such as `[green]Ready[/]` or `[white on navy]Approval[/]`; rendering wraps by visible text and preserves parsed colors. `Breadcrumb.AddItems(models, item => item.Label)` projects model-backed navigation trails, `BulletList.AddItems("Build", "Test")` and `BulletListItem.AddItems("Lint", "Sign")` accept quick params labels, `AddItems(IEnumerable<BulletListItem>)` accepts reusable nested item models, and bullet lists/checklists accept selector-based `AddItems(...)` overloads for model-backed MVC status views. Breadcrumb, bullet-list, and checklist item projections also accept per-item style strings, such as `Breadcrumb.AddItems(models, item => item.Label, item => item.StyleText)`, `BulletList.AddItems(models, item => item.Label, item => item.StyleText)`, or `Checklist.AddItems(models, item => item.Label, item => item.State, item => item.Description, item => item.StyleText)`. Breadcrumb, bullet-list, and checklist palette selectors can return `Color` or `RgbColor` directly when the view model owns palette objects. Calendar events, property-list label/value rows, shortcut key/description rows, and tree nodes support the same direct `Color` / `RgbColor` selector shape, so quick status notes can keep palette choices in the view model:

```csharp
AnsiConsole.Write(new Badge("ready").Success().Color(Color.Green));

AnsiConsole.Write(new Callout("Waiting for owner approval.")
    .Warning()
    .Title("Deploy")
    .BorderColor(Color.Yellow)
    .TitleColor(RgbColor.Yellow)
    .ContentColor(Color.White));

AnsiConsole.Write(new Breadcrumb()
    .AddItem("Home", "white on navy")
    .AddItems(new[]
    {
        new { Label = "Services", StyleText = "grey" },
        new { Label = "Deployments", StyleText = "green" }
    }, item => item.Label, item => item.StyleText)
    .CurrentColor(RgbColor.Aqua)
    .SeparatorColor(Color.DarkGray));

var notes = new BulletList();
notes.AddItem("Checks").AddItem("Smoke tests");
notes.AddItem("Release");
AnsiConsole.Write(notes);

var steps = new[]
{
    new { Label = "Restore", Done = true, Detail = "Packages restored" },
    new { Label = "Approval", Done = false, Detail = "Waiting for owner" }
};

AnsiConsole.Write(new BulletList()
    .MarkerColor(Color.Aqua)
    .TextColor(RgbColor.White)
    .AddItems(steps, step => step.Label, step => step.Done ? (ConsoleColor?)ConsoleColor.Green : ConsoleColor.Yellow));

AnsiConsole.Write(new Checklist()
    .LabelColor(Color.White)
    .DescriptionColor(RgbColor.Yellow)
    .AddSuccess("Build")
    .AddWarning("Approval", "Waiting for owner")
    .AddItems(new[] { "Docs", "Notify" }, ChecklistItemState.Skipped)
    .AddItems(
        steps,
        step => step.Label,
        step => step.Done ? ChecklistItemState.Complete : ChecklistItemState.Warning,
        step => step.Detail));

var readiness = new[]
{
    ChecklistItem.CreateStyled("Smoke tests", ChecklistItemState.Complete, labelStyle: "aqua"),
    ChecklistItem.Create("Owner approval", ChecklistItemState.Warning, "Waiting")
};
AnsiConsole.Write(new Checklist(readiness).Width(40));
```

## Themes

Use `ConsoulTheme` presets when you want a coherent look without hand-tuning every prompt, chart, and view helper. Themes update the existing `RenderOptions` schemes, so markup aliases such as `[prompt]`, `[subnote]`, `[option]`, and `[invalid]` follow the active theme.

```csharp
Consoul.ApplyTheme(ConsoulTheme.Modern);

Consoul.MarkupLine("[prompt]Deploy[/] [option]Production[/]");
```

For demos, nested views, or one-off panels, use a scoped theme so the previous render options are restored automatically:

```csharp
using (Consoul.UseTheme(ConsoulTheme.HighContrast))
{
    Consoul.Rule("Approval Required");
    Consoul.PropertyList(list =>
    {
        list.Add("Environment", "Production");
        list.Add("Status", "Waiting");
    });
}
```

Use `Consoul.UseRenderOptions(...)` when a POC view or demo needs scoped behavior changes beyond colors, such as prompt filtering, page sizes, or write suppression:

```csharp
using (Consoul.UseRenderOptions(() =>
{
    RenderOptions.ViewPromptFilterEnabled = true;
    RenderOptions.ViewPromptPageSize = 8;
    RenderOptions.WriteMode = RenderOptions.WriteModes.SuppressBlacklist;
    RenderOptions.BlacklistColors.Add(ConsoleColor.DarkGray);
}))
{
    Consoul.Render<OperationsView>();
}
```

## 5-Minute POC View

For quick proof-of-concept terminal views, use `Consoul.View(...)` to create a configurable `DynamicView<TModel>` before a workflow deserves its own view subclass.

```csharp
using ConsoulLibrary;
using System;
using System.Threading.Tasks;

var accent = new Style(ConsoleColor.Cyan);
var muted = new Style(ConsoleColor.DarkGray);

Consoul.View("Deployment Tools")
    .Option("Print status", () => Consoul.MarkupLine("Status: [green]All systems ready[/]."), new Style(ConsoleColor.Green))
    .Option("Run async check", async () =>
    {
        await Task.Delay(250);
        Consoul.Write("Check complete.", ConsoleColor.Cyan);
    }, accent)
    .Navigate<SettingsView>("Edit settings", muted)
    .Render();

public sealed class SettingsView : StaticView
{
    public SettingsView()
    {
        Title = "Settings";
        AddSeparator("General", new Style(ConsoleColor.DarkGray));
        AddOption("Say hello", () => Consoul.Write("Hello"), new Style(ConsoleColor.Cyan))
            .UseShortcut("h");
        AddConfirmOption(
            "Reset demo data",
            "Reset demo data?",
            () => Consoul.Write("Reset"),
            "yellow",
            description: "Asks before changing shared settings.")
            .UseShortcut("reset");
    }
}
```

Consoul is still centered on MVC-style console applications: views own rendering, models hold state, and options act like controller actions. For a model-backed screen, pass a model into `Consoul.View("Settings", model)` or use `Consoul.View<TModel>("Settings")` when a default-constructed model is enough for a quick POC. View options, separators, sections, help, navigation, command palettes, and edit-model entries accept `ConsoleColor` values, reusable `Style` objects, Spectre-style strings such as `"green"`, `"grey"`, `"aqua"`, and `"#22c55e"`, direct `Color` / `RgbColor` values for named static and dynamic menus, or model-backed `Color` / `RgbColor` selectors when a view model owns the palette.

Async options can be quick `Func<Task>` / `Func<TModel, Task>` callbacks for first-run POCs, including state-aware `OptionWhen(...)`, validation-aware `OptionWhenValid(...)`, and guarded `ConfirmOption(...)` / `AddConfirmOption(...)` actions. `DynamicView<TModel>` also exposes `AddOption(...)`, `AddConfirmOption(...)`, `AddNavigationOption(...)`, and `AddEditFormOption(...)` aliases for developers moving between named static screens and model-backed POC screens. Use cancellation-aware `Func<CancellationToken, Task>` / `Func<TModel, CancellationToken, Task>` callbacks when work should observe the active render token.

View error handling can be tuned per screen without changing application-wide defaults. Use `.ThrowOnError()` on a maturing view when controller-style actions should fail fast in tests or development, `.RenderErrorsAndContinue()` when an exploratory POC should keep the current behavior even if `RenderOptions.ViewErrorMode` is stricter, and `.WaitOnRenderError()` when an error should pause before the menu re-renders.

When a model-backed menu is driven by a list of action descriptors, project that collection directly instead of writing adapter loops:

```csharp
var actions = new[]
{
    new { Label = "Preview", Shortcut = "p", Color = ConsoleColor.Cyan, Description = "Runs validation only." },
    new { Label = "Deploy", Shortcut = "ship", Color = ConsoleColor.Green, Description = "Publishes the release." }
};

Consoul.View("Release", release)
    .AddOptions(
        actions,
        action => action.Label,
        (model, action) => model.LastAction = action.Label,
        action => action.Color,
        action => action.Description,
        (option, action) => option.UseShortcut(action.Shortcut).AddSearchTerms(action.Label))
    .Render();
```

When a view menu grows beyond a few options, keep using the same `StaticView` or `DynamicView<TModel>` and turn on prompt ergonomics where they are needed: set `PromptKeyboardNavigationEnabled`, `PromptFilterEnabled`, or `PromptPageSize` on the view. Set `RenderOptions.ViewPromptKeyboardNavigationEnabled`, `RenderOptions.ViewPromptFilterEnabled`, or `RenderOptions.ViewPromptPageSize` to make those defaults apply to newly constructed views across a POC app. In named static screens, use `AddOption(...)` for sync or async controller actions, `AddConfirmOption(...)` for guarded save/delete/deploy actions, and `AddNavigationOption<TView>(...)` when selecting an item should request a typed navigation transition. In model-backed screens, the same add-style names are aliases over `Option(...)`, `ConfirmOption(...)`, `Navigate(...)`, and `EditForm(...)`, so the model remains the view model rather than a separate form object. `DynamicView<TModel>.AddOptions(...)` can project a collection of action descriptors into options and use `isEnabled` / `disabledReason` selectors to keep generated actions visible but unavailable until model state allows them. Use `Navigate<TView>("Details", () => new DetailView(model), replace: false)` or the matching `AddNavigationOption<TView>(...)` factory overload when the destination view needs constructor parameters or injected state.

```csharp
var counter = new CounterModel();

Consoul.View("Deployment", deployment)
    .WithTitle(model => $"Counter: {model.Count}")
    .RenderModel(model =>
    {
        Consoul.Headline("Counter", new HeadlineOptions { Style = HeadlineStyle.Rule });
        Consoul.PropertyList(list =>
        {
            list.Add("Count", model.Count, ConsoleColor.Cyan);
            list.Add("State", model.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
        });
    })
    .Separator("Actions", "grey")
    .Option(model => $"Increment from {model.Count}", model => model.Count++)
    .DescribeLastOption(model => model.Count == 0
        ? "Runs the first increment."
        : "Increments from the current model state.")
    .ShortcutLastOption("i")
    .Separator("Maintenance", "grey")
    .OptionWhen(
        "Reset",
        model => model.Count = 0,
        model => model.Count > 0,
        "counter is already zero",
        "yellow")
    .ShortcutLastOption("r")
    .SearchLastOption("clear", "restart")
    .Render();

public sealed class CounterModel
{
    public int Count { get; set; }
}
```

Use `Separator(...)` when larger MVC menus need grouped action regions without changing selectable option indexes. Use `DescribeLastOption(...)` when a short action label needs context; the description is evaluated each render and participates in prompt filtering. Use `SearchLastOption(...)` for hidden aliases that should match filters or command palettes without changing the rendered controller-style action label. Use `ShortcutLastOption(...)` when frequent actions should be selectable by typed routine input or by key press in keyboard-enabled prompts. Use `OptionWhen(...)` when an action should stay visible but unavailable until model state is ready. Disabled options render with a reason and become selectable on the next render when the predicate returns true. Use `ConfirmOption(...)`, `DynamicView<TModel>.AddConfirmOption(...)`, or `StaticView.AddConfirmOption(...)` for actions that should ask for yes/no confirmation before running, such as saving settings, deleting data, or deploying to a shared environment. `OptionWhen(...)`, `OptionWhenValid(...)`, and `ConfirmOption(...)` all accept quick `Func<Task>` or `Func<TModel, Task>` async handlers when they do not need the cancellation token.

Add `.HelpOption()` to give exploratory users a built-in help screen generated from the current view's labels, descriptions, shortcuts, separators, disabled reasons, and Go Back action. The help surface is also reusable as `view.CreateHelp()` or `ViewHelpEntry`, so it can be captured into panels, layouts, tests, or documentation output. Manual help items and separators accept `ConsoleColor`, reusable `Style` objects, or Spectre-style style strings. Help labels, descriptions, disabled reasons, and separator labels can contain raw markup and still wrap by visible text; shortcuts remain literal so key text such as `[F5]` or `[green]` is never parsed as styling.

```csharp
Consoul.View("Deployment Tools", settings)
    .ShowBreadcrumbs()
    .ShowShortcuts()
    .Section("Summary", model => Consoul.PropertyList(model), new Style(ConsoleColor.Cyan))
    .Option("Preview", model => Preview(model), new Style(ConsoleColor.Cyan))
    .DescribeLastOption("Runs validation without changing the target environment.")
    .ShortcutLastOption("p")
    .OptionWhenValid("Deploy", model => Deploy(model), "fix validation issues first", new Style(ConsoleColor.Green))
    .ShortcutLastOption("d")
    .HelpOption("Help", null, new Style(ConsoleColor.DarkGray))
    .CommandPaletteOption("Command palette", "Command palette", new Style(ConsoleColor.Magenta))
    .Render();
```

`ShowBreadcrumbs()` uses the active `ViewRenderer` stack when a view was reached through `NavigateTo(...)`; direct renders fall back to the current view title. `ShowShortcuts()` renders shortcut hints from `ShortcutLastOption(...)`, attributed shortcuts, help options, and the generated Go Back action.

Use `.CommandPaletteOption()` when a POC view has enough actions that users may prefer to search rather than scan. The palette is generated from the same option labels, descriptions, shortcuts, separators, and disabled-state predicates as the normal view prompt. You can also call `view.ShowCommandPalette()` directly from a custom controller action when you want the palette to be the primary interaction.

`RenderModel(...)` can also return reusable `IConsoulRenderable` components. This keeps quick screens MVC-style without requiring adapter lambdas once a dashboard section deserves a named object:

```csharp
Consoul.View("Counter", counter)
    .RenderModel(model =>
    {
        var summary = new PropertyListEntry(new PropertyListOptions { Width = 40, LabelWidth = 10 });
        summary.Add("Count", model.Count, ConsoleColor.Cyan);
        summary.Add("State", model.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
        return summary;
    })
    .Option("Increment", model => model.Count++)
    .Render();
```

When a screen needs more than one dashboard chunk, append named sections. Sections render after the primary model renderer and before the option prompt, and they are re-evaluated every time an option mutates the model. Use `PropertyListSection(...)` for the common read-only model summary, `TableSection(...)` when the view model owns a collection that should render as a responsive table, `MetricsSection(...)` when the model owns compact KPI or health-state values, `RowsSection(...)`, `ColumnsSection(...)`, and `GridSection(...)` when the model owns composed dashboard rows, side-by-side summaries, or responsive cells, `LayoutSection(...)` when the model owns dashboard panes or nested layout regions, `TextBlockSection(...)` when the model owns wrapped explanatory text, `PanelSection(...)` when the model owns framed detail text, `CalloutSection(...)` when the model owns a status, warning, success, or error message, `ExceptionSection(...)` when the model owns a last error or diagnostic exception, `BreadcrumbSection(...)` when the model owns navigation context, `ShortcutBarSection(...)` when the model owns action hints or shortcut metadata, `BulletListSection(...)` for notes or nested diagnostics, `ChecklistSection(...)` for readiness or workflow checks, `CalendarSection(...)` when the model owns schedule state, `SparklineSection(...)` when the model owns trend samples, `BarChartSection(...)` / `BreakdownChartSection(...)` when dashboard collections should render as responsive charts, `TreeSection(...)` when the model owns nested diagnostic or dependency state, and `TimelineSection(...)` for workflow/history state:

```csharp
Consoul.View("Deployment", deployment)
    .PropertyListSection("Settings", new PropertyListOptions { Width = 40, LabelWidth = 12 })
    .TableSection(
        "Services",
        model => model.Services,
        new TableRenderOptions { MaximumTableWidth = 56 },
        ConsoleColor.Cyan,
        RuleTitleAlignment.Left,
        service => service.Name,
        service => service.State,
        service => service.Replicas)
    .MetricsSection(
        "Health",
        model => model.Health,
        metric => metric.Name,
        metric => metric.Value,
        metric => metric.Unit,
        metric => metric.Note,
        metric => metric.Kind,
        new MetricsOptions { Width = 56 },
        ConsoleColor.Green)
    .RowsSection(
        "Operations",
        model => model.Operations,
        row => row.Label + ": " + row.State,
        row => row.Color,
        new RowsOptions { Width = 56, Spacing = 0 },
        ConsoleColor.Cyan)
    .ColumnsSection(
        "Owners",
        model => model.Owners,
        owner => owner.Name,
        owner => owner.Role,
        owner => owner.Color,
        new ColumnsOptions { Width = 56, Columns = 2 },
        ConsoleColor.DarkGray)
    .GridSection(
        "Release facts",
        model => model.Facts,
        fact => fact.Value,
        fact => fact.Label,
        fact => fact.Color,
        new GridOptions { Width = 56, Columns = 2 },
        ConsoleColor.Cyan)
    .LayoutSection(
        "Dashboard panes",
        model => model.Panes,
        pane => pane.Content,
        pane => pane.Title,
        pane => pane.Color,
        new LayoutOptions { Width = 56, Columns = 2, MinimumPaneWidth = 16 },
        captureWidth: 28,
        titleColor: ConsoleColor.Cyan)
    .BreadcrumbSection(
        "Location",
        model => model.NavigationTrail,
        new BreadcrumbOptions { Width = 56, Separator = " / " },
        ConsoleColor.Cyan)
    .ShortcutBarSection(
        "Keys",
        model => model.Shortcuts,
        shortcut => shortcut.Key,
        shortcut => shortcut.Description,
        shortcut => shortcut.Disabled,
        shortcut => shortcut.KeyColor,
        shortcut => shortcut.DescriptionColor,
        new ShortcutBarOptions { Width = 56 },
        ConsoleColor.DarkGray)
    .CalloutSection(
        "Status",
        model => model.StatusMessage,
        model => model.StatusKind,
        new CalloutOptions { Title = "Deployment", Width = 56 },
        ConsoleColor.Yellow)
    .TextBlockSection(
        "Summary",
        model => model.SummaryText,
        new TextBlockOptions { Width = 56, Alignment = TextBlockAlignment.Left },
        ConsoleColor.Green)
    .PanelSection(
        "Details",
        model => model.DetailText,
        new PanelOptions { Title = "Release Notes", Width = 56 },
        ConsoleColor.Cyan)
    .ExceptionSection(
        "Last error",
        model => model.LastError,
        model => model.ErrorContext,
        new ExceptionRenderOptions { Width = 56, IncludeStackTrace = false },
        ConsoleColor.Red)
    .BulletListSection(
        "Notes",
        model => model.Notes,
        note => note.Text,
        note => note.Children,
        note => note.Color,
        note => note.MarkerColor,
        new BulletListOptions { Width = 56 },
        ConsoleColor.Cyan)
    .ChecklistSection(
        "Readiness",
        model => model.Readiness,
        check => check.Label,
        check => check.State,
        check => check.Description,
        new ChecklistOptions { Width = 56 },
        ConsoleColor.Yellow)
    .CalendarSection(
        "Release window",
        model => model.ReleaseMonth,
        model => model.ReleaseEvents,
        item => item.Date,
        item => item.Label,
        item => item.Color,
        new CalendarOptions { Width = 32 },
        ConsoleColor.Cyan)
    .SparklineSection(
        "Latency",
        model => model.LatencySamples,
        new SparklineOptions { Label = "P95 latency", Width = 56, ShowRange = true },
        ConsoleColor.Blue)
    .BarChartSection(
        "Build health",
        model => model.JobCounts,
        item => item.Label,
        item => item.Count,
        item => item.Color,
        new BarChartOptions { Width = 56, MaximumValue = 10 },
        ConsoleColor.Yellow)
    .TreeSection(
        "Dependencies",
        model => model.DependencyRoot,
        node => node.Name + " - " + node.State,
        node => node.Children,
        node => node.Color,
        new TreeOptions { Width = 56 },
        ConsoleColor.Cyan)
    .TimelineSection(
        "Workflow",
        model => model.Workflow,
        step => step.Title,
        step => step.State,
        step => step.Description,
        step => step.Timestamp,
        new TimelineOptions { Width = 56, TimestampFormat = "HH:mm" },
        ConsoleColor.Magenta)
    .Option("Refresh", model => Refresh(model))
    .Render();

Consoul.View("Counter", counter)
    .Section("Summary", model => Consoul.PropertyList(list =>
    {
        list.Add("Count", model.Count, ConsoleColor.Cyan);
        list.Add("State", model.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
    }))
    .Section(
        model => $"Audit {model.Count}",
        model => Consoul.TextBlock($"Mutation count {model.Count}", width: 48))
    .Option("Increment", model => model.Count++)
    .Render();
```

Use `SectionWhen(...)` when validation, warning, or detail sections should appear only after model state makes them relevant. Use `PropertyListSectionWhen(...)`, `TableSectionWhen(...)`, `MetricsSectionWhen(...)`, `RowsSectionWhen(...)`, `ColumnsSectionWhen(...)`, `GridSectionWhen(...)`, `LayoutSectionWhen(...)`, `TextBlockSectionWhen(...)`, `PanelSectionWhen(...)`, `CalloutSectionWhen(...)`, `ExceptionSectionWhen(...)`, `BreadcrumbSectionWhen(...)`, `ShortcutBarSectionWhen(...)`, `BulletListSectionWhen(...)`, `ChecklistSectionWhen(...)`, `CalendarSectionWhen(...)`, `SparklineSectionWhen(...)`, `BarChartSectionWhen(...)`, `BreakdownChartSectionWhen(...)`, `TreeSectionWhen(...)`, or `TimelineSectionWhen(...)` for generated summaries, collection tables, KPI blocks, composed dashboard regions, pane layouts, wrapped explanatory text, framed details, status messages, bounded diagnostics, navigation trails, action hints, notes, readiness checks, schedules, trend lines, charts, nested diagnostics, and workflow/history views that should appear only after the view model reaches the relevant state:

```csharp
Consoul.View("Counter", counter)
    .Section("Summary", model => Consoul.PropertyList(model))
    .SectionWhen(
        model => $"Audit {model.Count}",
        model => Consoul.Panel($"Changed {model.Count} time(s)", title: "Audit", width: 42),
        model => model.Count > 0,
        model => ConsoleColor.Yellow)
    .Option("Increment", model => model.Count++)
    .Render();
```

For one-off POC sections, use `Consoul.Renderable(...)` to promote a render action into the same reusable component shape:

```csharp
Consoul.View("Counter", counter)
    .RenderModel(model => Consoul.Renderable(() =>
    {
        Consoul.Rule("State", width: 40);
        Consoul.PropertyList(list =>
        {
            list.Add("Count", model.Count, ConsoleColor.Cyan);
            list.Add("Mode", model.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
        }, new PropertyListOptions { Width = 40, LabelWidth = 10 });
    }))
    .Option("Increment", model => model.Count++)
    .Render();
```

## View Helpers

Use alignment, badges, bar charts, breadcrumbs, breakdown charts, bullet lists, calendars, callouts, canvas drawings, checklists, columns, exception panels, figlet text, grids, layouts, headlines, metrics, padding, panels, property lists, rows, rules, shortcut bars, sparklines, text blocks, text paths, timelines, and trees as MVC-style view helpers to make model-backed screens easier to scan.

```csharp
Consoul.Headline("Build", new HeadlineOptions
{
    Style = HeadlineStyle.Block,
    Width = 64,
    BlockCharacter = '#'
});

Consoul.FigletText("POC", width: 64, color: ConsoleColor.Cyan);

Consoul.Breadcrumb("Home", "Operations", "Deployments");

Consoul.ShortcutBar(bar =>
{
    bar.Add("Enter", "Select");
    bar.Add("Esc", "Back");
}, new ShortcutBarOptions { Width = 64 });

Consoul.Calendar(new DateTime(2026, 6, 1), calendar =>
{
    calendar.Add(new DateTime(2026, 6, 18), ConsoleColor.Green, "Release");
    calendar.Add(new DateTime(2026, 6, 25), ConsoleColor.Yellow, "Freeze");
}, new CalendarOptions { Width = 32 });

Consoul.Rule("Current Build", alignment: RuleTitleAlignment.Left);
Consoul.TextBlock(
    "Build and deployment status for the current model.",
    width: 64,
    alignment: TextBlockAlignment.Center,
    color: ConsoleColor.Cyan);

Consoul.TextPath(
    @"C:\builds\consoul\artifacts\release-notes.json",
    width: 64,
    fileNameColor: ConsoleColor.Cyan);

AnsiConsole.Write(new TextPath(@"C:\builds\consoul\artifacts\release-notes.json")
    .Width(64)
    .DirectoryColor(Color.Grey)
    .StemColor(RgbColor.Aqua)
    .ExtensionColor(Color.Green)
    .SeparatorColor(RgbColor.DarkGray));

AnsiConsole.TextPathStyled(
    @"C:\builds\consoul\artifacts\release-notes.json",
    width: 64,
    directoryStyle: "grey",
    fileNameStyle: "aqua",
    separatorStyle: "darkgray");

Consoul.Badge("prod", BadgeKind.Info, writeLine: false);
Consoul.Write(" ", writeLine: false);
Consoul.Badge("ready", BadgeKind.Success);

Consoul.Callout(
    "Deployment is waiting for owner approval.",
    CalloutKind.Warning,
    new CalloutOptions { Width = 64 });

Consoul.Checklist(list =>
{
    list.Add("Build", ChecklistItemState.Complete);
    list.Add("Test", ChecklistItemState.Complete);
    list.Add("Approve", ChecklistItemState.Pending, "Waiting for owner sign-off.");
}, new ChecklistOptions { Width = 64 });

Consoul.Timeline(timeline =>
{
    timeline.Add("Build", TimelineItemState.Complete);
    timeline.Add("Review", TimelineItemState.Active, "Waiting for owner approval.");
    timeline.Add("Publish", TimelineItemState.Pending);
}, new TimelineOptions { Width = 64 });

Consoul.BulletList(list =>
{
    BulletListItem notes = list.Add("Release notes");
    notes.Add("Mention responsive rendering coverage.");
    notes.Add("Call out optional package splits.");
}, new BulletListOptions { Width = 64 });

Consoul.Metrics(metrics =>
{
    metrics.Add("Builds", 42, note: "Today", kind: MetricKind.Success);
    metrics.Add("Failures", 2, note: "Needs triage", kind: MetricKind.Warning);
    metrics.Add("Latency", 128, "ms", "P95", MetricKind.Info);
}, new MetricsOptions { Width = 64 });

Consoul.Sparkline(
    new[] { 12d, 10d, 11d, 8d, 7d, 9d },
    label: "Duration",
    width: 64,
    color: ConsoleColor.Green,
    showRange: true);

Consoul.Canvas(24, 5, canvas =>
{
    canvas.Rectangle(0, 1, 7, 3, '#', Color.Aqua);
    canvas.Text(1, 2, "Build", RgbColor.Aqua);
    canvas.Line(7, 2, 15, 2, '-');
    canvas.Rectangle(15, 1, 8, 3, '#', Color.Green);
    canvas.Text(16, 2, "Ship", RgbColor.Green);
});

Consoul.CanvasImage(new[,]
{
    { ConsoleColor.DarkBlue, ConsoleColor.Blue, ConsoleColor.Cyan },
    { ConsoleColor.DarkGreen, ConsoleColor.Green, ConsoleColor.Yellow }
}, new CanvasImageOptions { PixelWidth = 6, MaxWidth = 12 });

var settings = new Dictionary<string, object>
{
    { "Owner", "Platform" },
    { "Preview", true }
};
var statusRows = new[]
{
    new { Label = "Approval", Value = "Pending", Color = ConsoleColor.Yellow, StyleText = "yellow" },
    new { Label = "Window", Value = "Friday", Color = ConsoleColor.Cyan, StyleText = "white on navy" }
};

Consoul.PropertyList(list =>
{
    list.Add("Environment", "Production", "white on navy", "grey");
    list.Add("Status", "Green", "#22c55e", "yellow");
    list.Add("Retries", 3, ConsoleColor.Yellow);
    list.AddKeyValues(settings, "green", "grey");
    list.AddItems(statusRows, row => row.Label, row => row.Value, row => (ConsoleColor?)row.Color);
    list.AddItemsStyled(statusRows, row => row.Label + " styled", row => row.Value, row => row.StyleText, row => "grey");
});

Consoul.PropertyList(new
{
    Environment = "Production",
    Status = "Green",
    Retries = 3
}, new PropertyListOptions { LabelWidth = 16 });

Consoul.BarChart(chart =>
{
    chart.Add("Passing", 42, ConsoleColor.Green);
    chart.Add("Failing", 3, ConsoleColor.Red);
}, new BarChartOptions
{
    Label = "Build health",
    LabelAlignment = TextBlockAlignment.Center,
    MaximumValue = 50
});

Consoul.BreakdownChart(chart =>
{
    chart.Add("Queued", 12, ConsoleColor.Yellow);
    chart.Add("Running", 5, ConsoleColor.Cyan);
    chart.Add("Failed", 1, ConsoleColor.Red);
}, new BreakdownChartOptions
{
    Label = "Queue mix",
    LabelAlignment = TextBlockAlignment.Center
});

Consoul.Grid(grid =>
{
    grid.Add("Green", "Status", ConsoleColor.Green);
    grid.Add("12 seconds", "Duration", ConsoleColor.Cyan);
    grid.Add("4", "Artifacts", ConsoleColor.Yellow);
}, new GridOptions { Columns = 3 });

Consoul.Layout(layout =>
{
    layout.Add("Build and tests are green.\nArtifacts: 4", "Pipeline", ConsoleColor.Green);
    layout.Add("Deployment is waiting for approval.", "Release", ConsoleColor.Yellow);
}, new LayoutOptions { Columns = 2, MinimumPaneWidth = 18 });

Consoul.Columns(columns =>
{
    columns.Add("Build and tests are green.", "Summary", ConsoleColor.Green);
    columns.Add("Deployment is waiting for approval.", "Next step", ConsoleColor.Yellow);
}, new ColumnsOptions { Columns = 2 });

Consoul.Rows(rows =>
{
    rows.Add(() => Consoul.Rule("Checks", width: 40));
    rows.Add(() => Consoul.PropertyList(list =>
    {
        list.Add("Database", "Ready", ConsoleColor.Green);
        list.Add("Feature flags", "Ready", ConsoleColor.Green);
    }, new PropertyListOptions { Width = 40 }));
}, new RowsOptions { Width = 44, Spacing = 0 });

Consoul.Pad(
    () => Consoul.Panel("Ready for release", title: "Status", width: 36),
    left: 2,
    width: 42);

Consoul.Align(
    () => Consoul.Panel("Centered summary", title: "Overview", width: 36),
    TextBlockAlignment.Center,
    width: 52);

Consoul.Panel(
    () => Consoul.PropertyList(list =>
    {
        list.Add("Owner", "Platform", ConsoleColor.Cyan);
        list.Add("Approval", "Pending", ConsoleColor.Yellow);
    }, new PropertyListOptions { Width = 36 }),
    title: "Details",
    width: 44);

Consoul.Panel(
    "Status: Green\nDuration: 12 seconds\nArtifacts: 4",
    title: "Build Summary",
    borderColor: ConsoleColor.Green);

Consoul.Panel(
    "Build complete\nArtifacts: 4",
    new PanelOptions
    {
        Title = "Release",
        Footer = "Esc to close",
        Width = 48,
        BorderStyle = PanelBorderStyle.Rounded,
        TitleAlignment = TextBlockAlignment.Center,
        FooterAlignment = TextBlockAlignment.Right
    });

Consoul.Callout(
    () => Consoul.PropertyList(list =>
    {
        list.Add("Service", "Billing", ConsoleColor.Cyan);
        list.Add("Status", "Connection failed", ConsoleColor.Red);
    }, new PropertyListOptions { Width = 40 }),
    CalloutKind.Error,
    new CalloutOptions { Title = "Health Check", Width = 48 });
```

For POC diagnostics and failed view actions, render exceptions as a bounded panel:

```csharp
try
{
    RunPreview();
}
catch (Exception exception)
{
    Consoul.WriteException(
        exception,
        "Preview failed",
        new ExceptionRenderOptions
        {
            MaxStackFrames = 4
        });
}
```

Panel titles, context messages, and exception messages can contain raw markup for quick diagnostic emphasis. Generated labels and stack trace frames stay literal, so method names and file paths are not interpreted as styling:

```csharp
Consoul.WriteException(
    new InvalidOperationException("[green]Package rejected[/] for [[prod]]"),
    "[cyan]Deploying[/] [[prod]]",
    new ExceptionRenderOptions
    {
        Title = "[yellow]Deploy failure[/]",
        IncludeStackTrace = false
    });
```

Spectre-style exception snippets can use `ExceptionFormats` and `ExceptionSettings` while still rendering through Consoul's bounded diagnostic panel:

```csharp
catch (Exception exception)
{
    AnsiConsole.WriteException(
        exception,
        ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);

    AnsiConsole.WriteException(exception, new ExceptionSettings
    {
        Format = ExceptionFormats.NoStackTrace,
        Style = new ExceptionStyle
        {
            Exception = new Style(ConsoleColor.Gray),
            Message = new Style(ConsoleColor.White)
        }
    });
}
```

Trees are useful for nested model state, command plans, dependency graphs, and quick diagnostics. Fluent tree builders also support `TreeGuide` presets, `AddNodes(...)`, mixed scalar/existing-node/renderable labels, collapsed branches, renderable node labels, and raw markup labels for migration-friendly snippets.

```csharp
Consoul.Tree("[cyan]Deployment[/]", root =>
{
    TreeNode targets = root.Add("Targets");
    targets.Add("Production");
    targets.Add("Staging");

    TreeNode checks = root.Add("[yellow]Checks[/]");
    checks.Add("Database");
    checks.Add("[green]Feature flags[/]");
});
```

## Terminal Profile

Use `Consoul.Terminal` or the Spectre-style `AnsiConsole.Profile` alias when a POC view needs terminal-aware layout logic without touching `System.Console` directly. The profile includes width, height, redirection flags, virtual-driver status, Spectre-style `Capabilities`, and helpers such as `IsNarrow(...)`, `IsWide(...)`, and `ResolveWidth(...)`.

```csharp
TerminalProfile terminal = AnsiConsole.Profile;
TerminalCapabilities capabilities = terminal.Capabilities;
int width = terminal.ResolveWidth(96, minimumWidth: 40);

Consoul.View("Dashboard", model)
    .RenderModel(state =>
    {
        if (!capabilities.Interactive)
        {
            Consoul.MarkupLine("[grey]Running in redirected output mode.[/]");
        }

        if (terminal.IsNarrow())
        {
            Consoul.Rows(rows =>
            {
                rows.Add($"Service: {state.Service}");
                rows.Add($"Status: {state.Status}");
            }, new RowsOptions { Width = width });
        }
        else
        {
            Consoul.Columns(columns =>
            {
                columns.Add(state.Service, "Service", ConsoleColor.Cyan);
                columns.Add(state.Status, "Status", ConsoleColor.Green);
            }, new ColumnsOptions { Width = width, Columns = 2 });
        }
    })
    .Render();
```

## Capturing Render Output

Use `Consoul.Capture(...)`, `AnsiConsole.Capture(...)`, or injected `console.Capture(...)` when you need plain text output for logs, docs, or tests without writing to the active terminal. Capture uses the same internal console driver path as normal rendering, so banners, panels, tables, live displays, markup, reusable `IConsoulRenderable` widgets, Spectre-style `IRenderable` aliases, and MVC-style views can be checked with a virtual width. Use `MarkupEntry` when markup text should be captured or composed instead of written immediately.

```csharp
string snapshot = Consoul.Capture(() =>
{
    Consoul.MarkupLine("Status: [green]Ready[/]");
    Consoul.Panel("Build complete", title: "Summary", width: 40);
}, width: 80);

var reusablePanel = new PanelEntry(
    "Build complete",
    new PanelOptions { Title = "Summary", Width = 40 });

string panelSnapshot = Consoul.Capture(reusablePanel, width: 80);

string markupSnapshot = Consoul.Capture(new MarkupEntry("Status: [green]Ready[/]"), width: 80);

IConsoulRenderable quickSection = Consoul.Renderable(() =>
{
    Consoul.Rule("State", width: 40);
    Consoul.PropertyList(new { Service = "API", State = "Ready" });
});

IRenderable migratedSection = new Panel("Ready").Header("Migrated");
string migratedSnapshot = Consoul.Capture(migratedSection, width: 80);
IConsoulRenderable composableSection = Consoul.Renderable(migratedSection);
string injectedSnapshot = AnsiConsole.Console.Capture(migratedSection, width: 80);
```

Use `AnsiConsole.Record()` when porting Spectre-style snippets that expect an ongoing transcript while still writing to the active terminal. Recording is driver-backed, so MVC views and render helpers are captured through the same path they normally render through. `ExportText()` returns the plain transcript and `ExportHtml()` returns escaped preformatted HTML for lightweight reports.

```csharp
AnsiConsole.Record();

AnsiConsole.MarkupLine("Status: [green]Ready[/]");
AnsiConsole.Panel("Build complete", new PanelOptions { Title = "Summary", Width = 40 });

string transcript = AnsiConsole.ExportText();
string htmlTranscript = AnsiConsole.ExportHtml();
```

## Composing Render Helpers

`Consoul.Panel(...)`, `Consoul.Callout(...)`, `Consoul.Align(...)`, `Consoul.Pad(...)`, `Padder`, `Align`, `RowsEntry`, `GridEntry`, `ColumnsEntry`, and `LayoutEntry` can capture existing Consoul render helpers or reusable `IConsoulRenderable` objects as framed content, intent-colored callouts, aligned content, padded content, rows, cells, columns, or panes. If migrated helper code returns only the Spectre-style `IRenderable` alias, pass it to `Consoul.Renderable(...)` before composing it. This lets POC dashboards compose banners, markup messages, badges, panels, charts, validation summaries, property lists, tables, and text blocks without manual string capture.
Panel frames can use `PanelBorderStyle.Rounded`, `Square`, `Ascii`, `Heavy`, `Double`, or `None`, and can render aligned title/footer text through `PanelOptions`. For migration-friendly snippets, `new Panel("...")` wraps `PanelEntry` with fluent `Header(...)`, `Header(new PanelHeader(...))`, inline `Header(...)` overloads for `ConsoleColor`, `Style`, style strings, `Color`, and `RgbColor`, `Footer(...)`, `Footer(new PanelHeader(...))`, matching inline `Footer(...)` overloads, `Border(BoxBorder.Rounded)`, `Border(BoxBorder.None)`, `Border(BoxBorder.Safe)`, `Border(BoxBorder.Ascii2)`, `Border(BoxBorder.AsciiDoubleHead)`, `Border(BoxBorder.Markdown)`, `Border(BoxBorder.SimpleHeavy)`, `Border(BoxBorder.Minimal)`, `Border(BoxBorder.HeavyEdge)`, `Border(BoxBorder.HeavyHead)`, `Border(BoxBorder.DoubleEdge)`, `RoundedBorder()`, `AsciiBorder()`, `DoubleBorder()`, `NoBorder()`, `Collapse()`, `Expand()`, `Width(...)`, `Padding(all)`, `Padding(horizontal, vertical)`, four-side `Padding(...)`, alignment, and color helpers such as `BorderColor(...)`, `ContentColor(...)`, `HeaderColor(...)`, and `FooterColor(...)`. Those color helpers accept the same shared palette inputs, so MVC-style view models can keep palette values as `Color` or `RgbColor` without adapter loops. Spectre-style aliases map to the closest existing panel renderer. Set `PanelOptions.Expand = false` or call `Collapse()` when a compact POC widget should size to content instead of filling the terminal. `new Panel(renderable)` and `new Panel(() => { ... })` capture reusable renderables or render actions at render time, so later `Width(...)`, `Collapse()`, and `Padding(...)` calls still shape the captured inner content.

```csharp
Consoul.Panel(
    () => Consoul.PropertyList(list =>
    {
        list.Add("Environment", "Production", ConsoleColor.Cyan);
        list.Add("Owner", "Platform");
    }, new PropertyListOptions { Width = 32 }),
    title: "Snapshot",
    width: 40);

var statusBadge = new BadgeEntry("ready", BadgeKind.Success);
Consoul.Panel(statusBadge, title: "Status", width: 24);

AnsiConsole.Write(new Panel("Ready")
    .Header("Status", Color.Aqua, Justify.Center)
    .Footer("Esc", RgbColor.Yellow, Justify.Right)
    .Border(BoxBorder.Rounded)
    .BorderColor(Color.Green)
    .ContentColor(RgbColor.White)
    .Width(32));

Consoul.Columns(columns =>
{
    columns.Add(() => Consoul.Panel("Build complete", title: "Summary", width: 32), "Status");
    columns.Add(() => Consoul.PropertyList(list =>
    {
        list.Add("Environment", "Production", ConsoleColor.Cyan);
        list.Add("Owner", "Platform");
    }, new PropertyListOptions { Width = 32 }), "Details");
}, new ColumnsOptions { Columns = 2, MinimumColumnWidth = 18 });

Consoul.Rows(rows =>
{
    rows.Add(() => Consoul.Rule("Checks", width: 40));
    rows.Add(() => Consoul.TextBlock("Ready for deployment.", width: 40, color: ConsoleColor.Green));
}, new RowsOptions { Width = 44, Spacing = 0 });

var summary = new PanelEntry(
    "Build complete",
    new PanelOptions { Title = "Summary", Width = 32 });

var checklist = new ChecklistEntry(new ChecklistOptions { Width = 32 });
checklist.Add("Tests", ChecklistItemState.Complete);
checklist.Add("Docs", ChecklistItemState.Pending);

Consoul.Grid(grid =>
{
    grid.Add(summary, "Status");
    grid.Add(checklist, "Readiness");
}, new GridOptions { Width = 72, Columns = 2, MinimumColumnWidth = 24 });

var table = new TableView(new TableRenderOptions { ShowFooterHelp = false });
table.AddHeaders("Job", "State");
table.AddRow(new[] { "Sync worker", "Running" });
table.AddRow(new[] { "Report exporter", "Waiting" });

Consoul.Panel(table, title: "Jobs", width: 64, captureWidth: 56);

Consoul.Pad(
    () => Consoul.Panel("Build complete", title: "Summary", width: 32),
    left: 2,
    right: 1,
    width: 40);

Consoul.Align(
    () => Consoul.Panel("Build complete", title: "Summary", width: 32),
    TextBlockAlignment.Center,
    width: 52);
```

## Form And Object Editing

Use `DynamicView<TModel>.EditForm(...)`, `DynamicView<TModel>.AddEditFormOption(...)`, or `StaticView.AddEditFormOption(...)` when a view action needs a quick editor for a known settings object or exploratory model. All open the shared `EditObjectView` implementation; none creates a separate JSON editor path. `EditModel(...)` and the older JSON terminology remain available for compatibility around the same form document editor. The editor fits Consoul's MVC-style flow: a view action opens the editor, the object remains the model, and the editor returns control to the calling view.

```csharp
var settings = new DeploySettings
{
    Environment = "Development",
    RetryCount = 3,
    EnablePreviewMode = true
};

Consoul.View("Deploy Settings", settings)
    .RenderModel(model =>
    {
        var summary = new PropertyListEntry(new PropertyListOptions { LabelWidth = 18 });
        summary.AddModel(model);
        return summary;
    })
    .ValidationSummarySection(
        new ValidationSummaryOptions
        {
            Width = 56,
            ShowValidMessage = true,
            ValidMessage = "Settings look good."
        }
            .InvalidColor("red")
            .IssueColor("yellow")
            .ValidColor("green")
            .ValidMessageColor("cyan"),
        title: "Validation",
        titleColor: ConsoleColor.Yellow)
    .EditForm("Edit settings object", ConsoleColor.Green)
    .OptionWhenValid("Save", model =>
    {
        Consoul.Write($"Saved settings for {model.Environment}", ConsoleColor.Green);
    }, "fix validation issues first", ConsoleColor.Green)
    .Render();

public sealed class DeploySettings
{
    [Required(ErrorMessage = "Environment is required.")]
    public string Environment { get; set; }

    [Range(0, 10, ErrorMessage = "Retry count must be between 0 and 10.")]
    public int RetryCount { get; set; }

    public bool EnablePreviewMode { get; set; }
}
```

You can also open the editor directly when a full menu is unnecessary:

```csharp
var settings = new DeploySettings();
var editor = Consoul.Form(settings);
editor.Render();
```

Model-backed `DynamicView<TModel>` instances can call `.EditForm(...)` or `.AddEditFormOption(...)` to add the same editor as a menu option; named static screens can call `AddEditFormOption(...)` for a settings object stored in a field or supplied by a service. This keeps quick POC screens in the MVC-style flow: render the model, edit the model, then continue with view actions. `Consoul.EditObject(...)` and `.EditModel(...)` remain available compatibility names over the same form editor stack. Built-in property prompts, nested object editors, and custom `IPropertyEditor` implementations receive the active render token through `PropertyEditContext.CancellationToken`, so hosted POC views can cancel the whole edit flow cleanly.

Use `Consoul.PropertyList(model)` for a read-only model summary before opening the object editor. It honors `[Display]` names and order, `[DisplayName]`, `[Description]`, `[Browsable(false)]`, `[ScaffoldColumn(false)]`, `[Display(AutoGenerateField = false)]`, and `[PropertyEditorIgnore]`, so the summary and editor share the same model metadata. Use `PropertyListEntry.AddKeyValues(...)` for dictionary-backed settings or diagnostic state when creating a temporary model type would add noise, and `AddItems(...)` / `AddItemsStyled(...)` when a POC dashboard already has label/value rows in a view model. `Add(...)`, `AddKeyValues(...)`, `AddItems(...)`, and `AddModel(...)` all accept reusable `Style` objects or style strings for shared label/value palettes, and selector-backed `AddItems(...)` accepts `Color` / `RgbColor` value and label selectors directly. Generated object-editor menus also group properties with `[Display(GroupName = "...")]` or `[Category("...")]`, which keeps larger POC forms readable without custom view code.

Use `DynamicView<TModel>.ValidationSummarySection(...)` when a view should show DataAnnotations-style validation status above its menu on every render; pass `title` or use `ValidationSummarySectionStyled(...)` when the validation block should sit under the same titled section chrome as property lists, tables, and dashboards. Use `ValidationSummarySectionWhen(...)` for post-action checks, advanced validation details, or other validation blocks that should appear only after model state makes them relevant. Pair validation sections with `OptionWhenValid(...)` for save, publish, or deploy actions that should remain visible but disabled until the model is valid. Validation includes property attributes and object-level `IValidatableObject` results, so cross-field checks can guard controller-style actions too. `ValidationSummaryOptions.InvalidColor(...)`, `IssueColor(...)`, `ValidColor(...)`, and `ValidMessageColor(...)` accept `ConsoleColor`, reusable `Style` objects, Spectre-style strings, `Color`, and `RgbColor` values, so a model-backed POC view can share one palette between validation, prompts, and status widgets. Use `Consoul.ValidationSummary(model)` when an ad hoc action needs to render and inspect `ValidationSummaryResult` directly, or `new ValidationSummaryEntry(model, options).InvalidColor(Color.Red).IssueColor(RgbColor.Yellow)` when the same validation block should be captured, placed in a panel/layout, or returned from `DynamicView<TModel>.RenderModel(...)`.

When a simple property is edited, the default editor uses the same typed prompt stack as direct prompts. Text and numeric properties use `TextPrompt<T>`, boolean properties use `ConfirmPrompt`, and enum properties use metadata-backed selection prompts. Nullable booleans and enums add a clear-value choice for optional settings unless `[Required]` is present. The current property value is offered as the Enter default, `back`/`exit` cancels without mutating the model, and DataAnnotations-style validation is enforced during the edit, including common attributes such as `[Required]`, `[Range]`, `[StringLength]`, `[MinLength]`, `[MaxLength]`, plus context-aware `ValidationAttribute` types such as `[RegularExpression]`, `[EmailAddress]`, `[Compare]`, and custom validators.

The generated object-editor menu also renders property descriptions from `[Display]`, `[Description]`, and XML summaries, and filtering is enabled by default so users can search by label or help text while exploring a model.
Properties marked `[ReadOnly(true)]` or `[Editable(false)]` are omitted from the edit menu while remaining available to read-only summaries.

## Prompts

`SelectionPrompt` renders a numbered menu and returns a `PromptResult`, so callers can distinguish selection, cancellation, and default selection behavior. Use `AddSeparator(...)`, `SelectOption.Separator(...)`, or the Spectre-shaped `AddChoiceGroup(...)` alias for display-only group headings that do not consume selectable indexes. Use `AddGroup(...)` and `AddChild(...)` for tree-shaped choices; returned `SelectOption` objects support `UseColor(...)`, `UseStyle(...)`, `UseDisabledColor(...)`, and `UseDisabledStyle(...)` for per-choice palette control. Prompt-level color helpers and option add/style helpers accept `ConsoleColor`, `Style`, style strings, `Color`, and `RgbColor`, so MVC-style view models can keep prompt palette choices next to the state they describe. `SelectionMode.Leaf` keeps group rows visible while requiring a child selection, and `SelectionMode.Independent` lets the group itself be returned.

```csharp
var prompt = new SelectionPrompt("Pick an environment");
prompt.AddSeparator("Internal");
prompt.Add("Development", ConsoleColor.Green, isDefault: true);
prompt.Add("Staging", ConsoleColor.Yellow);
prompt.AddSeparator("Protected");
prompt.AddDisabled("Production", "approval required");

PromptResult result = prompt.Render();

if (result.IsCanceled)
{
    Consoul.Write("Canceled", ConsoleColor.DarkYellow);
}
else if (result.HasSelection)
{
    Consoul.Write($"You chose {prompt[result.Index]}");
}
```

For longer menus, set `PageSize`. Users can type `next`/`n` and `prev`/`p` while option numbers remain stable across all pages. Set `FilterEnabled = true` when users should be able to type `/text` to narrow a long menu and `/clear` to restore it; use `FilterCommandPrefix(...)`, `FilterHelpText(...)`, `ActiveFilterText(...)`, and `NoFilterMatchesText(...)` when a POC command palette needs custom copy. Set `KeyboardNavigationEnabled = true` when the same menu should support arrows, typed filtering, <kbd>Enter</kbd>, and <kbd>Esc</kbd>. Set `WrapAround = false`, call `UseWrapAround(false)`, or use the Spectre-shaped `WrapAround(false)` extension when arrow keys should clamp at the ends; use `WithSearchPlaceholderText(...)` or `SearchPlaceholderText(...)` for custom keyboard-search copy, `KeyboardNavigationHelpText(...)` / `KeyboardFilterHelpText(...)` for keyboard instructions, and `WithInvalidSelectionMessage(...)`, `InvalidSelectionMessage(...)`, or `InvalidChoiceMessage(...)` for friendlier malformed-input feedback.

```csharp
var pagedPrompt = new SelectionPrompt("Pick deployment target")
{
    PageSize = 5,
    FilterEnabled = true,
    KeyboardNavigationEnabled = true
};
pagedPrompt.AddSeparator("Development");
pagedPrompt.Add("Local");
pagedPrompt.Add("Shared dev");
pagedPrompt.AddSeparator("Release");
pagedPrompt.Add("Staging", ConsoleColor.Yellow);
pagedPrompt.AddDisabled("Production", "approval required");
int productionIndex = pagedPrompt.Add("Deploy production", ConsoleColor.Green);
pagedPrompt.AddSearchTerms(productionIndex, "prod", "ship");

// Users can type "/prod" and then choose the original option number.
PromptResult targetPage = pagedPrompt.Render();
```

For simple menus, `Consoul.Select(...)` returns the same result without manually creating a prompt:

```csharp
PromptResult environment = Consoul.Select("Pick an environment", false, "Development", "Staging", "Production");

if (environment.HasSelection)
{
    Consoul.Write($"Selected option {environment.Index + 1}");
}
```

Use the collection overload when a simple menu needs an Enter-to-accept default:

```csharp
PromptResult target = Consoul.Select(
    "Pick an environment",
    new[] { "Development", "Staging", "Production" },
    defaultOption: "Staging");
```

Typed selection returns both the prompt result and the selected object:

```csharp
var targets = new[]
{
    new DeployTarget("Development", "dev"),
    new DeployTarget("Production", "prod")
};

SelectionResult<DeployTarget> target = Consoul.Select(
    "Pick a target",
    item => item.Name,
    false,
    targets);

if (target.HasSelection)
{
    Consoul.Write($"Deploying to {target.SelectedItem.Slug}");
}

public sealed class DeployTarget
{
    public DeployTarget(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public string Name { get; }
    public string Slug { get; }
}
```

Typed collection overloads also accept a default item or predicate:

```csharp
SelectionResult<DeployTarget> target = Consoul.Select(
    "Pick a target",
    targets,
    item => item.Name,
    targets[0]);
```

When a typed prompt needs custom options such as separators, disabled choices, filtering, or keyboard navigation, construct `SelectionPrompt<T>` and call `RenderSelection(...)`:

```csharp
var targetPrompt = new SelectionPrompt<DeployTarget>("Pick a target", labelSelector: item => item.Name)
{
    FilterEnabled = true,
    KeyboardNavigationEnabled = true
}.WithDescriptions(item => $"Deploy slug: {item.Slug}");
targetPrompt.AddSeparator("Internal");
targetPrompt.Add(targets[0], ConsoleColor.Green);
targetPrompt.AddDisabled(targets[1], "approval required");

SelectionResult<DeployTarget> configuredTarget = targetPrompt.RenderSelection();
```

Typed prompt metadata is order-insensitive: `UseConverter(...)`, `WithDescriptions(...)`, and `WithSearchTerms(...)` can be called before or after adding typed choices or groups, which keeps MVC-style controller/view code flexible when choices are projected from a model first.

Prompt titles and messages for text, secret, confirm, confirmation, selection, and multi-selection prompts can contain raw markup such as `[green]Deploy[/] [[prod]]`. Developer-authored validation, required, invalid-choice, invalid-selection, filter-help, search-placeholder, paging-helper, and disabled-reason text can use the same markup. Rendering preserves parsed colors and escaped brackets, while selection filtering searches the visible text instead of matching markup tags. Formatted filter/page values are escaped inside markup templates, and generated hints, defaults, yes/no choices, generated unavailable prefixes, and generated accepted-value messages remain literal so model values do not become accidental markup. Multi-selection prompts still skip disabled options for `all`, but explicit disabled indexes report the disabled reason.

For enum-backed menus, `Consoul.SelectEnum<TEnum>(...)` renders every enum value, honors common display metadata, and returns the selected enum directly:

```csharp
SelectionResult<DeployEnvironment> environment =
    Consoul.SelectEnum<DeployEnvironment>(
        "Pick environment",
        defaultValue: DeployEnvironment.Staging);

if (environment.HasSelection)
{
    Consoul.Write($"Deploying to {environment.SelectedItem}");
}
```

Use `Consoul.MultiSelectEnum<TEnum>(...)` for enum-backed checklists such as deployment targets, feature switches, or diagnostic categories:

```csharp
MultiSelectionResult<DeployEnvironment> targets =
    Consoul.MultiSelectEnum(
        "Pick deployment targets",
        new[] { DeployEnvironment.Development, DeployEnvironment.Staging, DeployEnvironment.Production },
        defaultValues: new[] { DeployEnvironment.Staging });

if (targets.HasSelection)
{
    Consoul.Write($"Deploying to {targets.SelectedItems.Count} targets");
}
```

For checkbox-style choices, `Consoul.MultiSelect(...)` accepts comma-separated indexes, ranges, `all`, and cancellation phrases:

```csharp
MultiSelectionResult steps = Consoul.MultiSelect(
    "Pick deployment steps",
    false,
    "Build",
    "Test",
    "Deploy");

if (steps.HasSelection)
{
    Consoul.MarkupLineInterpolated($"Selected [cyan]{steps.Indexes.Count}[/] steps.");
}
```

For an interactive POC checklist, construct `MultiSelectionPrompt` directly and turn on keyboard navigation:

```csharp
var checklist = new MultiSelectionPrompt("Pick deployment steps")
{
    KeyboardNavigationEnabled = true,
    FilterEnabled = true
};
checklist.Add("Build");
checklist.Add("Test");
checklist.Add("Deploy");

MultiSelectionResult checkedSteps = checklist.Render();
```

Collection overloads can mark default selections for Enter-only input. Direct prompt builders can also use `Select(...)` or `SelectAll()` before or after adding choices when adapting Spectre-style checklist snippets:

```csharp
MultiSelectionResult defaultSteps = Consoul.MultiSelect(
    "Pick deployment steps",
    new[] { "Build", "Test", "Deploy" },
    defaultOptions: new[] { "Build", "Deploy" });

MultiSelectionResult directDefaults = new MultiSelectionPrompt("Pick deployment steps")
    .Select("Build", "Deploy")
    .AddChoices("Build", "Test", "Deploy")
    .Render();
```

Typed multi-selection returns the selected objects directly:

```csharp
MultiSelectionResult<DeployTarget> targets = Consoul.MultiSelect(
    "Pick targets",
    item => item.Name,
    false,
    new DeployTarget("Development", "dev"),
    new DeployTarget("Production", "prod"));
```

Configurable typed checklists can use `MultiSelectionPrompt<T>.RenderSelection(...)`:

```csharp
var developmentTarget = new DeployTarget("Development", "dev");
var stagingTarget = new DeployTarget("Staging", "stage");
var productionTarget = new DeployTarget("Production", "prod");
var targetChecklist = new MultiSelectionPrompt<DeployTarget>("Pick targets", labelSelector: item => item.Name)
{
    FilterEnabled = true,
    KeyboardNavigationEnabled = true
}.WithDescriptions(item => $"Deploy slug: {item.Slug}")
 .Select(stagingTarget);
targetChecklist.Add(developmentTarget, isDefault: true);
targetChecklist.Add(stagingTarget);
targetChecklist.AddDisabled(productionTarget, "approval required");

MultiSelectionResult<DeployTarget> checkedTargets = targetChecklist.RenderSelection();
```

Checklist metadata uses the same order-insensitive typed selectors, so a POC view can add items from the model and attach display labels for choices and group headings, descriptions, or hidden search terms afterward without rebuilding the prompt.

For typed text input with validation, create a `TextPrompt<T>` through `Consoul.Text<T>(...)`:

```csharp
TextPromptResult<int> retries = Consoul.Text<int>("Retry count")
    .WithDefault(3)
    .Range(0, 10)
    .Render();

if (retries.HasValue)
{
    Consoul.MarkupLineInterpolated($"Using [cyan]{retries.Value}[/] retries.");
}
```

Common POC form checks are available as fluent helpers. String prompts support length checks, while typed numeric prompts use comparable range checks:

```csharp
TextPromptResult<string> serviceKey = Consoul.Text<string>("Service key")
    .TrimInput()
    .Required()
    .LengthBetween(3, 24)
    .Render();

TextPromptResult<int> workerCount = Consoul.Text<int>("Worker count")
    .Minimum(1)
    .Maximum(8)
    .Render();
```

Built-in parsing covers strings, numbers, booleans with yes/no shorthand, enums, `Guid`, `TimeSpan`, `DateTime`, `DateTimeOffset`, `Uri`, and `Version`:

```csharp
TextPromptResult<Guid> operationId = Consoul.Text<Guid>("Operation id").Render();

TextPromptResult<TimeSpan> timeout = Consoul.Text<TimeSpan>("Timeout")
    .WithDefault(TimeSpan.FromSeconds(30))
    .Render();

TextPromptResult<Uri> endpoint = Consoul.Text<Uri>("API endpoint").Render();
```

For known values, keep the prompt declarative. Accepted values are shown in the prompt by default:

```csharp
TextPromptResult<string> environment = Consoul.Text<string>("Environment")
    .AcceptOnly(new[] { "dev", "staging", "prod" }, comparer: StringComparer.OrdinalIgnoreCase)
    .Render();
```

Use the Spectre-shaped `Choices(...)` alias or direct `AddChoice(...)` / `AddChoices(...)` methods when adapting pasted snippets. Set `ShowChoices = false` or call `HideChoices()` for quieter prompts, use `HideDefaultValue()` when Enter should still accept a default without rendering it, call `EditableDefaultValue()` when keyboard users should start from an editable default buffer, and call `ClearOnFinish()` when a transient POC form should not leave the prompt text on screen after a successful entry. Call `Secret(...)` or set `IsSecret` when a typed prompt should mask input but still parse through `TextPrompt<T>`. Change `ChoicesSeparator(...)`, `ChoicesStyle(...)`, `DefaultValueStyle(...)`, or `MaskStyle(...)` when a view needs different hint styling; prompt style helpers accept reusable `Style` objects, color names, `Color`, and `RgbColor`. Pass a `StringComparer` to the `TextPrompt<T>` constructor when accepted choice text should be case-sensitive or use a custom comparer. For object choices, `UseConverter(...)` controls both rendered labels and display-text choice lookup, while `InvalidChoiceMessage(...)` customizes copy for unmatched display text. Set `Culture` or call `Culture(...)` when decimal, date, or duration input should parse and render defaults with a specific locale.

For POC commands that need files or folders, use the path prompt factories. They return the same result-first `TextPromptResult<string>` shape, trim pasted quotes, and can require the target to exist:

```csharp
TextPromptResult<string> script = Consoul.FilePath("Deployment script", mustExist: true)
    .Render();

TextPromptResult<string> output = Consoul.DirectoryPath("Output directory")
    .Render();
```

For masked secret input, use `Consoul.Secret(...)` or the immediate `Consoul.ReadSecret(...)` helper:

```csharp
SecretResult apiKey = Consoul.Secret("API key")
    .MinimumLength(16, "API key must contain at least 16 characters.")
    .Render();

if (apiKey.HasValue)
{
    ConfigureClient(apiKey.Value);
}
```

For yes/no decisions, `Consoul.Confirm(...)` returns a `ConfirmResult` so cancellation stays distinct from a negative answer:

```csharp
ConfirmResult deploy = Consoul.Confirm("Deploy to production?", defaultValue: false);

if (deploy.IsCanceled)
{
    Consoul.Write("Deployment canceled.", ConsoleColor.DarkYellow);
}
else if (deploy.Value)
{
    DeployProduction();
}
```

When adapting Spectre-shaped confirmation snippets, `ConfirmationPrompt` keeps property-style defaults and custom yes/no characters while still returning Consoul's explicit result:

```csharp
var deployPrompt = new ConfirmationPrompt("Deploy to production?")
{
    DefaultValue = false
}
.Yes('d')
.No('s')
.InvalidChoiceMessage("Choose d to deploy or s to skip.");

ConfirmResult deployDecision = AnsiConsole.Prompt(deployPrompt);
```

## Tables

`Consoul.Table(...)` renders model collections inline for quick POC views, while `TableView` remains available for full manual control. Both `TableView` and `DynamicTableView<T>` implement `IConsoulRenderable`, so they can be captured with `Consoul.Capture(table, ...)` or composed directly inside panels, rows, grids, columns, and layouts. When headers are inferred from model properties, Consoul honors `[TableColumn]`, `[Display(Name = "...")]`, `[DisplayName]`, and `[Description]` before falling back to readable PascalCase names.

```csharp
var jobs = new[]
{
    new JobStatus(1, "Sync worker", "Running"),
    new JobStatus(2, "Report exporter", "Waiting")
};

Consoul.Table(jobs, job => job.Id, job => job.Name, job => job.Status);

var services = new[]
{
    new { Name = "API", State = Consoul.BadgeText("ready", BadgeKind.Success, maxWidth: 12) },
    new { Name = "Billing", State = Consoul.BadgeText("blocked", BadgeKind.Error, maxWidth: 12) }
};

Consoul.Table(services, service => service.Name, service => service.State);
```

For MVC-style POC screens, put the table shape on the model and omit selectors:

```csharp
public sealed class JobStatus
{
    [TableColumn("Job", Order = 1)]
    public string Name { get; set; }

    [TableColumn("State", Order = 2)]
    public string State { get; set; }

    [TableColumn("Retries", Order = 3, Alignment = TableCellAlignment.Right, Format = "000")]
    public int RetryCount { get; set; }

    [TableColumn(Visible = false)]
    public string InternalId { get; set; }
}

Consoul.Table(jobs);
```

`DynamicTableView<T>` supports custom labels and computed columns:

```csharp
var table = new DynamicTableView<JobStatus>(new TableRenderOptions
{
    ShowFooterHelp = false,
    Title = "Active Jobs",
    Caption = "Filtered by the current view state."
})
{
    ShowSummary = true,
    ShowEmptyMessage = true,
    EmptyMessage = "No jobs match the current table view."
};
table.AddHeader(job => job.Name, "Job");
table.AddColumn("State", job => job.Status.ToUpperInvariant());
table.AddColumn("Retries", job => job.RetryCount, "000", TableCellAlignment.Right);
table.AddRows(jobs);
table.Filter(job => job.Status != "Done")
    .Search("sync")
    .SortBy(job => job.Name)
    .Page(1, 10);
table.RenderInline();
```

Use `VisibleContents`, `TotalRowCount`, `MatchedRowCount`, `VisibleRowCount`, and `ActiveSearchQuery` when a view needs to summarize the current filtered, searched, or paged table state. Set `SummaryFormatter` for custom summary text. Interactive selections over a transformed `DynamicTableView<T>` return selected visible model rows through `TableSelectionResult<T>`:

```csharp
TableSelectionResult<JobStatus> selectedJobs = table.Select("Open job");
if (selectedJobs.HasSelection)
{
    Consoul.Write($"Opening {selectedJobs.SelectedItem.Name}", ConsoleColor.Cyan);
}
```

`TableView.Select(...)` and `DynamicTableView<T>.Select(...)` distinguish cancellation, accepted empty selections, and one or more selected rows. Users can type a row number, move with arrows, press <kbd>Space</kbd> to toggle multiple rows, and press <kbd>Enter</kbd> to accept. Set `TableRenderOptions.ShowRowNumbers = true` when row-number input should be visible in the table itself. The older `Prompt(...)` methods remain available when a nullable single-row result is enough.

Set `TableRenderOptions.Title` and `Caption` when a table should keep its own heading in panels, grids, captured diagnostics, or model-rendered views. `TitleAlignment`, `CaptionAlignment`, `TitleScheme`, and `CaptionScheme` keep those labels bounded to the table width. The older `table.Render("Jobs", ConsoleColor.Cyan)` overload still works as a per-render title and takes precedence over the configured title.

For Spectre-style manual snippets, use `Table`. It wraps `TableView` with fluent `Title(...)`, `Title(new TableTitle(...))`, `Caption(...)`, `Caption(new TableTitle(...))`, `Caption(new TableCaption(...))`, inline title/caption style overloads, `Width(...)`, `Expand()`, `Collapse()`, named border helpers such as `Border(TableBorder.Rounded)`, `Border(TableBorder.None)`, `Border(TableBorder.Minimal)`, `Border(TableBorder.HeavyHead)`, `RoundedBorder()`, and `DoubleBorder()`, table-wide alignment/color helpers, row separators, row numbers, `AddColumns(...)`, configurable columns through `AddColumn("Name", column => column.RightAligned())`, synchronized post-add metadata through `table.Columns[index]`, object-friendly `AddRow(...)`, model-backed `AddRows(items, item => item.Name, ...)`, key/value diagnostic helpers through `AddKeyValueRows(...)`, and `TableColumn` padding helpers such as `Padding(...)`, `PadLeft(...)`, and `PadRight(...)`. Horizontal padding adjusts cell content; top and bottom padding render bounded blank table rows around content while preserving column alignment. `Table` and `TableColumn` both accept `LeftJustified()` and `RightJustified()` aliases alongside the aligned naming used by table examples. Use `Collapse()` for content-sized diagnostic tables and `Expand()` or `Width(...)` when an MVC view should intentionally fill its terminal layout. Common Spectre-style border names such as `Ascii2`, `AsciiDoubleHead`, `Markdown`, `SimpleHeavy`, `MinimalHeavyHead`, `MinimalDoubleHead`, `Horizontal`, `HeavyEdge`, `DoubleEdge`, and `Safe` map to the closest existing Consoul renderer. Add `using ConsoulLibrary.Color;` when you want aliases such as `Color.Grey`, `Color.Aqua`, `Color.FromHex(...)`, `Colors.Grey`, or `RgbColor.Red`.

```csharp
var manual = new Table()
    .Title("Jobs")
    .Width(48)
    .Border(TableBorder.Rounded)
    .BorderColor(Colors.Grey)
    .HeaderColor(ConsoleColor.Yellow)
    .FooterColor(ConsoleColor.Gray)
    .ShowRowNumbers()
    .RowNumberHeader("No")
    .AddColumn(new TableColumn("Job").Width(14).Padding(2, 1))
    .AddColumn("State", column => column.Centered())
    .AddColumn(new TableColumn("Retries").RightJustified().Width(8).PadRight(2).Footer("2"))
    .AddRow("Build", "Ready", 0)
    .AddRow("Deploy", "Waiting", 2);

manual.Columns[2].Footer = new Text("2", new Style(foreground: Colors.Yellow, decoration: Decoration.Bold));

AnsiConsole.Write(manual);
```

Raw Spectre-style markup strings in table headers and cells are treated as display markup, so snippets like `AddRow("[green]API[/]", "[white on navy]Ready[/]")` keep their responsive widths, preserve foreground/background color metadata, and render visible text instead of literal tags. Use reusable `Markup`, `Text`, or other renderable values when you want cell content that can be composed outside the table too.

Manual tables can still project model rows when a view needs a custom shell but the data should stay in a view model:

```csharp
manual.AddRows(
    jobs,
    job => new Text(job.Name, new Style(foreground: job.IsBlocked ? Colors.Yellow : Colors.Green)),
    job => new Markup(job.Status == "Ready" ? "[green]Ready[/]" : "[yellow]Waiting[/]"),
    job => job.RetryCount);
```

For quick MVC-style diagnostic views, `AddKeyValueRows(...)` turns dictionaries or other key/value sequences into a compact two-column table. If the table has no columns, Consoul adds `Key` and `Value`; if your view already configured column names, those names are preserved:

```csharp
var diagnostics = new Table()
    .Title("Runtime")
    .Width(48)
    .HideFooter()
    .AddKeyValueRows(new Dictionary<string, object>
    {
        { "Environment", "Development" },
        { "Retries", 2 },
        { "State", new Markup("[green]Ready[/]") }
    });

AnsiConsole.Write(diagnostics);
```

Renderable cells are captured into bounded table text, with `Markup` cells preserving their inline styling, so quick snippets can mix markup, panels, nested tables, and styled text without leaving the same table engine:

```csharp
manual.AddRow(new Text("Markup"), new Markup("[green]Ready[/]"), "-");
manual.AddRow(
    new Text("Details"),
    new Panel("[yellow]Review[/]").Border(BoxBorder.Ascii).Width(18),
    new Text("Styled", new Style(foreground: Colors.Aqua, decoration: Decoration.Italic)));
```

Manual `Table` rows are mutable for live dashboards and migration-friendly snippets:

```csharp
manual.InsertRow(1, "Deploy", "Waiting", 2);
manual.UpdateCell(0, 1, new Markup("[green]Ready[/]"));
manual.RemoveRow(2);
manual.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
manual.Rows.Insert(1, "Smoke", "Passing", 0);
manual.Rows.RemoveAt(2);
manual.Rows.Clear();
manual.Rows.Add("Worker", "Online", 0);
```

For full manual control and interactive table prompts, use `TableView`.

```csharp
var table = new TableView();
table.AddHeader("Id", TableCellAlignment.Right);
table.AddHeader("Name");
table.AddHeader("Status");
table.AddRow(new[] { "1", "Sync worker", "Running" });
table.AddRow(new[] { "2", "Report exporter", "Waiting" });

table.Render("Jobs", ConsoleColor.Cyan);
```

Use `TableRenderOptions.HeaderAlignment` and `ContentAlignment` for table-wide defaults, or per-column alignment through `TableView.SetColumnAlignment(...)`, `TableView.AddHeader(..., TableCellAlignment.Right)`, `DynamicTableView<T>.AddColumn(..., TableCellAlignment.Right)`, and `[TableColumn(Alignment = ...)]`. Manual `TableColumn` instances also accept `Padding(...)`, `PadLeft(...)`, and `PadRight(...)` when a diagnostic table needs extra breathing room without changing the model projection. Use `[TableColumn(Format = "yyyy-MM-dd")]`, `[DisplayFormat(DataFormatString = "{0} ms")]`, or `DynamicTableView<T>.AddColumn("Latency", valueSelector, "{0} ms")` when POC model values need scan-friendly display formatting.

For customized colors and borders, configure `TableRenderOptions`:

```csharp
var options = new TableRenderOptions
{
    TableWidthPercentage = 0.9m,
    Title = "Packages",
    Caption = "Optional packages are installed only when needed.",
    TitleAlignment = TextBlockAlignment.Center,
    HeaderScheme = new ColorScheme
    {
        Color = ConsoleColor.White,
        BackgroundColor = ConsoleColor.DarkBlue
    }
};
options.Lines.BorderStyle = TableBorderStyle.Rounded;

var table = new TableView(options);
table.AddHeaders("Package", "Purpose");
table.AddRow(new[] { "Consoul", "Core console UI" });
table.Render();
```

Set `ShowHeaders = false` when a model-backed table should render as a compact dashboard widget while still using headers for metadata and sizing:

```csharp
var compactOptions = new TableRenderOptions
{
    ShowFooterHelp = false,
    ShowHeaders = false,
    Title = "Current Target"
};
compactOptions.Lines.BorderStyle = TableBorderStyle.Rounded;

Consoul.Table(jobs, compactOptions, job => job.Name, job => job.Status);
```

For selection-heavy tables, show one-based row numbers without changing the underlying model headers:

```csharp
var selectionOptions = new TableRenderOptions
{
    ShowRowNumbers = true
};

DynamicTableView<JobStatus> selectable = Consoul.Table(
    jobs,
    selectionOptions,
    job => job.Name,
    job => job.Status);

TableSelectionResult<JobStatus> job = selectable.Select("Open job");
```

## Progress Bars

`SpinnerScope` is a lightweight in-place busy indicator for short operations without known progress. `StatusScope` renders short in-place status messages, `ProgressContext` tracks task rows, `LiveDisplayContext` redraws dashboards, and `ProgressBar` keeps a fixed render position for longer work.

```csharp
Consoul.Spinner("Loading package metadata", spinner =>
{
    spinner.Tick("Reading manifest");
    spinner.Tick("Resolving dependencies");
    spinner.Tick("Ready");
});

Consoul.Status("Running checks", status =>
{
    status.Update("Validating inputs", Color.Aqua);
    status.Status("Writing report", RgbColor.Green);
    status.UpdateStyled("Publishing report", new Style(ConsoleColor.Cyan));
});
```

The `AnsiConsole` facade also supports familiar `Start(...)` wrappers for quick ports and proof-of-concept workflows while still rendering through Consoul's MVC-style components:

```csharp
int checkedFiles = AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .SpinnerStyle(RgbColor.Yellow)
    .Start("Running checks", status =>
    {
        status.Status("Validating inputs", Color.Aqua);
        status.Status("Writing report", RgbColor.Green);
        return 42;
    });

AnsiConsole.Status()
    .RefreshRate(TimeSpan.FromMilliseconds(100))
    .AutoRefresh(false)
    .Start("Manual refresh", status =>
    {
        status.Status("Checking cache");
        status.Refresh();
    });

await AnsiConsole.Status()
    .StartContextAsync("Downloading", async status =>
    {
        status.Spinner = Spinner.Known.Dots;
        status.SpinnerStyle(Color.Aqua).Status("Resolving packages");
        await Task.Delay(1);
        status.Succeed("Packages ready");
    });

await AnsiConsole.ProgressAsync(async progress =>
{
    ProgressTask schema = progress.AddTask("Schema", 2);
    await Task.Delay(1);
    schema.Complete();
});

int processed = await AnsiConsole.ProgressAsync(async progress =>
{
    ProgressTask publish = progress.AddTask("Publish", 3);
    await Task.Delay(1);
    publish.Complete();
    return progress.CompletedTaskCount;
});

AnsiConsole.Progress().Start(progress =>
{
    ProgressTask build = progress.AddTask("Build");
    build.MaxValue = 4;
    build.Value = 2;
    build.Complete("Build done");
});
```

`Spinner.Known` includes common migration aliases such as `Pipe`, `Line2`, `SimpleDots`, `SimpleDotsScrolling`, `Dots2` through `Dots12`, `Star2`, `Flip`, `Hamburger`, `GrowVertical`, `GrowHorizontal`, `Balloon`, `Noise`, `Bounce`, `BoxBounce`, `BouncingBar`, `BouncingBall`, `Arrow`, `Triangle`, `Circle`, `Toggle`, `Clock`, `Moon`, `Pong`, and `BetaWave` so quick status POCs can reuse familiar spinner names while staying dependency-free.

Spinner, status, and single progress-bar messages can contain raw Spectre-style markup. Frames, active prefixes, success/failure prefixes, and bar glyphs remain literal, while the message body preserves parsed colors and escaped bracket text:

```csharp
Consoul.Status("[yellow]Preparing[/] [[prod]]", status =>
{
    status.Update("[cyan]Checking schema[/] [[prod]]");
});

Consoul.Spinner("[green]Loading[/] packages", spinner =>
{
    spinner.Tick("[cyan]Resolving[/] dependencies");
});

using var bar = new ProgressBar("[green]Starting[/] [[prod]]");
bar.Update(0.5, "[cyan]Halfway[/] [[prod]]", Color.Aqua, RgbColor.Green);
```

Static `AnsiConsole` and injected `IAnsiConsole` async helpers accept quick `Func<..., Task>` delegates for first-run POCs and token-aware `(context, cancellationToken) => ...` delegates when work should observe the active cancellation token. `StatusContext` supports property-style `Status`, `Spinner`, and `SpinnerStyle` updates plus direct `Succeed(...)` / `Fail(...)` completion helpers for copied Spectre-style status snippets. `StatusScope.Update(...)`, `StatusScope.Status(...)`, `StatusContext.Status(...)`, and `ProgressBar.Update(...)` accept `ConsoleColor`, `Color`, and `RgbColor` values for direct palette changes, while `UpdateStyled(...)` remains available for reusable foreground/background styles. `RefreshRate(...)` helpers on status builders/scopes and spinner builders/scopes accept milliseconds or `TimeSpan` values, and `SpinnerStyle(...)` helpers on status and spinner builders/scopes accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values.

Use `ProgressContext` when a workflow has several task rows:

```csharp
Consoul.Progress(progress =>
{
    ProgressTask build = progress.AddTask("Build", 4);
    ProgressTask test = progress.AddTask("Test", 3);

    build.Increment(2);
    test.Increment(1);
    build.Complete("Build done");
    test.Complete("Tests done");
});
```

For migration-friendly progress snippets, `ProgressContext.AddTask(...)` keeps Consoul's existing `AddTask("Build", 4)` shape and also accepts Spectre-style `autoStart` and `maxValue` parameters:

```csharp
ProgressTask queued = progress.AddTask("Download package", autoStart: false, maxValue: 2048);
queued.StartTask();
queued.Value = 1024;
queued.StopTask();
```

`ProgressTask` also supports direct `Description`, `Value`, `MaxValue`, `Tag`, and `HideWhenCompleted` members, plus `Id`, `StartedAt`, `LastUpdatedAt`, `StartTime`, `StopTime`, `Elapsed`, `ElapsedTime`, `RemainingTime`, `Percentage`, `Speed`, `IsStarted`, `IsFinished`, `StartTask()`, `StopTask()`, and `IsIndeterminate(...)`. Property-named helpers such as `task.Description("Download").MaxValue(2048).Value(1024).Tag("nuget").HideWhenCompleted()` are available for chain-oriented pasted snippets. Tasks created without a max value default to `100`, matching common progress examples. Use `ProgressTaskSettings` when a task should be created pending, indeterminate, or with a configured max value:

```csharp
ProgressTask download = progress.AddTask(
    "Download package",
    new ProgressTaskSettings
    {
        AutoStart = false,
        MaxValue = 2048,
        IsIndeterminate = true
    });
```

Task descriptions passed to `AddTask(...)`, `Description`, `Complete(...)`, `Fail(...)`, and `Start(...)` can contain raw Spectre-style markup. Default progress rows and `TaskDescriptionColumn` rows measure by visible text, preserve parsed colors, and keep escaped brackets such as `[[prod]]` literal:

```csharp
ProgressTask publish = progress.AddTask("[green]Build[/] for [[prod]]", 4);
publish.Update(2);
publish.Complete("[cyan]Build ready[/] for [[prod]]");
```

Use ordered insertion when a controller discovers work dynamically but the MVC-style console view should stay grouped:

```csharp
ProgressTask build = progress.AddTask("Build", autoStart: false);
ProgressTask deploy = progress.AddTaskAfter("Deploy", build, autoStart: false);
ProgressTask restore = progress.AddTaskBefore("Restore", build, autoStart: false);
ProgressTask test = progress.AddTaskAt("Test", 2, autoStart: false);

restore.StartTask();
build.StartTask();
test.StartTask();
deploy.StartTask();

progress.RemoveTask(build);
progress.Refresh();
```

Configured progress columns now control the rendered row order and content, including compact text-only rows and operational metrics.

```csharp
int processed = AnsiConsole.Progress()
    .Columns(
        new TaskDescriptionColumn(),
        new ProgressBarColumn { Width = 24 }
            .CompleteColor(Color.Green)
            .RemainingColor(RgbColor.DarkGray),
        new PercentageColumn())
    .RefreshRate(TimeSpan.FromMilliseconds(100))
    .AutoRefresh(false)
    .Start(progress =>
    {
        ProgressTask task = progress.AddTask("Processing records");
        task.IsIndeterminate();
        task.Description = "Discovering total";
        progress.Refresh();

        task.IsIndeterminate(false);
        task.MaxValue = 10;
        task.Value = 5;
        task.Description = "Processing files";
        progress.Refresh();

        task.Value = 10;
        return 10;
    });
```

Available columns include `TaskDescriptionColumn`, `ProgressBarColumn`, `PercentageColumn`, `SpinnerColumn`, `TextColumn`, `DownloadedColumn`, `TransferSpeedColumn`, `RemainingTimeColumn`, and `ElapsedTimeColumn`. `TextColumn` renders literal text or task templates such as `{task.Description}`, `{task.Value}`, `{task.MaxValue}`, `{task.Percentage}`, `{task.State}`, `{task.ElapsedTime}`, `{task.RemainingTime}`, `{task.Speed}`, and `{task.Tag}`; the text can contain raw markup and still shrinks inside narrow terminals. `ProgressBarColumn.Width` is a preferred bar width that stays responsive. `CompleteStyle`/`CompletedStyle`, `FinishedStyle`, `RemainingStyle`, `IndeterminateStyle`, `TextColumn.Style`, and `SpinnerColumn.Style` are honored for Spectre-style initializer snippets, and `CompleteColor(...)`, `FinishedColor(...)`, `RemainingColor(...)`, `IndeterminateColor(...)`, `TextColor(...)`, and `SpinnerStyle(...)` accept `ConsoleColor`, `Color`, and `RgbColor` values for quick MVC-style palette wiring. `UseRenderHook(...)` receives the generated row renderable and current task list, may return a replacement renderable, and is captured back into the fixed progress row width so custom rows remain terminal-safe.

```csharp
AnsiConsole.Progress()
    .Columns(
        new TaskDescriptionColumn(),
        new TextColumn("[grey]{task.Value}/{task.MaxValue}[/]"),
        new ProgressBarColumn(),
        new PercentageColumn())
    .Start(progress =>
    {
        ProgressTask task = progress.AddTask("Processing records", maxValue: 10);
        task.Update(5);
    });
```

Tasks can also represent failures and retry flow:

```csharp
Consoul.Progress(progress =>
{
    ProgressTask publish = progress.AddTask("Publish", 3);

    publish.Increment(1);
    publish.Fail("Publish failed");
    publish.Start("Retry publish");
    publish.Complete("Publish ready");
});
```

Use `Consoul.Live(...)` when the dashboard itself should be redrawn as model state changes:

```csharp
Consoul.Live(live =>
{
    int queued = 3;

    live.Refresh(() =>
    {
        Consoul.PropertyList(list =>
        {
            list.Add("Queued", queued, ConsoleColor.Yellow);
            list.Add("Running", 1, ConsoleColor.Cyan);
        });
    });

    queued--;
    live.Refresh(() =>
    {
        Consoul.PropertyList(list =>
        {
            list.Add("Queued", queued, ConsoleColor.Yellow);
            list.Add("Running", 2, ConsoleColor.Cyan);
        });
    });
}, new LiveDisplayOptions { Title = "Queue" });
```

For migration-friendly dashboard snippets, pass a renderable target to `AnsiConsole.Live(...)`, mutate that target or replace it with `UpdateTarget(...)`, then call `Refresh()` when a mutable target changes. Overflow options crop captured renderable output inside the active terminal while the default visible mode keeps the existing direct rendering behavior.

```csharp
var status = new Table()
    .Width(42)
    .HideFooter()
    .AddColumns("Service", "State")
    .AddRow("API", "Starting");

int refreshes = AnsiConsole.Live(status)
    .Overflow(VerticalOverflow.Crop)
    .Cropping(VerticalOverflowCropping.Top)
    .AutoClear(true)
    .Start(live =>
    {
        status.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
        live.Refresh();

        status.Rows.Clear();
        status.Rows.Add("Worker", "Online");
        live.Refresh();
        return live.RefreshCount;
    });
```

Named `Layout` panes work well as MVC-style view regions for live POCs. Build the layout once, use `Size(...)`, `Ratio(...)`, `MinimumSize(...)`, or `Invisible()` when regions need sizing hints, update the panes from model/controller actions, and refresh the target.

```csharp
var dashboard = new Layout("Dashboard")
    .SplitRows(
        new Layout("Header").Size(3),
        new Layout("Body").SplitColumns(
            new Layout("Status").Ratio(2).MinimumSize(18),
            new Layout("Metrics").Ratio(1),
            new Layout("Debug").Invisible()));

dashboard["Header"].Update("Starting");
dashboard["Status"].Update(status);
dashboard["Metrics"].Update("Jobs: 1");

AnsiConsole.Live(dashboard).Start(live =>
{
    status.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
    dashboard["Header"].Update("Running");
    dashboard["Metrics"].Update("Jobs: 2");
    live.Refresh();
});
```

```csharp
using (var progress = new ProgressBar("Starting")
{
    BarWidth = 80,
    BlockCharacter = '#',
    EmptyCharacter = '-',
    ShowTrack = true,
    ShowPercentage = true
})
{
    for (int i = 0; i <= 10; i++)
    {
        progress.UpdateStyled(
            i / 10.0,
            $"Step {i}/10",
            messageStyle: new Style(ConsoleColor.Cyan),
            barStyle: new Style(ConsoleColor.Green));
        Thread.Sleep(100);
    }
}
```

## Views

Use `Consoul.View(...)`, `Consoul.View("Title", model)`, or `Consoul.View<TModel>("Title")` for first-run POCs, then graduate to `StaticView` or `DynamicView<T>` when the workflow deserves a named class.

```csharp
using ConsoulLibrary.Color;

[View("Tools")]
public sealed class ToolsView : StaticView
{
    public ToolsView()
    {
        AddOption("Say hello", () => Consoul.Write("Hello"));
        AddSeparator("Build", Color.Aqua);
        AddOption("Run task", RunTask, RgbColor.Green)
            .Describe("Runs the sample workload.")
            .UseShortcut("r");
        AddHelpOption("Help", null, Color.Green);
    }

    private static void RunTask()
    {
        using (var progress = new ProgressBar("Working"))
        {
            progress.Update(1.0, "Done");
        }
    }
}

Consoul.Render<ToolsView>();
```

For model-backed POC screens, keep the same MVC-style surface with `DynamicView<T>` instead of switching to a separate fluent view abstraction:

```csharp
using ConsoulLibrary.Color;

public sealed class DeploySettings
{
    public bool Previewed { get; set; }
    public bool Deployed { get; set; }
}

[View("Details")]
public sealed class DeployDetailsView : StaticView
{
}

var settings = new DeploySettings();

Consoul.View("Deploy", settings)
    .Separator("Build", Color.Aqua)
    .Option("Preview", model => model.Previewed = true, RgbColor.Yellow, "Runs a dry run.")
    .AddConfirmOption("Deploy", "Deploy now?", model => model.Deployed = true, Color.Green)
    .AddEditFormOption("Edit settings", RgbColor.Cyan, enableDocumentEditor: true)
    .AddNavigationOption<DeployDetailsView>("Details", Color.Aqua, replace: false)
    .HelpOption("Help", null, RgbColor.Green)
    .Render();
```

The sample app includes small POC examples for dynamic menus, badges, breadcrumbs, bullet lists, callouts, metrics, shortcut bars, timelines, table display, tree display, and form/object editing under `Consoul.Test/Views/PocViews.cs`.

## Routines

Routines let automated demos and smoke tests feed input to prompts without real keyboard input.

```csharp
public sealed class DemoRoutine : Routine
{
    public DemoRoutine()
    {
        Enqueue(new RoutineInput
        {
            Value = "Deploy",
            Method = RoutineInput.InputMethod.OptionText
        });
        Enqueue(new RoutineInput { Value = "yes" });
    }
}

Routines.InitializeRoutine(new DemoRoutine(), "Demo");
```

`OptionText` inputs resolve against the current prompt's option labels, shortcuts, and hidden search terms, so scripted demos can survive menu reordering while MVC-style action labels stay readable.

Routine input transforms can use the lightweight core settings API:

```csharp
Routines.ConfigureTransforms(new Dictionary<string, string>
{
    ["ApiKey"] = "local-dev-key"
});

var input = new RoutineInput
{
    Value = "{{ApiKey}}",
    Transforms = new[]
    {
        new InputTransform { Key = "ApiKey", UseAppSettings = true }
    }
};

Consoul.Write(input.Value);
```

XML routines can declare reusable variables so recorded POC demos stay environment-neutral without repeating transform blocks on every input:

```xml
<XmlRoutineDoc>
  <Meta>
    <Name>Deploy demo</Name>
    <DateCreated>2026-07-05T00:00:00Z</DateCreated>
    <UseDelays>false</UseDelays>
    <Variables>
      <Variable key="Environment" value="production" />
      <Variable>
        <Key>Region</Key>
        <Value>east</Value>
      </Variable>
    </Variables>
  </Meta>
  <Routines>
    <Routine name="Deploy">
      <Meta>
        <Variables>
          <Service>api</Service>
        </Variables>
      </Meta>
      <Inputs>
        <Input>
          <Value>{{Environment}}/{{Service}}/{{Region}}</Value>
          <Method>Value</Method>
        </Input>
      </Inputs>
    </Routine>
  </Routines>
</XmlRoutineDoc>
```

`new XmlRoutine(path, "Deploy")` replays `production/api/east`. Variables under a routine override document-level variables for that named path, and `XmlRoutine.SaveInputs(...)` writes `XmlRoutine.Variables` back to XML for reusable smoke-test scripts.

If your application already uses `Microsoft.Extensions.Configuration`, install `Consoul.Extensions.Configuration` for typed configuration helpers.

## Form And Object Editor

`Consoul.Form(...)`, `DynamicView<T>.EditForm(...)`, `DynamicView<T>.AddEditFormOption(...)`, `StaticView.AddEditFormOption(...)`, and `EditObjectView` provide the reflection-based editor for object properties. Metadata, nested object handling, and JSON-style value rendering stay in one editor stack. Core Consoul includes a dependency-free JSON-style value layout inside the form document editor. Install `Consoul.Text.Json` if you want those values to use `System.Text.Json`, or assign `EditObjectView.FormValueSerializer` directly for a custom formatter.

Use `[DisableFormDocumentEditor]` on model types that should keep only the legacy property-by-property menu. The older `[DisableJsonEditor]` name is still honored for source compatibility, but new MVC-style object editors should prefer the form document name.

```csharp
var settings = new MySettings();
var editor = Consoul.Form(settings);
editor.Render();
```

## Optional Integrations

Install only the integrations your application needs:

```bash
dotnet add package Consoul.Extensions.Logging
dotnet add package Consoul.Extensions.Configuration
dotnet add package Consoul.Text.Json
```

Each optional package depends on `Consoul` and its respective integration library.

## License

Consoul is licensed under the GNU Lesser General Public License v3.0.
