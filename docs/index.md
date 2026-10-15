---
title: Documentation
description: Build interactive .NET console apps with prompts, reusable views, tables, and live progress.
searchable: true
home: true
---
<span class="eyebrow">The .NET console UI toolkit</span>

# Give your console a little soul.

<p class="lead">Turn command-line tools into interactive apps. Compose readable output, collect validated input, and connect reusable views to your C# models.</p>

<div class="actions">
  <a class="button" href="{{ '/getting-started.html' | relative_url }}">Build your first app →</a>
  <a class="button secondary" href="{{ '/examples.html' | relative_url }}">Explore examples</a>
</div>

<figure class="terminal-figure">
  <img src="{{ '/assets/screenshots/overview.svg' | relative_url }}" width="760" height="394" alt="Captured Consoul dashboard: a service table with API Healthy and Worker Starting, followed by a Build summary panel.">
  <figcaption>A small dashboard, composed with a table and panel. Captured from the library at a fixed width; terminal colors are omitted.</figcaption>
</figure>

## Start with a few lines

This site documents Consoul 3.0.0. Once that version is available on your package source:

```shell
dotnet add package Consoul --version 3.0.0
```

```csharp
using ConsoulLibrary;

Consoul.MarkupLine("[cyan]Hello from Consoul![/]");
Consoul.Panel("Your first console app is ready.", title: "Welcome", width: 48);
```

The [getting-started guide](getting-started.md) walks through a complete menu app. For local testing before publication, follow the [local package instructions](nuget.md#install-in-a-consuming-application).

## Pick a building block

<div class="feature-grid">
  <a class="feature-card" href="{{ '/prompts.html' | relative_url }}"><span class="card-label">01 / INPUT</span><strong>Ask better questions</strong><span>Typed input, validation, selection lists, and explicit cancellation.</span></a>
  <a class="feature-card" href="{{ '/views.html' | relative_url }}"><span class="card-label">02 / VIEWS</span><strong>Make a menu an app</strong><span>Connect actions and model-backed sections to reusable screens.</span></a>
  <a class="feature-card" href="{{ '/tables.html' | relative_url }}"><span class="card-label">03 / DATA</span><strong>Give your data structure</strong><span>Readable tables with formatting, model binding, and selection.</span></a>
  <a class="feature-card" href="{{ '/progress.html' | relative_url }}"><span class="card-label">04 / FEEDBACK</span><strong>Show work as it happens</strong><span>Progress tasks, status messages, spinners, and live displays.</span></a>
</div>

Use [output and styling](core-console.md) to add markup, themes, and composed visuals. Use [routines](routines.md) to automate repeated console interactions.

## Optional packages

Keep the core small and add the integrations you need:

| Package | Adds |
| --- | --- |
| `Consoul.Cli` | Commands, arguments, help, and completion |
| `Consoul.Extensions.Logging` | `ILogger` output through Consoul |
| `Consoul.Extensions.Configuration` | Configuration-backed routine transforms |
| `Consoul.Text.Json` | JSON rendering and form-value serialization |

[Choose an integration and see examples →](integrations.md)

## Moving to 3.0?

Read the [migration guide](migration-3.0.md) for package and API changes, then check the [release notes](releases/3.0.0.md) for known limitations. The core targets .NET Standard 2.0, .NET Core 3.1, and .NET Framework 4.6.1; the quick start uses a modern .NET console app.

Found a problem? [Open a GitHub issue](https://github.com/tbm0115/Consoul/issues) with your package version, terminal, operating system, and a small reproducer. Include terminal width for layout issues.
