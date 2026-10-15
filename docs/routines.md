---
title: Scripted routines
description: Automate repeated console input with scripts and configuration transforms.
searchable: true
---
# Routines

## When to use this
Routines let you automate console interactions—perfect for demos, scripted smoke tests, or running canned data entry flows. They replay scripted keystrokes without requiring manual input.

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
    if (next.Delay.HasValue)
    {
        Thread.Sleep(next.Delay.Value);
    }
}
```

Routine inputs can select prompt options by label, shortcut, or registered hidden search term instead of hard-coding numeric positions:

```csharp
public sealed class DeployRoutine : Routine
{
    public DeployRoutine()
    {
        Enqueue(new RoutineInput
        {
            Value = "Deploy",
            Method = RoutineInput.InputMethod.OptionText
        });
    }
}

Routines.InitializeRoutine(new DeployRoutine(), "Deploy demo");

var prompt = new SelectionPrompt("Pick action");
prompt.Add("Preview");
int deploy = prompt.Add("Deploy");
prompt.AddSearchTerms(deploy, "ship", "release");

PromptResult result = prompt.Render();
```

XML routines can declare reusable variables at the document level and override them for a named routine. Inputs can then use `{{Token}}` placeholders without repeating transform metadata on every input:

```xml
<XmlRoutineDoc>
  <Meta>
    <Name>Deploy demo</Name>
    <DateCreated>2026-07-05T00:00:00Z</DateCreated>
    <UseDelays>false</UseDelays>
    <Variables>
      <Variable key="Environment" value="production" />
      <Variable>
        <Key>Region</Key>
        <Value>east</Value>
      </Variable>
    </Variables>
  </Meta>
  <Routines>
    <Routine name="Deploy">
      <Meta>
        <Description>Deploys a service.</Description>
        <Variables>
          <Service>api</Service>
        </Variables>
      </Meta>
      <Inputs>
        <Input>
          <Value>{{Environment}}/{{Service}}/{{Region}}</Value>
          <Method>Value</Method>
        </Input>
      </Inputs>
    </Routine>
  </Routines>
</XmlRoutineDoc>
```

Loading `new XmlRoutine(path, "Deploy")` replays `production/api/east`. Routine-level variables override document variables for that routine, which keeps shared smoke-test data in one place while allowing a specific branch to target another service or environment.

## Advanced tips
* **Integration with prompts** – Selection prompts register their options before reading input, and `Routines.RegisterOptions(prompt)` remains available for custom prompt flows. `RoutineInput.InputMethod.OptionText` resolves input against option labels, shortcuts, and hidden search terms, then feeds the current numeric option into prompt parsing. This allows automation scripts to remain resilient even if option ordering changes, and supports branching flows when the routine selects menu options based on dynamic state.
* **XML variables** - Use a document-level `<Variables>` block for values shared by several named routines, and a routine-level `<Variables>` block for branch-specific overrides. `XmlRoutine.SaveInputs(...)` writes `XmlRoutine.Variables` back to XML, so recorded demos can be cleaned up into reusable scripts without hand-editing every input value.
* **Recording** – Populate `Routines.UserInputs` and call `XmlRoutine.SaveInputs` to capture real sessions for later playback. Store these artefacts alongside your project to share reproducible demos with teammates, and check them into source control so they evolve with the codebase.
* **Delay handling & timing** – Respect `RoutineInput.Delay` to mimic the cadence of real users when replaying steps. Use varying delays to highlight loading states or to throttle API-driven workflows, and insert explicit waits before actions that depend on external resources (API calls, file operations) to stabilise demos.
* **Monitoring live input** – Flip `Routines.MonitorInputs` to `true` to accumulate what users type during manual runs. You can then export these via `XmlRoutine.SaveInputs` to create new scripted journeys, or analyse the captured inputs to identify common errors in onboarding.
* **Testing** – Combine routines with CI to validate interactive flows without relying on human testers. Pair with `Consoul.RenderOptions.WriteMode = WritePlainText` to keep CI logs readable while automation runs, and use environment-specific routine folders (e.g., `./routines/ci/`) to isolate flaky experiments from stable smoke tests.
* **Dependency injection** – Build small helper classes that wrap `Routines.InitializeRoutine` so you can inject scripted input providers during tests while keeping manual entry for production. This pattern mirrors Spectre.Console’s `IAnsiConsole` abstraction and helps you toggle between live and scripted inputs per environment.
