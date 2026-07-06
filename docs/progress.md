# Progress

## When to use this
Choose the progress APIs when you need to show active work such as downloads, migrations, processing queues, short status updates, or quick busy indicators. Consoul’s spinner, status, and progress helpers work well in environments where full-screen UIs are impractical.

## Key types
* `SpinnerScope` – Maintains a small in-place spinner for short operations without known progress.
* `SpinnerOptions` – Configures frames, colors, width, and final success/failure prefixes.
* `StatusScope` – Maintains a short in-place status message for an operation.
* `StatusOptions` – Configures status prefixes, colors, width, and refresh timing.
* `ProgressContext` – Maintains multiple in-place task rows for a workflow.
* `ProgressTask` – Represents one task row in a `ProgressContext`.
* `ProgressTaskState` – Describes whether a progress task is pending, active, completed, or failed.
* `ProgressTaskSettings` – Configures max value, auto-start behavior, and indeterminate mode when adding a task.
* `LiveDisplayContext` – Repeatedly refreshes a dashboard or model-backed view.
* `LiveDisplayOptions` – Configures live display titles, clearing behavior, and completion colors.
* `ProgressBar` – Tracks total units and renders textual progress bars.
* `FixedMessage` – Underlies the progress bar layout by reserving buffer positions.
* `RenderOptions` – Provides default colours for completed and remaining segments.

## Minimal example
```csharp
Consoul.Status("Preparing migration", status =>
{
    status.Update("Checking schema");
    status.Update("Applying changes");
});

Consoul.Spinner("Loading package metadata", spinner =>
{
    spinner.Tick("Reading manifest");
    spinner.Tick("Resolving dependencies");
    spinner.Tick("Ready");
});

Consoul.Progress(progress =>
{
    ProgressTask schema = progress.AddTask("Schema", 2);
    ProgressTask data = progress.AddTask("Data", 3);

    schema.Increment(1);
    data.Increment(1);
    schema.Complete();
    data.Complete();
});

Consoul.Live(live =>
{
    int queued = 3;
    live.Refresh(() =>
    {
        Consoul.PropertyList(list =>
        {
            list.Add("Queued", queued, ConsoleColor.Yellow);
            list.Add("Running", 1, ConsoleColor.Cyan);
        });
    });

    queued--;
    live.Refresh(() =>
    {
        Consoul.PropertyList(list =>
        {
            list.Add("Queued", queued, ConsoleColor.Yellow);
            list.Add("Running", 2, ConsoleColor.Cyan);
        });
    });
}, new LiveDisplayOptions { Title = "Queue" });

var bar = new ProgressBar("Synchronising data")
{
    BarWidth = 80,
    BlockCharacter = '#',
    EmptyCharacter = '-',
    ShowTrack = true,
    ShowPercentage = true
};

for (int i = 0; i <= 10; i++)
{
    double progress = i / 10d;
    bar.Update(
        progress,
        message: $"Processed {i * 100} items",
        messageColor: Color.Aqua,
        barColor: RgbColor.Green);
    Thread.Sleep(150);
}
```

For migration-friendly POC code, the `AnsiConsole` facade exposes familiar `Start(...)` wrappers while keeping the same Consoul rendering and view-model flow:

```csharp
int appliedSteps = AnsiConsole.Status()
    .Spinner(Spinner.Known.Star)
    .SpinnerStyle(RgbColor.Yellow)
    .Start("Preparing migration", status =>
    {
        status.Status("Checking schema");
        status.Status("Applying changes");
        return 2;
    });

AnsiConsole.Status()
    .RefreshRate(TimeSpan.FromMilliseconds(100))
    .AutoRefresh(false)
    .Start("Manual refresh", status =>
    {
        status.Status("Checking cache");
        status.Refresh();
    });

await AnsiConsole.Status()
    .StartContextAsync("Downloading", async status =>
    {
        status.Spinner = Spinner.Known.Dots;
        status.SpinnerStyle(Color.Aqua).Status("Resolving packages");
        await Task.Delay(1);
        status.Succeed("Packages ready");
    });

await AnsiConsole.ProgressAsync(async progress =>
{
    ProgressTask schema = progress.AddTask("Schema", 2);
    await Task.Delay(1);
    schema.Complete();
});

int processed = await AnsiConsole.ProgressAsync(async progress =>
{
    ProgressTask publish = progress.AddTask("Publish", 3);
    await Task.Delay(1);
    publish.Complete();
    return progress.CompletedTaskCount;
});

AnsiConsole.Live(new LiveDisplayOptions { Title = "Queue" }).Start(live =>
{
    live.Refresh(() => Consoul.Write("Queue ready"));
    live.Succeed("Done");
});
```

