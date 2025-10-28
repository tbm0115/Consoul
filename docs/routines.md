# Routines

## When to use this
Routines let you automate console interactions—perfect for demos, scripted smoke tests, or running canned data entry flows. They can watch for file changes and replay sequences without requiring manual input.

## Key types
* `Routines` – Central registry that loads and executes routines from XML definitions and tracks buffered inputs.
* `Routine` / `XmlRoutine` – Provide queue-like storage for scripted inputs and metadata such as delays.
* `RoutineInput` – Represents a single keystroke or line of text to replay.

## Minimal example
```csharp
// Load an XML routine and prime the input buffer
var routine = new XmlRoutine("./scripts/demo.xml");
Routines.InitializeRoutine(routine, routine.Name);

while (Routines.HasBuffer())
{
    RoutineInput next = Routines.Next();
    Consoul.Write($"Replaying input: {next.Value}", ConsoleColor.DarkCyan);
    Thread.Sleep(next.Delay);
}
```

## Advanced tips
* **Integration with prompts** – Register prompts with `Routines.RegisterOptions(prompt)` so scripted routines can select options by index or label.
* **Recording** – Populate `Routines.UserInputs` and call `XmlRoutine.SaveInputs` to capture real sessions for later playback.
* **Delay handling** – Respect `RoutineInput.Delay` to mimic the cadence of real users when replaying steps.
* **Testing** – Combine routines with CI to validate interactive flows without relying on human testers.
