using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Color;

namespace ConsoulLibrary.Test.Views
{
    /// <summary>
    /// Demonstrates building a menu view without subclassing <see cref="StaticView"/>.
    /// </summary>
    public class PocMenuView : IView
    {
        private readonly DynamicView<object> _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocMenuView"/> class.
        /// </summary>
        public PocMenuView()
        {
            _view = Consoul.View("5-Minute POC View")
                .Option("Say hello", () =>
                {
                    Consoul.MarkupLine("Hello from a [green]dynamic proof-of-concept view[/].");
                    Consoul.Wait();
                })
                .Option("Preview modern theme", PreviewModernTheme)
                .Option("Preview exception panel", PreviewExceptionPanel)
                .Navigate<PocCounterView>("Try a model-backed view")
                .Navigate<PocCanvasView>("Show a canvas")
                .Navigate<PocChecklistView>("Show a checklist")
                .Navigate<PocBulletListView>("Show bullet lists")
                .Navigate<PocTableView>("Show a table")
                .Navigate<PocMetricsView>("Show metrics")
                .Navigate<PocSparklineView>("Show a sparkline")
                .Navigate<PocTreeView>("Show a tree")
                .Navigate<PocTimelineView>("Show a timeline")
                .Navigate<PocRowsView>("Show stacked rows")
                .Navigate<PocPadView>("Show padded content")
                .Navigate<PocAlignView>("Show aligned content")
                .Navigate<PocPanelView>("Show composed panels")
                .Navigate<PocCalloutView>("Show callouts")
                .Navigate<PocBadgeView>("Show badges")
                .Navigate<PocFigletView>("Show figlet text")
                .Navigate<PocJsonView>("Show JSON diagnostics")
                .Navigate<PocTextPathView>("Show text paths")
                .Navigate<PocBreadcrumbView>("Show breadcrumbs")
                .Navigate<PocShortcutBarView>("Show shortcut bar")
                .Option("Show release calendar", ShowReleaseCalendar)
                .Option("Choose sample tasks", ChooseSampleTasks)
                .Option("Configure retries", ConfigureRetries)
                .Option("Run status sample", RunStatusSample)
                .Option("Run spinner sample", RunSpinnerSample)
                .Option("Run progress sample", RunProgressSample)
                .Navigate<PocSettingsView>("Edit settings");
        }

        private static void ChooseSampleTasks()
        {
            MultiSelectionResult result = Consoul.MultiSelect(
                "Choose tasks",
                false,
                "Build",
                "Test",
                "Deploy");

            if (!result.IsCanceled)
            {
                Consoul.MarkupLine($"Selected [cyan]{result.Indexes.Count}[/] task(s).");
            }

            Consoul.Wait();
        }

        private static void ShowReleaseCalendar()
        {
            DateTime month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Consoul.Headline("Release", new HeadlineOptions { Width = 40, Style = HeadlineStyle.Rule });
            Consoul.Calendar(month, calendar =>
            {
                calendar.Add(DateTime.Today, ConsoleColor.Cyan, "Today");
                calendar.Add(DateTime.Today.AddDays(3), ConsoleColor.Green, "Release");
            }, new CalendarOptions { Width = 32, ShowAdjacentDays = true });

            Consoul.Wait();
        }

        private static void PreviewModernTheme()
        {
            using (Consoul.UseTheme(ConsoulTheme.Modern))
            {
                Consoul.Headline("Modern", new HeadlineOptions { Width = 48, Style = HeadlineStyle.Block });
                Consoul.MarkupLine("[prompt]Prompt[/] [subnote]hint[/] [option]option[/] [invalid]error[/]");
                Consoul.PropertyList(list =>
                {
                    list.Add("Default", RenderOptions.DefaultColor);
                    list.Add("Prompt", RenderOptions.PromptColor);
                    list.Add("Option", RenderOptions.OptionColor);
                }, new PropertyListOptions { Width = 36, LabelWidth = 8 });
                Consoul.Wait();
            }
        }