`Spinner.Known` includes common migration aliases such as `Pipe`, `Line2`, `SimpleDots`, `SimpleDotsScrolling`, `Dots2` through `Dots12`, `Star2`, `Flip`, `Hamburger`, `GrowVertical`, `GrowHorizontal`, `Balloon`, `Noise`, `Bounce`, `BoxBounce`, `BouncingBar`, `BouncingBall`, `Arrow`, `Triangle`, `Circle`, `Toggle`, `Clock`, `Moon`, `Pong`, and `BetaWave` so quick status POCs can reuse familiar spinner names while staying dependency-free.

Spinner, status, and single progress-bar messages can contain raw Spectre-style markup. Frames, active prefixes, success/failure prefixes, and bar glyphs remain literal, while the message body preserves parsed colors and escaped bracket text:

```csharp
Consoul.Status("[yellow]Preparing[/] [[prod]]", status =>
{
    status.Update("[cyan]Checking schema[/] [[prod]]");
});

Consoul.Spinner("[green]Loading[/] packages", spinner =>
{
    spinner.Tick("[cyan]Resolving[/] dependencies");
});

using var bar = new ProgressBar("[green]Starting[/] [[prod]]");
bar.Update(0.5, "[cyan]Halfway[/] [[prod]]");
```

`ProgressContext.AddTask(...)` keeps Consoul's existing `AddTask("Build", 4)` shape and also accepts Spectre-style `autoStart` and `maxValue` parameters for pasted snippets:

```csharp
ProgressTask queued = progress.AddTask("Download package", autoStart: false, maxValue: 2048);
queued.StartTask();
queued.Value = 1024;
queued.StopTask();
```

`ProgressTask` supports migration-friendly direct assignment for `Description`, `Value`, `MaxValue`, `Tag`, and `HideWhenCompleted`, plus `Id`, `StartedAt`, `LastUpdatedAt`, `StartTime`, `StopTime`, `Elapsed`, `ElapsedTime`, `RemainingTime`, `Percentage`, `Speed`, `IsStarted`, `IsFinished`, `StartTask()`, `StopTask()`, and `IsIndeterminate(...)`. Property-named helpers such as `task.Description("Download").MaxValue(2048).Value(1024).Tag("nuget").HideWhenCompleted()` are available for chain-oriented pasted snippets. Tasks created without an explicit max value default to `100`, which matches common progress examples. Use `ProgressTaskSettings` when pasted snippets configure a task at creation time:

```csharp
ProgressTask download = progress.AddTask(
    "Download package",
    new ProgressTaskSettings
    {
        AutoStart = false,
        MaxValue = 2048,
        IsIndeterminate = true
    });

download.Start();
download.IsIndeterminate(false);
download.Value = 1024;
download.Complete("Package ready");
```

Task descriptions can contain raw Spectre-style markup in both the default progress row and `TaskDescriptionColumn` rows. Consoul trims and pads by visible text, renders parsed colors, and keeps escaped brackets such as `[[prod]]` literal:

```csharp
ProgressTask publish = progress.AddTask("[green]Build[/] for [[prod]]", 4);
publish.Update(2);
publish.Complete("[cyan]Build ready[/] for [[prod]]");
```

Use ordered insertion when your controller discovers work out of order but the terminal view should stay grouped in a predictable sequence:

```csharp
ProgressTask build = progress.AddTask("Build", autoStart: false);
ProgressTask deploy = progress.AddTaskAfter("Deploy", build, autoStart: false);
ProgressTask restore = progress.AddTaskBefore("Restore", build, autoStart: false);
ProgressTask test = progress.AddTaskAt("Test", 2, autoStart: false);

restore.StartTask();
build.StartTask();
test.StartTask();
deploy.StartTask();

progress.RemoveTask(build);
progress.Refresh();
```

Column configuration drives Consoul's responsive row renderer: include only the columns you want to see, in the order you want them rendered.

