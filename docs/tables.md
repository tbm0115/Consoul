# Tables

## When to use this
Tables help present structured data (logs, metrics, inventories) with aligned columns and optional theming. They are perfect for dashboards and admin tools where readability matters.

## Key types
* `TableView` – Defines headers, column widths, and rows for rendering tabular data.
* `TableRenderOptions` – Controls padding, alignment, and border characters.
* `RenderOptions` – Supplies fallback colours for headers and borders.

## Minimal example
```csharp
var table = new TableView();
table.AddHeaders("Service", "Status", "Duration");
table.AddRow(new[] { "Auth", "OK", "120 ms" });
table.AddRow(new[] { "Payments", "Degraded", "450 ms" });
table.AddRow(new[] { "Search", "Failed", "–" });

table.TableRenderOptions.HeaderScheme = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);
table.TableRenderOptions.WhitespaceCharacater = '·';
table.TableRenderOptions.TableWidthPercentage = 0.9m;

table.Render();
```

## Advanced tips
* **Window width** – Pass explicit console dimensions to avoid wrapping on narrow terminals. This also simplifies unit tests.
* **Whitespace character** – Configure `TableRenderOptions.WhitespaceCharacter` to customise the filler between columns (e.g., dots or dashes).
* **Styling per row** – Apply different `ColorScheme` values per row to highlight warnings or failures.
* **Export** – Use `table.Render()` without writing to the console if you need to export the formatted table into logs or files.
