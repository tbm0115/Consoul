# Prompts

[Documentation index](index.md) · [Install the 3.0 preview](nuget.md) · [Migration guide](migration-3.0.md)

In the 3.0 preview, end-of-input cancels prompts without accepting a default. A real blank line still follows the configured empty-input/default behavior. Check `IsCanceled`, `HasSelection`, or `HasValue` before using the result; see [redirected-input migration](migration-3.0.md#handle-end-of-input-separately-from-a-blank-line).

## When to use this
Use prompts when you want users to select from a list of discrete options, especially in command menus or configuration flows. Consoul prompts support default selections, per-option colours, and cancelation phrases such as “back” or “exit”.

## Key types
* `AnsiConsole.Prompt(...)` - Spectre-style prompt entry point that renders Consoul prompts while preserving result-first outcomes.
* `SelectionPrompt` – Base prompt for string-labelled options.
* `SelectionPrompt<T>` – Generic prompt that maps complex objects to display labels and can return `SelectionResult<T>` through `RenderSelection(...)`.
* `Consoul.SelectEnum<TEnum>` / `Consoul.MultiSelectEnum<TEnum>` – Result-first enum selectors with metadata-backed labels and defaults.
* `MultiSelectionPrompt` / `MultiSelectionPrompt<T>` – Checkbox-style prompts for selecting multiple string-labelled or typed options.
* `TextPrompt<T>` – Typed text prompt with parsing, defaults, validation, and cancellation support.
* `Consoul.FilePath(...)` / `Consoul.DirectoryPath(...)` – Text prompt factories for normalized file and directory path input.
* `SecretPrompt` – Masked text prompt for passwords, API keys, and other sensitive values.
* `ConfirmPrompt` / `ConfirmationPrompt` – Yes/no confirmation prompts with defaults and cancellation support; `ConfirmationPrompt` mirrors common Spectre-style property names while still returning `ConfirmResult`.
* `SelectOption` – Represents an individual option with colour and render style metadata.
* `PromptResult` – Returned by `SelectionPrompt.Render`, exposing `HasSelection`, `Index`, and `IsCanceled`.
* `MultiSelectionResult` – Returned by `MultiSelectionPrompt.Render`, exposing selected indexes and cancellation state.
* `TextPromptResult<T>` – Returned by `TextPrompt<T>.Render`, exposing a typed value and cancellation state.
* `SecretResult` – Returned by `SecretPrompt.Render`, exposing `HasValue`, `Value`, and `IsCanceled`.
* `ConfirmResult` – Returned by confirmation prompts, exposing `HasValue`, `Value`, and `IsCanceled`.

## Minimal example
```csharp
var prompt = new SelectionPrompt("Select a database migration to run");
prompt.AddSeparator("Safe migrations");
prompt.Add("Initial schema", ConsoleColor.Green, isDefault: true);
prompt.Add("Add auditing", ConsoleColor.Yellow);
prompt.AddSeparator("Restricted migrations");
prompt.AddDisabledDescribed(
    "Drop legacy tables",
    "Removes data no longer referenced by the current schema.",
    "maintenance window required");

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

For quick menus, `Consoul.Select(...)` can mark a default option directly:

```csharp
PromptResult environment = Consoul.Select(
    "Pick environment",
    new[] { "Development", "Staging", "Production" },
    defaultOption: "Staging");
```

Use `SelectionPrompt<T>.RenderSelection(...)` when a typed prompt needs separators, disabled options, paging, filtering, keyboard navigation, or custom colors but should still return the selected domain object:

```csharp
var targetPrompt = new SelectionPrompt<DeployTarget>("Pick target", labelSelector: target => target.Name)
{
    FilterEnabled = true,
    KeyboardNavigationEnabled = true
}.WithDescriptions(target => $"Deploy slug: {target.Slug}");
targetPrompt.AddSeparator("Internal");
targetPrompt.Add(new DeployTarget("Development", "dev"));
targetPrompt.AddDisabled(new DeployTarget("Production", "prod"), "approval required");

SelectionResult<DeployTarget> target = targetPrompt.RenderSelection();
```

Typed prompt metadata is order-insensitive: `UseConverter(...)`, `WithDescriptions(...)`, and `WithSearchTerms(...)` can be called before or after adding typed choices or groups, which keeps MVC-style controller/view code flexible when choices are projected from a model first.

Prompt titles and messages for text, secret, confirm, confirmation, selection, and multi-selection prompts can contain raw markup such as `[green]Deploy[/] [[prod]]`. Developer-authored validation, required, invalid-choice, invalid-selection, filter-help, search-placeholder, paging-helper, and disabled-reason text can use the same markup. Rendering preserves parsed colors, RGB/hex metadata, ANSI-capable text decorations, and escaped brackets, while selection filtering searches the visible text instead of matching markup tags. Formatted filter/page values are escaped inside markup templates, and generated hints, defaults, yes/no choices, generated unavailable prefixes, and generated accepted-value messages remain literal so model values do not become accidental markup.

For snippets that start from Spectre.Console-style builders, the same prompt can be written with migration-friendly aliases. `Title(...)`, `PromptStyle(...)`, `InstructionsText(...)`, `InstructionStyle(...)`, `AddChoice(...)`, `AddChoices(...)`, `AddChoiceGroup(...)`, `AddGroup(...)`, `AddChild(...)`, `UseConverter(...)`, `SearchEnabled(...)`, `EnableSearch(...)`, `UseKeyboardNavigation(...)`, `UseWrapAround(...)`/`WrapAround(...)`, `WithPageSize(...)`/`PageSize(...)`, `WithMoreChoicesText(...)`/`MoreChoicesText(...)`, `WithSearchPlaceholderText(...)`/`SearchPlaceholderText(...)`, `FilterCommandPrefix(...)`, `FilterHelpText(...)`, `ActiveFilterText(...)`, `NoFilterMatchesText(...)`, `KeyboardNavigationHelpText(...)`, `KeyboardFilterHelpText(...)`, `WithInvalidSelectionMessage(...)`/`InvalidSelectionMessage(...)`, `DefaultValue(...)`, `OnCancel(...)`, the `CancelResult` property, `HighlightStyle(...)`, `DisabledStyle(...)`, `SearchHighlightStyle(...)`, and text-prompt `ParseWith(...)` all configure the existing prompt type rather than a separate rendering system. `InvalidChoiceMessage(...)` is also available as a Spectre-shaped alias for selection and multi-selection invalid input. Prompt color helpers accept `ConsoleColor` values, reusable `Style` objects, `Color` / `RgbColor` palette values, and foreground style strings; add `using ConsoulLibrary.Color;` for the shared palette aliases:

```csharp
var development = new DeployTarget("Development", "dev");
var production = new DeployTarget("Production", "prod");

var spectreStyleTarget = new SelectionPrompt<DeployTarget>()
    .Title("Pick target")
    .PromptStyle(Color.Aqua)
    .InstructionsText("Choose a deployment target.")
    .InstructionStyle(RgbColor.DarkGray)
    .UseConverter(target => target.Name)
    .WithDescriptions(target => $"Deploy slug: {target.Slug}")
    .EnableSearch()
    .UseKeyboardNavigation()
    .WrapAround(false)
    .PageSize(8)
    .MoreChoicesText("Showing {0}-{1} of {2}.")
    .SearchPlaceholderText("Search targets")
    .InvalidSelectionMessage("Choose a listed target.")
    .HighlightStyle(new Style(ConsoleColor.White, ConsoleColor.DarkBlue))
    .DisabledStyle(Color.Red)
    .SearchHighlightStyle(RgbColor.Yellow)
    .DefaultValue(production);

spectreStyleTarget.AddChoice(development);
spectreStyleTarget.AddChoice(production);

SelectionResult<DeployTarget> selectedTarget = AnsiConsole.Prompt(spectreStyleTarget);
```

Prompt builders implement `IPrompt<TResult>` for services that want to accept a prompt object without knowing its concrete type. Use `AnsiConsole.PromptAsync(...)`, `AnsiConsole.AskAsync(...)`, and `AnsiConsole.ConfirmAsync(...)` when an MVC-style controller action is already asynchronous; the result still exposes cancellation explicitly.

```csharp
IPrompt<SelectionResult<DeployTarget>> targetPrompt = spectreStyleTarget;
SelectionResult<DeployTarget> asyncTarget = await AnsiConsole.PromptAsync(targetPrompt);

TextPromptResult<int> retries = await AnsiConsole.AskAsync("Retries", 3);
ConfirmResult preview = await AnsiConsole.ConfirmAsync("Enable preview?", defaultValue: true);
```

Use shortcuts when a POC menu should behave like a command palette while keeping numbered fallback input. Shortcuts are displayed beside the option; routine input can type the shortcut, and keyboard prompts can press the same key.

```csharp
var actionPrompt = new SelectionPrompt("Pick deployment action")
{
    KeyboardNavigationEnabled = true
};
actionPrompt.AddShortcut("Preview", "p", ConsoleColor.Cyan);
actionPrompt.AddDescribedShortcut("Deploy", "Deploys the selected target.", "d", ConsoleColor.Green);
actionPrompt.AddDisabledShortcut("Destroy", "x", "approval required");

PromptResult action = actionPrompt.Render();
```

For long menus, set `PageSize`. Users can type `next`/`n` and `prev`/`p` to move between pages while option numbers stay stable across the full list. Turn on `FilterEnabled` when POC menus need quick narrowing; users can type `/text` to filter and `/clear` to restore the full list. Customize the filter command and helper copy with `FilterCommandPrefix(...)`, `FilterHelpText(...)`, `ActiveFilterText(...)`, and `NoFilterMatchesText(...)`. For a more interactive menu, set `KeyboardNavigationEnabled` so users can move with arrow keys, type to filter, press <kbd>Enter</kbd> to select, or press <kbd>Esc</kbd> to cancel. Use `KeyboardNavigationHelpText(...)` and `KeyboardFilterHelpText(...)` for keyboard-mode instructions, and use `InvalidSelectionMessage`, `WithInvalidSelectionMessage(...)`, or the Spectre-shaped `InvalidChoiceMessage(...)` alias when a POC menu needs friendlier copy than `Invalid selection!`.

```csharp
var longPrompt = new SelectionPrompt("Pick migration")
{
    PageSize = 8,
    FilterEnabled = true,
    KeyboardNavigationEnabled = true
};
longPrompt.AddSeparator("Safe migrations");
longPrompt.Add("Initial schema");
longPrompt.Add("Add auditing");
longPrompt.AddSeparator("Restricted migrations");
longPrompt.AddDisabled("Drop legacy tables", "maintenance window required");
int deployIndex = longPrompt.Add("Deploy");
longPrompt.AddSearchTerms(deployIndex, "ship", "release");

// Users can type "/audit" to show matching options, then choose the original number.
PromptResult migration = longPrompt.Render();
```

## Multi-selection

Use `MultiSelectionPrompt` or `Consoul.MultiSelect(...)` when users need to pick more than one item. Input accepts comma-separated indexes, ranges, `all`, and cancelation phrases. For interactive POC checklists, construct `MultiSelectionPrompt` directly and set `KeyboardNavigationEnabled = true`; users can move with arrows, press <kbd>Space</kbd> to toggle, press <kbd>Enter</kbd> to accept, and type to filter when `FilterEnabled` is also true.

```csharp
MultiSelectionResult result = Consoul.MultiSelect(
    "Select deployment stages",
    false,
    "Build",
    "Test",
    "Package",
    "Deploy");

if (result.HasSelection)
{
    foreach (int index in result.Indexes)
    {
        Consoul.Write($"Selected option {index + 1}");
    }
}
```

Collection overloads can mark Enter-to-accept defaults without manually building a prompt:

```csharp
MultiSelectionResult defaults = Consoul.MultiSelect(
    "Select deployment stages",
    new[] { "Build", "Test", "Package", "Deploy" },
    defaultOptions: new[] { "Build", "Deploy" });
```

Typed multi-selection maps selected indexes back to domain objects:

```csharp
MultiSelectionResult<DeployTarget> targets = Consoul.MultiSelect(
    "Select targets",
    target => target.Name,
    false,
    new DeployTarget("Development", "dev"),
    new DeployTarget("Production", "prod"));
```

For configurable typed checklists, use `MultiSelectionPrompt<T>.RenderSelection(...)`:

```csharp
var checklist = new MultiSelectionPrompt<DeployTarget>("Select targets", labelSelector: target => target.Name)
{
    FilterEnabled = true,
    KeyboardNavigationEnabled = true
}.WithDescriptions(target => $"Deploy slug: {target.Slug}");
checklist.Add(new DeployTarget("Development", "dev"), isDefault: true);
checklist.AddDisabled(new DeployTarget("Production", "prod"), "approval required");

MultiSelectionResult<DeployTarget> selectedTargets = checklist.RenderSelection();
```

Checklist metadata uses the same order-insensitive typed selectors, so a POC view can add items from the model and attach display labels for choices and group headings, descriptions, or hidden search terms afterward without rebuilding the prompt.

## Enum selection

Use `Consoul.SelectEnum<TEnum>(...)` for mode, environment, and log-level choices. Labels use `[Display(Name = "...")]`, `[DisplayName]`, or `[Description]` when present, then fall back to split PascalCase.

```csharp
SelectionResult<DeployEnvironment> environment =
    Consoul.SelectEnum<DeployEnvironment>(
        "Pick environment",
        defaultValue: DeployEnvironment.Staging);

if (environment.HasSelection)
{
    ConfigureEnvironment(environment.SelectedItem);
}
```

Filter the presented values or provide custom labels when a workflow should not expose every enum member:

```csharp
SelectionResult<DeployEnvironment> target = Consoul.SelectEnum(
    "Pick target",
    new[] { DeployEnvironment.Staging, DeployEnvironment.Production },
    value => value.ToString().ToUpperInvariant());
```

Use `Consoul.MultiSelectEnum<TEnum>(...)` when an enum-backed workflow needs several selected values, such as feature flags, deployment targets, or diagnostic categories:

```csharp
MultiSelectionResult<DeployEnvironment> targets =
    Consoul.MultiSelectEnum(
        "Pick deployment targets",
        new[] { DeployEnvironment.Development, DeployEnvironment.Staging, DeployEnvironment.Production },
        defaultValues: new[] { DeployEnvironment.Staging });

if (targets.HasSelection)
{
    foreach (DeployEnvironment target in targets.SelectedItems)
    {
        Consoul.Write($"Queued {target}");
    }
}
```

## Typed text input

Use `Consoul.Text<T>(...)` when a prompt needs parsing, defaults, validation, or cancellation-aware result handling.

```csharp
TextPromptResult<int> retries = Consoul.Text<int>("Retry count")
    .DefaultValue(3)
    .ValidationErrorMessage("Enter a whole number.")
    .Range(0, 10)
    .Render();

if (retries.HasValue)
{
    Consoul.Write($"Using {retries.Value} retries.");
}
```

Common POC form checks are available as fluent helpers, so simple view prompts can stay declarative:

```csharp
TextPromptResult<string> serviceKey = Consoul.Text<string>("Service key")
    .TrimInput()
    .Required()
    .LengthBetween(3, 24)
    .Render();

TextPromptResult<int> workerCount = Consoul.Text<int>("Worker count")
    .Minimum(1)
    .Maximum(8)
    .Render();
```

Built-in parsing covers strings, numbers, booleans with yes/no shorthand, enums, `Guid`, `TimeSpan`, `DateTime`, `DateTimeOffset`, `Uri`, and `Version` including nullable value types when empty input is allowed:

```csharp
TextPromptResult<Guid> operationId = Consoul.Text<Guid>("Operation id").Render();

TextPromptResult<TimeSpan> timeout = Consoul.Text<TimeSpan>("Timeout")
    .WithDefault(TimeSpan.FromSeconds(30))
    .Render();

TextPromptResult<Uri> endpoint = Consoul.Text<Uri>("API endpoint").Render();
```

For known values, use `AcceptOnly(...)` instead of writing a custom validator, or use the Spectre-shaped `AddChoice(...)` / `AddChoices(...)` methods when adapting pasted snippets. Accepted values are shown in the prompt by default and still return through `TextPromptResult<T>`:

```csharp
TextPromptResult<string> environment = Consoul.Text<string>("Environment")
    .AcceptOnly(new[] { "dev", "staging", "prod" }, comparer: StringComparer.OrdinalIgnoreCase)
    .Render();

TextPromptResult<DeployTarget> target = new TextPrompt<DeployTarget>("Target")
    .UseConverter(item => item.Name)
    .AddChoice(development)
    .AddChoices(staging, production)
    .Render();
```

Set `ShowChoices = false` or call `HideChoices()` for quieter prompts. Use `ShowDefaultValue(false)` or `HideDefaultValue()` when Enter should still accept a default without rendering it in the prompt. Call `EditableDefaultValue()` when keyboard users should start from an editable default buffer, and call `ClearOnFinish()` when a transient POC form should not leave the prompt text on screen after a successful entry. Call `Secret(...)` or set `IsSecret` when a typed prompt should mask input but still parse through `TextPrompt<T>`. Change `ChoicesSeparator`, call `ChoicesSeparator(...)`, or set `ChoicesStyle(...)` / `DefaultValueStyle(...)` / `MaskStyle(...)` when a view needs different hint styling. Prompt style helpers accept `ConsoleColor`, `Style`, style strings, `Color`, and `RgbColor` values. Pass a `StringComparer` to the `TextPrompt<T>` constructor when typed choice text should be case-sensitive or use a custom comparer. For object choices, `UseConverter(...)` controls both the rendered labels and display-text choice lookup.

Use path prompt factories when a POC command needs a normalized file or directory without writing boilerplate validation:

```csharp
TextPromptResult<string> script = Consoul.FilePath("Deployment script", mustExist: true)
    .Render();

TextPromptResult<string> output = Consoul.DirectoryPath("Output directory")
    .Render();

if (script.HasValue && output.HasValue)
{
    RunScript(script.Value, output.Value);
}
```

## Secret input

Use `Consoul.Secret(...)` or `Consoul.ReadSecret(...)` when a prompt should capture a password, API key, token, or connection secret without echoing typed characters.

```csharp
SecretResult apiKey = Consoul.Secret("API key")
    .MinimumLength(16, "API key must contain at least 16 characters.")
    .Render();

if (apiKey.HasValue)
{
    ConfigureClient(apiKey.Value);
}
```

To hide input without showing mask characters, set `MaskCharacter` to `null`:

```csharp
var prompt = Consoul.Secret("Access token");
prompt.MaskCharacter = null;
SecretResult token = prompt.Render();
```

The same prompt can be configured with builder aliases when adapting snippets:

```csharp
SecretResult token = Consoul.Secret("Token")
    .Title("Access token")
    .Mask('#')
    .MaskStyle("aqua")
    .RequiredErrorMessage("Token is required.")
    .Render();
```

## Confirmation

Use `Consoul.Confirm(...)` for yes/no decisions where cancellation should remain distinct from a negative answer.

```csharp
ConfirmResult deploy = Consoul.Confirm("Deploy to production?", defaultValue: false);

if (deploy.IsCanceled)
{
    Consoul.Write("Deployment canceled.", ConsoleColor.DarkYellow);
}
else if (deploy.Value)
{
    DeployProduction();
}
```

For custom colors or wording, create a configurable `ConfirmPrompt` through `Consoul.Confirmation(...)`:

```csharp
ConfirmResult preview = Consoul.Confirmation("Enable preview mode?")
    .DefaultValue(true)
    .PromptStyle("aqua")
    .ValidationErrorMessage("Use yes or no.")
    .Render();
```

When adapting Spectre-shaped snippets, `ConfirmationPrompt` exposes property-style defaults and custom yes/no characters while still flowing through `AnsiConsole.Prompt(...)` as a `ConfirmResult`:

```csharp
var deployPrompt = new ConfirmationPrompt("Deploy to production?")
{
    DefaultValue = false
}
.Yes('d')
.No('s')
.ChoicesStyle("blue")
.DefaultValueStyle("green")
.InvalidChoiceMessage("Choose d to deploy or s to skip.");

ConfirmResult deploy = AnsiConsole.Prompt(deployPrompt);
```

## Advanced tips
* **Builder aliases** - Use `Title(...)`, `PromptStyle(...)`, `Style(...)`, `InstructionStyle(...)`, `InstructionsText(...)`, `HighlightStyle(...)`, `DisabledStyle(...)`, `SearchHighlightStyle(...)`, `AddChoice(...)`, `AddChoices(...)`, `AddChoiceGroup(...)`, `AddGroup(...)`, `AddChild(...)`, `UseConverter(...)`, `EnableSearch(...)`, `UseKeyboardNavigation(...)`, `UseWrapAround(...)`, `WrapAround(...)`, `PageSize(...)`, `MoreChoicesText(...)`, `WithSearchPlaceholderText(...)`, `SearchPlaceholderText(...)`, `FilterCommandPrefix(...)`, `FilterHelpText(...)`, `ActiveFilterText(...)`, `NoFilterMatchesText(...)`, `KeyboardNavigationHelpText(...)`, `KeyboardFilterHelpText(...)`, `WithInvalidSelectionMessage(...)`, `InvalidSelectionMessage(...)`, `DefaultValue(...)`, `Select(...)`, `SelectAll()`, `OnCancel(...)`, the `CancelResult` property, `ParseWith(...)`, `ValidationErrorMessage(...)`, `InvalidChoiceMessage(...)`, `Yes(...)`, `No(...)`, `RequireEnter(...)`, `Secret(...)`, `Mask(...)`, `MaskStyle(...)`, `Required(...)`, `NotRequired(...)`, `AllowEmpty()`, `NotEmpty(...)`, `MinimumLength(...)`, `MaximumLength(...)`, `LengthBetween(...)`, `Choices(...)`, `ShowChoices(...)`, `HideChoices()`, `ShowDefaultValue(...)`, `HideDefaultValue()`, `EditableDefaultValue(...)`, `ClearOnFinish(...)`, `ChoicesSeparator(...)`, `ChoicesStyle(...)`, `DefaultValueStyle(...)`, and `Culture(...)` when adapting prompt snippets. Color helpers accept `ConsoleColor` values, reusable `Style` objects, `Color`, `RgbColor`, and foreground style strings such as `"grey"`, `"aqua"`, `"#22c55e"`, and `"white on navy"`. On `SelectionPrompt` and `MultiSelectionPrompt`, `InvalidChoiceMessage(...)` is an alias for invalid-selection feedback; on `ConfirmationPrompt` and text choices, it configures invalid choice copy. On `ConfirmationPrompt`, `ChoicesStyle(...)` styles non-default yes/no choices and `DefaultValueStyle(...)` styles the default choice. These methods set the existing Consoul prompt properties and still return result-first `PromptResult`, `SelectionResult<T>`, `MultiSelectionResult`, `TextPromptResult<T>`, `SecretResult`, and `ConfirmResult` values.
* **Invalid selection copy** - Use `InvalidSelectionMessage`, `WithInvalidSelectionMessage(...)`, `InvalidSelectionMessage(...)`, or the Spectre-shaped `InvalidChoiceMessage(...)` alias to customize malformed menu input for selection and multi-selection prompts. The default remains `Invalid selection!`, and raw markup in custom messages is parsed before the prompt repeats.
* **Prompt abstractions** - Use `IPrompt<TResult>` when a reusable service or MVC controller action should accept a configurable prompt and call `AnsiConsole.Prompt(...)` or `AnsiConsole.PromptAsync(...)` later. This keeps the prompt surface close to Spectre-style snippets while retaining Consoul's explicit result objects.
* **Value-style migration** - Prompt results can be assigned to their selected value for common pasted snippets such as `int port = AnsiConsole.Ask<int>("Port")`, `if (AnsiConsole.Confirm("Deploy?"))`, `string secret = Consoul.Secret("Token").Render()`, and `List<string> targets = AnsiConsole.Prompt(new MultiSelectionPrompt<string>().AddChoices(...))`. Prefer the result object when your view needs to distinguish cancellation from a default value.
* **Default option** – Set `isDefault: true` on an option, pass `defaultOption` to `Consoul.Select(...)`, or pass a default item/predicate to typed `Consoul.Select(...)` to accept that value when the user presses enter. Combine this with `PromptResult.HasSelection` to branch logic when users accept the default versus explicitly typing an index.
* **Grouped menus** – Use `AddSeparator(...)`, `SelectOption.Separator(...)`, or the Spectre-shaped `AddChoiceGroup(...)` alias to add display-only headings inside `SelectionPrompt` and `MultiSelectionPrompt`. Separators do not consume numeric indexes, cannot be selected, and are ignored by routine option registration. Use `AddGroup(...)`, `AddChild(...)`, or `prompt.AddGroup("Build").AddChild("Compile")` when a prompt needs tree-shaped choices. Returned `SelectOption` objects support `UseColor(...)`, `UseStyle(...)`, `UseDisabledColor(...)`, and `UseDisabledStyle(...)` so individual choices, groups, children, and separators can share the same palette objects as MVC-style views. `SelectionMode.Leaf` keeps single-selection group rows visible but requires selecting a child; multi-selection group input expands to selectable children. Set `Mode = SelectionMode.Independent` when the group itself should be returned as a selection.
* **Descriptive options** – Use `SelectOption.Describe(...)`, `AddDescribed(...)`, `AddDisabledDescribed(...)`, or `WithDescriptions(...)` on typed prompts when short labels need extra context. Descriptions render as subnotes under each option and are included in filtering, which helps users find actions by intent as well as by label. Raw markup in labels, descriptions, and disabled reasons is measured and filtered by visible text. Use `AddSearchTerms(...)` or typed `WithSearchTerms(...)` when an alias such as `ship` should match `Deploy` without rendering that alias in the menu.
* **Disabled options** – Use `AddDisabled(...)` when a menu action should remain visible but unavailable because model state is not ready. Disabled options render with a reason and cannot be selected; multi-selection prompts skip disabled options when users type `all`, and show the disabled reason when users explicitly type a disabled index. Disabled reasons can contain raw markup, and unavailable feedback keeps the generated prefix literal while parsing the developer-authored reason.
* **Paged prompts** – Set `PageSize` on `SelectionPrompt` or `MultiSelectionPrompt` when option lists are long. The prompt renders one page at a time and accepts `next`, `n`, `prev`, or `p` as navigation commands. Customize `MoreChoicesText` when your application needs different paging copy.
* **Filtered prompts** – Set `FilterEnabled = true` on `SelectionPrompt` or `MultiSelectionPrompt` to support `/text` and `/clear` commands. Filtering checks labels, descriptions, shortcuts, and hidden search terms. It changes what is visible but keeps the original numeric prefixes, so routine input and MVC-style view actions can continue to treat selected indexes as stable model/action indexes. Set `SearchHighlightStyle(...)` on selection and multi-selection prompts to color visible text that matches the active filter. In multi-selection prompts, `all` selects the matching options when a filter is active.
* **Keyboard menus** – Set `KeyboardNavigationEnabled = true` on `SelectionPrompt` or `MultiSelectionPrompt` when a POC menu should feel interactive. Routine input still uses the line-based number flow, while real terminal users can use arrows, <kbd>Home</kbd>/<kbd>End</kbd>, <kbd>Enter</kbd>, and <kbd>Esc</kbd>. Multi-selection prompts also use <kbd>Space</kbd> to toggle the highlighted option. When filtering is enabled, typed characters update the filter and <kbd>Backspace</kbd> edits it. Set `WrapAround = false`, call `UseWrapAround(false)`, or use the Spectre-shaped `WrapAround(false)` extension when arrow keys should clamp at the first and last available options; set `SearchPlaceholderText`, call `WithSearchPlaceholderText(...)`, or use `SearchPlaceholderText(...)` to customize empty keyboard search copy.
* **Cancelation phrases** – Users can type “back”, “exit”, or “go back” to cancel. Check `PromptResult.IsCanceled` instead of comparing against sentinel values, and honour cancelation inside your view or command loop. Typed prompts can use `OnCancel(...)` to expose a domain fallback on `SelectedItem` or `SelectedItems` while preserving the canceled result state. When wrapping prompts in `StaticView`/`DynamicView`, forward the cancel signal to `GoBack()` so navigation stays consistent.
* **Confirmation results** – `ConfirmResult.Value == false` means the user answered no; `ConfirmResult.IsCanceled` means the prompt was abandoned. Keep those branches separate in destructive workflows. In model-backed views, `DynamicView<T>.ConfirmOption(...)` wraps the same behavior around an option action.
* **Generic prompts** – Use `SelectionPrompt<T>.RenderSelection(...)` or `MultiSelectionPrompt<T>.RenderSelection(...)` to work directly with domain objects while still keeping separators, disabled options, paging, filtering, colors, and keyboard navigation. `UseConverter(...)`, `WithDescriptions(...)`, and `WithSearchTerms(...)` refresh existing typed choices and converter-backed group headings as well as future choices, so builder order can follow the natural shape of your MVC view code. `Render()` remains available when you explicitly want raw indexes.
* **Multi-selection defaults** – Set `isDefault: true` when adding options to `MultiSelectionPrompt`, call `Select(...)` / `SelectAll()` on a string or typed `MultiSelectionPrompt`, pass `defaultOptions` to `Consoul.MultiSelect(...)`, pass default items/predicates to typed `Consoul.MultiSelect(...)`, or pass `defaultValues` to `Consoul.MultiSelectEnum(...)` to accept those defaults on Enter. `Select(...)` and `SelectAll()` can run before or after `AddChoice(...)` / `AddChoices(...)`, which keeps default checklist state near the MVC view model that owns it while still accepting pasted builder snippets. Set `allowEmpty: true` when “none” or an empty response should be valid.
* **Typed text validation** – `TextPrompt<T>.Validate` can return a message when a value should be rejected, accept a predicate plus message for common checks, or return `PromptValidationResult.Success()` / `PromptValidationResult.Error(...)` for structured success/error validation. Use `TrimInput()`, `Required()`, `NotEmpty(...)`, `Minimum(...)`, `Maximum(...)`, and `Range(...)` for frequent POC form rules; string prompts also support `MinimumLength(...)`, `MaximumLength(...)`, and `LengthBetween(...)`. Built-in parsing covers common inputs such as GUIDs, durations, dates, endpoints, versions, and enums. Set `Culture` or call `Culture(...)` when decimal, date, or duration input should parse and render defaults with a specific locale. Use `AcceptOnly(...)`, `AddChoice(...)`, `AddChoices(...)`, or the Spectre-shaped `Choices(...)` alias for known values; accepted values render as prompt choices unless `ShowChoices` is disabled or `HideChoices()` is called, and `UseConverter(...)` lets object choices render and resolve by display text. Pass a `StringComparer` into the `TextPrompt<T>` constructor when accepted choice text should use exact casing or another comparer. Use `InvalidChoiceMessage(...)` when object-backed choices need custom copy for unmatched display text. Use `ShowDefaultValue(false)` or `HideDefaultValue()` to keep Enter-to-accept defaults quiet, call `EditableDefaultValue()` when users should edit a seeded default, call `ClearOnFinish()` for transient form prompts, call `Secret(...)` for masked typed input, and use `ChoicesStyle(...)`, `DefaultValueStyle(...)`, or `MaskStyle(...)` for colored hint and mask text. Use `AllowEmpty()` or `NotRequired()` when an optional prompt should return an empty/default typed value, and use `ParseWith(...)` for domain-specific values such as slugs, file modes, or connection names. Use `Consoul.FilePath(...)` and `Consoul.DirectoryPath(...)` when the domain value is a path; they trim pasted quotes and can require an existing file or directory.
* **Secret validation** – `SecretPrompt.Validate(...)` and `MinimumLength(...)` repeat masked input until the value passes without echoing rejected secrets to the console.
* **Custom render styles** – `OptionRenderStyle.Checkbox` remains available for custom prompt derivatives. For ordinary checkbox-style input, prefer `MultiSelectionPrompt` so parsing, cancellation, and selected indexes are handled consistently.
* **Clearing vs. appending** – Toggle the `clear` constructor argument or `ClearConsole` property to control whether the prompt redraws the screen. This is helpful when prompts live inside views that manage their own buffers or when you want to preserve command history above the prompt.
* **Input preprocessing** – Derive from `SelectionPrompt` and override `Render` when you need to normalise user input—uppercase commands, trim whitespace, or support synonyms—before the base implementation attempts to parse selections.
* **Accessibility** – Provide numeric prefixes in labels (e.g., `"1) Start"`) so screen readers or copy/paste workflows can identify options quickly, and favour high-contrast colour schemes using `SelectOption.Color` for low-vision scenarios.
