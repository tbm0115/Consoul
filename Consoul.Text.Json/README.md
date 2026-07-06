# Consoul.Text.Json

`Consoul.Text.Json` is the optional `System.Text.Json` integration for Consoul's MVC-style object editor and JSON display helpers. It keeps the core `Consoul` package free of `System.Text.Json` while allowing applications to opt into platform JSON serialization, syntax-colored JSON snapshots, and JSON-style value rendering inside the shared form document editor.

## Installation

Install from NuGet:

```bash
dotnet add package Consoul.Text.Json
```

Or install from the GitHub package registry:

```bash
dotnet add package Consoul.Text.Json --source "https://nuget.pkg.github.com/tbm0115/index.json"
```

This package depends on:

| Package | Purpose |
| --- | --- |
| `Consoul` | Core MVC-style object editor views and console rendering. |
| `System.Text.Json` | JSON serialization for form/object-editor values and JSON snapshot rendering. |

## Basic Usage

Call `UseSystemTextJsonForFormValues()` before rendering an `EditObjectView` directly or through `DynamicView<TModel>.EditModel(...)`:

```csharp
using ConsoulLibrary;

ConsoulTextJsonExtensions.UseSystemTextJsonForFormValues();

var settings = new DeploymentSettings
{
    Environment = "staging",
    RetryCount = 3
};

var editor = Consoul.EditObject(settings);
editor.Render();
```

The form document editor's JSON-style value layout will use `JsonSerializer.Serialize(...)` for values.

## Render JSON Snapshots

Use `ConsoulTextJsonExtensions.Json(...)` for raw JSON or `JsonValue(...)` for model snapshots:

```csharp
using ConsoulLibrary;

ConsoulTextJsonExtensions.Json("{\"status\":\"ready\",\"count\":3}");

ConsoulTextJsonExtensions.JsonValue(new
{
    Environment = "Development",
    Preview = true,
    RetryCount = 3
}, new JsonTextOptions
{
    Width = 64
});
```

If your MVC-style view or service already depends on `IAnsiConsole`, use the injected-console extension methods so the same JSON rendering flows through the active facade, fake console, or writer-backed console:

```csharp
public sealed class DiagnosticsPanel
{
    private readonly IAnsiConsole _console;

    public DiagnosticsPanel(IAnsiConsole console)
    {
        _console = console;
    }

    public void Render(DeploymentSettings settings)
    {
        _console.JsonValue(settings, new JsonTextOptions { Width = 64 });
    }
}
```

JSON rendering is token-colored, pretty-printed by default, and bounded to the current Consoul terminal width. Invalid JSON renders a friendly bounded error unless `JsonTextOptions.ThrowOnInvalidJson` is enabled.

Use `JsonTextEntry` when a JSON snapshot should be captured, placed in a panel/layout, or returned from a model-backed view:

```csharp
var snapshot = JsonTextEntry.FromValue(
    settings,
    options: new JsonTextOptions { Width = 56 });

Consoul.View("Diagnostics", settings)
    .RenderModel(model => JsonTextEntry.FromValue(model, options: new JsonTextOptions { Width = 56 }))
    .Option("Refresh", model => LoadLatest(model))
    .Render();

Consoul.Panel(snapshot, title: "Settings Snapshot", width: 64, captureWidth: 56);
```

For Spectre-style snippets, use `JsonText`. It is still part of this optional package, so the core `Consoul` package remains free of `System.Text.Json`:

```csharp
using ConsoulLibrary;
using ConsoulLibrary.Color;

AnsiConsole.Write(new JsonText("{\"status\":\"ready\",\"count\":3,\"items\":[1,2]}")
    .Width(64)
    .Indentation("..")
    .MemberColor(Colors.Aqua)
    .StringStyle(new Style(Colors.Green))
    .NumberColor(Colors.Blue)
    .BooleanStyle(new Style(Colors.Yellow))
    .NullStyle(new Style(Colors.Red))
    .BracesStyle(new Style(Colors.Magenta))
    .BracketsStyle(new Style(Colors.DarkMagenta))
    .ColonStyle(new Style(Colors.Cyan))
    .CommaStyle(new Style(Colors.Gray)));
```

Model snapshots can use the same fluent builder with `JsonText.FromValue(...)`:

```csharp
using System.Text.Json;

AnsiConsole.Write(JsonText
    .FromValue(settings, serializerOptions: new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    })
    .Width(64)
    .MemberColor(Colors.Aqua)
    .NumberColor(Colors.Blue));
```

Initializer-style configuration is also supported when a reusable JSON snapshot should be built declaratively:

```csharp
var json = new JsonText("{\"status\":\"ready\",\"items\":[1]}")
{
    Width = 64,
    Indentation = "   ",
    MemberStyle = new Style(Colors.Aqua),
    StringStyle = new Style(Colors.Green),
    NumberStyle = new Style(Colors.Blue),
    BracesStyle = new Style(Colors.Magenta),
    BracketsStyle = new Style(Colors.DarkMagenta),
    ColonStyle = new Style(Colors.Cyan),
    CommaStyle = new Style(Colors.Gray)
};

AnsiConsole.Write(json);
```

`JsonText` wraps `JsonTextEntry`, so it can be captured, placed in panels/layouts, or returned from MVC-style dynamic view sections just like other Consoul renderables.

## Configure JsonSerializerOptions

Pass serializer options when you want custom formatting:

```csharp
using ConsoulLibrary;
using System.Text.Json;

var options = new JsonSerializerOptions
{
    WriteIndented = false,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

ConsoulTextJsonExtensions.UseSystemTextJsonForFormValues(options);
```

## Reset To A Custom Serializer

The core editor exposes a serializer hook:

```csharp
EditObjectView.FormValueSerializer = (value, type) =>
{
    if (value == null)
    {
        return "null";
    }

    return $"\"{value}\"";
};
```

`ConsoulTextJsonExtensions.UseSystemTextJsonForObjectEditor(...)` and `EditObjectView.JsonValueSerializer` remain available as compatibility aliases for callers that adopted the original object-editor or JSON-editor terminology. New code should think of this package as the serializer/coloring companion for `EditObjectView` and reusable `JsonTextEntry` snapshots.

Installing this package is only necessary when you want `System.Text.Json` behavior specifically. The core package includes a lightweight dependency-free formatter for common value types.

## When To Use This Package

Use `Consoul.Text.Json` when:

- You use `DynamicView<TModel>.EditModel(...)` or `EditObjectView`.
- You want JSON-style values to match `System.Text.Json` behavior.
- You want syntax-colored JSON snapshots in MVC-style diagnostic or proof-of-concept views.
- You are comfortable carrying a `System.Text.Json` package dependency in the consuming project.

Use only `Consoul` when the dependency-free form/object-editor formatter is enough.

## License

Consoul.Text.Json is licensed under the GNU Lesser General Public License v3.0.
