# Tables

[Documentation index](index.md) · [Install the 3.0 preview](nuget.md) · [Migration guide](migration-3.0.md)

## When to use this
Tables help present structured data (logs, metrics, inventories) with aligned columns and optional theming. They are perfect for dashboards and admin tools where readability matters.

## Key types
* `Table` - Spectre-style fluent builder for manual tables, backed by `TableView`.
* `TableTitle` / `TableCaption` - Reusable title and caption metadata for manual table snippets. Titles and captions can start from strings or reusable renderables such as `Markup`, `Text`, or captured render actions. `TableTitle` is also accepted by `Caption(...)` for Spectre-shaped snippets.
* `TableColumn` - Configures manual table headers, preferred widths, alignment, cell padding, no-wrap intent, and column footers.
* `TableView` – Defines headers, column widths, and rows for rendering tabular data.
* `DynamicTableView<T>` – Projects model objects into table rows using property selectors or computed columns.
* `IConsoulRenderable` – Implemented by both table types so table output can be captured or composed inside panels, rows, grids, columns, and layouts.
* `TableColumnAttribute` – Controls inferred model-column labels, ordering, alignment, and visibility.
* `TableRenderOptions` – Controls padding, alignment, title/caption text, border characters, and named border presets.
* `TableSelectionResult` – Returned by interactive table selection, exposing selected row indexes and cancellation state.
* `TableSelectionResult<T>` – Returned by model-backed table selection, exposing selected row indexes and model objects.
* `Consoul.Table(...)` – Quick helper for rendering model collections inline.
* `RenderOptions` – Supplies fallback colours for headers and borders.

## Minimal example
For proof-of-concept views, render model objects directly:

```csharp
var services = new[]
{
    new ServiceStatus("Auth", "OK", 120),
    new ServiceStatus("Payments", "Degraded", 450)
};

Consoul.Table(services, service => service.Name, service => service.Status, service => service.DurationMs);
```

For MVC-style POC views, annotate the model and omit selectors:

```csharp
public sealed class ServiceStatus
{
    [TableColumn("Service", Order = 1)]
    public string Name { get; set; }

    [TableColumn("State", Order = 2)]
    public string Status { get; set; }

    [TableColumn("Latency", Order = 3, Alignment = TableCellAlignment.Right, Format = "{0} ms")]
    public int DurationMs { get; set; }

    [TableColumn(Visible = false)]
    public string InternalId { get; set; }
}

Consoul.Table(services);
```

Use `DynamicTableView<T>` when you need custom labels or computed columns:

```csharp
var options = new TableRenderOptions
{
    ShowFooterHelp = false,
    Title = "Service Health",
    Caption = "Filtered to degraded services.",
    TitleAlignment = TextBlockAlignment.Center,
    CaptionAlignment = TextBlockAlignment.Right
};
options.Lines.BorderStyle = TableBorderStyle.Rounded;

var table = new DynamicTableView<ServiceStatus>(options)
{
    ShowSummary = true,
    ShowEmptyMessage = true,
    EmptyMessage = "No services match the current table view."
};
table.AddHeader(service => service.Name, "Service");
table.AddColumn("Latency", service => service.DurationMs, "{0} ms", TableCellAlignment.Right);
table.AddRows(services);
table.Filter(service => service.Status != "OK")
    .Search("degraded")
    .SortBy(service => service.DurationMs, descending: true)
    .Page(1, 5);
table.RenderInline();

Consoul.Panel(table, title: "Filtered services", width: 72, captureWidth: 64);

TableSelectionResult<ServiceStatus> selected = table.Select("Open service");
if (selected.HasSelection)
{
    ServiceStatus service = selected.SelectedItem;
    Consoul.Write($"Opening {service.Name}", ConsoleColor.Cyan);
}
```

For Spectre-style manual snippets, use the fluent `Table` builder. It renders through the same responsive `TableView` engine and can be passed to `AnsiConsole.Write(...)`, `Consoul.Panel(...)`, `RowsEntry`, `GridEntry`, `ColumnsEntry`, or `Consoul.Capture(...)`. Add `using ConsoulLibrary.Color;` when you want color aliases such as `Color.Grey`, `Color.Aqua`, `Colors.Grey`, or RGB-shaped values such as `Color.FromHex(...)`, `RgbColor.Red`, and `RgbColor.Rgb(...)`.