        private static void PreviewExceptionPanel()
        {
            try
            {
                ThrowPreviewFailure();
            }
            catch (Exception exception)
            {
                Consoul.WriteException(
                    exception,
                    "A proof-of-concept action failed.",
                    new ExceptionRenderOptions
                    {
                        Width = 56,
                        MaxStackFrames = 3
                    });
            }

            Consoul.Wait();
        }

        private static void ThrowPreviewFailure()
        {
            throw new InvalidOperationException("Preview data source returned an unexpected state.");
        }

        private static void ConfigureRetries()
        {
            TextPromptResult<int> retries = Consoul.Text<int>("Retry count")
                .WithDefault(3)
                .Validate(value => value >= 0 ? null : "Retry count cannot be negative.")
                .Render();

            if (retries.HasValue)
            {
                Consoul.MarkupLine($"Retries set to [cyan]{retries.Value}[/].");
            }

            Consoul.Wait();
        }

        private static void RunProgressSample()
        {
            Consoul.Progress(progress =>
            {
                ProgressTask build = progress.AddTask("Build", 4);
                ProgressTask test = progress.AddTask("Test", 3);

                build.Increment(2);
                test.Increment(1);
                build.Complete("Build done");
                test.Complete("Tests done");
            }, new ProgressContextOptions { Width = 48, LabelWidth = 12 });

            Consoul.Wait();
        }

        private static void RunStatusSample()
        {
            Consoul.Status("Preparing proof-of-concept workflow", status =>
            {
                status.Update("Checking inputs");
                status.Update("Ready");
            });

            Consoul.Wait();
        }

        private static void RunSpinnerSample()
        {
            Consoul.Spinner("Loading proof-of-concept data", spinner =>
            {
                spinner.Tick("Reading configuration");
                spinner.Tick("Preparing dashboard model");
                spinner.Tick("Ready");
            }, new SpinnerOptions { MaxWidth = 48 });

            Consoul.Wait();
        }

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title
        {
            get => _view.Title;
            set => _view.Title = value;
        }

        /// <summary>
        /// Gets a value indicating whether a request to navigate back has been made.
        /// </summary>
        public bool GoBackRequested => _view.GoBackRequested;

        /// <summary>
        /// Signals a request to navigate back from the current view.
        /// </summary>
        public void GoBack()
        {
            _view.GoBack();
        }

        /// <summary>
        /// Renders the view synchronously.
        /// </summary>
        public void Render()
        {
            _view.Render();
        }

