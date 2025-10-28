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
        Consoul.Write("High scores unavailable", ConsoleColor.Yellow);
    }

    [ViewOption("Exit", ConsoleColor.DarkGray)]
    private void Exit()
    {
        Consoul.Write("Goodbye!", ConsoleColor.Gray);
        GoBack();
    }
}
```

## Advanced tips
* **Attribute-driven options** – Decorate methods with `ViewOptionAttribute` or `DynamicViewOptionAttribute` to declare choices alongside their logic. Attribute metadata can control colour, ordering, and whether an option is hidden until a predicate returns true.
* **Go back behaviour** – Each view automatically appends a “Go back” option and now uses `PromptResult` cancelation rather than the legacy `Consoul.EscapeIndex` constant. Customise the label via `ViewAttribute.GoBackMessage` or by changing `RenderOptions.DefaultGoBackMessage`.
* **Async rendering** – Use `RenderAsync()` on dynamic views when options trigger asynchronous operations; the framework awaits each action before re-rendering. Combine this with `CancellationToken` parameters on option handlers to respond to user aborts or window closures.
* **Shared state & navigation** – Pass dependencies via constructors or properties; views are regular classes and can store fields, timers, or other stateful components. Invoke `Render()`/`RenderAsync()` on another view inside an option to chain screens, or set `GoBackRequested` to unwind to a parent loop.
* **Composed dashboards** – `DynamicView` options can render tables or progress bars inline, then rehydrate their options list after completing background work. This makes it easy to build monitoring dashboards driven by timers or file watchers.