```csharp
var manual = new Table()
    .Title(new TableTitle(new Markup("[cyan]Service Status[/]"), Justify.Left))
    .Caption(new TableTitle(new Text("Updated from the latest poll."), "grey", Justify.Right))
    .Width(56)
    .Border(TableBorder.Rounded)
    .BorderColor(Colors.Grey)
    .HeaderColor("yellow")
    .FooterColor("grey")
    .HideFooter()
    .AddColumn(new TableColumn("Service").Width(16).Padding(2, 1))
    .AddColumn(new Markup("[yellow]Status[/]"), Justify.Center)
    .AddColumn(new TableColumn("Duration").RightAligned().Width(12).PadRight(2).Footer("570 ms"))
    .AddRow("Auth", "OK", "120 ms")
    .AddRow("Payments", "Degraded", "450 ms")
    .AddRow("Search", "Failed", "-");

manual.Columns[2].Footer = new Text("570 ms", new Style(foreground: Colors.Yellow, decoration: Decoration.Bold));

AnsiConsole.Write(manual);
```

Raw Spectre-style markup strings in headers and cells are treated as display markup for pasted POC snippets, so tags such as `[green]Ready[/]` do not leak into the table or distort responsive column widths. Foreground/background colors, RGB/hex metadata, and ANSI-capable text decorations are preserved while layout still measures the stripped visible text. Use `Markup`, `Text`, or another renderable cell value when you want reusable styled content. `AddColumn(new Markup("[yellow]State[/]"))`, `AddColumns(renderableHeaders)`, and `AddOptionalColumn(renderableHeader, values, Justify.Right)` capture renderable headers directly when a table header is already produced by a reusable visual.

Manual tables can also project model rows when an MVC-style view needs a custom table shell but the row data should stay in the view model:

```csharp
manual.AddRows(
    services,
    service => new Text(service.Name, new Style(foreground: service.IsHealthy ? Colors.Green : Colors.Yellow)),
    service => new Markup(service.Status == "OK" ? "[green]OK[/]" : "[yellow]Degraded[/]"),
    service => service.DurationMs);
```

Manual table cells can also accept reusable renderables. Consoul captures the rendered content into the table cell, which keeps mixed-content snippets portable while still using the responsive `TableView` layout:

```csharp
var details = new Table()
    .Border(TableBorder.None)
    .HideFooter()
    .HideHeaders()
    .AddColumns("Metric", "Value")
    .AddRow("CPU", "95%");

var mixed = new Table()
    .RoundedBorder()
    .AddColumn("Type", column => column.Width(10))
    .AddColumn("Example")
    .AddRow(new Text("Markup"), new Markup("[green]Success[/] :check_mark:"))
    .AddRow(new Text("Panel"), new Panel("[yellow]Warning[/]").Border(BoxBorder.Ascii))
    .AddRow(new Text("Nested"), details)
    .AddRow(
        new Text("Text"),
        new Text("Styled text", new Style(foreground: Colors.Aqua, decoration: Decoration.Italic)));

AnsiConsole.Write(mixed);
```

For full manual control, create a `TableView`:

```csharp
var table = new TableView();
table.AddHeader("Service");
table.AddHeader("Status");
table.AddHeader("Duration", TableCellAlignment.Right);
table.AddRow(new[] { "Auth", "OK", "120 ms" });
table.AddRow(new[] { "Payments", "Degraded", "450 ms" });
table.AddRow(new[] { "Search", "Failed", "–" });

table.TableRenderOptions.HeaderScheme = new ColorScheme
{
    Color = ConsoleColor.White,
    BackgroundColor = ConsoleColor.DarkBlue
};
table.TableRenderOptions.WhitespaceCharacter = '·';
table.TableRenderOptions.TableWidthPercentage = 0.9m;
table.TableRenderOptions.Lines.BorderStyle = TableBorderStyle.Square;
table.TableRenderOptions.Title = "Service Status";
table.TableRenderOptions.Caption = "Updated from the latest poll.";

table.Render();
```

Hide headers when a table is acting as a compact dashboard widget but should still use model metadata for column setup:

