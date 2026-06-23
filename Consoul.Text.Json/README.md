# Consoul.Text.Json

`Consoul.Text.Json` is the optional `System.Text.Json` integration for Consoul's object editor. It keeps the core `Consoul` package free of `System.Text.Json` while allowing applications to opt into the platform serializer for JSON-style value rendering.

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
| `Consoul` | Core object editor and console rendering. |
| `System.Text.Json` | JSON serialization for object-editor values. |

## Basic Usage

Call `UseSystemTextJsonForObjectEditor()` before rendering an `EditObjectView`:

```csharp
using ConsoulLibrary;

ConsoulTextJsonExtensions.UseSystemTextJsonForObjectEditor();

var settings = new DeploymentSettings
{
    Environment = "staging",
    RetryCount = 3
};

var editor = new EditObjectView(settings);
editor.Render();
```

The object editor's JSON-style display will use `JsonSerializer.Serialize(...)` for values.

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

ConsoulTextJsonExtensions.UseSystemTextJsonForObjectEditor(options);
```

## Reset To A Custom Serializer

The core editor exposes a serializer hook:

```csharp
EditObjectView.JsonValueSerializer = (value, type) =>
{
    if (value == null)
    {
        return "null";
    }

    return $"\"{value}\"";
};
```

Installing this package is only necessary when you want `System.Text.Json` behavior specifically. The core package includes a lightweight dependency-free formatter for common value types.

## When To Use This Package

Use `Consoul.Text.Json` when:

- You use `EditObjectView`.
- You want JSON-style values to match `System.Text.Json` behavior.
- You are comfortable carrying a `System.Text.Json` package dependency in the consuming project.

Use only `Consoul` when the dependency-free object-editor formatter is enough.

## License

Consoul.Text.Json is licensed under the GNU Lesser General Public License v3.0.
