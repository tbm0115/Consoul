# Core console APIs

## When to use this
Start here whenever you need structured console output or simple input helpers without adopting the view or routine systems. These APIs sit closest to `System.Console` and can be introduced incrementally into existing apps.

## Key types
* `Consoul.Capture` - Records Consoul render output to plain text using a virtual console width, without writing to the active terminal.
* `Consoul.Terminal` / `AnsiConsole.Profile` / `TerminalProfile` - Reports the active terminal width, height, redirection state, and virtual-driver status for responsive layout decisions.
* `IConsoulRenderable` - Shared contract for reusable visual components such as panels, grids, rows, charts, tables, text blocks, action-backed sections, and other entry objects.
* `IRenderable` - Spectre-style alias for reusable visual components; Consoul renderables implement both contracts, and custom `IRenderable` snippets can be adapted with `Consoul.Renderable(...)`.
* `AnsiConsole` - Spectre-style entry points that delegate to Consoul while preserving result-first prompt outcomes, including `IPrompt<TResult>`, `PromptAsync`, `AskAsync`, and `ConfirmAsync` helpers for async view actions.
* `IAnsiConsole` / `AnsiConsole.Console` - Injectable facade for services and reusable POC components that should not call static APIs directly. `AnsiConsole.Console` can also be assigned in tests; static facade helpers route through the assigned facade until the previous value is restored. Additive extension methods provide primitive and formatted `Write`/`WriteLine` overloads without expanding the core interface.
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
    .Color("aqua"));

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

Consoul.Layout(layout =>
{
    layout.Add("Package prepared\nApprovals pending", "Pipeline", ConsoleColor.Cyan);
    layout.Add("Release window: Friday\nOwner: Platform", "Operations", ConsoleColor.Yellow);
}, new LayoutOptions { Columns = 2, MinimumPaneWidth = 18 });

var dashboard = new Layout("Dashboard")
    .SplitRows(
        new Layout("Header").Size(3),
        new Layout("Body").SplitColumns(
            new Layout("Status").Ratio(2).MinimumSize(18),
            new Layout("Metrics").Ratio(1),
            new Layout("Debug").Invisible()));

dashboard["Header"].Update(new Panel("POC deployment view").Header("Dashboard"));
dashboard["Status"].Update(new Table()
    .HideFooter()
    .AddColumns("Service", "State")
    .AddRow("API", "Ready"));
dashboard["Metrics"].Update("Jobs: 2");
AnsiConsole.Write(dashboard);

