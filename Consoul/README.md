# Consoul

`Consoul` is the dependency-light core package for building interactive console applications with prompts, views, tables, progress bars, fixed messages, and scriptable routines.

The core package intentionally avoids `Microsoft.Extensions.*` and `System.Text.Json` package dependencies. Optional integration packages are available when you want those ecosystems:

| Package | Purpose |
| --- | --- |
| `Consoul.Extensions.Logging` | Adds `Microsoft.Extensions.Logging` support. |
| `Consoul.Extensions.Configuration` | Bridges `Microsoft.Extensions.Configuration` into routine transforms. |
| `Consoul.Text.Json` | Uses `System.Text.Json` for object-editor value rendering. |

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

Consoul.Write("Welcome to Consoul", ConsoleColor.Cyan);

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

## 5-Minute POC View

For quick proof-of-concept terminal views, use `Consoul.View(...)` instead of creating a view subclass up front.

```csharp
using ConsoulLibrary;
using System.Threading.Tasks;

Consoul.View("Deployment Tools")
    .Option("Print status", () => Consoul.Write("All systems ready.", ConsoleColor.Green))
    .Option("Run async check", async cancellationToken =>
    {
        await Task.Delay(250, cancellationToken);
        Consoul.Write("Check complete.", ConsoleColor.Cyan);
    })
    .Navigate<SettingsView>("Edit settings")
    .Render();

public sealed class SettingsView : StaticView
{
    public SettingsView()
    {
        Title = "Settings";
        Options.Add(new ViewOption("Say hello", () => Consoul.Write("Hello")));
    }
}
```

## Prompts

`SelectionPrompt` renders a numbered menu and returns a `PromptResult`, so callers can distinguish selection, cancellation, and default selection behavior.

```csharp
var prompt = new SelectionPrompt("Pick an environment");
prompt.Add("Development", ConsoleColor.Green, isDefault: true);
prompt.Add("Staging", ConsoleColor.Yellow);
prompt.Add("Production", ConsoleColor.Red);

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

For simple menus, `Consoul.Select(...)` returns the same result without manually creating a prompt:

```csharp
PromptResult environment = Consoul.Select("Pick an environment", false, "Development", "Staging", "Production");

if (environment.HasSelection)
{
    Consoul.Write($"Selected option {environment.Index + 1}");
}
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

## Tables

`TableView` renders column-aware tables and recalculates layout against the current terminal width.

```csharp
var table = new TableView();
table.AddHeaders("Id", "Name", "Status");
table.AddRow(new[] { "1", "Sync worker", "Running" });
table.AddRow(new[] { "2", "Report exporter", "Waiting" });

table.Render("Jobs", ConsoleColor.Cyan);
```

For customized colors and borders, configure `TableRenderOptions`:

```csharp
var options = new TableRenderOptions
{
    MaximumTableWidth = 80,
    HeaderScheme = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue)
};

var table = new TableView(options);
table.AddHeaders("Package", "Purpose");
table.AddRow(new[] { "Consoul", "Core console UI" });
table.Render();
```

## Progress Bars

`ProgressBar` keeps a fixed render position and can update in place.

```csharp
using (var progress = new ProgressBar("Starting"))
{
    for (int i = 0; i <= 10; i++)
    {
        progress.Update(i / 10.0, $"Step {i}/10");
        Thread.Sleep(100);
    }
}
```

## Views

Use `Consoul.View(...)` for first-run POCs, then graduate to `StaticView` or `DynamicView<T>` when the workflow deserves a named class.

```csharp
[View("Tools")]
public sealed class ToolsView : StaticView
{
    public ToolsView()
    {
        Options.Add(new ViewOption("Say hello", () => Consoul.Write("Hello")));
        Options.Add(new ViewOption("Run task", RunTask, ConsoleColor.Green));
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

The sample app includes small POC examples for fluent menus, table display, and object editing under `Consoul.Test/Views/PocViews.cs`.

## Routines

Routines let automated demos and smoke tests feed input to prompts without real keyboard input.

```csharp
public sealed class DemoRoutine : Routine
{
    public DemoRoutine()
    {
        Enqueue(new RoutineInput { Value = "1" });
        Enqueue(new RoutineInput { Value = "yes" });
    }
}

Routines.InitializeRoutine(new DemoRoutine(), "Demo");
```

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

If your application already uses `Microsoft.Extensions.Configuration`, install `Consoul.Extensions.Configuration` for typed configuration helpers.

## Object Editor

`EditObjectView` provides a reflection-based editor for object properties. Core Consoul includes a dependency-free JSON-style display for the editor. Install `Consoul.Text.Json` if you want the display to use `System.Text.Json`.

```csharp
var settings = new MySettings();
var editor = new EditObjectView(settings);
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