```csharp
var compactOptions = new TableRenderOptions
{
    ShowFooterHelp = false,
    ShowHeaders = false,
    Title = "Current Target"
};
compactOptions.Lines.BorderStyle = TableBorderStyle.Rounded;

Consoul.Table(services, compactOptions, service => service.Name, service => service.Status);
```

Show row numbers when users will type row indexes during table selection:

```csharp
var selectionOptions = new TableRenderOptions
{
    ShowRowNumbers = true
};

DynamicTableView<ServiceStatus> selectable = Consoul.Table(
    services,
    selectionOptions,
    service => service.Name,
    service => service.Status);

TableSelectionResult<ServiceStatus> result = selectable.Select("Open service");
```

## Advanced tips
* **Window width** – Pass explicit console dimensions to avoid wrapping on narrow terminals. This also simplifies unit tests. Adjust `TableRenderOptions.TableWidthPercentage` or `MaximumTableWidth` to cap the rendered width regardless of buffer size, and recompute widths when you detect `Consoul.WindowResized` events.
* **Model projections** – Use `Consoul.Table(items, item => item.Name, ...)` for quick POC views. Omit selectors to render public scalar properties automatically. Add `[TableColumn("Label", Order = n)]` to control inferred labels and order, `[TableColumn(Alignment = TableCellAlignment.Right)]` to align numeric columns, `[TableColumn(Format = "yyyy-MM-dd")]` or `[DisplayFormat(DataFormatString = "{0} ms")]` to format values, or `[TableColumn(Visible = false)]` to hide internal properties. Inferred and selector-based headers honor `[TableColumn]`, `[Display(Name = "...")]`, `[DisplayName]`, and `[Description]` metadata before falling back to split PascalCase names. Use `DynamicTableView<T>.AddColumn(...)` for computed values, or `Table.AddRows(items, item => item.Name, ...)` when a manual border/column shell should project model rows without a hand-written loop.
* **Column alignment** – Set `TableRenderOptions.HeaderAlignment` or `ContentAlignment` for table-wide defaults. Use `TableView.SetColumnAlignment(...)`, `TableView.AddHeader(..., TableCellAlignment.Right)`, `DynamicTableView<T>.AddColumn(..., TableCellAlignment.Right)`, `new TableColumn("Retries").RightAligned()`, `new TableColumn("Retries").RightJustified()`, or `[TableColumn(Alignment = ...)]` for per-column alignment.
* **Manual builder polish** - `Table` provides migration-friendly helpers such as `Title(new TableTitle(...))`, `Title(new Markup("[cyan]Status[/]"))`, `Caption(new TableTitle(...))`, `Caption(new TableCaption(...))`, `Caption(renderable)`, inline `Title(..., Style, Justify)` / `Caption(..., Style, Justify)` overloads, `Width(...)`, `WidthPercentage(...)`, `Expand()`, `Collapse()`, `Border(TableBorder.Rounded)`, `Border(TableBorder.None)`, `RoundedBorder()`, `SquareBorder()`, `AsciiBorder()`, `Ascii2Border()`, `AsciiDoubleHeadBorder()`, `MarkdownBorder()`, `HeavyBorder()`, `HeavyEdgeBorder()`, `HeavyHeadBorder()`, `DoubleBorder()`, `DoubleEdgeBorder()`, `SafeBorder()`, `NoBorder()`, `MinimalBorder()`, `MinimalHeavyHeadBorder()`, `MinimalDoubleHeadBorder()`, `HorizontalBorder()`, `SimpleBorder()`, `SimpleHeavyBorder()`, `BorderColor(...)`, `HeaderColor(...)`, `ContentColor(...)`, `AlternatingRowColor(...)`, `TitleColor(...)`, `CaptionColor(...)`, `FooterColor(...)`, `Centered()`, `LeftAligned()`, `LeftJustified()`, `RightAligned()`, `RightJustified()`, `ShowRowSeparators(...)`, `ShowHeaders(...)`, `ShowFooter(...)`, `ShowFooters()`, `HideFooters()`, `RowNumberHeader(...)`, object-friendly `AddRow(...)`, selector-backed `AddRows(items, item => item.Value, ...)`, key/value diagnostics through `AddKeyValueRows(...)`, `AddEmptyRow()`, `InsertRow(...)`, `RemoveRow(...)`, and `UpdateCell(...)` overloads. Use `Collapse()` when a quick diagnostic table should stay content-sized on wide terminals, or `Expand()`/`Width(...)` when a view should claim the available layout space. Use `TableColumn`, `AddColumn("Retries", Justify.Right)`, `AddColumn(new Markup("[yellow]Retries[/]"), Justify.Right)`, `AddColumns(renderableHeaders)`, `AddOptionalColumn("Latency", values, Justify.Right)`, `AddOptionalColumn(renderableHeader, values, Justify.Right)`, or `AddColumn("Name", column => column.RightAligned().Width(10))` when a manual table column needs preferred width, alignment, padding through `Padding(...)`, `PadLeft(...)`, or `PadRight(...)`, no-wrap intent, a renderable header, a footer, or header/footer cell styles through `HeaderColor(...)`, `HeaderStyle(...)`, `FooterColor(...)`, and `FooterStyle(...)`. Top and bottom padding render bounded blank table rows around the cell content, using the largest vertical padding requested by the row's columns so mixed columns stay aligned. Raw markup strings in title/caption text, headers, and cells are measured by their visible text and preserve foreground/background colors, RGB/hex metadata, and ANSI-capable text decorations, while title/caption overloads, `AddRow(...)`, selector-backed `AddRows(...)`, `InsertRow(...)`, `UpdateCell(...)`, `AddKeyValueRows(...)`, and `table.Rows` capture `IConsoulRenderable` values such as `Markup`, `Text`, `Panel`, and nested `Table` instances into bounded text. After a column is added, `table.Columns[index]` exposes synchronized metadata, so updates such as `table.Columns[1].Footer = new Text("Total", new Style(foreground: Color.Yellow, decoration: Decoration.Bold))`, `table.Columns[1].FooterCellStyle = Style.Bold.WithForeground(Color.Yellow)`, or `table.Columns[0].Header = "Task"` are reflected in the next render. Color helpers accept `ConsoleColor` values, reusable `Style` objects, style strings such as `"grey"`, `"aqua"`, `"#22c55e"`, and `"white on navy"`, Spectre-style `Color` values, `RgbColor` values, and aliases from `ConsoulLibrary.Color.Colors`.
* **Justify aliases** - Manual table builders also accept `Justify.Left`, `Justify.Center`, and `Justify.Right` for table title/caption alignment, table-wide header/content alignment, `TableColumn` alignment, direct manual columns such as `AddColumn("Retries", Justify.Right)`, and optional columns such as `AddOptionalColumn("Latency", values, Justify.Right)`.
* **Live row updates** - Manual `Table` exposes `Rows` for Spectre-style mutable dashboard snippets. Use `table.UpdateCell(...)`, `table.InsertRow(...)`, `table.RemoveRow(...)`, `table.Rows.Update(row, column, new Markup("[green]Ready[/]"))`, `table.Rows.Insert(...)`, `table.Rows.RemoveAt(...)`, `table.Rows.Clear()`, and `table.Rows.Add(...)` when a target-based `AnsiConsole.Live(table)` display changes in place. Keep model-backed MVC tables as projections over your source data and rebuild them when filters or model state change.
* **Footer rows vs help** - `ShowFooter(...)` and `HideFooter()` preserve Consoul's legacy behavior for the interactive navigation-help line. Use Spectre-style `ShowFooters()` and `HideFooters()` to control configured `TableColumn.Footer(...)` rows.
* **Title and caption** – Set `TableRenderOptions.Title` and `Caption`, or call `Table.Title(new TableTitle(...))`, `Table.Title(renderable)`, `Table.Caption(new TableTitle(...))`, `Table.Caption(new TableCaption(...))`, or `Table.Caption(renderable)`, when a table should carry its own heading in a panel, grid, captured diagnostic, or model-rendered view. Use `TitleAlignment`, `CaptionAlignment`, `TitleScheme`, and `CaptionScheme` for bounded, table-width-aligned text. The legacy `table.Render("Message")` argument still works as a per-render title and takes precedence over `TableRenderOptions.Title`.
* **Header visibility** – Set `TableRenderOptions.ShowHeaders = false` when a table should use headers for metadata and sizing but render as a compact value list inside a panel, layout, or dashboard row.
* **POC data shaping** – Use `Filter(...)`, `Search(...)`, `SortBy(...)`, and `Page(...)` on `DynamicTableView<T>` to shape model-backed rows before rendering or prompting. `Search(...)` matches visible cell text, so quick diagnostic views can narrow rows without writing a custom predicate. `VisibleContents`, `TotalRowCount`, `MatchedRowCount`, `VisibleRowCount`, and `ActiveSearchQuery` expose the transformed view state for summaries and tests, and `Select(...)` returns selected visible model rows.
* **Result-first selection** – Prefer `TableView.Select(...)` or `DynamicTableView<T>.Select(...)` for new interactive table flows. The result object distinguishes cancellation, accepted empty selections, and selected rows. Users can type a row number, move with arrow keys, press <kbd>Space</kbd> to toggle multiple rows, and press <kbd>Enter</kbd> to accept the highlighted or toggled rows. Set `TableRenderOptions.ShowRowNumbers = true` when row-number input should be visible in the table itself. Legacy `Prompt(...)` remains available when a nullable single-row result is enough.
* **Summaries & empty states** – Set `ShowSummary = true` for an automatic row-count summary below transformed tables, or assign `SummaryFormatter` for custom wording. Set `ShowEmptyMessage = true` and customize `EmptyMessage` when filters/search/page operations may hide every row. Manual `TableView` callers can set `TableRenderOptions.SummaryText` and `TableRenderOptions.EmptyMessage` directly.
* **Inline vs. interactive** – `Consoul.Table(...)`, `RenderInline()`, and the `IConsoulRenderable` implementation render at the current cursor without clearing the console. `TableView.Render()` keeps the existing interactive behavior and can include footer help for table prompts.
* **Whitespace character & borders** – Configure `TableRenderOptions.WhitespaceCharacter` to customise the filler between columns (e.g., dots or dashes). Set `TableRenderOptions.Lines.BorderStyle` to `Square`, `Rounded`, `Ascii`, `Heavy`, or `Double` for polished table joins, or leave it as `Simple` to preserve the legacy repeated-line output. Use `TableBorder.None`, `TableBorder.Minimal`, `NoBorder()`, or `MinimalBorder()` for compact value-list rendering. Common Spectre-style border names such as `Ascii2`, `AsciiDoubleHead`, `Markdown`, `SimpleHeavy`, `MinimalHeavyHead`, `MinimalDoubleHead`, `Horizontal`, `HeavyEdge`, `HeavyHead`, `DoubleEdge`, and `Safe` map to the closest existing Consoul line renderer and also have matching `...Border()` helpers so pasted POC snippets stay dependency-free and responsive. You can still swap out raw horizontal and vertical characters through `TableRenderOptions.Lines` for markdown-like tables or CSV-style separators.
* **Styling per row** – Toggle between `ContentScheme1`, `ContentScheme2`, and `SelectionScheme` to highlight warnings or active rows. You can set these schemes per render pass to reflect live status changes, and use `SelectionScheme` when the table accompanies a `SelectionPrompt` so the highlighted row aligns with the user’s current choice.
* **Export & composition** – Use `Consoul.Capture(table, width: 100)` if you need to export formatted tables into logs, files, or snapshot tests. Add a `TableView` or `DynamicTableView<T>` directly to `RowsEntry`, `GridEntry`, `ColumnsEntry`, or `LayoutEntry` when a dashboard needs a table beside panels, charts, or model summaries.
* **Sorting & paging** – Preprocess your data before adding rows; many consumers combine `TableView` with LINQ to support custom sorting, filtering, or paging scenarios within dynamic views. Persist the source data separately from the rendered rows so you can rebuild the table when filters change without losing formatting rules.
* **Conditional columns** – Use `table.AddOptionalColumn("Latency", values)` to add a manual column only when the candidate values contain visible content; the method returns whether the column was added so row selectors can stay aligned. Model-backed tables can call `DynamicTableView<T>.AddOptionalColumn(...)` or `AddOptionalHeader(...)` after rows are loaded to skip empty detail columns automatically. This keeps POC tables compact on narrow terminals while still providing detail when model data is present.
