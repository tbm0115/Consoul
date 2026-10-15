# Consoul documentation

Consoul builds console applications from models, reusable views, prompts, and renderable components. These Markdown guides can be read directly on GitHub; no documentation generator is required.

This branch documents the **3.0.0-preview.1** candidate. Start with [installation and package sources](nuget.md), the [2.x migration guide](migration-3.0.md), and [release notes and known limitations](releases/3.0.0-preview.1.md). Publishing the packages and release tag is a separate step.

## Start here

| Goal | Read |
| --- | --- |
| Install or package the preview | [NuGet guide](nuget.md) |
| Upgrade a 2.x application | [Migration to 3.0](migration-3.0.md) |
| Build your first menu | [Core package quick start](../Consoul/README.md#quick-start) and [5-minute POC view](../Consoul/README.md#5-minute-poc-view) |
| Choose an API or copy a focused example | Component guides below |
| Review changes and remaining limits | [3.0.0-preview.1 release notes](releases/3.0.0-preview.1.md) |

## Component guides

| Guide | Covers |
| --- | --- |
| [Core console APIs](core-console.md) | Output, markup, themes, terminal profiles, capture, and composed visuals |
| [Prompts](prompts.md) | Typed input, selection, validation, cancellation, and secrets |
| [Views](views.md) | Static and dynamic views, model-backed sections, navigation, and forms |
| [Tables](tables.md) | Manual and model-backed tables, selection, formatting, and layout |
| [Progress](progress.md) | Progress bars, task progress, status, spinners, and live displays |
| [Routines](routines.md) | Scripted input, XML routines, and configuration transforms |

For the complete example catalog, see the [core package README](../Consoul/README.md).

## Optional packages

The core package does not require `Microsoft.Extensions.*` or `System.Text.Json`. Add an integration only when your application needs it, and keep Consoul package versions aligned during the preview.

| Package guide | Use it for |
| --- | --- |
| [Consoul.Cli](../Consoul.Cli/README.md) | Command arguments, options, validation, help, completion, and launching views |
| [Consoul.Extensions.Logging](../Consoul.Extensions.Logging/README.md) | Sending `ILogger` output through Consoul |
| [Consoul.Extensions.Configuration](../Consoul.Extensions.Configuration/README.md) | Reading routine transforms from `IConfiguration` |
| [Consoul.Text.Json](../Consoul.Text.Json/README.md) | JSON rendering and `System.Text.Json` form-value serialization |

## Preview feedback

Use the [GitHub issue tracker](https://github.com/tbm0115/Consoul/issues) to report problems. Include the Consoul version, target framework, operating system, terminal, whether input/output is redirected, and a small reproducer. For layout reports, include terminal width and the affected text; wide characters and clipped emoji have [known limitations](releases/3.0.0-preview.1.md#known-limitations).