```csharp
int processed = AnsiConsole.Progress()
    .Columns(
        new TaskDescriptionColumn(),
        new ProgressBarColumn { Width = 24 }
            .CompleteColor(Color.Green)
            .RemainingColor(RgbColor.DarkGray),
        new PercentageColumn())
    .RefreshRate(TimeSpan.FromMilliseconds(100))
    .AutoRefresh(false)
    .HideCompleted(true)
    .Start(progress =>
    {
        ProgressTask task = progress.AddTask("Processing records");
        task.IsIndeterminate();
        task.Description = "Discovering total";
        progress.Refresh();

        task.IsIndeterminate(false);
        task.MaxValue = 10;
        task.Value = 5;
        task.Description = "Processing files";
        progress.Refresh();

        task.Value = 10;
        return 10;
    });
```

Available progress columns include `TaskDescriptionColumn`, `ProgressBarColumn`, `PercentageColumn`, `SpinnerColumn`, `TextColumn`, `DownloadedColumn`, `TransferSpeedColumn`, `RemainingTimeColumn`, and `ElapsedTimeColumn`. `TextColumn` renders literal text or task templates such as `{task.Description}`, `{task.Value}`, `{task.MaxValue}`, `{task.Percentage}`, `{task.State}`, `{task.ElapsedTime}`, `{task.RemainingTime}`, `{task.Speed}`, and `{task.Tag}`; the text can contain raw markup and still shrinks inside narrow terminals. `ProgressBarColumn.Width` is a preferred bar-body width that still shrinks inside narrow terminals. `CompleteStyle`/`CompletedStyle`, `FinishedStyle`, `RemainingStyle`, `IndeterminateStyle`, `TextColumn.Style`, and `SpinnerColumn.Style` are honored when configured, and `CompleteColor(...)`, `FinishedColor(...)`, `RemainingColor(...)`, `IndeterminateColor(...)`, `TextColor(...)`, and `SpinnerStyle(...)` accept `ConsoleColor`, `Color`, and `RgbColor` values for quick MVC-style palette wiring. `UseRenderHook(...)` receives the generated row renderable plus the current task list and can return a replacement renderable; Consoul captures that replacement back into the fixed progress row width, so custom POC progress rows stay bounded. Omitted columns stay omitted, so compact POC screens can show only a bar or only text and percentage:

```csharp
AnsiConsole.Progress()
    .Columns(
        new SpinnerColumn { Spinner = Spinner.Known.Dots },
        new TaskDescriptionColumn(),
        new TextColumn("[grey]{task.Value}/{task.MaxValue}[/]"),
        new DownloadedColumn(),
        new TransferSpeedColumn(),
        new RemainingTimeColumn(),
        new ElapsedTimeColumn())
    .Start(progress =>
    {
        ProgressTask download = progress.AddTask("Download package", 2048);
        download.Value = 1024;
    });
```

Progress tasks can also model failures and retries without leaving your MVC-style workflow code:

```csharp
Consoul.Progress(progress =>
{
    ProgressTask publish = progress.AddTask("Publish package", 3);

    publish.Increment(1);
    publish.Fail("Publish failed");

    if (progress.HasFailures)
    {
        Consoul.Write("Some tasks need attention", ConsoleColor.Red);
    }

    publish.Start("Retry publish");
    publish.Complete("Publish ready");
});
```

Live dashboards can also use a target-based `AnsiConsole.Live(...)` shape for migration-friendly POC code. Mutate a renderable and call `Refresh()`, or replace it with `UpdateTarget(...)`; result-returning `Start(...)` and `StartAsync(...)` overloads are available. Static and injected-console async helpers accept quick `Func<..., Task>` and result-returning `Func<..., Task<T>>` delegates for first-run POCs as well as token-aware delegates for cancellable workflows.

```csharp
var status = new Table()
    .Width(42)
    .HideFooter()
    .AddColumns("Service", "State")
    .AddRow("API", "Starting");

int refreshes = AnsiConsole.Live(status)
    .Overflow(VerticalOverflow.Crop)
    .Cropping(VerticalOverflowCropping.Top)
    .AutoClear(true)
    .Start(live =>
    {
        status.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
        live.Refresh();

        status.Rows.Clear();
        status.Rows.Add("Worker", "Online");
        live.Refresh();
        return live.RefreshCount;
    });
```

