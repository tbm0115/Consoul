---
title: Upgrade from 2.x
description: Migrate package references, configuration, logging, and input handling to 3.0.
searchable: true
---
# Migrating to Consoul 3.0

This guide covers Consoul **3.0.0**. Rebuild and test consuming applications when upgrading from 2.x; the package split and routine configuration signatures change assembly/API contracts. The steps below cover package references, configuration, and input handling.

## Choose the packages you use

The `Consoul` package keeps the `ConsoulLibrary` namespace and now has no optional infrastructure package references. Types that previously arrived transitively through the core may need an explicit package reference in your application.

| Your application uses | Upgrade action |
| --- | --- |
| Views, prompts, tables, markup, progress, or routines | Reference `Consoul`. |
| `ConsoulLogger`, `ConsoulLoggerProvider`, or `AddConsoulLogger()` from 2.x | Add `Consoul.Extensions.Logging`; these types moved out of the core assembly. The namespace remains `ConsoulLibrary`. |
| `IConfiguration` to configure routine transforms | Add `Consoul.Extensions.Configuration` and use its explicit setup helpers. |
| `System.Text.Json` formatting for form values or JSON visuals | Add `Consoul.Text.Json` and opt into the serializer hook when needed. |
| `CommandApp`, typed command settings, or command-line view entry points | Add the new `Consoul.Cli` package. |

Once 3.0.0 is available on your configured source, pin the packages you need:

```bash
dotnet add package Consoul --version 3.0.0
dotnet add package Consoul.Extensions.Logging --version 3.0.0
dotnet add package Consoul.Extensions.Configuration --version 3.0.0
dotnet add package Consoul.Text.Json --version 3.0.0
dotnet add package Consoul.Cli --version 3.0.0
```

The optional packages reference the core automatically. Use only the lines relevant to your app. See [NuGet setup](nuget.md) for unpublished local builds and authenticated feeds; these examples do not imply publication.

The core includes `netstandard2.0`, `netcoreapp3.1`, and `net461` assets. Optional packages target `netstandard2.0` and bring their own dependency requirements. Review the restored dependency graph for older applications; retaining a core target framework does not establish that every optional integration has been exercised on that runtime.

## Update routine configuration

Two signatures differ from 2.0.2:

| API | 2.x | 3.0 |
| --- | --- | --- |
| `Routines.getAppSettings()` return type | `IConfigurationSection` | `RoutineSettingsSection` |
| `Routines.InitializeRoutine(args, configuration)` second parameter | `IConfigurationRoot` | `object`, handled through a compatibility adapter |

Source calls passing an existing configuration object may still compile, but precompiled callers must be rebuilt. If you assigned `getAppSettings()` to `IConfigurationSection` or called configuration-provider extensions on it, keep your application's `IConfiguration` separately and use the integration package to configure Consoul:

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

configuration.InitializeConsoulRoutine(args);
```

If you only need transforms, call `configuration.ConfigureConsoulRoutines()` instead. The configuration shape remains:

```json
{
  "Consoul": {
    "Transforms": {
      "Environment": "staging"
    }
  }
}
```

For a core-only application, configure a dictionary without any configuration provider:

```csharp
using System.Collections.Generic;
using ConsoulLibrary;

Routines.ConfigureTransforms(new Dictionary<string, string>
{
    ["Environment"] = "staging"
});

Routines.InitializeRoutine(args);
```

Without explicit configuration, the core tries `appsettings.json` in the working directory before the Consoul assembly directory and uses empty settings when no usable settings are loaded. Configure settings explicitly when the location or validation behavior matters to your application.

## Preserve JSON serialization behavior when opting in

The core form editor uses a dependency-free value formatter. Installing `Consoul.Text.Json` makes JSON helpers available; enable the serializer before rendering forms if you need `System.Text.Json` value semantics:

```csharp
using ConsoulLibrary;
using System.Text.Json;

ConsoulTextJsonExtensions.UseSystemTextJsonForFormValues(
    new JsonSerializerOptions { WriteIndented = false });
```

The hook is `EditObjectView.FormValueSerializer`. `EditObjectView.JsonValueSerializer` and `UseSystemTextJsonForObjectEditor(...)` remain compatibility aliases for applications that used the earlier development-branch names. This is an optional integration, not a requirement for basic object editing.

## Keep cancellation explicit

Legacy helpers such as `Consoul.Prompt(...)`, `Consoul.Input<T>(...)`, and `Consoul.Ask(...)` remain available. For new flows, use result objects so canceled input is distinct from a valid `0`, `false`, or empty value:

```csharp
using ConsoulLibrary;

TextPromptResult<int> result = new TextPrompt<int>("Retries").Render();
if (result.HasValue)
{
    Consoul.Write($"Retries: {result.Value}");
}
```

Selection results expose `HasSelection`, `Index`, and `IsCanceled`; scalar results expose `HasValue`, `Value`, and `IsCanceled`. Value-style implicit conversions are conveniences that discard the cancellation signal. Prefer checking the result before changing application state. See [prompts](prompts.md) for typed selection, validators, and cancellation tokens.

## Handle end-of-input separately from a blank line

`Consoul.Read()` returns `null` when the input stream ends without more text. An actual blank line still returns `string.Empty`, and a final line without a newline is returned before EOF. Code reading a redirected stream should stop on `null`:

```csharp
using ConsoulLibrary;

string line;
while ((line = Consoul.Read()) != null)
{
    Consoul.Write(line);
}
```

Typed, selection, multi-selection, confirmation, and secret prompts treat EOF as cancellation; EOF does not silently accept a default value. Pressing Enter on a real blank line still follows the prompt's configured default/empty-input behavior. Legacy `Input(...)` returns `null`, `Input<T>(...)` returns `default(T)`, `Ask(...)` returns `false`, and `PromptForFilepath(...)` returns `null` when input ends. If your old retry loop tested only for an empty string, update it to handle EOF explicitly.

Redirected reads decode multibyte text with the console's input encoding, including UTF-8 emoji, and preserve the last unterminated line. Display-cell width remains a separate rendering limitation described below.

## Treat familiar builder names as migration helpers

`AnsiConsole`, `IAnsiConsole`, `Panel`, `Table`, `Layout`, `Style`, and the CLI builders offer familiar Spectre-style names over Consoul's own implementation. Import `ConsoulLibrary` and, for palette values, `ConsoulLibrary.Color`. These types do not establish binary compatibility with Spectre.Console or identical behavior for every similarly named option.

For example, table cells are bounded single-line cells, `TableColumn.NoWrap` retains compatibility metadata, and some border aliases map to the nearest Consoul border preset. Review the rendered result rather than assuming every pasted snippet has identical output.

## Check the application in its real terminal

Exercise your app's initial menu, cancel/back actions, narrow-window layouts, redirected input, and redirected output after upgrading. Consoul's layout measurements still use UTF-16 length in several rendering paths. CJK text, combining characters, and emoji can occupy different terminal widths; clipping can split a surrogate pair. UTF-8 input decoding and display-cell measurement are separate concerns. Use simple single-cell labels where exact layout is required until this limitation is resolved.

See the [release notes](releases/3.0.0.md) for the release's scope and the [component guides](index.md#pick-a-building-block) for the current API patterns.
