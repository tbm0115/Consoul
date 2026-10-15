# Core console APIs

[Documentation index](index.md) · [Install the 3.0 preview](nuget.md) · [Migration guide](migration-3.0.md)

The preview supports markup and emoji substitution, but layout measurement still uses UTF-16 length in several paths. Wide characters, combining marks, and clipped emoji can misalign or be truncated incorrectly. See the [known limitations](releases/3.0.0-preview.1.md#known-limitations).

## When to use this
Start here whenever you need structured console output or simple input helpers without adopting the view or routine systems. These APIs sit closest to `System.Console` and can be introduced incrementally into existing apps.

## Key types
* `Consoul.Capture` - Records Consoul render output to plain text using a virtual console width, without writing to the active terminal.
* `Consoul.Terminal` / `AnsiConsole.Profile` / `TerminalProfile` - Reports the active terminal width, height, redirection state, and virtual-driver status for responsive layout decisions.
* `IConsoulRenderable` - Shared contract for reusable visual components such as panels, grids, rows, charts, tables, text blocks, action-backed sections, and other entry objects.
* `IRenderable` - Spectre-style alias for reusable visual components; Consoul renderables implement both contracts, and custom `IRenderable` snippets can be adapted with `Consoul.Renderable(...)`.
* `AnsiConsole` - Spectre-style entry points that delegate to Consoul while preserving result-first prompt outcomes, including `IPrompt<TResult>`, `PromptAsync`, `AskAsync`, `ConfirmAsync`, `Select(...)`, `MultiSelect(...)`, `SelectEnum<TEnum>(...)`, `MultiSelectEnum<TEnum>(...)`, `FilePath(...)`, and `DirectoryPath(...)` helpers for async and controller-style view actions.
* `IAnsiConsole` / `AnsiConsole.Console` - Injectable facade for services and reusable POC components that should not call static APIs directly. `AnsiConsole.Console` can also be assigned in tests; static facade helpers route through the assigned facade until the previous value is restored. Additive extension methods provide primitive/formatted `Write`/`WriteLine`, quick panel, and result-first `Select(...)` / `MultiSelect(...)` / enum-selection / path-prompt overloads without expanding the core interface.
* `AnsiConsole.Cursor` / `IAnsiConsoleCursor` - Spectre-style cursor visibility and movement helpers over Consoul's active console driver.
* `Markup` / `Text` / `Paragraph` / `FigletText` / `Badge` / `Callout` / `Breadcrumb` / `BulletList` / `Checklist` / `Rule` / `Panel` / `Table` / `Tree` / `BarChart` / `BreakdownChart` / `Metrics` / `Sparkline` / `Timeline` / `ShortcutBar` / `Calendar` / `Canvas` / `Rows` / `Columns` / `Grid` / `Padder` / `Align` / `Layout` - Spectre-style builder aliases over Consoul's reusable text, status, list, rule, panel, table, tree, chart, dashboard, calendar, canvas, and composition renderers.
* `Consoul.Write` – Primary output helper that respects `RenderOptions.WriteMode` and color schemes.
* `Consoul.Markup` / `Consoul.MarkupLine` / `Consoul.MarkupLineInterpolated` – Inline color helpers for strings with tags such as `[green]Ready[/]`, `[#22c55e]Ready[/]`, `[bold red]Blocked[/]`, `[link=https://example.com]docs[/]`, or emoji shortcodes such as `:check_mark:`.
* `Consoul.WriteException` – Responsive diagnostic panels for exceptions, context messages, bounded stack traces, and Spectre-style `ExceptionFormats` / `ExceptionSettings` adapters.
* `RenderOptions` – Global configuration for default colors, write modes, prompt text, and invalid message styles.
* `ConsoulTheme` – Preset and custom color schemes that update the existing render options.
* `ColorScheme` – Bundles foreground/background combinations for repeated use.
* `Consoul.Wait` – Displays a “Press enter to continue” style message and pauses execution.
* `Consoul.Input` / `Consoul.Ask` / `Consoul.Read` / `Consoul.FilePath` / `Consoul.DirectoryPath` – Input helpers for free-form text, boolean confirmations, path prompts, and asynchronous reading.
* `Consoul.Align` / `Consoul.Badge` / `Consoul.BadgeText` / `Consoul.BarChart` / `Consoul.Breadcrumb` / `Consoul.BreakdownChart` / `Consoul.BulletList` / `Consoul.Calendar` / `Consoul.Callout` / `Consoul.Canvas` / `Consoul.Checklist` / `Consoul.Columns` / `Consoul.FigletText` / `Consoul.Grid` / `Consoul.Layout` / `Consoul.Headline` / `Consoul.Metrics` / `Consoul.Pad` / `Consoul.Panel` / `Consoul.PropertyList` / `Consoul.Rows` / `Consoul.Rule` / `Consoul.ShortcutBar` / `Consoul.Sparkline` / `Consoul.Table` / `Consoul.TextBlock` / `Consoul.TextPath` / `Consoul.Timeline` / `Consoul.Tree` / `Consoul.ValidationSummary` – Lightweight view helpers for structured, responsive output.

Exception diagnostic panels parse raw markup in human-authored panel titles, context messages, and exception messages, while generated labels and stack trace frames remain literal.

## Minimal example
```csharp
using ConsoulLibrary;
using ConsoulLibrary.Color;

Consoul.ApplyTheme(ConsoulTheme.Modern);
RenderOptions.DefaultScheme = new ColorScheme
{
    Color = ConsoleColor.White,
    BackgroundColor = ConsoleColor.Black
};
RenderOptions.InvalidScheme = new ColorScheme
{
    Color = ConsoleColor.White,
    BackgroundColor = ConsoleColor.DarkRed
};

Consoul.MarkupLine("[green]System initialised[/]");
Consoul.MarkupLine("[green]:check_mark:[/] Ready [red]:x:[/] Failed");
if (!Consoul.Ask("Proceed with deployment?"))
{
    Consoul.Write("Operation canceled", ConsoleColor.Yellow);
    return;
}

TextPromptResult<string> environment = Consoul.Text<string>("Target environment")
    .WithDefault("Development")
    .Render();
if (!environment.HasValue)
{
    return;
}

string environmentName = environment.Value;
Consoul.MarkupLineInterpolated($"Deploying to [cyan]{environmentName}[/]");

Consoul.Headline("Deploy", new HeadlineOptions
{
    Style = HeadlineStyle.Block,
    Width = 40
});

Consoul.FigletText("POC", width: 40, color: ConsoleColor.Cyan);

AnsiConsole.Write(new Text("Ready", ConsoleColor.Green));
AnsiConsole.Write(new Text("Owner approval", "white on navy"));

AnsiConsole.Write(new Paragraph("Status: ", ConsoleColor.Cyan)
    .AppendLineStyled("Ready", "green")
    .AppendMarkup("Next [yellow]review[/]")
    .Width(40)
    .Ellipsis());

AnsiConsole.Write(new FigletText("POC")
    .Width(40)
    .Fitted()
    .Pad(false)
    .Color("bold #22c55e"));

Consoul.Breadcrumb("Home", "Operations", "Deployments");

Consoul.ShortcutBar(bar =>
{
    bar.Add("Enter", "Select");
    bar.Add("Esc", "Back");
}, new ShortcutBarOptions { Width = 40 });

Consoul.Calendar(new DateTime(2026, 6, 1), calendar =>
{
    calendar.Add(new DateTime(2026, 6, 18), ConsoleColor.Green, "Release");
}, new CalendarOptions { Width = 32 });

Consoul.TextBlock(
    "Deployment overview",
    width: 40,
    alignment: TextBlockAlignment.Center,
    color: ConsoleColor.Cyan);

Consoul.TextPath(
    @"C:\builds\consoul\artifacts\release-notes.json",
    width: 40,
    fileNameColor: ConsoleColor.Cyan);

AnsiConsole.Write(new TextPath(@"C:\builds\consoul\artifacts\release-notes.json")
    .Width(40)
    .DirectoryColor(Color.Grey)
    .StemColor(RgbColor.Aqua)
    .ExtensionColor(Color.Green));

Consoul.Badge("prod", BadgeKind.Info, writeLine: false);
Consoul.Write(" ", writeLine: false);
Consoul.Badge("ready", BadgeKind.Success);

Consoul.Callout(
    "Deployment is waiting for owner approval.",
    CalloutKind.Warning,
    new CalloutOptions { Width = 40 });

Consoul.Checklist(list =>
{
    list.Add("Build", ChecklistItemState.Complete);
    list.Add("Test", ChecklistItemState.Complete);
    list.Add("Approve", ChecklistItemState.Pending, "Waiting for owner sign-off.");
}, new ChecklistOptions { Width = 40 });

Consoul.Timeline(timeline =>
{
    timeline.Add("Build", TimelineItemState.Complete);
    timeline.Add("Review", TimelineItemState.Active, "Waiting for owner approval.");
    timeline.Add("Publish", TimelineItemState.Pending);
}, new TimelineOptions { Width = 40 });

Consoul.BulletList(list =>
{
    BulletListItem notes = list.Add("Release notes");
    notes.Add("Mention responsive rendering coverage.");
    notes.Add("Call out optional package splits.");
}, new BulletListOptions { Width = 40 });

Consoul.Metrics(metrics =>
{
    metrics.Add("Builds", 42, note: "Today", kind: MetricKind.Success);
    metrics.Add("Failures", 2, note: "Needs triage", kind: MetricKind.Warning);
    metrics.Add("Latency", 128, "ms", "P95", MetricKind.Info);
}, new MetricsOptions { Width = 40 });

Consoul.Sparkline(
    new[] { 12d, 10d, 11d, 8d, 7d, 9d },
    label: "Duration",
    width: 40,
    color: ConsoleColor.Green,
    showRange: true);

Consoul.Canvas(24, 5, canvas =>
{
    canvas.Rectangle(0, 1, 7, 3, '#', Color.Cyan);
    canvas.Text(1, 2, "Build", RgbColor.Cyan);
    canvas.Line(7, 2, 15, 2, '-');
    canvas.Rectangle(15, 1, 8, 3, '#', Color.Green);
    canvas.Text(16, 2, "Ship", RgbColor.Green);
});

AnsiConsole.Write(new Canvas(4, 2)
    .SetPixel(0, 0, Color.Red)
    .SetPixel(0, 1, RgbColor.Green)
    .SetPixel(1, 0, RgbColor.Blue)
    .SetPixel(1, 1, Color.Yellow)
    .MaxWidth(4));

AnsiConsole.Write(new Canvas(16, 4)
    .DefaultColor(RgbColor.DarkGray)
    .Rectangle(0, 0, 15, 3, '#', Color.Cyan)
    .Text(3, 1, "API", RgbColor.Aqua));

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

Consoul.PropertyList(list =>
{
    list.Add("Environment", environmentName, Style.Plain.WithForeground(RgbColor.Aqua));
    list.Add("Status", "Pending", ConsoleColor.Yellow);
    list.Add("Retries", 3);
    list.AddKeyValues(settings, "green", "grey");
});

Consoul.PropertyList(new
{
    Environment = environmentName,
    Status = "Pending",
    Retries = 3
}, new PropertyListOptions { LabelWidth = 14 });

Consoul.BarChart(chart =>
{
    chart.Add("Ready", 3, ConsoleColor.Green);
    chart.Add("Pending", 1, ConsoleColor.Yellow);
}, new BarChartOptions
{
    Label = "Jobs",
    LabelAlignment = TextBlockAlignment.Center
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
    grid.Add(environmentName, "Environment", ConsoleColor.Cyan);
    grid.Add("Pending", "Status", ConsoleColor.Yellow);
}, new GridOptions { Columns = 2 });

AnsiConsole.Write(new Grid()
    .AddColumn(14)
    .AddColumn(10, Justify.Right)
    .Collapse()
    .AddRows(
        new[] { "Build", "Ready" },
        new[] { "Deploy", "Waiting" }));

Consoul.Layout(layout =>
{
    layout.Add("Package prepared\nApprovals pending", "Pipeline", ConsoleColor.Cyan);
    layout.Add("Release window: Friday\nOwner: Platform", "Operations", ConsoleColor.Yellow);
}, new LayoutOptions { Columns = 2, MinimumPaneWidth = 18 });

var summaryPanes = new[]
{
    LayoutPane.Create("Ready", "Status", Color.Green).Size(12),
    LayoutPane.Create("Jobs: 2", "Metrics", RgbColor.Aqua)
};
AnsiConsole.Write(new Layout(summaryPanes).Width(36).ColumnCount(2));

var dashboard = new Layout("Dashboard")
    .TitleColor(Color.Aqua)
    .ContentColor(RgbColor.White)
    .SplitRows(
        new Layout("Header").Size(3),
        new Layout("Body").SplitColumns(
            new Layout("Status").Ratio(2).MinimumSize(18),
            new Layout("Metrics").Ratio(1),
            new Layout("Debug").Invisible()));

dashboard["Header"].Update("POC deployment view", Color.Aqua);
dashboard["Status"].Update(new Table()
    .HideFooter()
    .AddColumns("Service", "State")
    .AddRow("API", "Ready"));
dashboard["Metrics"].Update("Jobs: 2", RgbColor.Green);
AnsiConsole.Write(dashboard);

AnsiConsole.Write(new Columns()
    .ColumnCount(3)
    .AddColumn("Prepare package and verify approvals.", "Plan", ConsoleColor.Cyan)
    .AddEmptyColumn("Gap")
    .AddColumn("Run deployment after the approval gate clears.", "Next", ConsoleColor.Yellow));

AnsiConsole.Write(new Rows()
    .Width(44)
    .Spacing(0)
    .AddRule("Checks", width: 40)
    .AddRow(() => Consoul.PropertyList(list =>
    {
        list.Add("Database", "Ready", ConsoleColor.Green);
        list.Add("Feature flags", "Ready", ConsoleColor.Green);
    }, new PropertyListOptions { Width = 40 })));

Consoul.Pad(
    () => Consoul.Panel("Ready for release", title: "Status", width: 36),
    left: 2,
    width: 42);

AnsiConsole.Write(new Padder(new Panel("Ready").Header("Status"))
{
    Padding = new Padding(2, 1),
    Expand = true
}.Color(RgbColor.Green));

AnsiConsole.Write(Align.Center(new Panel("Centered summary").Header("Overview"))
    .Width(48)
    .Color(Color.Aqua));

// Align.Left(...), Align.Center(...), and Align.Right(...) also accept text, render actions, and row collections.

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

var summary = new PanelEntry(
    "Build ready",
    new PanelOptions { Title = "Summary", Width = 32 });

var checklist = new ChecklistEntry(new ChecklistOptions { Width = 32 });
checklist.Add("Tests", ChecklistItemState.Complete);
checklist.Add("Docs", ChecklistItemState.Pending);

Consoul.Rows(rows =>
{
    rows.Add(summary);
    rows.Add(checklist);
}, new RowsOptions { Width = 36, Spacing = 0 });

Consoul.Tree("Deployment", root =>
{
    root.Add("Environment: " + environmentName);
    root.Add("Status: Pending");
});

try
{
    throw new InvalidOperationException("Preview failed");
}
catch (Exception exception)
{
    Consoul.WriteException(
        exception,
        "Deployment preview failed",
        new ExceptionRenderOptions { MaxStackFrames = 4 });

    AnsiConsole.WriteException(
        exception,
        ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
}
```

## Terminal profile

Use `Consoul.Terminal` or the Spectre-style `AnsiConsole.Profile` alias when a POC screen needs to branch on terminal width or avoid interactive behavior in redirected output. The profile is backed by Consoul's internal console driver, so it reflects virtual widths inside `Consoul.Capture(...)` and test scopes.

```csharp
TerminalProfile terminal = AnsiConsole.Profile;
TerminalCapabilities capabilities = terminal.Capabilities;

int dashboardWidth = terminal.ResolveWidth(100, minimumWidth: 40);

if (!capabilities.Interactive)
{
    Consoul.MarkupLine("[grey]Running in redirected output mode.[/]");
}

if (terminal.IsNarrow())
{
    Consoul.Rows(rows =>
    {
        rows.Add("Status: Ready");
        rows.Add("Owner: Platform");
    }, new RowsOptions { Width = dashboardWidth });
}
else
{
    Consoul.Columns(columns =>
    {
        columns.Add("Status: Ready", "Pipeline", ConsoleColor.Green);
        columns.Add("Owner: Platform", "Operations", ConsoleColor.Cyan);
    }, new ColumnsOptions { Width = dashboardWidth, Columns = 2 });
}
```

## Advanced tips
* **Write modes & fallbacks** – `RenderOptions.WriteMode` lets you suppress all output or only colourised output. This is useful when running tests or logging to CI pipelines. When authoring custom writers, call `RenderOptions.GetColorOrDefault` to respect the active mode automatically.
* **Terminal-aware layout** – `Consoul.Terminal` and `AnsiConsole.Profile` give you a snapshot of width, height, input/output redirection, virtual rendering, Spectre-style `Capabilities` (`Ansi`, `Unicode`, `Links`, `Interactive`, `Legacy`, and `ColorSystem`), and helper methods such as `IsNarrow(...)`, `IsWide(...)`, and `ResolveWidth(...)`. Prefer them over direct `Console.BufferWidth` calls so CI, capture, redirected output, and fake-console tests use the same fallback behavior as Consoul's renderers.
* **Cursor helpers** – Use `AnsiConsole.Cursor.Hide()`, `Show()`, `SetPosition(column, line)`, and `MoveUp(...)` / `MoveDown(...)` / `MoveLeft(...)` / `MoveRight(...)` when adapting cursor-aware Spectre-style snippets. Movement is clamped through Consoul's active console driver so fake-console tests and redirected output do not need a real terminal handle.
* **Alternate screens** - Use `AnsiConsole.AlternateScreen(...)`, `AnsiConsole.AlternateScreenAsync(...)`, or injected-console `console.AlternateScreen(...)` helpers for temporary dashboards that should not leave their full-frame output in the main scrollback. The scope emits ANSI alternate-buffer sequences only when `AnsiConsole.Profile.Capabilities.Ansi` is enabled; redirected or explicitly non-ANSI outputs run the action as a safe no-op wrapper.
* **Plain output parity** - `AnsiConsole.Write(...)` and `WriteLine(...)` include Spectre-style primitive, character-array, culture-aware, renderable, and composite-format overloads. `WriteAnsi(Action)` is available on both `AnsiConsole` and `IAnsiConsole` for snippets that emit raw terminal control sequences while still participating in the assigned-facade test path.
* **Formatted markup** - `Consoul.Markup(...)`, `Consoul.MarkupLine(...)`, `AnsiConsole.Markup(...)`, `AnsiConsole.MarkupLine(...)`, and the injectable-console extension methods accept composite-format overloads such as `AnsiConsole.MarkupLine("Target: [green]{0}[/]", target)`. Use `Markup.FromFormat(...)` when a controller or model-backed view should return a reusable markup renderable instead of writing immediately. Template tags stay active while formatted values are escaped, so model values containing square brackets render literally.
* **Prompt palette helpers** - `SelectionPrompt`, `MultiSelectionPrompt`, `TextPrompt<T>`, `SecretPrompt`, `ConfirmPrompt`, and `ConfirmationPrompt` style helpers accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values. Scalar prompts expose direct `Color` / `RgbColor` overloads for `PromptStyle(...)`, `Style(...)`, `InvalidStyle(...)`, and mask helpers, so instance-style migrated snippets preserve RGB/hex metadata instead of falling back through `ConsoleColor` conversions. On ANSI-capable terminals, prompt, invalid, instruction, accepted-choice, default-value, disabled, search-highlight, description, and mask styles can emit 24-bit or 256-color ANSI while still mapping to readable console colors elsewhere.
* **Inline markup** – `Consoul.MarkupLine` supports `ConsoleColor` names, common aliases such as `[aqua]`, named schemes such as `[prompt]` and `[invalid]`, RGB/hex colors such as `[#22c55e]`, background colors like `[white on blue]`, and doubled brackets for literals. True-color ANSI terminals render RGB/hex values with 24-bit SGR sequences, 256-color terminals emit 8-bit SGR sequences, and simpler terminals map them to the nearest `ConsoleColor`. Common Spectre-style modifiers such as `bold`, `dim`, `italic`, `underline`, `strike`, `blink`, `reverse`, `conceal`, and `link` are accepted for portability; ANSI-capable terminals render decorations with SGR control sequences, link tags render as OSC-8 hyperlinks when `AnsiConsole.Profile.Capabilities.Links` is enabled, and fallback terminals keep plain visible text with the configured colors. Known emoji shortcodes such as `:check_mark:`, `:x:`, `:warning:`, and `:rocket:` are replaced before color tags are rendered, and `Emoji.Replace(...)`, `Emoji.Get(...)`, and `Emoji.Known` expose the same mapping for reusable view helpers. Both `[/]` and named close tags such as `[/red]` close the current style. Use `MarkupLineInterpolated(...)`, `Markup.FromInterpolated(...)`, `Markup.FromFormat(...)`, `EscapeMarkup(...)`, `Markup.Escape(...)`, `RemoveMarkup(...)`, or `Markup.Remove(...)` when inserting model/user values or stripping tags for logs and tests. Unknown tags and unknown emoji aliases render as plain text so exploratory views fail softly. Use `MarkupEntry` or the migration-friendly `new Markup("...")` alias when the same markup should be captured, placed in a panel/layout, or returned from a model-backed view. Use `Style.Parse(...)`, `Style.ParseColor(...)`, `Style.Plain.WithForeground(...)`, `Color.Red`, `Color.FromHex(...)`, `Color.Rgb(...)`, `RgbColor.Red`, or `RgbColor.FromHex(...)` when builder code needs the same string/RGB parsing or immutable style composition outside markup; parsed styles preserve RGB/hex and decoration metadata so style-aware text, panel, rule, table-title/caption, and markup paths can emit 24-bit or 256-color ANSI on capable terminals while falling back to readable foreground/background console colors elsewhere. Common builders such as `Text`, `Panel`, `Table`, `Rule`, `Tree`, `BarChart`, `BreakdownChart`, `Metrics`, `Sparkline`, `Timeline`, `Badge`, `Callout`, `Breadcrumb`, `BulletList`, `Checklist`, `ShortcutBar`, `Calendar`, `Rows`, `Columns`, `Grid`, `Layout`, `Align`, `TextPath`, `Canvas`, `PropertyListEntry`, and `ViewHelpEntry` accept reusable `Style` objects directly in their color/style helper methods.
* **Structured diagnostics** – Use `Consoul.WriteException` inside view actions or `catch` blocks to keep error type, message, inner exception and stack trace content wrapped inside the current terminal width. Aggregate exceptions flatten by default so async and parallel POC failures show each child error; set `MaxAggregateExceptions` to cap noisy batches or `FlattenAggregateExceptions = false` to preserve the original inner-exception chain. Spectre-style snippets can pass `ExceptionFormats.NoStackTrace`, `ShortenPaths`, `ShortenTypes`, `ShortenMethods`, `ShortenEverything`, or `ShowLinks`; these flags adapt to Consoul's existing renderer. Use `ExceptionSettings` and `ExceptionStyle` when a migrated snippet wants named exception/message/stack styles while keeping MVC-style diagnostics bounded and testable.
* **Renderable aliases** – Use `AnsiConsole.Write(renderable)` or `AnsiConsole.WriteLine(renderable)` for reusable components, or `AnsiConsole.Render(renderable)` when adapting older Spectre-style snippets. Injected `IAnsiConsole` values also get `console.Write(renderable)`, `console.WriteLine(renderable)`, `console.Render(renderable)`, and `console.Capture(renderable, width)` through facade extension methods when a helper returns the Spectre-style `IRenderable` alias. All paths render the same Consoul `IConsoulRenderable` objects, including markup, panels, tables, rows, grids, layouts, and MVC-style view sections; use `Consoul.Renderable(IRenderable)` to adapt a custom alias-only component for composition. `Rows.AddRows(...)`, `Columns.AddColumns(...)`, `Grid.AddRows(...)`, and `Grid.AddCells(...)` accept text, renderable, and mixed object params as well as enumerables, `Grid.AddColumns(columns)` accepts generated `GridColumn` metadata, and `Layout.AddPanes(panes)` accepts prepared `LayoutPane` collections, while selector-backed `Layout.AddPanes(...)` projects model collections into text, renderable, action, pane, or nested-layout regions, so copied fluent layout snippets do not need array/list wrappers or adapter loops. Their model-projection color/style selectors preserve `Style` decorations and RGB/hex metadata on capable terminals, so view models can own the dashboard palette without flattening to nearest `ConsoleColor` first.
* **Themes** – Call `Consoul.ApplyTheme(ConsoulTheme.Modern)` for an application-wide look, or wrap a section in `using (Consoul.UseTheme(ConsoulTheme.HighContrast))` to restore the previous colors automatically.
* **Table polish** – Set `TableRenderOptions.Lines.BorderStyle` to `Rounded`, `Square`, `Ascii`, `Heavy`, or `Double` when a model-backed POC table needs stronger visual framing than the legacy simple separators. The `Table` builder also accepts `Border(TableBorder.Rounded)`, `Border(TableBorder.None)`, `DoubleBorder()`, `NoBorder()`, `SafeBorder()`, `MarkdownBorder()`, `AsciiDoubleHeadBorder()`, `MinimalBorder()`, `HorizontalBorder()`, `HeavyHeadBorder()`, `LeftJustified()`, `RightJustified()`, `AddColumn("Name", column => column.RightAligned())`, synchronized `table.Columns[index]` metadata, inline manual rows such as `AddRows(new[] { "API", "Ready" }, new[] { "Worker", "Queued" })`, selector-backed `AddRows(items, item => item.Value, ...)`, and renderable row cells for pasted manual snippets. Color helpers accept `ConsoleColor` values, reusable `Style` objects, style strings such as `"grey"`, `"yellow"`, `"#22c55e"`, and `"white on navy"`, Spectre-style values such as `Color.Red` and `Color.FromHex(...)`, RGB values such as `RgbColor.Red` and `RgbColor.FromHex(...)`, and aliases from `ConsoulLibrary.Color.Colors`.
* **Rule polish** - Use `new Rule("...", Justify.Left)`, `new Rule(new Markup("[cyan]...[/]"), Justify.Left)`, `new Rule("...", ConsoleColor.Cyan, Justify.Right)`, `new Rule("...", Color.Aqua, Justify.Right)`, `Consoul.Rule("...", Justify.Right, width: 40, color: ConsoleColor.Cyan)`, `AnsiConsole.Rule("...", Justify.Left, width: 40)`, injected `console.Rule("...", Justify.Center, width: 40)`, `new Rule("...").LeftAligned().RuleStyle("grey").TitleStyle("cyan")`, `new Rule("...").RuleTitle(renderable).RightAligned().RuleColor(Color.Green).TitleColor(RgbColor.Aqua)`, or object-initializer snippets such as `new Rule { Title = "Deploy", Justification = Justify.Left, Style = new Style(ConsoleColor.DarkGray) }`, when adapting Spectre-style rule snippets while keeping the existing responsive rule renderer. Rule color helpers accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values; style strings and `Style` objects preserve decoration and RGB/hex metadata on capable terminals. Rule titles can contain raw markup or reusable renderables such as `Markup`, `Text`, and captured render actions; Consoul measures the visible title text and preserves parsed title colors when markup is available.
* **Panel polish** – Set `PanelOptions.BorderStyle` to `Rounded`, `Square`, `Ascii`, `Heavy`, `Double`, or `None` when a dashboard section needs a clearer frame or padding without visible borders. Use `TitleAlignment(...)`, `Footer`, and `FooterAlignment(...)` to render command hints or status text directly in the panel frame or borderless title/footer lines. Use `new Panel("...").Title("Status", Color.Aqua).TitleAlignment(Justify.Center).Footer("Esc", "white on navy", Justify.Right).BorderColor(Color.Green).ContentColor(RgbColor.White).Padding(1, 0, 1, 0).Border(BoxBorder.Rounded).Width(...)`, `new Panel("...").Header(new Markup("[cyan]Status[/]")).Footer(new Markup("[grey]Esc[/]"))`, `new Panel("...").Padding(2, 1)`, `new Panel("...").NoBorder()`, `new Panel("...").Collapse()`, `new Panel(renderable)`, `new Panel(() => { ... })`, or injected-facade helpers such as `console.Panel("Ready", title: "Status", width: 32, color: ConsoleColor.Green)` when adapting Spectre-style examples without leaving Consoul's reusable renderable path. Raw markup in panel content, headers, and footers is measured by visible text and rendered with parsed foreground/background colors, ANSI decorations, and RGB/hex metadata on capable terminals. `BoxBorder.None`, `BoxBorder.Safe`, `BoxBorder.Ascii2`, `BoxBorder.AsciiDoubleHead`, `BoxBorder.Markdown`, `BoxBorder.Simple`, `BoxBorder.SimpleHeavy`, `BoxBorder.Minimal`, `BoxBorder.MinimalHeavyHead`, `BoxBorder.MinimalDoubleHead`, `BoxBorder.HeavyEdge`, `BoxBorder.HeavyHead`, `BoxBorder.DoubleEdge`, `SafeBorder()`, `MarkdownBorder()`, `AsciiDoubleHeadBorder()`, `MinimalBorder()`, `MinimalHeavyHeadBorder()`, `HorizontalBorder()`, `HeavyHeadBorder()`, `DoubleEdgeBorder()`, `NoBorder()`, `Collapse()`, `Expand()`, `PanelOptions.Expand`, `PanelHeader`, `Footer(new PanelHeader(...))`, and inline `Header(...)` / `Title(...)` / `Footer(...)` overloads for renderables, `ConsoleColor`, `Style`, style strings, `Color`, and `RgbColor` are also accepted for pasted snippets and map to the closest existing panel renderer. Renderable/action panels capture their content at render time so fluent width and padding changes shape the captured inner layout.
* **Exception polish** - Use raw markup in exception panel titles, context messages, and exception messages when a quick diagnostic needs status color, such as `[yellow]Preview failed[/]` or `[cyan]Deploying[/] [[prod]]`. Generated labels and stack trace frames stay literal, so method names and file paths are not interpreted as markup.
* **Tree polish** - Use raw markup labels such as `new Tree("[cyan]Deployment[/]")`, `root.Add("[green]API[/]")`, or `root.Add("[white on navy]Approval[/]")` when a quick MVC-style diagnostic tree needs color without separate label objects. Tree rendering measures and wraps the visible label text, preserves parsed foreground/background colors, RGB/hex metadata, and ANSI decorations, and still keeps unknown tags or doubled brackets literal. `TextColor(...)`, `Style(...)`, `Color(...)`, `BranchColor(...)`, `AddNode(...)`, selector-backed `AddNodes(...)`, recursive `AddTree(...)`, and post-creation `TreeNode.Style(...)` / `TreeNode.Color(...)` helpers accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values so MVC-style model trees can carry palette and child-state selectors without adapter loops.
* **Visual builders** - Use `new Text("...")`, `new Paragraph().Append(...)`, `new TextPath("...")`, `new FigletText("...")`, `new Tree("...")`, `new BarChart().Add(...)`, `new BreakdownChart().Add(...)`, `new Metrics().Add(...)`, `new Metrics().AddSuccess(...)`, `new Sparkline(values).Label(...)`, `new Timeline().Add(...)`, `new Timeline().AddComplete(...)`, `new ShortcutBar().Add(...)`, `new ShortcutBar().AddShortcut(...)`, `new Calendar(...)`, and `new CanvasImage(...)` for migration-friendly snippets that still render through Consoul's responsive visual components. `Text` exposes `Empty`, `NewLine`, `Length`, `Lines`, `Justification`, `Overflow`, direct color/style constructors such as `new Text("Ready", ConsoleColor.Green)`, `new Text("Deploy", Color.Aqua)`, `new Text("Trend", RgbColor.Green)`, and `new Text("Blocked", "white on navy")`, raw markup rendering for snippets such as `new Text("Status: [green]Ready[/]")`, and the object-initializer-friendly `Style` property, while `Paragraph` preserves per-append styles, supports direct `Color`/`RgbColor` constructor and append overloads, `AppendLine(...)`, `AppendStyled(...)`, `AppendLineStyled(...)`, `AppendMarkup(...)`, and interpolated markup append helpers, and emits `Link` metadata as OSC-8 hyperlinks when the active terminal profile supports links. `TextPath` supports root, directory, stem, leaf, extension, separator, ellipsis, width helpers, and segment `Color(...)`/`Style(...)` aliases that preserve RGB/hex and decoration metadata for path-heavy diagnostics. `FigletText` supports `FullSize()`, `Fitted()`, `Smushed()`, `LayoutMode(...)`, `Pad(...)`, and `Color(...)` over Consoul's dependency-free block glyph renderer, so splash titles can be compact without loading external font files while preserving RGB/hex colors and ANSI decorations on capable terminals. `HeadlineOptions` carries the same text style metadata for plain, boxed, rule, and block headlines. `Canvas` remains useful for MVC-style diagrams with labels, lines, and boxes, and accepts `Set(...)`, `Text(...)`, `Line(...)`, `Rectangle(...)`, `FillRectangle(...)`, `SetPixel(...)`, `DefaultColor(...)`, `MaxWidth(...)`, `Scale(false)`, and `NoScale()` for compact Spectre-style sketches. `CanvasImage` accepts in-memory pixels or `ConsoleColor` matrices and renders terminal half-block thumbnails with `PixelWidth`, `MaxWidth`, `MaxHeight`, `Scale(...)`, `NoScale()`, and resampler helpers while keeping image-file decoding out of the core dependency graph. Tree builders accept `Guide(TreeGuide.Ascii)`, `TreeGuide.Line`, `TreeGuide.DoubleLine`, `TreeGuide.BoldLine`, `Style(...)`, `Color(...)`, `BranchColor(...)`, `AddNodes(...)`, mixed scalar/existing-node/renderable values, selector-backed style strings, post-creation `TreeNode.Style(...)` / `TreeNode.Color(...)`, `TreeNode.Collapse()`, tree-level `Collapse()`, raw markup labels, and captured `IConsoulRenderable` labels such as `Markup`, `Text`, `Panel`, or small tables. Bar and breakdown charts accept chainable `Add(...)`, selector-based `Add(models, labelSelector, valueSelector, colorSelector)` / `AddItems(...)` overloads, prepared item params such as `BarChart.Add(bar1, bar2)` / `BarChart.AddItems(bar1, bar2)` and `BreakdownChart.Add(slice1, slice2)` / `BreakdownChart.AddItems(slice1, slice2)`, and raw markup in chart titles and item labels, so MVC-style POC views can chart model collections without hand-written item loops; `BarChart` and `BreakdownChart` label, segment/bar, value, and item style helpers preserve RGB/hex colors and ANSI decorations on capable terminals. Chart aliases such as `BarChart.MaxValue(...)`, `WithMaxValue(...)`, `UseValueFormatter(...)`, `ShowValue()`, `HideValue()`, `BreakdownChart.UseValueFormatter(...)`, and `UsePercentageFormatter(...)` route copied snippets through the same responsive option model. Sparkline accepts immediate value params such as `Sparkline.AddValues(12, 10, 14)` or `Consoul.Sparkline(line => line.AddRange(12, 10, 14))`, and aliases such as `MinValue(...)`, `MaxValue(...)`, `WithMinValue(...)`, `WithMaxValue(...)`, and `UseValueFormatter(...)` route copied trend snippets through the same range and formatter options. Metrics, timelines, shortcut bars, calendars, and view-help surfaces accept selector-based or reusable item models for model-backed dashboard state until render time, and prepared dashboard item params such as `Metrics.Add(metric1, metric2)` / `Metrics.AddMetrics(metric1, metric2)`, `Timeline.Add(step1, step2)` / `Timeline.AddSteps(step1, step2)`, `ShortcutBar.Add(shortcut1, shortcut2)` / `ShortcutBar.AddShortcuts(shortcut1, shortcut2)`, `Calendar.AddCalendarEvents(releaseDay, freezeDay)`, and `ViewHelpEntry.AddItems(deployHelp, docsHelp)` keep small MVC view summaries terse. Metric labels/values/units/notes, timeline titles/descriptions, shortcut bar keys/descriptions, view-help labels/descriptions/separators, and sparkline labels can contain raw markup and still wrap, pad, and truncate by visible text. Metrics `LabelColor(...)`, `ValueColor(...)`, `NoteColor(...)`, `InfoColor(...)`, `SuccessColor(...)`, `WarningColor(...)`, and `ErrorColor(...)` helpers, Timeline title/description/marker-state/connector helpers, ShortcutBar key/description/separator/disabled helpers, Calendar header/weekday/day/today/marked-day/event helpers, Callout border/title/content helpers, ViewHelp label/separator/options metadata, and Sparkline `LabelColor(...)`, `LineColor(...)`, and `RangeColor(...)` helpers accept `ConsoleColor` values, reusable `Style` objects, `Color` and `RgbColor` values, and style strings while preserving RGB/hex colors and ANSI decorations on capable terminals. Metrics, timelines, shortcut bars, and calendars also accept selectors returning `Color` or `RgbColor` directly, preserving that RGB metadata from view-model state rather than forcing adapter loops. Calendar builders accept mutable `Year`, `Month`, and `Day` values, `CalendarEvents`, `CalendarEvent`, `AddCalendarEvent(...)` overloads, `UseCulture(...)`, `ShowTitle()` / `HideTitle()`, `ShowWeekdays()` / `HideWeekdays()`, `ShowAdjacentDays()` / `HideAdjacentDays()`, `ShowToday()` / `HideToday()`, `HeaderStyle(...)`, `HighlightStyle(...)`, `HeaderColor(...)`, `WeekdayColor(...)`, `DayColor(...)`, `AdjacentDayColor(...)`, `TodayColor(...)`, and `MarkedDayColor(...)` for pasted date/event snippets. Common builder color helpers, item factories, per-item `Add...` overloads, and canvas drawing methods accept `ConsoleColor` values, reusable `Style` objects, `Color` and `RgbColor` values, and style strings such as `"grey"`, `"aqua"`, `"#22c55e"`, and `"white on navy"`.
* **Chart sizing** - `BarChart` and `BreakdownChart` default to terminal-width rendering when no explicit width is set. Use `Collapse()` or set `BarChartOptions.Expand` / `BreakdownChartOptions.Expand` to `false` for measured compact cards, and use `Expand()` to return a chart to terminal-width rendering inside MVC-style dashboard rows.
* **Metric palette helpers** - `Metrics.LabelColor(...)`, `ValueColor(...)`, `NoteColor(...)`, `InfoColor(...)`, `SuccessColor(...)`, `WarningColor(...)`, and `ErrorColor(...)` accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values, preserving RGB/hex colors and ANSI decorations on capable terminals so quick MVC-style dashboard metrics can share the same palette constants as tables, prompts, and status widgets.
* **Model-backed visual projections** - Manual tables, breadcrumbs, rows, columns, grids, layouts, tree nodes, and sparklines also accept selector-based model projections: `Table.AddRows(models, item => item.Name, item => item.Status)`, `Breadcrumb.AddItems(models, item => item.Label)`, `Rows.AddRows(models, item => item.Summary)`, `Columns.AddColumns(models, item => item.Panel, item => item.Title)`, `Grid.AddCells(models, item => item.Value, item => item.Label)`, `Layout.AddPanes(models, item => item.Content, item => item.Title)`, `Tree.AddNodes(models, item => item.Name, item => item.Color)`, `Tree.AddTree(models, item => item.Name, item => item.Children, item => item.StyleText)`, `TreeNode.AddNodes(models, ...)`, and `Sparkline.AddValues(models, item => item.Value)`. Breadcrumbs, rows, columns, grids, layouts, tree nodes, bar charts, and breakdown charts can also project per-item style strings such as `"green"`, `"#22c55e"`, or `"white on navy"` directly from model data. Breadcrumbs, rows, columns, grids, layouts, bullet lists, nested bullet items, checklists, metrics, timelines, calendars, property lists, shortcut bars, trees, tree nodes, bar charts, and breakdown charts also accept selectors returning `Color` or `RgbColor` directly, so palette values can stay in MVC-style view-model state. Use these alongside chart, metrics, timeline, shortcut bar, calendar, bullet-list, and checklist projections so MVC-style view sections can render domain collections without manual adapter loops.
* **Alignment vocabulary** - Use `Justify.Left`, `Justify.Center`, and `Justify.Right` with text, figlet text, rules, panel headers/footers, table titles/captions, manual table columns, and `DynamicView<T>.Section(...)` / `SectionWhen(...)` rule titles when adapting Spectre-style examples. Text-like builders (`Text`, `Paragraph`, and `FigletText`) and `Rule` accept `LeftAligned()`, `Centered()`, and `RightAligned()` alongside `LeftJustified()` and `RightJustified()`, so copied snippets and MVC-style view sections can use the same vocabulary as grids, tables, and alignment wrappers. The existing `TextBlockAlignment`, `RuleTitleAlignment`, and `TableCellAlignment` enums remain available for native Consoul code.
* **Facade helpers** - `AnsiConsole` forwards Consoul's helper-style calls for badges, headlines, figlet text, calendars, callouts, validation summaries, timelines, lists, grids, canvases, canvas images, rows, layouts, padding, alignment, text blocks, paths, breadcrumbs, shortcut bars, property lists, metrics, charts, sparklines, status scopes, progress contexts, live displays, result-first selection shortcuts, capture, and recording. Use these for pasted procedural snippets, then move reusable sections into `IConsoulRenderable` components or `DynamicView<TModel>` sections as the POC matures. Dynamic view options accept both quick `Func<Task>` / `Func<TModel, Task>` callbacks and cancellation-aware async callbacks, keeping first-run MVC-style POC actions terse without hiding cancellation support.
* **Injectable facade** - Depend on `IAnsiConsole` and pass `AnsiConsole.Console` when reusable services, controller-style actions, or POC screen components should be easy to unit test. The interface tracks the static `AnsiConsole` helper method names, including output, prompts, panels, rules, tables, trees, lists, grids, layouts, calendars, canvases, canvas images, validation/property summaries, metrics, charts, status/progress/live scopes, capture, and recording while delegating to the active Consoul console driver, so virtual drivers and capture still work without a real terminal handle. Additive extension helpers cover quick panel overloads for text, captured actions, and reusable renderables, plus `console.Select(...)`, `console.MultiSelect(...)`, `console.SelectEnum<TEnum>(...)`, and `console.MultiSelectEnum<TEnum>(...)` overloads that return result-first prompt objects for controller actions without expanding the core interface. Use `AnsiConsole.Create(new AnsiConsoleSettings { Out = new AnsiConsoleOutput(writer, width: 80), In = new StringReader("Production\n"), Ansi = AnsiSupport.Yes, ColorSystem = ColorSystemSupport.Standard, Interactive = InteractionSupport.No })` when a Spectre-style test or embedded host should render through a specific `TextWriter`, read prompt input from a specific `TextReader`, and expose deterministic `Profile.Capabilities`. For static-facade test setup, assign `AnsiConsole.Console` to a custom facade inside a `try`/`finally` block and restore the previous value when the test ends.
* **Status and list builders** - Use `new Badge("...")`, `new Callout("...")`, `new Breadcrumb()`, `new BulletList()`, and `new Checklist()` for compact dashboard state, navigation hints, notes, and readiness lists. Their marker, label, content, border, separator, key, disabled color helpers, and per-item factories accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values, so MVC-style POC screens can share palette values between prompts, tables, status bars, and validation summaries. Badge labels, breadcrumb labels, bullet-list markers/text, checklist markers/labels/descriptions, timeline titles/descriptions, property-list labels/values, and shortcut-bar keys/descriptions can contain raw markup such as `[green]Ready[/]` or `[white on navy]Approval[/]`; rendering wraps by visible text and preserves parsed colors, RGB/hex metadata, and ANSI-capable text decorations. Callout border/title/content helpers plus Breadcrumb item/current/separator helpers preserve the same RGB/hex and decoration metadata through their renderers. `Breadcrumb.AddItems("Home", "Deploy")` accepts quick params labels for controller/view navigation hints, `Breadcrumb.AddItems(models, item => item.Label)` projects model-backed navigation trails, and `BulletList.AddItems("Build", "Test")` / `BulletListItem.AddItems("Lint", "Sign")` accept quick params labels, while `AddItems(IEnumerable<BulletListItem>)` accepts reusable nested item models and `Checklist.AddItems(item1, item2)` accepts prepared readiness items. Breadcrumbs, bullet lists, nested bullet items, and checklists also accept selector-based `AddItems(models, ...)` overloads, and item projections can use per-item style strings such as `Breadcrumb.AddItems(models, item => item.Label, item => item.StyleText)`, `BulletList.AddItems(models, item => item.Label, item => item.StyleText)`, or `Checklist.AddItems(models, item => item.Label, item => item.State, item => item.Description, item => item.StyleText)`, which keeps model-backed MVC views from hand-copying item loops into the render path. Those same breadcrumb, bullet-list, nested bullet-item, and checklist projections can return `Color` or `RgbColor` selectors when the model already carries palette state, with breadcrumb selectors preserving true-color metadata on capable terminals.
* **Composition builders** - Use `new Rows()`, `new Columns()`, `new Grid()`, `new Padder(...)`, `new Align(...)`, and `new Layout()` to compose reusable renderables into quick dashboard-style POC views. Their common color helpers accept `ConsoleColor`, reusable `Style` objects, Spectre-style style strings, `Color`, and `RgbColor` values, and item-level row, cell, column, and pane overloads can use the same palette vocabulary for shared view chrome. `Rows.AddRow(...)`, `Columns.AddColumn(...)`, `Grid.AddCell(...)`, `Layout.AddPane(...)`, `LayoutEntry.Add(...)`, `LayoutPane.Create(...)`, `LayoutPane.Update(...)`, `RowsItem.Create(...)`, and `GridCell.Create(...)` accept direct `Color` and `RgbColor` values for single-item dashboard sketches without `new Style(...)` wrappers. `Layout.TitleColor(...)` and `Layout.ContentColor(...)` also accept direct palette values so MVC-style view regions can share one model-owned palette. `Rows`, `Columns`, and `Grid` accept enumerable string, renderable, or mixed object collections, and the static `Consoul.Rows(...)`, `Consoul.Columns(...)`, `Consoul.Grid(...)`, `AnsiConsole.Rows(...)`, `AnsiConsole.Columns(...)`, and `AnsiConsole.Grid(...)` helpers use the same mixed-value path for procedural snippets. Aligned text, padded text, row text, column content, grid headers, grid cell text, layout pane titles, and layout pane content can contain raw markup and still wrap, collapse, pad, align, and render by visible text. `Rows.AddRows(...)`, `Columns.AddColumns(...)`, `Grid.AddRows(new[] { "API", "Ready" }, new[] { "Worker", "Queued" })`, and `Grid.AddCells(...)` accept text, renderable, and mixed object params as well as enumerables for pasted fluent snippets; selector-backed overloads such as `Rows.AddRows(models, item => item.Summary, item => item.StyleText)`, `Columns.AddColumns(models, item => item.Content, item => item.Header, item => item.StyleText)`, `Grid.AddCells(models, item => item.Value, item => item.Label, item => item.StyleText)`, and `Layout.AddPanes(models, item => item.Content, item => item.Title, item => item.StyleText)` project MVC view-model collections into reusable dashboard sections without hand-copying loop bodies into render methods. `new Layout(panes)` and `Layout.AddPanes(panes)` accept prepared `LayoutPane` collections, while selector-backed `Layout.AddPanes(...)` can return plain values, reusable renderables, render actions, configured `LayoutPane` instances, or nested `Layout` regions. `Layout.SplitColumns(renderable1, renderable2)` and `SplitRows(...)` accept reusable renderables directly for static dashboard composition, while named nested layouts keep mutable MVC-style view regions addressable. `RowsOptions.Expand = false`, `ColumnsOptions.Expand = false`, `new Rows(...).Collapse()`, or `new Columns(...).Collapse()` auto-size compact stacks and side-by-side groups. `Padder` supports `Padding` and `Expand` property initializers as well as fluent helpers, `Padding(horizontal, vertical)`, `Color(...)` wrappers, and `new Padding(horizontal, vertical)` for left/right and top/bottom spacing. `Align` supports `HorizontalAlignment`, `VerticalAlignment`, `Width(...)`, `Height(...)`, `Color(...)`, `LeftJustified()`, `RightJustified()`, and captured renderable content for pasted alignment snippets, with padded/aligned text and spacing preserving RGB/hex colors and ANSI decorations on capable terminals. Captured `Align` and `Padder` content stays plain text after capture, while direct text parses raw markup. `Grid.AddColumn(column => ...)` and `Grid.AddColumns(columns)` accept inline or generated column metadata; `GridColumn` supports width, no-wrap, left/right padding, `LeftAligned()`, `Centered()`, `RightAligned()`, and `Justify` alignment for pasted grid snippets. Grid rows and cells accept mixed plain values, strings, `GridCell` instances, reusable renderables, and selector-backed model cells, so model-backed property grids can grow into richer MVC-style dashboard sections without adapter lambdas. `GridOptions.Expand = false` or `new Grid().Collapse()` auto-sizes compact property grids, while `Expand()` restores terminal-width dashboard rendering. Named `Layout` panes can be updated with `layout["Status"].Update(...)`, and nested layouts accept `Size(...)`, `Ratio(...)`, `MinimumSize(...)`, `Visible(...)`, `Invisible()`, and selector-backed model regions so live MVC-style dashboard regions stay mutable without creating a separate view abstraction.
* **Model summaries** – Call `Consoul.PropertyList(model)` for read-only MVC-style state summaries. The model summary path uses the same `[Display]` names/order, `[DisplayName]`, `[Description]`, `[Browsable(false)]`, auto-generation opt-outs, and `[PropertyEditorIgnore]` metadata honored by the object editor, and `PropertyListEntry.Add(...)`, `AddModel(...)`, `AddKeyValues(...)`, `AddItems(...)`, and `AddItemsStyled(...)` accept style strings or reusable `Style` objects for quick POC emphasis while preserving RGB/hex colors and ANSI decorations for labels and values on capable terminals. Use `AddKeyValues(...)` for dictionary-backed settings or diagnostic state when creating a temporary model type would add noise, use `AddItems(item1, item2)` for prepared summary rows, and use selector-backed `AddItems(...)` / `AddItemsStyled(...)` when label/value rows already live in a view model; selector-backed `AddItems(...)` can also return `Color` or `RgbColor` label/value colors directly. Generated object-editor menus also group related edit actions with `[Display(GroupName = "...")]` or `[Category("...")]`. Call `Consoul.ValidationSummary(model)` before save actions to render DataAnnotations-style validation issues in a bounded callout, or create a `ValidationSummaryEntry` when that validation block should be captured or composed with other renderables; `AddIssue(...)` and `AddIssues(issue1, issue2)` append controller/business-rule failures to the model validation result. `ValidationSummaryOptions.InvalidColor(...)`, `IssueColor(...)`, `ValidColor(...)`, and `ValidMessageColor(...)` accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values while preserving RGB/hex colors and ANSI decorations on capable terminals for shared MVC view palettes.
* **Composable widgets** - `Consoul.Panel(...)`, `Consoul.Callout(...)`, `Consoul.Align(...)`, `Consoul.Pad(...)`, `Padder`, `Align`, `RowsEntry.Add(...)`, `GridEntry.Add(...)`, `ColumnsEntry.Add(...)`, and `LayoutEntry.Add(...)` accept render actions or `IConsoulRenderable` objects, so dashboards can frame, highlight, align, inset, stack, or place banners, markup messages, badges, panels, validation summaries, property lists, charts, tables, and text blocks without manual string capture. Use `Consoul.Renderable(() => { ... })` when a one-off render action should be passed to renderable-only APIs such as model-backed view sections, or `Consoul.Renderable(customRenderable)` when a migrated helper exposes only the Spectre-style `IRenderable` alias. Captured content is rendered as plain text inside the parent layout and inherits the panel, callout, alignment, padding, row, cell, column, or pane color you provide.
* **Scoped overrides** – Wrap demo screens, nested MVC-style views, or one-off sections in `using (Consoul.UseRenderOptions(() => { ... }))` to tweak prompt filtering, page sizes, write modes, colors, or blacklist behavior and restore the previous `RenderOptions` automatically.
* **Custom continue prompts** – Override `RenderOptions.ContinueMessage` to localise or rephrase the pause text used by `Consoul.Wait`. Adjust `RenderOptions.SubnoteScheme` if you want the pause prompt to use a unique colour without altering other helpers.
* **Integration with logging** – Register `ConsoulLoggerProvider` in a `HostApplicationBuilder` to emit structured log messages through Consoul’s formatting. Update `ConsoulLogger.LogLevelToColorMap` if you prefer different colours per severity.
* **Testing and export** - Use `Consoul.Capture(() => { ... }, width: 80)`, `Consoul.Capture(renderable, width: 80)`, `AnsiConsole.Capture(renderable, width: 80)`, or `console.Capture(renderable, width: 80)` when unit testing or exporting a scoped render helper without writing to the active terminal. Use `AnsiConsole.Record()`, `ExportText()`, and `ExportHtml()` when porting Spectre-style snippets that expect an ongoing transcript while output still reaches the active terminal. Both paths exercise the same internal console driver route as normal rendering, support cursor-aware helpers such as live displays, and avoid invalid console-handle failures in CI. Use `Console.SetIn` only for code that reads directly from the BCL console rather than through Consoul prompts or routines.
