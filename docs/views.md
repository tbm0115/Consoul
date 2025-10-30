# Views

## When to use this
Views are ideal for multi-step workflows—dashboards, menus, or game scenes—where you want to encapsulate options and state per screen. They integrate prompts with lifecycle events so you can build entire applications from composable classes.

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

`StaticView` and `DynamicView<T>` expose protected helpers to request navigation without manually instantiating other views. Call `NavigateTo<TView>()` to replace the current view (the default), `NavigateTo<TView>(replace: false)` to push a new view on the stack, `NavigateTo(() => new DetailView(arg))` when you need to supply constructor parameters, or `GoBack()` to pop to the previous screen. The renderer executes these requests inside an iterative loop, preventing recursive render chains and the stack overflows that could occur when options previously invoked `new OtherView().Render()`.

## Advanced tips
* **Attribute-driven options** – Decorate methods with `ViewOptionAttribute` or `DynamicViewOptionAttribute` to declare choices alongside their logic. Attribute metadata can control colour, ordering, and whether an option is hidden until a predicate returns true; leverage these hooks to gate admin-only commands or feature flags.
* **Go back behaviour** – Each view automatically appends a “Go back” option and now uses `PromptResult` cancelation rather than the legacy `Consoul.EscapeIndex` constant. Customise the label via `ViewAttribute.GoBackMessage` or by changing `RenderOptions.DefaultGoBackMessage`, and check `GoBackRequested` after `RenderAsync` returns to run cleanup when a user leaves mid-operation.
* **Async rendering** – Use `RenderAsync()` on dynamic views when options trigger asynchronous operations; the framework awaits each action before re-rendering. Combine this with `CancellationToken` parameters on option handlers to respond to user aborts or window closures, and surface progress updates by re-rendering interim messages via `Consoul.Write`.
* **Shared state & navigation** – Pass dependencies via constructors or properties; views are regular classes and can store fields, timers, or other stateful components. Use `NavigateTo<TView>()` (or `NavigateTo<TView>(replace: false)` when you need to push) or `GoBack()` inside an option to request transitions; the renderer now applies these commands iteratively so the call stack stays flat even through long navigation chains. Inject shared services (logging, data providers) to keep options thin and testable.
* **Composed dashboards** – `DynamicView` options can render tables or progress bars inline, then rehydrate their options list after completing background work. This makes it easy to build monitoring dashboards driven by timers or file watchers; schedule refreshes with `System.Threading.Timer` to trigger `RenderAsync()` when data changes.
* **Choice callbacks** – Supply a `ChoiceCallback` to track navigation analytics, enforce authorisation, or log audit trails. Because the callback receives the executed option, you can centralise cross-cutting concerns instead of duplicating logic inside every handler.

## Object editing

`EditObjectView` now renders the target model as annotated JSON so users can arrow through property values before pressing <kbd>Enter</kbd> to edit. Each line includes JavaScript-style comments sourced from `[Display]` metadata and the model's XML documentation (including `<see cref="..."/>` references). Use the left/right or up/down arrows—or the <kbd>Tab</kbd> key—to change the selection, and <kbd>Esc</kbd> to return to the menu.

### Metadata-aware prompts

When a property is selected the editor surfaces the resolved display name, description and summary before invoking an appropriate prompt. For simple types the default prompt respects the property's type; string properties ending with `Path` automatically use `Consoul.PromptForFilepath`. You can opt into other editors by decorating properties with `PropertyEditorAttribute`:

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
    public object? Format(PropertyEditContext context, object? value)
        => value is string text
            ? Path.GetRelativePath(AppContext.BaseDirectory, text)
            : value;
}

public sealed class ScriptOptions
{
    [PropertyEditor(typeof(FilePathPropertyEditor))]
    [PropertyValueFormatter(typeof(RelativePathFormatter))]
    public string ScriptPath { get; set; } = string.Empty;
}
```

Complex types (objects, collections and dictionaries) still launch nested `EditObjectView` instances so you can drill into hierarchies while preserving the legacy menu options for power users.