## Advanced tips
* **Responsive single bars** - Set `BarWidth` to your preferred maximum; `ProgressBar` clamps the rendered width to the current terminal on every update. Enable `ShowTrack` and `ShowPercentage` when you want a complete bounded row for quick POC dashboards. `Update(...)` accepts `ConsoleColor`, `Color`, and `RgbColor` values for quick warning/success states, while `UpdateStyled(...)`, `StatusScope.UpdateStyled(...)`, and `FixedMessage.RenderStyled(...)` accept reusable `Style` objects when a controller owns foreground/background styling.
* **Spinners** – Use `Consoul.Spinner` when work is active but you do not have a reliable percentage. Call `Tick()` as phases complete, or let the action overload mark success/failure automatically after the delegate exits.
* **Status scopes** – Use `Consoul.Status` for short operations where a full progress bar would be noise. The action overload automatically marks success, marks failure before rethrowing exceptions, and still lets you call `Succeed` or `Fail` yourself for custom final text. Use `AnsiConsole.Status().StartContext(...)` or `StartContextAsync(...)` when a migrated snippet prefers property-style `Status`, `Spinner`, and `SpinnerStyle` updates; `StatusContext` also exposes `Succeed(...)` and `Fail(...)` directly. `StatusScope.Update(...)`, `StatusScope.Status(...)`, and `StatusContext.Status(...)` accept `ConsoleColor`, `Color`, and `RgbColor` values for direct message color changes. `RefreshRate(...)` helpers on status builders/scopes and spinner builders/scopes accept milliseconds or `TimeSpan` values for throttled manual-refresh POCs. `SpinnerStyle(...)` helpers on status builders, status scopes, status contexts, spinner builders, and spinner scopes accept `ConsoleColor`, reusable `Style` objects, style strings, `Color`, and `RgbColor` values. `StatusAsync`, `SpinnerAsync`, `ProgressAsync`, and `LiveAsync` accept both `async context => ...` and `(context, cancellationToken) => ...` delegate shapes, including result-returning `Task<T>` delegates for MVC-style controller actions that compute a value while rendering scoped feedback.
* **Progress contexts** – Use `Consoul.Progress` when a workflow has several task rows. Each `ProgressTask` supports `Update`, `Increment`, `Complete`, `Fail`, `Start`, `StartTask`, `StopTask`, `IsStarted`, task metadata such as `Id`, `Tag`, `Percentage`, `Speed`, `ElapsedTime`, and `RemainingTime`, and creation through `AddTask(..., autoStart, maxValue)` or `ProgressTaskSettings`. Set `HideWhenCompleted` on one task to override the context-wide `HideCompleted(...)` setting. Use `RefreshRate(...)`, `AddTaskAt`, `AddTaskBefore`, `AddTaskAfter`, and `RemoveTask` when a controller discovers work dynamically but the view should remain grouped and redraws should be throttled. The context exposes `CompletedTaskCount`, `FailedTaskCount`, `HasFailures`, and `IsFinished` for view-model decisions, and disposes its fixed rows automatically when the action exits.
* **Live displays** – Use `Consoul.Live` when a POC dashboard should redraw a model snapshot instead of updating one fixed row. Keep the render action side-effect free where possible, mutate the model between refreshes, and compose normal helpers such as `PropertyList`, `Table`, `Grid`, or charts inside `live.Refresh(...)`.
* **Fractional updates** – The `Update(double progress, …)` overload expects a 0–1 value and clamps out-of-range, NaN, and infinity values before rendering. Pair the fraction with a message to surface ETA or processed counts, and round your display text to avoid jitter from floating-point noise.
* **Minimal width & resizing** – Set `BarWidth` to a preferred maximum and let the bar clamp itself against the current terminal on each update. The rendered row stays bounded even when the terminal is narrower than the preferred width.
* **Color overrides & glyphs** – Pass explicit colors into `Update` for special states (warning, error) without changing global render options, or pass reusable `Style` objects into `UpdateStyled` when your MVC-style view model already carries foreground/background choices. Swap `BlockCharacter` to reflect success/failure with ASCII glyphs such as `#`, `=`, or `█`, and align your color choices with `RenderOptions.DefaultScheme` so the bar respects your broader theme.
* **Integration with tasks** – Combine progress bars with views to create dashboards that refresh status in response to user input. `ProgressBar` can be fed by background `Task` callbacks—just coordinate access if multiple threads update the same instance and throttle updates to avoid flicker on high-frequency jobs.
* **Multiple bars** – Instantiate multiple `ProgressBar` instances with different labels to track concurrent operations. Render them sequentially and refresh in-place by reusing each bar instance instead of recreating them for every update; consider grouping related bars with `Consoul.Write` headings for readability.
* **Completion hooks** – Listen for task completion and call `Update(1, message: "Completed")` to signal that the bar is finished, then optionally render a summary line and switch the bar’s colour scheme to a success or failure tone.