        /// <summary>
        /// Renders the view asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public Task RenderAsync(CancellationToken cancellationToken = default)
        {
            return _view.RenderAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Demonstrates an MVC-style dynamic view whose options act on a model.
    /// </summary>
    public class PocCounterView : IView
    {
        private readonly DynamicView<PocCounter> _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocCounterView"/> class.
        /// </summary>
        public PocCounterView()
        {
            _view = Consoul.View("POC Counter", new PocCounter())
                .WithTitle(counter => "POC Counter: " + counter.Count)
                .RenderModel(counter =>
                {
                    Consoul.Headline("Counter", new HeadlineOptions { Width = 48, Style = HeadlineStyle.Rule });
                    Consoul.TextBlock(
                        "This model-backed view renders state through composable view helpers.",
                        new TextBlockOptions
                        {
                            Width = 48,
                            Alignment = TextBlockAlignment.Center,
                            BottomPadding = 1,
                            Scheme = new ConsoulLibrary.Color.ColorScheme
                            {
                                Color = ConsoleColor.Cyan,
                                BackgroundColor = ConsoleColor.Black
                            }
                        });
                    Consoul.PropertyList(list =>
                    {
                        list.Add("Count", counter.Count, ConsoleColor.Cyan);
                        list.Add("State", counter.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
                    }, new PropertyListOptions { Width = 36, LabelWidth = 8 });
                    Consoul.Columns(columns =>
                    {
                        columns.Add("Use for prose-heavy dashboard sections.", "Columns", ConsoleColor.Cyan);
                        columns.Add("Use property lists for labeled model state.", "PropertyList", ConsoleColor.Green);
                    }, new ColumnsOptions { Columns = 2, MinimumColumnWidth = 18 });
                    Consoul.BarChart(chart =>
                    {
                        chart.Add("Count", counter.Count, ConsoleColor.Cyan);
                        chart.Add("Remaining", Math.Max(0, 10 - counter.Count), ConsoleColor.DarkYellow);
                    }, new BarChartOptions { MaximumValue = 10, Width = 36 });
                    Consoul.BreakdownChart(chart =>
                    {
                        chart.Add("Used", counter.Count, ConsoleColor.Cyan);
                        chart.Add("Open", Math.Max(0, 10 - counter.Count), ConsoleColor.DarkYellow);
                    }, new BreakdownChartOptions { Width = 36 });
                    Consoul.Panel("Current count: " + counter.Count, title: "Model");
                })
                .Option(counter => "Increment from " + counter.Count, counter => counter.Count++)
                .OptionWhen(
                    "Reset",
                    counter => counter.Count = 0,
                    counter => counter.Count > 0,
                    "counter is already zero",
                    ConsoleColor.DarkYellow);
        }

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title
        {
            get => _view.Title;
            set => _view.Title = value;
        }

        /// <summary>
        /// Gets a value indicating whether a request to navigate back has been made.
        /// </summary>
        public bool GoBackRequested => _view.GoBackRequested;

        /// <summary>
        /// Signals a request to navigate back from the current view.
        /// </summary>
        public void GoBack()
        {
            _view.GoBack();
        }

        /// <summary>
        /// Renders the view synchronously.
        /// </summary>
        public void Render()
        {
            _view.Render();
        }

        /// <summary>
        /// Renders the view asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public Task RenderAsync(CancellationToken cancellationToken = default)
        {
            return _view.RenderAsync(cancellationToken);
        }

        private sealed class PocCounter
        {
            public int Count { get; set; }
        }
    }

    /// <summary>
    /// Demonstrates ASCII canvas drawing for quick diagrams.
    /// </summary>
    public class PocCanvasView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocCanvasView"/> class.
        /// </summary>
        public PocCanvasView()
        {
            Title = "POC Canvas View";
        }

        /// <summary>
        /// Renders a small deployment flow diagram.
        /// </summary>
        [ViewOption("Render flow diagram")]
        public void RenderFlowDiagram()
        {
            Consoul.Panel(
                () => Consoul.Canvas(34, 7, canvas =>
                {
                    canvas.Rectangle(0, 1, 9, 3, '#', ConsoleColor.Cyan);
                    canvas.Text(2, 2, "Build", ConsoleColor.Cyan);
                    canvas.Line(9, 2, 15, 2, '-');
                    canvas.Rectangle(15, 1, 8, 3, '#', ConsoleColor.Yellow);
                    canvas.Text(17, 2, "Test", ConsoleColor.Yellow);
                    canvas.Line(23, 2, 29, 2, '-');
                    canvas.Rectangle(29, 1, 5, 3, '#', ConsoleColor.Green);
                    canvas.Text(30, 2, "Ship", ConsoleColor.Green);
                }, new CanvasOptions { MaxWidth = 40 }),
                title: "Flow",
                width: 44,
                borderColor: ConsoleColor.Cyan);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates a checklist-style dashboard section.
    /// </summary>
    public class PocChecklistView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocChecklistView"/> class.
        /// </summary>
        public PocChecklistView()
        {
            Title = "POC Checklist View";
        }

        /// <summary>
        /// Renders a release readiness checklist.
        /// </summary>
        [ViewOption("Render readiness checklist")]
        public void RenderReadinessChecklist()
        {
            Consoul.Panel(
                () => Consoul.Checklist(list =>
                {
                    list.Add("Build artifacts", ChecklistItemState.Complete);
                    list.Add("Responsive rendering tests", ChecklistItemState.Complete);
                    list.Add("Approval gate", ChecklistItemState.Pending, "Waiting for release owner sign-off.");
                    list.Add("Smoke test", ChecklistItemState.Warning, "Run against the target terminal before publishing.");
                }, new ChecklistOptions { Width = 44 }),
                title: "Readiness",
                width: 52,
                borderColor: ConsoleColor.Cyan);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates bullet lists for grouped notes and requirements.
    /// </summary>
    public class PocBulletListView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocBulletListView"/> class.
        /// </summary>
        public PocBulletListView()
        {
            Title = "POC Bullet List View";
        }

        /// <summary>
        /// Renders nested release notes.
        /// </summary>
        [ViewOption("Render grouped notes")]
        public void RenderGroupedNotes()
        {
            Consoul.Panel(
                () => Consoul.BulletList(list =>
                {
                    BulletListItem release = list.Add("Release notes");
                    release.Add("Mention responsive rendering coverage.");
                    release.Add("Call out optional package splits.");
                    BulletListItem validation = list.Add("Validation");
                    validation.Add("Run full tests.");
                    validation.Add("Run solution build.");
                }, new BulletListOptions { Width = 44 }),
                title: "Notes",
                width: 52,
                borderColor: ConsoleColor.Cyan);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates a minimal table-focused proof-of-concept view.
    /// </summary>
    public class PocTableView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocTableView"/> class.
        /// </summary>
        public PocTableView()
        {
            Title = "POC Table View";
        }

        /// <summary>
        /// Renders a small table of work items.
        /// </summary>
        [ViewOption("Render work-item table")]
        public void RenderWorkItems()
        {
            var items = new List<PocWorkItem>
            {
                new PocWorkItem { Name = "Prototype menu", Status = "Done", Owner = "Alex", Priority = 3 },
                new PocWorkItem { Name = "Wire table", Status = "In review", Owner = "Sam", Priority = 1 },
                new PocWorkItem { Name = "Tune settings", Status = "Next", Owner = "Jordan", Priority = 2 }
            };

            var tableOptions = new TableRenderOptions { ShowFooterHelp = false };
            tableOptions.Lines.BorderStyle = TableBorderStyle.Rounded;

            var table = DynamicTableView<PocWorkItem>.Create(items, tableOptions)
                .Filter(item => item.Status != "Done")
                .Search("review")
                .SortBy(item => item.Priority);
            table.ShowSummary = true;
            table.ShowEmptyMessage = true;
            table.EmptyMessage = "No active work items match the current search.";

            var rows = new RowsEntry(new RowsOptions { Width = 64, Spacing = 1 });
            rows.Add(new RuleEntry("Work Items", new RuleOptions { Width = 64 }));
            rows.Add(table, captureWidth: 64);
            Consoul.Panel(
                rows,
                title: "Current proof-of-concept work",
                width: 72,
                borderColor: ConsoleColor.Cyan,
                captureWidth: 64);

            Consoul.Wait();
        }

        private sealed class PocWorkItem
        {
            [TableColumn("Work Item", Order = 1)]
            public string Name { get; set; }

            [TableColumn("State", Order = 2)]
            public string Status { get; set; }

            [TableColumn(Order = 3)]
            public string Owner { get; set; }

            [TableColumn(Order = 4)]
            public int Priority { get; set; }
        }
    }

    /// <summary>
    /// Demonstrates compact dashboard metrics for model-backed proof-of-concept screens.
    /// </summary>
    public class PocMetricsView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocMetricsView"/> class.
        /// </summary>
        public PocMetricsView()
        {
            Title = "POC Metrics View";
        }

        /// <summary>
        /// Renders a compact metrics summary.
        /// </summary>
        [ViewOption("Render metrics")]
        public void RenderMetrics()
        {
            Consoul.Breadcrumb("Home", "Operations", "Metrics");
            Consoul.Metrics(metrics =>
            {
                metrics.Add("Builds", 42, note: "Today", kind: MetricKind.Success);
                metrics.Add("Failures", 2, note: "Needs triage", kind: MetricKind.Warning);
                metrics.Add("Latency", 128, "ms", "P95", MetricKind.Info);
                metrics.Add("Queue", 7, note: "Pending work", kind: MetricKind.Default);
            }, new MetricsOptions { Width = 56, Columns = 2 });

            Consoul.ShortcutBar(bar =>
            {
                bar.Add("F5", "Refresh");
                bar.Add("Esc", "Back");
            }, new ShortcutBarOptions { Width = 56 });

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates a compact trend chart for dashboard-style summaries.
    /// </summary>
    public class PocSparklineView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocSparklineView"/> class.
        /// </summary>
        public PocSparklineView()
        {
            Title = "POC Sparkline View";
        }

        /// <summary>
        /// Renders a small duration trend.
        /// </summary>
        [ViewOption("Render duration trend")]
        public void RenderDurationTrend()
        {
            Consoul.Panel(
                () => Consoul.Sparkline(
                    new[] { 12d, 10d, 11d, 8d, 7d, 9d, 6d },
                    new SparklineOptions
                    {
                        Label = "Duration",
                        Width = 42,
                        ShowRange = true,
                        LineScheme = new ConsoulLibrary.Color.ColorScheme
                        {
                            Color = ConsoleColor.Cyan,
                            BackgroundColor = ConsoleColor.Black
                        }
                    }),
                title: "Trend",
                width: 52,
                borderColor: ConsoleColor.Cyan);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates a minimal tree-focused proof-of-concept view.
    /// </summary>
    public class PocTreeView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocTreeView"/> class.
        /// </summary>
        public PocTreeView()
        {
            Title = "POC Tree View";
        }

        /// <summary>
        /// Renders a small tree of deployment state.
        /// </summary>
        [ViewOption("Render deployment tree")]
        public void RenderDeploymentTree()
        {
            Consoul.Rule("Deployment");
            Consoul.Tree("Deployment", root =>
            {
                TreeNode targets = root.Add("Targets");
                targets.Add("Production");
                targets.Add("Staging");

                TreeNode checks = root.Add("Checks", ConsoleColor.Yellow);
                checks.Add("Database");
                checks.Add("Feature flags");
            });
            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates timeline rendering for workflow history and upcoming steps.
    /// </summary>
    public class PocTimelineView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocTimelineView"/> class.
        /// </summary>
        public PocTimelineView()
        {
            Title = "POC Timeline View";
        }

        /// <summary>
        /// Renders a release workflow timeline.
        /// </summary>
        [ViewOption("Render release timeline")]
        public void RenderReleaseTimeline()
        {
            Consoul.Panel(
                () => Consoul.Timeline(timeline =>
                {
                    timeline.Add("Restore packages", TimelineItemState.Complete, "NuGet cache restored and lock file validated.");
                    timeline.Add("Run smoke tests", TimelineItemState.Complete, "Responsive rendering checks passed.");
                    timeline.Add("Approve preview", TimelineItemState.Active, "Waiting for owner sign-off before release.");
                    timeline.Add("Publish package", TimelineItemState.Pending, "Push package artifacts after approval.");
                }, new TimelineOptions { Width = 48 }),
                title: "Release Flow",
                width: 56,
                borderColor: ConsoleColor.Cyan);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates stacking captured render helpers as rows.
    /// </summary>
    public class PocRowsView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocRowsView"/> class.
        /// </summary>
        public PocRowsView()
        {
            Title = "POC Rows View";
        }

        /// <summary>
        /// Renders stacked row sections.
        /// </summary>
        [ViewOption("Render stacked dashboard rows")]
        public void RenderRows()
        {
            Consoul.Rows(rows =>
            {
                rows.Add(() => Consoul.Rule("Checks", width: 44));
                rows.Add(() => Consoul.PropertyList(list =>
                {
                    list.Add("Database", "Ready", ConsoleColor.Green);
                    list.Add("Feature flags", "Ready", ConsoleColor.Green);
                    list.Add("Deploy window", "Open", ConsoleColor.Cyan);
                }, new PropertyListOptions { Width = 44, LabelWidth = 14 }));
                rows.Add(() => Consoul.Panel("Rows compose existing render helpers without manual string capture.", title: "Notes", width: 44));
            }, new RowsOptions { Width = 48, Spacing = 1 });

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates padding captured render helper output.
    /// </summary>
    public class PocPadView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocPadView"/> class.
        /// </summary>
        public PocPadView()
        {
            Title = "POC Pad View";
        }

        /// <summary>
        /// Renders padded dashboard content.
        /// </summary>
        [ViewOption("Render padded panel")]
        public void RenderPaddedPanel()
        {
            Consoul.Pad(
                () => Consoul.Panel("Padding can inset any captured Consoul render helper.", title: "Padded", width: 44),
                left: 3,
                right: 1,
                top: 1,
                bottom: 1,
                width: 52,
                color: ConsoleColor.Cyan,
                captureWidth: 48);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates aligning captured render helper output.
    /// </summary>
    public class PocAlignView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocAlignView"/> class.
        /// </summary>
        public PocAlignView()
        {
            Title = "POC Align View";
        }

        /// <summary>
        /// Renders aligned dashboard content.
        /// </summary>
        [ViewOption("Render centered panel")]
        public void RenderAlignedPanel()
        {
            Consoul.Align(
                () => Consoul.Panel("Alignment can center or right-align any captured Consoul render helper.", title: "Aligned", width: 44),
                TextBlockAlignment.Center,
                width: 60,
                color: ConsoleColor.Cyan,
                captureWidth: 48);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates intent-colored callouts for proof-of-concept view messages.
    /// </summary>
    public class PocCalloutView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocCalloutView"/> class.
        /// </summary>
        public PocCalloutView()
        {
            Title = "POC Callout View";
        }

        /// <summary>
        /// Renders callouts for common dashboard states.
        /// </summary>
        [ViewOption("Render callouts")]
        public void RenderCallouts()
        {
            Consoul.Callout("Configuration loaded from the local preview profile.", CalloutKind.Info, new CalloutOptions { Width = 56 });
            Consoul.Callout("Build completed and artifacts are ready to inspect.", CalloutKind.Success, new CalloutOptions { Width = 56 });
            Consoul.Callout("Deployment is waiting for owner approval.", CalloutKind.Warning, new CalloutOptions { Width = 56 });
            Consoul.Callout(
                () => Consoul.PropertyList(list =>
                {
                    list.Add("Service", "Billing", ConsoleColor.Cyan);
                    list.Add("Status", "Connection failed", ConsoleColor.Red);
                }, new PropertyListOptions { LabelWidth = 8 }),
                CalloutKind.Error,
                new CalloutOptions { Title = "Health Check", Width = 56 });

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates compact state badges for proof-of-concept dashboards.
    /// </summary>
    public class PocBadgeView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocBadgeView"/> class.
        /// </summary>
        public PocBadgeView()
        {
            Title = "POC Badge View";
        }

        /// <summary>
        /// Renders badges inline and as plain table values.
        /// </summary>
        [ViewOption("Render badges")]
        public void RenderBadges()
        {
            Consoul.Badge("prod", BadgeKind.Info, writeLine: false);
            Consoul.Write(" ", writeLine: false);
            Consoul.Badge("ready", BadgeKind.Success, writeLine: false);
            Consoul.Write(" ", writeLine: false);
            Consoul.Badge("owner review", BadgeKind.Warning);

            var services = new[]
            {
                new BadgeService("API", Consoul.BadgeText("ready", BadgeKind.Success, maxWidth: 12)),
                new BadgeService("Worker", Consoul.BadgeText("warn", BadgeKind.Warning, maxWidth: 12)),
                new BadgeService("Billing", Consoul.BadgeText("blocked", BadgeKind.Error, maxWidth: 12))
            };

            Consoul.Table(services, service => service.Name, service => service.State);
            Consoul.Wait();
        }

        private sealed class BadgeService
        {
            public BadgeService(string name, string state)
            {
                Name = name;
                State = state;
            }

            public string Name { get; }

            public string State { get; }
        }
    }

    /// <summary>
    /// Demonstrates panels that capture existing render helpers as content.
    /// </summary>
    public class PocPanelView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocPanelView"/> class.
        /// </summary>
        public PocPanelView()
        {
            Title = "POC Panel View";
        }

        /// <summary>
        /// Renders a composed dashboard panel.
        /// </summary>
        [ViewOption("Render composed panel")]
        public void RenderComposedPanel()
        {
            var properties = new PropertyListEntry(new PropertyListOptions { LabelWidth = 12 });
            properties.Add("Environment", "Development", ConsoleColor.Cyan);
            properties.Add("Status", "Ready", ConsoleColor.Green);
            properties.Add("Retries", 3, ConsoleColor.Yellow);

            Consoul.Panel(
                properties,
                title: "Snapshot",
                width: 52,
                borderColor: ConsoleColor.Cyan,
                captureWidth: 40);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates large figlet-style text for splash screens and section headers.
    /// </summary>
    public class PocFigletView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocFigletView"/> class.
        /// </summary>
        public PocFigletView()
        {
            Title = "POC Figlet View";
        }

        /// <summary>
        /// Renders large ASCII dashboard text.
        /// </summary>
        [ViewOption("Render figlet title")]
        public void RenderFigletTitle()
        {
            Consoul.FigletText("Consoul", width: 56, color: ConsoleColor.Cyan);
            Consoul.Panel("Use figlet-style text for application splash screens or major MVC-style view sections.", title: "Figlet Text", width: 56);
            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates optional System.Text.Json rendering for diagnostic snapshots.
    /// </summary>
    public class PocJsonView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocJsonView"/> class.
        /// </summary>
        public PocJsonView()
        {
            Title = "POC JSON View";
        }

        /// <summary>
        /// Renders a syntax-colored JSON snapshot.
        /// </summary>
        [ViewOption("Render JSON snapshot")]
        public void RenderJsonSnapshot()
        {
            var snapshot = JsonTextEntry.FromValue(new
            {
                Environment = "Development",
                Preview = true,
                RetryCount = 3,
                Features = new[] { "tables", "prompts", "views" }
            }, options: new JsonTextOptions { Width = 56 });

            Consoul.Panel(snapshot, title: "JSON Snapshot", width: 64, captureWidth: 56);
            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates responsive path rendering for build and diagnostic views.
    /// </summary>
    public class PocTextPathView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocTextPathView"/> class.
        /// </summary>
        public PocTextPathView()
        {
            Title = "POC Text Path View";
        }

        /// <summary>
        /// Renders a long artifact path.
        /// </summary>
        [ViewOption("Render artifact path")]
        public void RenderArtifactPath()
        {
            Consoul.TextPath(
                @"C:\Users\LightWorks\Documents\GitHub\Consoul\artifacts\preview\release-notes.json",
                width: 64,
                directoryColor: ConsoleColor.DarkGray,
                fileNameColor: ConsoleColor.Cyan,
                separatorColor: ConsoleColor.DarkCyan);

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates responsive breadcrumb rendering for MVC-style view navigation.
    /// </summary>
    public class PocBreadcrumbView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocBreadcrumbView"/> class.
        /// </summary>
        public PocBreadcrumbView()
        {
            Title = "POC Breadcrumb View";
        }

        /// <summary>
        /// Renders navigation context for a settings screen.
        /// </summary>
        [ViewOption("Render breadcrumb")]
        public void RenderBreadcrumb()
        {
            Consoul.Breadcrumb(
                new BreadcrumbOptions
                {
                    Width = 56,
                    CurrentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                    }
                },
                "Home",
                "Operations",
                "Preview Environments",
                "Settings Editor");

            Consoul.Callout("Breadcrumbs give POC views lightweight navigation context without a custom layout.", CalloutKind.Info, new CalloutOptions { Width = 56 });
            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates responsive shortcut hints for available view actions.
    /// </summary>
    public class PocShortcutBarView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocShortcutBarView"/> class.
        /// </summary>
        public PocShortcutBarView()
        {
            Title = "POC Shortcut Bar View";
        }

        /// <summary>
        /// Renders shortcut hints for a proof-of-concept view.
        /// </summary>
        [ViewOption("Render shortcut bar")]
        public void RenderShortcutBar()
        {
            Consoul.Breadcrumb("Home", "Operations", "Shortcut Preview");
            Consoul.ShortcutBar(bar =>
            {
                bar.Add("Enter", "Run selected action", keyColor: ConsoleColor.Cyan);
                bar.Add("F5", "Refresh preview", keyColor: ConsoleColor.Green);
                bar.Add("Esc", "Go back", keyColor: ConsoleColor.Yellow);
                bar.Add("?", "Help", disabled: true);
            }, new ShortcutBarOptions { Width = 56 });

            Consoul.Callout("Shortcut bars make POC screens easier to operate without writing a custom footer.", CalloutKind.Info, new CalloutOptions { Width = 56 });
            Consoul.Wait();
        }
    }

    /// <summary>
    /// Demonstrates editing a settings object from a small proof-of-concept view.
    /// </summary>
    public class PocSettingsView : IView
    {
        private readonly PocSettings _settings = new PocSettings
        {
            Environment = "Development",
            EnablePreviewMode = true,
            RetryCount = 3
        };
        private readonly DynamicView<PocSettings> _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocSettingsView"/> class.
        /// </summary>
        public PocSettingsView()
        {
            _view = Consoul.View("POC Settings View", _settings)
                .RenderModel(settings => BuildSettingsSummary(settings))
                .EditModel("Edit settings object", ConsoleColor.Green)
                .ConfirmOption(
                    settings => "Save preview",
                    settings => "Save settings for " + settings.Environment + "?",
                    settings =>
                    {
                        ValidationSummaryResult validation = Consoul.ValidationSummary(
                            settings,
                            new ValidationSummaryOptions
                            {
                                Width = 48,
                                ShowValidMessage = true,
                                ValidTitle = "Ready"
                            });
                        if (!validation.IsValid)
                        {
                            Consoul.Wait();
                            return;
                        }

                        Consoul.Write("Settings saved for " + settings.Environment + ".", ConsoleColor.Green);
                        Consoul.Wait();
                    }, settings => ConsoleColor.Cyan);
        }

        private static IConsoulRenderable BuildSettingsSummary(PocSettings settings)
        {
            var summary = new PropertyListEntry(new PropertyListOptions { Width = 40, LabelWidth = 18 });
            summary.AddModel(settings);

            var rows = new RowsEntry(new RowsOptions { Width = 48, Spacing = 1 });
            rows.Add(
                new PanelEntry(
                    "This screen edits a plain settings model using the built-in object editor.",
                    new PanelOptions { Title = "POC Editor", Width = 48 }),
                captureWidth: 48);
            rows.Add(summary, captureWidth: 40);

            return rows;
        }

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title
        {
            get => _view.Title;
            set => _view.Title = value;
        }

        /// <summary>
        /// Gets a value indicating whether a request to navigate back has been made.
        /// </summary>
        public bool GoBackRequested => _view.GoBackRequested;

        /// <summary>
        /// Signals a request to navigate back from the current view.
        /// </summary>
        public void GoBack()
        {
            _view.GoBack();
        }

        /// <summary>
        /// Renders the view synchronously.
        /// </summary>
        public void Render()
        {
            _view.Render();
        }

        /// <summary>
        /// Renders the view asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public Task RenderAsync(CancellationToken cancellationToken = default)
        {
            return _view.RenderAsync(cancellationToken);
        }

        private sealed class PocSettings
        {
            [Display(Name = "Environment")]
            [Required(ErrorMessage = "Environment is required.")]
            public string Environment { get; set; }

            public bool EnablePreviewMode { get; set; }

            [Range(0, 10, ErrorMessage = "Retry count must be between 0 and 10.")]
            public int RetryCount { get; set; }
        }
    }
}
