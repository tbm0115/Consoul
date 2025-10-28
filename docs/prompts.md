# Prompts

## When to use this
Use prompts when you want users to select from a list of discrete options, especially in command menus or configuration flows. Consoul prompts support default selections, per-option colours, and cancelation phrases such as “back” or “exit”.

## Key types
* `SelectionPrompt` – Base prompt for string-labelled options.
* `SelectionPrompt<T>` – Generic prompt that maps complex objects to display labels.
* `SelectOption` – Represents an individual option with colour and render style metadata.
* `PromptResult` – Returned by `SelectionPrompt.Render`, exposing `HasSelection`, `Index`, and `IsCanceled`.

## Minimal example
```csharp
var prompt = new SelectionPrompt("Select a database migration to run");
prompt.Add("Initial schema", ConsoleColor.Green, isDefault: true);
prompt.Add("Add auditing", ConsoleColor.Yellow);
prompt.Add("Drop legacy tables", ConsoleColor.Red);

PromptResult result = prompt.Render();
if (result.IsCanceled)
{
    Consoul.Write("No migration executed", ConsoleColor.DarkYellow);
}
else if (result.HasSelection)
{
    switch (result.Index)
    {
        case 0:
            RunInitialMigration();
            break;
        case 1:
            RunAuditMigration();
            break;
        case 2:
            RunDropMigration();
            break;
    }
}
```

## Advanced tips
* **Default option** – Set `isDefault: true` on an option to accept it when the user presses enter without typing anything.
* **Cancelation phrases** – Users can type “back”, “exit”, or “go back” to cancel. Check `PromptResult.IsCanceled` instead of comparing against sentinel values.
* **Generic prompts** – Use `SelectionPrompt<T>` with a `labelSelector` to work directly with domain objects rather than indexes.
* **Custom render styles** – `OptionRenderStyle` lets you switch between indexable, bullet, and custom rendering modes when presenting options.