Consoul.Columns(columns =>
{
    columns.Add("Prepare package and verify approvals.", "Plan", ConsoleColor.Cyan);
    columns.Add("Run deployment after the approval gate clears.", "Next", ConsoleColor.Yellow);
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

AnsiConsole.Write(new Padder(new Panel("Ready").Header("Status"))
{
    Padding = new Padding(2, 1),
    Expand = true
}.Color(RgbColor.Green));

AnsiConsole.Write(new Align(new Panel("Centered summary").Header("Overview"), HorizontalAlignment.Center)
    .Width(48)
    .Color(Color.Aqua));

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
* **Inline markup** – `Consoul.MarkupLine` supports `ConsoleColor` names, common aliases such as `[aqua]`, named schemes such as `[prompt]` and `[invalid]`, nearest-match hex colors such as `[#22c55e]`, background colors like `[white on blue]`, and doubled brackets for literals. Common Spectre-style modifiers such as `bold`, `dim`, `italic`, `underline`, `strike`, and `link` are accepted for portability; link tags render as OSC-8 hyperlinks when `AnsiConsole.Profile.Capabilities.Links` is enabled, while unsupported visual decorations render with the current color scheme. Known emoji shortcodes such as `:check_mark:`, `:x:`, `:warning:`, and `:rocket:` are replaced before color tags are rendered, and `Emoji.Replace(...)`, `Emoji.Get(...)`, and `Emoji.Known` expose the same mapping for reusable view helpers. Both `[/]` and named close tags such as `[/red]` close the current style. Use `MarkupLineInterpolated(...)`, `Markup.FromInterpolated(...)`, `Markup.FromFormat(...)`, `EscapeMarkup(...)`, `Markup.Escape(...)`, `RemoveMarkup(...)`, or `Markup.Remove(...)` when inserting model/user values or stripping tags for logs and tests. Unknown tags and unknown emoji aliases render as plain text so exploratory views fail softly. Use `MarkupEntry` or the migration-friendly `new Markup("...")` alias when the same markup should be captured, placed in a panel/layout, or returned from a model-backed view. Use `Style.Parse(...)`, `Style.ParseColor(...)`, `Style.Plain.WithForeground(...)`, `Color.Red`, `Color.FromHex(...)`, `Color.Rgb(...)`, `RgbColor.Red`, or `RgbColor.FromHex(...)` when builder code needs the same string/RGB parsing or immutable style composition outside markup; parsed styles preserve decoration metadata even though the default 16-color renderer only displays foreground/background colors. Common builders such as `Text`, `Panel`, `Table`, `Rule`, `Tree`, `BarChart`, `BreakdownChart`, `Metrics`, `Sparkline`, `Timeline`, `Badge`, `Callout`, `Breadcrumb`, `BulletList`, `Checklist`, `ShortcutBar`, `Calendar`, `Rows`, `Columns`, `Grid`, `Layout`, `Align`, `TextPath`, `Canvas`, and `PropertyListEntry` accept reusable `Style` objects directly in their color/style helper methods.
* **Structured diagnostics** – Use `Consoul.WriteException` inside view actions or `catch` blocks to keep error type, message, inner exception and stack trace content wrapped inside the current terminal width. Spectre-style snippets can pass `ExceptionFormats.NoStackTrace`, `ShortenPaths`, `ShortenTypes`, `ShortenMethods`, `ShortenEverything`, or `ShowLinks`; these flags adapt to Consoul's existing renderer. Use `ExceptionSettings` and `ExceptionStyle` when a migrated snippet wants named exception/message/stack styles while keeping MVC-style diagnostics bounded and testable.
* **Renderable aliases** – Use `AnsiConsole.Write(renderable)` or `AnsiConsole.WriteLine(renderable)` for reusable components, or `AnsiConsole.Render(renderable)` when adapting older Spectre-style snippets. Injected `IAnsiConsole` values also get `console.Write(renderable)`, `console.WriteLine(renderable)`, `console.Render(renderable)`, and `console.Capture(renderable, width)` through facade extension methods when a helper returns the Spectre-style `IRenderable` alias. All paths render the same Consoul `IConsoulRenderable` objects, including markup, panels, tables, rows, grids, layouts, and MVC-style view sections; use `Consoul.Renderable(IRenderable)` to adapt a custom alias-only component for composition. `Rows.AddRows(...)`, `Columns.AddColumns(...)`, and `Grid.AddCells(...)` accept text, renderable, and mixed object params as well as enumerables, while `Layout.AddPanes(...)` projects model collections into text, renderable, action, pane, or nested-layout regions, so copied fluent layout snippets do not need array/list wrappers or adapter loops.
* **Themes** – Call `Consoul.ApplyTheme(ConsoulTheme.Modern)` for an application-wide look, or wrap a section in `using (Consoul.UseTheme(ConsoulTheme.HighContrast))` to restore the previous colors automatically.
* **Table polish** – Set `TableRenderOptions.Lines.BorderStyle` to `Rounded`, `Square`, `Ascii`, `Heavy`, or `Double` when a model-backed POC table needs stronger visual framing than the legacy simple separators. The `Table` builder also accepts `Border(TableBorder.Rounded)`, `Border(TableBorder.None)`, `DoubleBorder()`, `NoBorder()`, `LeftJustified()`, `RightJustified()`, `AddColumn("Name", column => column.RightAligned())`, synchronized `table.Columns[index]` metadata, selector-backed `AddRows(items, item => item.Value, ...)`, and renderable row cells for pasted manual snippets. Color helpers accept `ConsoleColor` values, reusable `Style` objects, style strings such as `"grey"`, `"yellow"`, `"#22c55e"`, and `"white on navy"`, Spectre-style values such as `Color.Red` and `Color.FromHex(...)`, RGB values such as `RgbColor.Red` and `RgbColor.FromHex(...)`, and aliases from `ConsoulLibrary.Color.Colors`.
* **Rule polish** - Use `new Rule("...", Justify.Left)`, `new Rule("...", ConsoleColor.Cyan, Justify.Right)`, `new Rule("...", Color.Aqua, Justify.Right)`, `new Rule("...").Justify(Justify.Left).RuleStyle("grey").TitleStyle("cyan")`, `new Rule("...").RuleColor(Color.Green).TitleColor(RgbColor.Aqua)`, or object-initializer snippets such as `new Rule { Title = "Deploy", Justification = Justify.Left, Style = new Style(ConsoleColor.DarkGray) }`, when adapting Spectre-style rule snippets while keeping the existing responsive rule renderer. Rule color helpers accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values. Rule titles can contain raw markup such as `[cyan]Checks[/]`; Consoul measures the visible title text and preserves the parsed title colors.
* **Panel polish** – Set `PanelOptions.BorderStyle` to `Rounded`, `Square`, `Ascii`, `Heavy`, `Double`, or `None` when a dashboard section needs a clearer frame or padding without visible borders. Use `TitleAlignment`, `Footer`, and `FooterAlignment` to render command hints or status text directly in the panel frame or borderless title/footer lines. Use `new Panel("...").Header("Status", Color.Aqua, Justify.Center).Footer("Esc", RgbColor.Yellow, Justify.Right).BorderColor(Color.Green).ContentColor(RgbColor.White).Padding(1, 0, 1, 0).Border(BoxBorder.Rounded).Width(...)`, `new Panel("...").Padding(2, 1)`, `new Panel("...").NoBorder()`, `new Panel("...").Collapse()`, `new Panel(renderable)`, or `new Panel(() => { ... })` when adapting Spectre-style examples without leaving Consoul's reusable renderable path. Raw markup in panel content, headers, and footers is measured by visible text and rendered with parsed foreground/background colors. `BoxBorder.None`, `BoxBorder.Safe`, `BoxBorder.Ascii2`, `BoxBorder.AsciiDoubleHead`, `BoxBorder.Markdown`, `BoxBorder.Simple`, `BoxBorder.SimpleHeavy`, `BoxBorder.Minimal`, `BoxBorder.MinimalHeavyHead`, `BoxBorder.MinimalDoubleHead`, `BoxBorder.HeavyEdge`, `BoxBorder.HeavyHead`, `BoxBorder.DoubleEdge`, `NoBorder()`, `Collapse()`, `Expand()`, `PanelOptions.Expand`, `PanelHeader`, `Footer(new PanelHeader(...))`, and inline `Header(...)` / `Footer(...)` overloads for `ConsoleColor`, `Style`, style strings, `Color`, and `RgbColor` are also accepted for pasted snippets and map to the closest existing panel renderer. Renderable/action panels capture their content at render time so fluent width and padding changes shape the captured inner layout.
* **Exception polish** - Use raw markup in exception panel titles, context messages, and exception messages when a quick diagnostic needs status color, such as `[yellow]Preview failed[/]` or `[cyan]Deploying[/] [[prod]]`. Generated labels and stack trace frames stay literal, so method names and file paths are not interpreted as markup.
* **Tree polish** - Use raw markup labels such as `new Tree("[cyan]Deployment[/]")`, `root.Add("[green]API[/]")`, or `root.Add("[white on navy]Approval[/]")` when a quick MVC-style diagnostic tree needs color without separate label objects. Tree rendering measures and wraps the visible label text, preserves parsed foreground/background colors, and still keeps unknown tags or doubled brackets literal.
* **Visual builders** - Use `new Text("...")`, `new Paragraph().Append(...)`, `new TextPath("...")`, `new FigletText("...")`, `new Tree("...")`, `new BarChart().AddItem(...)`, `new BreakdownChart().AddItem(...)`, `new Metrics().AddSuccess(...)`, `new Sparkline(values).Label(...)`, `new Timeline().AddComplete(...)`, `new ShortcutBar().AddShortcut(...)`, `new Calendar(...)`, and `new CanvasImage(...)` for migration-friendly snippets that still render through Consoul's responsive visual components. `Text` exposes `Empty`, `NewLine`, `Length`, `Lines`, `Justification`, `Overflow`, direct color/style constructors such as `new Text("Ready", ConsoleColor.Green)`, `new Text("Deploy", Color.Aqua)`, `new Text("Trend", RgbColor.Green)`, and `new Text("Blocked", "white on navy")`, raw markup rendering for snippets such as `new Text("Status: [green]Ready[/]")`, and the object-initializer-friendly `Style` property, while `Paragraph` preserves per-append styles, supports direct `Color`/`RgbColor` constructor and append overloads, `AppendLine(...)`, `AppendStyled(...)`, `AppendLineStyled(...)`, `AppendMarkup(...)`, and interpolated markup append helpers, and emits `Link` metadata as OSC-8 hyperlinks when the active terminal profile supports links. `TextPath` supports root, directory, stem, leaf, extension, separator, ellipsis, width helpers, and segment `Color(...)`/`Style(...)` aliases for path-heavy diagnostics. `FigletText` supports `FullSize()`, `Fitted()`, `Smushed()`, `LayoutMode(...)`, and `Pad(...)` over Consoul's dependency-free block glyph renderer, so splash titles can be compact without loading external font files. `Canvas` remains useful for MVC-style diagrams with labels, lines, and boxes, and accepts `Set(...)`, `Text(...)`, `Line(...)`, `Rectangle(...)`, `FillRectangle(...)`, `SetPixel(...)`, `DefaultColor(...)`, `MaxWidth(...)`, and `Scale(false)` for compact Spectre-style sketches. `CanvasImage` accepts in-memory pixels or `ConsoleColor` matrices and renders terminal half-block thumbnails with `PixelWidth`, `MaxWidth`, `MaxHeight`, and resampler helpers while keeping image-file decoding out of the core dependency graph. Tree builders accept `Guide(TreeGuide.Ascii)`, `TreeGuide.Line`, `TreeGuide.DoubleLine`, `TreeGuide.BoldLine`, `AddNodes(...)`, mixed scalar/existing-node/renderable values, selector-backed style strings, `TreeNode.Collapse()`, tree-level `Collapse()`, raw markup labels, and captured `IConsoulRenderable` labels such as `Markup`, `Text`, `Panel`, or small tables. Bar and breakdown charts accept selector-based `AddItems(models, labelSelector, valueSelector, colorSelector)` overloads and raw markup in chart titles and item labels, so MVC-style POC views can chart model collections without hand-written item loops. Metrics, timelines, shortcut bars, and calendars accept selector-based `AddMetrics(models, ...)`, `AddSteps(models, ...)`, `AddShortcuts(models, ...)`, and `AddCalendarEvents(models, ...)` overloads; metrics can use `AddMetricsStyled(...)`, timelines can use `AddStepsStyled(...)`, calendars can use `AddCalendarEventsStyled(...)`, and shortcut bars can use selector-backed style overloads so label/value/note/title/marker/date/key/description palette choices can stay in model-backed dashboard state until render time. Metric labels/values/units/notes and sparkline labels can contain raw markup and still wrap, pad, and truncate by visible text. Sparkline `LabelColor(...)`, `LineColor(...)`, and `RangeColor(...)` helpers accept `ConsoleColor` values, reusable `Style` objects, `Color` and `RgbColor` values, and style strings. Metrics and timelines also accept selectors returning `Color` or `RgbColor` directly, which keeps health-state palette values in the view model rather than in adapter loops. Calendar builders accept mutable `Year`, `Month`, and `Day` values, `CalendarEvents`, `CalendarEvent`, `AddCalendarEvent(...)` overloads, `HeaderStyle(...)`, `HighlightStyle(...)`, `HeaderColor(...)`, `WeekdayColor(...)`, `DayColor(...)`, `AdjacentDayColor(...)`, `TodayColor(...)`, and `MarkedDayColor(...)` for pasted date/event snippets. Common builder color helpers, item factories, per-item `Add...` overloads, and canvas drawing methods accept `ConsoleColor` values, reusable `Style` objects, `Color` and `RgbColor` values, and style strings such as `"grey"`, `"aqua"`, `"#22c55e"`, and `"white on navy"`.
* **Metric palette helpers** - `Metrics.LabelColor(...)`, `ValueColor(...)`, `NoteColor(...)`, `InfoColor(...)`, `SuccessColor(...)`, `WarningColor(...)`, and `ErrorColor(...)` accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values, so quick MVC-style dashboard metrics can share the same palette constants as tables, prompts, and status widgets.
* **Model-backed visual projections** - Manual tables, breadcrumbs, rows, columns, grids, layouts, tree nodes, and sparklines also accept selector-based model projections: `Table.AddRows(models, item => item.Name, item => item.Status)`, `Breadcrumb.AddItems(models, item => item.Label)`, `Rows.AddRows(models, item => item.Summary)`, `Columns.AddColumns(models, item => item.Panel, item => item.Title)`, `Grid.AddCells(models, item => item.Value, item => item.Label)`, `Layout.AddPanes(models, item => item.Content, item => item.Title)`, `Tree.AddNodes(models, item => item.Name, item => item.Color)`, `TreeNode.AddNodes(models, ...)`, and `Sparkline.AddValues(models, item => item.Value)`. Breadcrumbs, rows, columns, grids, layouts, tree nodes, bar charts, and breakdown charts can also project per-item style strings such as `"green"`, `"#22c55e"`, or `"white on navy"` directly from model data. Breadcrumbs, rows, columns, grids, layouts, bullet lists, nested bullet items, checklists, metrics, timelines, calendars, property lists, shortcut bars, trees, tree nodes, bar charts, and breakdown charts also accept selectors returning `Color` or `RgbColor` directly, so palette values can stay in MVC-style view-model state. Use these alongside chart, metrics, timeline, shortcut bar, calendar, bullet-list, and checklist projections so MVC-style view sections can render domain collections without manual adapter loops.
* **Alignment vocabulary** - Use `Justify.Left`, `Justify.Center`, and `Justify.Right` with text, figlet text, rules, panel headers/footers, table titles/captions, and manual table columns when adapting Spectre-style examples. The existing `TextBlockAlignment`, `RuleTitleAlignment`, and `TableCellAlignment` enums remain available for native Consoul code.
* **Facade helpers** - `AnsiConsole` forwards Consoul's helper-style calls for badges, headlines, figlet text, calendars, callouts, validation summaries, timelines, lists, grids, canvases, canvas images, rows, layouts, padding, alignment, text blocks, paths, breadcrumbs, shortcut bars, property lists, metrics, charts, sparklines, status scopes, progress contexts, live displays, capture, and recording. Use these for pasted procedural snippets, then move reusable sections into `IConsoulRenderable` components or `DynamicView<TModel>` sections as the POC matures. Dynamic view options accept both quick `Func<Task>` / `Func<TModel, Task>` callbacks and cancellation-aware async callbacks, keeping first-run MVC-style POC actions terse without hiding cancellation support.
* **Injectable facade** - Depend on `IAnsiConsole` and pass `AnsiConsole.Console` when reusable services, controller-style actions, or POC screen components should be easy to unit test. The interface tracks the static `AnsiConsole` helper method names, including output, prompts, panels, rules, tables, trees, lists, grids, layouts, calendars, canvases, canvas images, validation/property summaries, metrics, charts, status/progress/live scopes, capture, and recording while delegating to the active Consoul console driver, so virtual drivers and capture still work without a real terminal handle. Use `AnsiConsole.Create(new AnsiConsoleSettings { Out = new AnsiConsoleOutput(writer, width: 80), In = new StringReader("Production\n"), Ansi = AnsiSupport.Yes, ColorSystem = ColorSystemSupport.Standard, Interactive = InteractionSupport.No })` when a Spectre-style test or embedded host should render through a specific `TextWriter`, read prompt input from a specific `TextReader`, and expose deterministic `Profile.Capabilities`. For static-facade test setup, assign `AnsiConsole.Console` to a custom facade inside a `try`/`finally` block and restore the previous value when the test ends.
* **Status and list builders** - Use `new Badge("...")`, `new Callout("...")`, `new Breadcrumb()`, `new BulletList()`, and `new Checklist()` for compact dashboard state, navigation hints, notes, and readiness lists. Their marker, label, content, border, separator, key, disabled color helpers, and per-item factories accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values, so MVC-style POC screens can share palette values between prompts, tables, status bars, and validation summaries. Badge labels, breadcrumb labels, bullet-list item text, checklist labels/descriptions, timeline titles/descriptions, property-list labels/values, and shortcut-bar keys/descriptions can contain raw markup such as `[green]Ready[/]` or `[white on navy]Approval[/]`; rendering wraps by visible text and preserves parsed colors. `Breadcrumb.AddItems(models, item => item.Label)`, `BulletList.AddItems("Build", "Test")`, and `BulletListItem.AddItems("Lint", "Sign")` accept quick params labels, while `AddItems(IEnumerable<BulletListItem>)` accepts reusable nested item models. Breadcrumbs, bullet lists, nested bullet items, and checklists also accept selector-based `AddItems(models, ...)` overloads, and item projections can use per-item style strings such as `Breadcrumb.AddItems(models, item => item.Label, item => item.StyleText)`, `BulletList.AddItems(models, item => item.Label, item => item.StyleText)`, or `Checklist.AddItems(models, item => item.Label, item => item.State, item => item.Description, item => item.StyleText)`, which keeps model-backed MVC views from hand-copying item loops into the render path. Those same breadcrumb, bullet-list, nested bullet-item, and checklist projections can return `Color` or `RgbColor` selectors when the model already carries palette state.
* **Composition builders** - Use `new Rows()`, `new Columns()`, `new Grid()`, `new Padder(...)`, `new Align(...)`, and `new Layout()` to compose reusable renderables into quick dashboard-style POC views. Their common color helpers accept `ConsoleColor`, reusable `Style` objects, Spectre-style style strings, `Color`, and `RgbColor` values, and item-level row, cell, column, and pane overloads can use the same palette vocabulary for shared view chrome. `Rows`, `Columns`, and `Grid` accept enumerable string, renderable, or mixed object collections, and the static `Consoul.Rows(...)`, `Consoul.Columns(...)`, `Consoul.Grid(...)`, `AnsiConsole.Rows(...)`, `AnsiConsole.Columns(...)`, and `AnsiConsole.Grid(...)` helpers use the same mixed-value path for procedural snippets. Aligned text, padded text, row text, column content, grid headers, grid cell text, layout pane titles, and layout pane content can contain raw markup and still wrap, collapse, pad, align, and render by visible text. `Rows.AddRows(...)`, `Columns.AddColumns(...)`, and `Grid.AddCells(...)` accept text, renderable, and mixed object params as well as enumerables for pasted fluent snippets; selector-backed overloads such as `Rows.AddRows(models, item => item.Summary, item => item.StyleText)`, `Columns.AddColumns(models, item => item.Content, item => item.Header, item => item.StyleText)`, `Grid.AddCells(models, item => item.Value, item => item.Label, item => item.StyleText)`, and `Layout.AddPanes(models, item => item.Content, item => item.Title, item => item.StyleText)` project MVC view-model collections into reusable dashboard sections without hand-copying loop bodies into render methods. `Layout.AddPanes(...)` selectors can return plain values, reusable renderables, render actions, configured `LayoutPane` instances, or nested `Layout` regions. `RowsOptions.Expand = false`, `ColumnsOptions.Expand = false`, `new Rows(...).Collapse()`, or `new Columns(...).Collapse()` auto-size compact stacks and side-by-side groups. `Padder` supports `Padding` and `Expand` property initializers as well as fluent helpers, `Padding(horizontal, vertical)`, `Color(...)` wrappers, and `new Padding(horizontal, vertical)` for left/right and top/bottom spacing. `Align` supports `HorizontalAlignment`, `VerticalAlignment`, `Width(...)`, `Height(...)`, `Color(...)`, `LeftJustified()`, `RightJustified()`, and captured renderable content for pasted alignment snippets. Captured `Align` and `Padder` content stays plain text after capture, while direct text parses raw markup. `GridColumn` supports width, no-wrap, left/right padding, `LeftAligned()`, `Centered()`, `RightAligned()`, and `Justify` alignment for pasted grid snippets. Grid rows and cells accept mixed plain values, strings, `GridCell` instances, reusable renderables, and selector-backed model cells, so model-backed property grids can grow into richer MVC-style dashboard sections without adapter lambdas. `GridOptions.Expand = false` or `new Grid().Collapse()` auto-sizes compact property grids, while `Expand()` restores terminal-width dashboard rendering. Named `Layout` panes can be updated with `layout["Status"].Update(...)`, and nested layouts accept `Size(...)`, `Ratio(...)`, `MinimumSize(...)`, `Visible(...)`, `Invisible()`, and selector-backed model regions so live MVC-style dashboard regions stay mutable without creating a separate view abstraction.
* **Model summaries** – Call `Consoul.PropertyList(model)` for read-only MVC-style state summaries. The model summary path uses the same `[Display]` names/order, `[DisplayName]`, `[Description]`, `[Browsable(false)]`, auto-generation opt-outs, and `[PropertyEditorIgnore]` metadata honored by the object editor, and `PropertyListEntry.Add(...)`, `AddModel(...)`, `AddKeyValues(...)`, `AddItems(...)`, and `AddItemsStyled(...)` accept style strings or reusable `Style` objects for quick POC emphasis. Use `AddKeyValues(...)` for dictionary-backed settings or diagnostic state when creating a temporary model type would add noise, and use selector-backed `AddItems(...)` / `AddItemsStyled(...)` when label/value rows already live in a view model. Generated object-editor menus also group related edit actions with `[Display(GroupName = "...")]` or `[Category("...")]`. Call `Consoul.ValidationSummary(model)` before save actions to render DataAnnotations-style validation issues in a bounded callout, or create a `ValidationSummaryEntry` when that validation block should be captured or composed with other renderables. `ValidationSummaryOptions.InvalidColor(...)`, `IssueColor(...)`, `ValidColor(...)`, and `ValidMessageColor(...)` accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values for shared MVC view palettes.
* **Composable widgets** - `Consoul.Panel(...)`, `Consoul.Callout(...)`, `Consoul.Align(...)`, `Consoul.Pad(...)`, `Padder`, `Align`, `RowsEntry.Add(...)`, `GridEntry.Add(...)`, `ColumnsEntry.Add(...)`, and `LayoutEntry.Add(...)` accept render actions or `IConsoulRenderable` objects, so dashboards can frame, highlight, align, inset, stack, or place banners, markup messages, badges, panels, validation summaries, property lists, charts, tables, and text blocks without manual string capture. Use `Consoul.Renderable(() => { ... })` when a one-off render action should be passed to renderable-only APIs such as model-backed view sections, or `Consoul.Renderable(customRenderable)` when a migrated helper exposes only the Spectre-style `IRenderable` alias. Captured content is rendered as plain text inside the parent layout and inherits the panel, callout, alignment, padding, row, cell, column, or pane color you provide.
* **Scoped overrides** – Wrap demo screens, nested MVC-style views, or one-off sections in `using (Consoul.UseRenderOptions(() => { ... }))` to tweak prompt filtering, page sizes, write modes, colors, or blacklist behavior and restore the previous `RenderOptions` automatically.
* **Custom continue prompts** – Override `RenderOptions.ContinueMessage` to localise or rephrase the pause text used by `Consoul.Wait`. Adjust `RenderOptions.SubnoteScheme` if you want the pause prompt to use a unique colour without altering other helpers.
* **Integration with logging** – Register `ConsoulLoggerProvider` in a `HostApplicationBuilder` to emit structured log messages through Consoul’s formatting. Update `ConsoulLogger.LogLevelToColorMap` if you prefer different colours per severity.
* **Testing and export** - Use `Consoul.Capture(() => { ... }, width: 80)`, `Consoul.Capture(renderable, width: 80)`, `AnsiConsole.Capture(renderable, width: 80)`, or `console.Capture(renderable, width: 80)` when unit testing or exporting a scoped render helper without writing to the active terminal. Use `AnsiConsole.Record()`, `ExportText()`, and `ExportHtml()` when porting Spectre-style snippets that expect an ongoing transcript while output still reaches the active terminal. Both paths exercise the same internal console driver route as normal rendering, support cursor-aware helpers such as live displays, and avoid invalid console-handle failures in CI. Use `Console.SetIn` only for code that reads directly from the BCL console rather than through Consoul prompts or routines.
