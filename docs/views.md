---
title: Views and menus
description: Build reusable console screens with actions and model-backed sections.
searchable: true
---
# Views

## When to use this
Views are ideal for multi-step workflows—dashboards, menus, or game scenes—where you want to encapsulate options and state per screen. They integrate prompts with lifecycle events so you can build entire applications from composable classes.

## Quick start

```csharp
using ConsoulLibrary;

Consoul.View("Tools")
    .Option("Say hello", () =>
    {
        Consoul.Write("Hello!");
        Consoul.Wait();
    })
    .Render();
```

Select an action by number; enter `back` to leave the view. Start with the [complete menu recipe](examples.md#interactive-menus), then use model-backed sections as the app grows.

## Key types
* `StaticView` – Presents a fixed set of options defined via attributes or collection initialisation.
* `DynamicView` – Rebuilds its option list every time it renders, enabling state-aware labels and colours.
* `ViewOption` / `DynamicOption<T>` – Describe the work performed when a user selects an option.
* `ChoiceCallback` – Optional delegate that receives notification whenever an option is executed.

## Minimal example
```csharp
[View("Main menu")]
public class MainMenu : StaticView
{
    public MainMenu() : base()
    {
    }

    [ViewOption("Start job", ConsoleColor.Green)]
    private void StartJob()
    {
        Consoul.Write("Starting job...", ConsoleColor.Green);
        GoBack();
    }

    [ViewOption("Show high scores", ConsoleColor.Cyan)]
    private void ShowScores()
    {
        Consoul.Write("Loading scores...", ConsoleColor.Yellow);
        NavigateTo<HighScoresView>(replace: false);
    }

    [ViewOption("Exit", ConsoleColor.DarkGray)]
    private void Exit()
    {
        Consoul.Write("Goodbye!", ConsoleColor.Gray);
        GoBack();
    }
}
```

### Navigation helpers

`StaticView` and `DynamicView<T>` expose protected helpers to request navigation without manually instantiating other views. Call `NavigateTo<TView>()` to replace the current view (the default), `NavigateTo<TView>(replace: false)` to push a new view on the stack, `NavigateTo(() => new DetailView(arg))` when you need to supply constructor parameters, or `GoBack()` to pop to the previous screen. Constructor-style `DynamicView<T>` POC menus can expose the same flow with `Navigate<TView>("Details", () => new DetailView(model), replace: false)` or the StaticView-shaped `AddNavigationOption<TView>(...)` alias. Constructor-defined `StaticView` menus can use `AddNavigationOption<TView>("Details", () => new DetailView(model), replace: false)` when the destination needs injected state. The renderer executes these requests inside an iterative loop, preventing recursive render chains and the stack overflows that could occur when options previously invoked `new OtherView().Render()`.

## Advanced tips
* **Attribute-driven options** – Decorate methods with `ViewOptionAttribute` or `DynamicViewOptionAttribute` to declare choices alongside their logic. Attribute metadata can control colour, descriptions, shortcuts, and hidden search aliases; use `ViewOptionAttribute.SearchTerms` or `DynamicViewOptionAttribute.SearchTermsMethod` when a controller-style action label should stay short but still be findable by domain words.
* **POC model construction** – Use `Consoul.View("Title", model)` when a screen owns an existing model instance, or `Consoul.View<TModel>("Title")` when a parameterless model is enough for a first sketch. Both paths return `DynamicView<TModel>`, so the screen still renders model state and treats options as controller-style actions.
* **State-aware options** – Use `DynamicView<T>.OptionWhen(...)` when a model action should remain visible but disabled until the current model satisfies a predicate. Use `DynamicView<T>.OptionWhenValid(...)` for save, publish, deploy, or apply actions that should wait for DataAnnotations-style model validation. The option is re-evaluated on each render and shows the disabled reason through the normal prompt renderer. Async POC handlers can use `Func<Task>` or `Func<TModel, Task>` overloads when they do not need the cancellation token.
* **Confirmed actions** – Use `DynamicView<T>.ConfirmOption(...)`, `DynamicView<T>.AddConfirmOption(...)`, or `StaticView.AddConfirmOption(...)` for save, delete, deploy, and other guarded actions. They use the same `ConfirmResult` semantics as `Consoul.Confirm(...)`, so canceled or declined confirmations leave the action untouched and return to the view loop. Async POC handlers can also use `Func<Task>`, `Func<CancellationToken, Task>`, `Func<TModel, Task>`, or `Func<TModel, CancellationToken, Task>` overloads when they do not need all parameters.
* **Prompt ergonomics** – Set `PromptKeyboardNavigationEnabled`, `PromptFilterEnabled`, or `PromptPageSize` on a `StaticView` or `DynamicView<T>` when a POC menu needs arrow navigation, typed filtering, or paging without manually constructing a prompt. Use `StaticView.AddOption(...)` for constructor-defined sync or async actions, `StaticView.AddConfirmOption(...)` for guarded static actions, `StaticView.AddNavigationOption<TView>(...)` for typed or factory navigation actions, `DynamicView<T>.AddOption(...)`, `DynamicView<T>.AddConfirmOption(...)`, `DynamicView<T>.AddNavigationOption(...)`, `DynamicView<T>.AddEditFormOption(...)`, `DynamicView<T>.Separator(...)`, `StaticView.AddSeparator(...)`, or `ViewOption.Separator(...)` to group larger menus without changing selectable option indexes. The dynamic add-style names are aliases over `Option(...)`, `ConfirmOption(...)`, `Navigate(...)`, and `EditForm(...)`, preserving the same MVC-style model-backed view rather than introducing a separate fluent view or form view layer. Use `DynamicView<T>.AddOptions(...)` when a model-backed POC menu should project a collection of action descriptors into selectable controller actions without hand-written loops; optional `isEnabled` and `disabledReason` selectors keep projected actions visible but unavailable until the view model is ready. Navigation options can target a view type or a factory such as `() => new DetailView(model)` when the destination needs constructor parameters. These APIs accept `ConsoleColor` values, reusable `Style` objects, style strings such as `"green"`, `"grey"`, `"aqua"`, and `"#22c55e"`, direct `Color` / `RgbColor` values for constructor-defined static menus, dynamic menus, and named sections, `Style` selectors such as `model => new Style(model.Accent, Decoration.Bold)`, or `Color` / `RgbColor` selectors when the model carries the palette. Style metadata is carried through the view prompt, command palette, generated help, and section titles on ANSI-capable terminals instead of being flattened to the nearest console color. Use `DynamicView<T>.DescribeLastOption(...)` or `ViewOption.Describe(...)` to add searchable helper text under options while keeping controller-style action labels short. Use `DynamicView<T>.SearchLastOption(...)` or `ViewOption.AddSearchTerms(...)` for hidden aliases like `ship`, `publish`, or `prod`. Use `DynamicView<T>.ShortcutLastOption(...)`, `ViewOption.UseShortcut(...)`, or `ViewOptionAttribute.Shortcut` when frequent actions should be selectable by a typed or keyboard shortcut.
* **Generated help** – Use `DynamicView<T>.HelpOption(...)` or `StaticView.AddHelpOption(...)` when exploratory users need an in-app reference for a POC menu. The generated `ViewHelpEntry` uses the current view's labels, descriptions, shortcuts, separators, disabled reasons, and Go Back action, and `CreateHelp(...)` returns the same help surface for panels, layouts, captures, or tests. Help, command palette, and view-help entries accept reusable `Style` objects, style strings, `Color`, and `RgbColor` values, so constructor-defined static menus and model-backed dynamic menus can share the same palette. `ViewHelpEntry` and `ViewHelpOptions` preserve RGB/hex colors and ANSI decorations for labels, separators, title rules, shortcuts, descriptions, and disabled-only rows on capable terminals while falling back to readable console colors. Help labels, descriptions, disabled reasons, and separator labels can contain raw markup and still wrap by visible text, while shortcut text remains literal.
* **View chrome** – Use `ShowBreadcrumbs()` and `ShowShortcuts()` on `DynamicView<T>` or `StaticView` when a multi-screen POC app needs lightweight navigation context. Breadcrumb chrome is populated from `ViewRenderer` navigation stack entries created by `NavigateTo(...)`, while shortcut chrome reuses existing shortcut metadata plus the generated Go Back action.
* **Command palette** – Use `DynamicView<T>.CommandPaletteOption(...)` or `StaticView.AddCommandPaletteOption(...)` when view actions should be searchable. Pass a `Style` object, style string, `Color`, or `RgbColor` when a POC view uses a shared palette. The palette reuses option labels, descriptions, shortcuts, hidden search aliases, separators, disabled reasons, and filtering behavior, then invokes the selected action and returns to the same view loop.
* **View-local errors** – Use `ThrowOnError()`, `RenderErrorsAndContinue()`, `UseErrorMode(...)`, or `WaitOnRenderError(...)` on a `StaticView` or `DynamicView<T>` when one MVC-style screen needs different behavior from global `RenderOptions.ViewErrorMode` or `RenderOptions.WaitOnError`. This keeps permissive exploratory menus and stricter controller-style views in the same application without changing process-wide defaults.
* **Go back behaviour** – Each view automatically appends a “Go back” option and now uses `PromptResult` cancelation rather than the legacy `Consoul.EscapeIndex` constant. Customise the label via `ViewAttribute.GoBackMessage` or by changing `RenderOptions.DefaultGoBackMessage`, and check `GoBackRequested` after `RenderAsync` returns to run cleanup when a user leaves mid-operation.
* **Async rendering** – Use `RenderAsync()` on dynamic views when options trigger asynchronous operations; the framework awaits each action before re-rendering. Combine this with `CancellationToken` parameters on option handlers to respond to user aborts or window closures, and surface progress updates by re-rendering interim messages via `Consoul.Write`.
* **Shared state & navigation** – Pass dependencies via constructors or properties; views are regular classes and can store fields, timers, or other stateful components. Use `NavigateTo<TView>()` (or `NavigateTo<TView>(replace: false)` when you need to push) or `GoBack()` inside an option to request transitions; the renderer now applies these commands iteratively so the call stack stays flat even through long navigation chains. Inject shared services (logging, data providers) to keep options thin and testable.
* **Composed dashboards** – `DynamicView` options can render bar charts, breakdown charts, calendars, columns, exception diagnostics, grids, headlines, property lists, sparklines, spinners, tables, text blocks, trees, panels, or progress bars inline, then rehydrate their options list after completing background work. This makes it easy to build monitoring dashboards driven by timers or file watchers; schedule refreshes with `System.Threading.Timer` to trigger `RenderAsync()` when data changes.
* **Reusable model sections** – `DynamicView<T>.RenderModel(...)` accepts either the original write action or a reusable `IConsoulRenderable`/model-to-renderable factory. Use `DynamicView<T>.Section(...)` to append named MVC-style dashboard sections after the primary model renderer and before the option prompt; section titles are re-evaluated on every render, so mutated model state appears immediately after an action completes. Use `DynamicView<T>.PropertyListSection(...)` for a generated read-only model summary, `DynamicView<T>.TableSection(...)` for a generated responsive table from a collection owned by the view model, `DynamicView<T>.MetricsSection(...)` for compact KPI or health-state values, `DynamicView<T>.RowsSection(...)`, `ColumnsSection(...)`, and `GridSection(...)` for composed dashboard rows, side-by-side summaries, or responsive cells owned by the view model, `DynamicView<T>.LayoutSection(...)` for responsive dashboard panes or nested layout regions owned by the view model, `DynamicView<T>.TextBlockSection(...)` for wrapped explanatory text owned by the view model, `DynamicView<T>.PanelSection(...)` for framed detail text owned by the view model, `DynamicView<T>.CalloutSection(...)` for status/warning/success/error messages owned by the view model, `DynamicView<T>.ExceptionSection(...)` for a last error or diagnostic exception owned by the view model, `DynamicView<T>.BreadcrumbSection(...)` for model-owned navigation context, `DynamicView<T>.ShortcutBarSection(...)` for model-owned action hints or shortcut metadata, `DynamicView<T>.BulletListSection(...)` for notes or nested diagnostics, `DynamicView<T>.ChecklistSection(...)` for readiness or workflow checks, `DynamicView<T>.CalendarSection(...)` for schedule state, `DynamicView<T>.SparklineSection(...)` for trend values, `DynamicView<T>.BarChartSection(...)` / `BreakdownChartSection(...)` for responsive chart rows, `DynamicView<T>.TreeSection(...)` for nested dependency or diagnostic state, and `DynamicView<T>.TimelineSection(...)` for workflow or history steps projected from the model. Use `DynamicView<T>.SectionWhen(...)`, `PropertyListSectionWhen(...)`, `TableSectionWhen(...)`, `MetricsSectionWhen(...)`, `RowsSectionWhen(...)`, `ColumnsSectionWhen(...)`, `GridSectionWhen(...)`, `LayoutSectionWhen(...)`, `TextBlockSectionWhen(...)`, `PanelSectionWhen(...)`, `CalloutSectionWhen(...)`, `ExceptionSectionWhen(...)`, `BreadcrumbSectionWhen(...)`, `ShortcutBarSectionWhen(...)`, `BulletListSectionWhen(...)`, `ChecklistSectionWhen(...)`, `CalendarSectionWhen(...)`, `SparklineSectionWhen(...)`, `BarChartSectionWhen(...)`, `BreakdownChartSectionWhen(...)`, `TreeSectionWhen(...)`, `TimelineSectionWhen(...)`, or `ValidationSummarySectionWhen(...)` for validation, warnings, generated summaries, advanced details, or other model sections that should appear only after state makes them relevant. Section title colors accept `ConsoleColor`, reusable `Style` objects, style strings, direct `Color` / `RgbColor` values, or model selectors returning `Color` / `RgbColor`, and `Section(...)` / `SectionWhen(...)` also accept `Justify.Left`, `Justify.Center`, and `Justify.Right` for copied rule-title alignment snippets. That keeps quick dashboards visually clear without introducing a second view abstraction. Use `Consoul.Renderable(() => { ... })` when a one-off render action should participate in that reusable component flow without creating a custom entry class.
* **Scoped themes** – Wrap a view render, dashboard section, or sample workflow in `using (Consoul.UseTheme(ConsoulTheme.Modern))` to preview a cohesive style without permanently changing application-wide render options.
* **Choice callbacks** – Supply a `ChoiceCallback` to track navigation analytics, enforce authorisation, or log audit trails. Because the callback receives the executed option, you can centralise cross-cutting concerns instead of duplicating logic inside every handler.

## Form and object editing

`Consoul.Form(...)`, `DynamicView<T>.EditForm(...)`, `DynamicView<T>.AddEditFormOption(...)`, `StaticView.AddEditFormOption(...)`, and `EditObjectView` all use the same object-editor stack behind quick settings editors, exploratory object editors, and the earlier JSON-view work. The JSON terminology remains only as compatibility aliases for the same form document editor. It renders the target model as an annotated form document so users can arrow through property values before pressing <kbd>Enter</kbd> to edit. The value layout is JSON-style for scanability, and each line includes JavaScript-style comments sourced from `[Display]` metadata and the model's XML documentation (including `<see cref="..."/>` references). Use the left/right or up/down arrows—or the <kbd>Tab</kbd> key—to change the selection, and <kbd>Esc</kbd> to return to the menu.

For model-backed POC screens, call `.PropertyListSection("Summary")` for the common read-only model summary, `.RenderModel(model => new PropertyListEntry(...).AddModel(model))` when a custom summary object is clearer, and `.EditForm("Edit settings")` or `.AddEditFormOption("Edit settings")` to add the same editor as a menu option. For named static menus, store or inject the settings object and call `AddEditFormOption("Edit settings", settings)`. Use `Consoul.Form(model).Render()` only when you want to open the editor directly without a surrounding view. `EditModel(...)` and `Consoul.EditObject(...)` remain available as compatibility names for callers that already use object-editor terminology.

Use `Consoul.PropertyList(model)` for read-only summaries and `DynamicView<T>.ValidationSummarySection(..., title: "Validation")` before save actions. Use `ValidationSummarySectionWhen(...)` for post-action checks or advanced validation details that should appear only after model state makes them relevant. Both helpers reuse the same model metadata patterns as form editing, so POC views can render, validate, edit, and guard actions on one model without hand-wiring every property. `[Display(Order = ...)]` controls generated property order, and generated object-editor menus group related fields with `[Display(GroupName = "...")]` or `[Category("...")]`. `[Browsable(false)]`, `[Display(AutoGenerateField = false)]`, `[ScaffoldColumn(false)]`, and `[PropertyEditorIgnore]` hide properties from generated model surfaces, while `[ReadOnly(true)]` and `[Editable(false)]` remove properties from edit menus but keep them available for read-only summaries. Validation summaries include property attributes and object-level `IValidatableObject` results, so cross-field checks also disable `OptionWhenValid(...)` actions. `ValidationSummaryOptions.InvalidColor(...)`, `IssueColor(...)`, `ValidColor(...)`, and `ValidMessageColor(...)` accept `ConsoleColor`, reusable `Style` objects, or Spectre-style strings for the same palette used by prompts and status widgets. When validation output belongs in a reusable dashboard section, return or compose a `ValidationSummaryEntry` the same way you would a property list, table, panel, or chart.

### Metadata-aware prompts

When a property is selected the editor surfaces the resolved display name, description and summary before invoking an appropriate prompt. Generated property menu options also render those descriptions and enable filtering by default, so users can type `/owner`, `/path`, or another hint from metadata to find a field quickly. For simple text and numeric types the default prompt uses `TextPrompt<T>` so the property's current value is available as the Enter default, cancellation leaves the property unchanged, and DataAnnotations-style validation is enforced while the user edits. Frequent rules such as `[Required]`, `[Range]`, `[StringLength]`, `[MinLength]`, and `[MaxLength]` get purpose-built prompt hints, while other `ValidationAttribute` types such as `[RegularExpression]`, `[EmailAddress]`, `[Compare]`, or custom attributes are invoked through context-aware validation when available. Boolean properties use `ConfirmPrompt`, and enum properties use metadata-backed selection prompts with `[Display]`, `[DisplayName]`, or `[Description]` labels. Nullable boolean and enum properties use clearable selection prompts so users can intentionally unset optional values; `[Required]` suppresses that clear choice. String properties ending with `Path` automatically use `Consoul.PromptForFilepath`. Built-in editors, nested editors, and custom `IPropertyEditor` implementations receive the active render token through `PropertyEditContext.CancellationToken`. You can opt into other editors by decorating properties with `PropertyEditorAttribute`:

```csharp
public sealed class AdapterConfiguration
{
    [PropertyEditor(typeof(FilePathPropertyEditor))]
    [Display(Name = "Adapter", Description = "Full path to the adapter implementation.")]
    public string AdapterPath { get; set; } = string.Empty;
}
```

If you need to normalise values before assignment—for example trimming a partial path—implement `IPropertyValueFormatter` and apply `PropertyValueFormatterAttribute` to the property. Formatters receive the edit context so they can inspect the model, documentation and original value:

```csharp
public sealed class RelativePathFormatter : IPropertyValueFormatter
{
    public object Format(PropertyEditContext context, object value)
    {
        if (value is string text)
        {
            return Path.GetRelativePath(AppContext.BaseDirectory, text);
        }

        return value;
    }
}

public sealed class ScriptOptions
{
    [PropertyEditor(typeof(FilePathPropertyEditor))]
    [PropertyValueFormatter(typeof(RelativePathFormatter))]
    public string ScriptPath { get; set; } = string.Empty;
}
```

Complex types (objects, collections and dictionaries) still launch nested `EditObjectView` instances so you can drill into hierarchies while preserving the legacy menu options for power users.

### Metadata resolvers

Some properties surface values whose structure is only known at runtime—for example, dictionaries that map to constructor parameters in an external assembly. Decorate these members with `PropertyMetadataResolverAttribute` to plug in a custom resolver that supplies documentation, editors or formatters without relying on reflection against the runtime value.

```csharp
public sealed class OptionsMetadataResolver : IPropertyMetadataResolver
{
    public ResolvedPropertyMetadata Resolve(PropertyInfo property)
    {
        var documentation = new PropertyDocumentation(
            property,
            "Options",
            "Key/value pairs configuring the remote adapter.",
            () => "Each key should match a constructor parameter on the adapter type.");

        return new ResolvedPropertyMetadata(
            documentation,
            new DefaultPropertyEditor(),
            null);
    }
}

public sealed class AdapterConfiguration
{
    [PropertyMetadataResolver(typeof(OptionsMetadataResolver))]
    public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
}
```

Resolvers can construct rich guidance even when the property value references remote types, enabling consistent prompts and comments throughout the form document editor.

#### Layered editors

Some data surfaces as opaque `object` graphs—think JSON blobs, plugin settings loaded from another assembly, or dictionaries whose values describe constructor parameters. To keep these scenarios discoverable the resolver can also expose additional layers through `IPropertyLayerProvider`. Each layer describes a named editing surface with an optional description and a handler that receives the active `PropertyEditContext`. When the user presses <kbd>Enter</kbd> on the property, Consoul prompts whether to run the default editor or one of the custom layers.

Implementations can parse JSON Schema files, spin up another `EditObjectView`, or invoke bespoke editors for nested structures. The handler can mutate `context.CurrentValue` and return `true` to signal that the new value should be applied automatically. If the layer wants to persist the value directly it can call `context.ApplyValue` and still return `true` while marking the `PropertyEditLayer` as not applying the context value. Returning `false` now falls back to the default editor so layers can bootstrap data (for example by materialising dictionary entries) and then let the built-in prompts continue the session. Use `context.CancellationToken` when a layer renders prompts or nested editors so a canceled MVC-style view exits the whole object-edit flow cleanly.

```csharp
public sealed class OptionsMetadataResolver : IPropertyMetadataResolver
{
    public ResolvedPropertyMetadata Resolve(PropertyInfo property)
    {
        var documentation = new PropertyDocumentation(
            property,
            "Options",
            "Key/value pairs configuring the remote adapter.",
            () => "Each key should match a constructor parameter on the adapter type.");

        return new ResolvedPropertyMetadata(
            documentation,
            new DefaultPropertyEditor(),
            null,
            new SchemaLayerProvider());
    }
}

private sealed class SchemaLayerProvider : IPropertyLayerProvider
{
    public IEnumerable<PropertyEditLayer> GetLayers(PropertyEditContext context)
    {
        return new[]
        {
            new PropertyEditLayer(
                "Edit via schema",
                "Loads the remote JSON schema and opens a nested editor for the selected dictionary entry.",
                RunSchemaEditor,
                true)
        };
    }

    private static bool RunSchemaEditor(PropertyEditContext context)
    {
        // Locate the JSON schema based on the current model and open a tailored view.
        var view = new EditObjectView(JsonSchemaParser.BuildModel(context.CurrentValue));
        view.RenderAsync(context.CancellationToken).GetAwaiter().GetResult();
        context.CurrentValue = view.Model;
        return true; // the updated model will be applied by Consoul.
    }
}
```

Because layers participate in the same prompt flow, users can switch between schema-driven editors, remote type reflection and the built-in prompts without leaving the form document workspace.
