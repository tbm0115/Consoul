using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using ConsoulLibrary.Views;
using ConsoulLibrary.Views.Editing;
using Xunit;
using ColorScheme = ConsoulLibrary.Color.ColorScheme;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Views
{
    /// <summary>
    /// Tests proof-of-concept view APIs and compatibility behavior.
    /// </summary>
    public class PocViewHardeningTests
    {
        /// <summary>
        /// Dynamic views execute synchronous programmatic options and then continue until the user exits.
        /// </summary>
        [Fact]
        public void DynamicView_Render_SelectsSynchronousOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                bool called = false;

                try
                {
                    var view = Consoul.View("POC")
                        .Option("Run", () => called = true);

                    view.Render();

                    Assert.True(called);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Run");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views execute asynchronous programmatic options with the active cancellation token.
        /// </summary>
        [Fact]
        public async Task DynamicView_RenderAsync_SelectsAsynchronousOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                bool called = false;

                try
                {
                    var view = Consoul.View("POC")
                        .Option("Run async", cancellationToken =>
                        {
                            called = !cancellationToken.IsCancellationRequested;
                            return Task.CompletedTask;
                        });

                    await view.RenderAsync();

                    Assert.True(called);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Run async");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views execute plain Task-returning POC options without forcing callers to accept a cancellation token.
        /// </summary>
        [Fact]
        public async Task DynamicView_RenderAsync_SelectsTaskOptionWithoutTokenParameter()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                bool called = false;

                try
                {
                    var view = Consoul.View("POC")
                        .Option(
                            "Run quick async",
                            async () =>
                            {
                                await Task.Yield();
                                called = true;
                            },
                            "green",
                            "Runs a quick proof-of-concept task.");

                    await view.RenderAsync();

                    Assert.True(called);
                    VisualAssert.WroteText(scope.Driver, "Runs a quick proof-of-concept task.");
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views expose StaticView-style add-option aliases without leaving the model-backed view flow.
        /// </summary>
        [Fact]
        public async Task DynamicView_AddOptionAliases_RunSyncAsyncModelAndTokenActions()
        {
            using (var scope = new ConsoleDriverScope(80))
            using (var cancellation = new CancellationTokenSource())
            {
                InitializeRoutine("1", "2", "3", "4", "5");
                var model = new CounterModel();
                int syncCount = 0;
                int asyncCount = 0;
                CancellationToken observedToken = default;

                try
                {
                    var view = Consoul.View("POC", model)
                        .AddOption(
                            "Sync",
                            () => syncCount++,
                            "aqua",
                            "Runs a synchronous POC action.")
                        .AddOption(
                            "Async",
                            async () =>
                            {
                                await Task.Yield();
                                asyncCount++;
                            },
                            new Style(ConsoleColor.Green),
                            "Runs an asynchronous POC action.")
                        .AddOption(
                            "Model",
                            item => item.Count++,
                            ConsoleColor.Yellow,
                            "Mutates the active model.")
                        .AddOption(
                            "Token",
                            async (item, cancellationToken) =>
                            {
                                await Task.Yield();
                                observedToken = cancellationToken;
                                item.Count++;
                            },
                            new Style(ConsoleColor.Cyan),
                            "Observes the active render token.");

                    await view.RenderAsync(cancellation.Token);

                    Assert.Equal(1, syncCount);
                    Assert.Equal(1, asyncCount);
                    Assert.Equal(2, model.Count);
                    Assert.Equal(cancellation.Token, observedToken);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Runs a synchronous POC action.");
                    VisualAssert.WroteText(scope.Driver, "Observes the active render token.");
                    AssertColoredWrite(scope.Driver, "Async", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Token", ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views can project model-adjacent action descriptors into selectable options without hand-written loops.
        /// </summary>
        [Fact]
        public void DynamicView_AddOptions_ProjectsCollectionIntoMenuActions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("ship", "4");
                var model = new CounterModel();
                var actions = new[]
                {
                    new ViewActionDescriptor("Preview", 1, ConsoleColor.Cyan, "Runs validation only.", "p"),
                    new ViewActionDescriptor("Deploy", 2, ConsoleColor.Green, "Publishes the release.", "ship"),
                    new ViewActionDescriptor("Archive", 4, ConsoleColor.Yellow, "Moves the release out of the active queue.", "a")
                };

                try
                {
                    var view = Consoul.View("Release", model)
                        .AddOptions(
                            actions,
                            item => item.Label,
                            (counter, item) => counter.Count += item.Amount,
                            item => item.Color,
                            item => item.Description,
                            (option, item) => option.UseShortcut(item.Shortcut).AddSearchTerms(item.Label));

                    view.Render();

                    Assert.Equal(2, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Publishes the release.");
                    AssertColoredWrite(scope.Driver, "Deploy", ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Projected dynamic view options can be disabled from the current model state without hand-written option loops.
        /// </summary>
        [Fact]
        public void DynamicView_AddOptions_DisablesProjectedActionsUntilModelStateChanges()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("ship", "p", "ship", "4");
                var model = new CounterModel();
                var actions = new[]
                {
                    new ViewActionDescriptor("Preview", 1, ConsoleColor.Cyan, "Runs validation only.", "p"),
                    new ViewActionDescriptor("Deploy", 2, ConsoleColor.Green, "Publishes the release.", "ship"),
                    new ViewActionDescriptor("Archive", 4, ConsoleColor.Yellow, "Moves the release out of the active queue.", "a")
                };

                try
                {
                    var view = Consoul.View("Release", model)
                        .AddOptions(
                            items: actions,
                            label: item => item.Label,
                            action: (counter, item) => counter.Count += item.Amount,
                            isEnabled: (counter, item) => item.Label != "Deploy" || counter.Count > 0,
                            disabledReason: (counter, item) => item.Label == "Deploy" ? "run preview first" : null,
                            color: item => item.Color,
                            description: item => item.Description,
                            configure: (option, item) => option.UseShortcut(item.Shortcut).AddSearchTerms(item.Label));

                    view.Render();

                    Assert.Equal(3, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "disabled: run preview first");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: run preview first");
                    AssertColoredWrite(scope.Driver, "Deploy", ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic model options can stay visible but disabled until model state makes them available.
        /// </summary>
        [Fact]
        public void DynamicView_OptionWhen_DisablesUntilModelStateChanges()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2", "1", "3");
                var model = new ApprovalModel();
                int deployCount = 0;

                try
                {
                    var view = Consoul.View("Release", model)
                        .OptionWhen(
                            "Deploy",
                            release =>
                            {
                                deployCount++;
                                release.Deployed = true;
                            },
                            release => release.Approved,
                            "approval required",
                            ConsoleColor.Green)
                        .Option("Approve", release => release.Approved = true, ConsoleColor.Yellow);

                    view.Render();

                    Assert.True(model.Approved);
                    Assert.True(model.Deployed);
                    Assert.Equal(1, deployCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Predicate-gated dynamic options can run Task-returning actions without forcing POC code to accept a cancellation token.
        /// </summary>
        [Fact]
        public async Task DynamicView_OptionWhen_RunsTaskOptionsWithoutTokenParameter()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2", "3");
                var model = new ApprovalModel
                {
                    Approved = true
                };
                bool refreshed = false;

                try
                {
                    var view = Consoul.View("Release", model)
                        .OptionWhen(
                            "Refresh status",
                            async () =>
                            {
                                await Task.Yield();
                                refreshed = true;
                            },
                            release => release.Approved,
                            "approval required",
                            "green")
                        .OptionWhen(
                            release => "Deploy approved=" + release.Approved,
                            (Func<ApprovalModel, Task>)(async release =>
                            {
                                await Task.Yield();
                                release.Deployed = true;
                            }),
                            release => refreshed,
                            release => "refresh required",
                            release => new Style(ConsoleColor.Cyan));

                    await view.RenderAsync();

                    Assert.True(refreshed);
                    Assert.True(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Refresh status");
                    VisualAssert.WroteText(scope.Driver, "Deploy approved=True");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmed dynamic options run only after the user confirms the action.
        /// </summary>
        [Fact]
        public void DynamicView_ConfirmOption_RunsAfterConfirmation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "yes", "2");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ConfirmOption(
                            "Deploy",
                            "Deploy now?",
                            release => release.Deployed = true,
                            ConsoleColor.Green);

                    view.Render();

                    Assert.True(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Deploy now? [y/N]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmed dynamic options can run Task-returning actions without forcing POC code to accept a cancellation token.
        /// </summary>
        [Fact]
        public async Task DynamicView_ConfirmOption_RunsTaskOptionWithoutTokenParameter()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "yes", "2");
                bool called = false;

                try
                {
                    var view = Consoul.View("Release")
                        .ConfirmOption(
                            "Deploy async",
                            "Deploy async now?",
                            async () =>
                            {
                                await Task.Yield();
                                called = true;
                            },
                            "green");

                    await view.RenderAsync();

                    Assert.True(called);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Deploy async now? [y/N]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmed model options can run Task-returning actions while keeping labels and confirmation text model-aware.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_ConfirmOption_RunsTaskModelOptionWithoutTokenParameter()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "yes", "2");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ConfirmOption(
                            release => "Deploy approved=" + release.Approved,
                            release => "Deploy approved=" + release.Approved + "?",
                            async release =>
                            {
                                await Task.Yield();
                                release.Deployed = true;
                            },
                            release => new Style(ConsoleColor.Green));

                    await view.RenderAsync();

                    Assert.True(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Deploy approved=False");
                    VisualAssert.WroteText(scope.Driver, "Deploy approved=False? [y/N]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmed dynamic options skip the action when the user declines the confirmation.
        /// </summary>
        [Fact]
        public void DynamicView_ConfirmOption_SkipsWhenDeclined()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "no", "2");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ConfirmOption(
                            "Deploy",
                            "Deploy now?",
                            release => release.Deployed = true,
                            ConsoleColor.Green);

                    view.Render();

                    Assert.False(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Deploy now? [y/N]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views expose StaticView-style confirmed action aliases with descriptions and cancellation-token flow.
        /// </summary>
        [Fact]
        public async Task DynamicView_AddConfirmOptionAliases_RunConfirmedActionsAndSkipDeclinedActions()
        {
            using (var scope = new ConsoleDriverScope(80))
            using (var cancellation = new CancellationTokenSource())
            {
                InitializeRoutine("1", "no", "2", "yes", "3", "yes", "4", "yes", "5");
                var model = new CounterModel();
                int declinedCount = 0;
                int asyncCount = 0;
                CancellationToken observedToken = default;

                try
                {
                    var view = Consoul.View("Release", model)
                        .AddConfirmOption(
                            "Declined",
                            "Run declined?",
                            () => declinedCount++,
                            ConsoleColor.Red,
                            description: "Should not run.")
                        .AddConfirmOption(
                            "Confirmed async",
                            "Run async?",
                            async () =>
                            {
                                await Task.Yield();
                                asyncCount++;
                            },
                            "green",
                            description: "Runs async.")
                        .AddConfirmOption(
                            "Confirmed model",
                            "Run model?",
                            item => item.Count++,
                            ConsoleColor.Yellow,
                            description: "Mutates the active model.")
                        .AddConfirmOption(
                            "Confirmed token",
                            "Run token?",
                            async cancellationToken =>
                            {
                                await Task.Yield();
                                observedToken = cancellationToken;
                            },
                            new Style(ConsoleColor.Cyan),
                            description: "Observes confirmation token.");

                    await view.RenderAsync(cancellation.Token);

                    Assert.Equal(0, declinedCount);
                    Assert.Equal(1, asyncCount);
                    Assert.Equal(1, model.Count);
                    Assert.Equal(cancellation.Token, observedToken);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Run declined? [y/N]");
                    VisualAssert.WroteText(scope.Driver, "Run token? [y/N]");
                    VisualAssert.WroteText(scope.Driver, "Should not run.");
                    VisualAssert.WroteText(scope.Driver, "Observes confirmation token.");
                    AssertColoredWrite(scope.Driver, "Confirmed async", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Confirmed token", ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views render current model state and re-render after model actions mutate it.
        /// </summary>
        [Fact]
        public void DynamicModelView_Render_RendersAndMutatesModel()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .RenderModel(counter => Consoul.Write("Count: " + counter.Count, ConsoleColor.Cyan))
                        .Option(counter => "Increment from " + counter.Count, counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Count: 0");
                    VisualAssert.WroteText(scope.Driver, "Count: 1");
                    VisualAssert.WroteText(scope.Driver, "Increment from 0");
                    VisualAssert.WroteText(scope.Driver, "Increment from 1");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Default-constructed model-backed dynamic views keep first-run POC screens in the MVC-style view path.
        /// </summary>
        [Fact]
        public void DynamicModelView_DefaultConstructedModel_RendersAndMutatesModel()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");

                try
                {
                    var view = Consoul.View<CounterModel>("Counter")
                        .RenderModel(counter => Consoul.Write("Count: " + counter.Count, ConsoleColor.Cyan))
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, view.Model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Count: 0");
                    VisualAssert.WroteText(scope.Driver, "Count: 1");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render reusable components directly from the current model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_RenderModel_AcceptsReusableRenderableFactory()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .RenderModel(counter =>
                        {
                            var list = new PropertyListEntry(new PropertyListOptions
                            {
                                Width = 40,
                                LabelWidth = 10
                            });
                            list.Add("Count", counter.Count, ConsoleColor.Cyan);
                            list.Add("State", counter.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
                            return list;
                        })
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Count");
                    VisualAssert.WroteText(scope.Driver, "0");
                    VisualAssert.WroteText(scope.Driver, "Changed");
                    VisualAssert.WroteText(scope.Driver, "Increment");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can promote ad hoc render actions into reusable renderable sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_RenderModel_AcceptsRenderableActionFactory()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .RenderModel(counter => Consoul.Renderable(() =>
                        {
                            Consoul.Rule("State", width: 40);
                            Consoul.PropertyList(list =>
                            {
                                list.Add("Count", counter.Count, ConsoleColor.Cyan);
                                list.Add("Mode", counter.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
                            }, new PropertyListOptions { Width = 40, LabelWidth = 10 });
                        }))
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "State");
                    VisualAssert.WroteText(scope.Driver, "Idle");
                    VisualAssert.WroteText(scope.Driver, "Changed");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can append named MVC-style sections that re-render from current model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_Section_AppendsNamedSectionsAndRerenders()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .RenderModel(counter => Consoul.Write("Header count: " + counter.Count, ConsoleColor.Cyan))
                        .Section(
                            "Summary",
                            counter => Consoul.PropertyList(list =>
                            {
                                list.Add("Count", counter.Count, ConsoleColor.Cyan);
                                list.Add("State", counter.Count == 0 ? "Idle" : "Changed", ConsoleColor.Green);
                            }, new PropertyListOptions { Width = 48, LabelWidth = 10 }),
                            ConsoleColor.Yellow)
                        .Section(
                            counter => "Audit " + counter.Count,
                            counter => Consoul.TextBlock("Mutation count " + counter.Count, width: 48, color: ConsoleColor.DarkGray),
                            counter => ConsoleColor.Green)
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Header count: 0");
                    VisualAssert.WroteText(scope.Driver, "Header count: 1");
                    VisualAssert.WroteText(scope.Driver, "Summary");
                    VisualAssert.WroteText(scope.Driver, "Idle");
                    VisualAssert.WroteText(scope.Driver, "Changed");
                    VisualAssert.WroteText(scope.Driver, "Audit 0");
                    VisualAssert.WroteText(scope.Driver, "Audit 1");
                    VisualAssert.WroteText(scope.Driver, "Mutation count 1");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic view sections can render reusable components from the current model.
        /// </summary>
        [Fact]
        public void DynamicModelView_Section_AcceptsRenderableFactory()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .Section(
                            "Snapshot",
                            counter => new PanelEntry(
                                "Panel count: " + counter.Count,
                                new PanelOptions { Title = "Counter panel", Width = 42 }),
                            ConsoleColor.Cyan)
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Snapshot");
                    VisualAssert.WroteText(scope.Driver, "Counter panel");
                    VisualAssert.WroteText(scope.Driver, "Panel count: 0");
                    VisualAssert.WroteText(scope.Driver, "Panel count: 1");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic view sections accept Spectre-style Justify aliases without leaving the MVC section API.
        /// </summary>
        [Fact]
        public void DynamicModelView_Sections_AcceptJustifyAlignmentAliases()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                var model = new CounterModel();
                Func<CounterModel, IConsoulRenderable> factory = counter => new Text("Factory body " + counter.Count);
                Func<CounterModel, IConsoulRenderable> visibleFactory = counter => new Text("Visible factory body " + counter.Count);

                try
                {
                    DynamicView<CounterModel> view = Consoul.View("Counter", model)
                        .Section(
                            "Left Action",
                            counter => Consoul.Write("Left body " + counter.Count, ConsoleColor.White),
                            Justify.Left)
                        .Section(
                            "Right Action",
                            counter => Consoul.Write("Right body " + counter.Count, ConsoleColor.White),
                            ConsoleColor.Cyan,
                            Justify.Right)
                        .Section(
                            "Styled Center",
                            counter => Consoul.Write("Styled body " + counter.Count, ConsoleColor.White),
                            "aqua",
                            Justify.Center)
                        .Section(
                            "Renderable Right",
                            new Text("Renderable body"),
                            Justify.Right)
                        .Section(
                            "Rgb Renderable",
                            new Text("RGB renderable body"),
                            RgbColor.FromHex("#22c55e"),
                            Justify.Center)
                        .Section(
                            "Factory Left",
                            factory,
                            SpectreColor.Aqua,
                            Justify.Left)
                        .Section(
                            counter => "Dynamic Right " + counter.Count,
                            counter => Consoul.Write("Dynamic body " + counter.Count, ConsoleColor.White),
                            counter => ConsoleColor.Yellow,
                            Justify.Right)
                        .SectionWhen(
                            "Visible Right",
                            counter => Consoul.Write("Visible body " + counter.Count, ConsoleColor.White),
                            counter => true,
                            Justify.Right)
                        .SectionWhen(
                            "Visible Styled",
                            counter => Consoul.Write("Visible styled body " + counter.Count, ConsoleColor.White),
                            counter => true,
                            "yellow",
                            Justify.Center)
                        .SectionWhen(
                            "Visible Renderable",
                            new Text("Visible renderable body"),
                            counter => true,
                            Justify.Right)
                        .SectionWhen(
                            "Visible Factory",
                            visibleFactory,
                            counter => true,
                            SpectreColor.Green,
                            Justify.Center);

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Left body 0");
                    VisualAssert.WroteText(scope.Driver, "Right body 0");
                    VisualAssert.WroteText(scope.Driver, "Styled body 0");
                    VisualAssert.WroteText(scope.Driver, "Renderable body");
                    VisualAssert.WroteText(scope.Driver, "RGB renderable body");
                    VisualAssert.WroteText(scope.Driver, "Factory body 0");
                    VisualAssert.WroteText(scope.Driver, "Dynamic body 0");
                    VisualAssert.WroteText(scope.Driver, "Visible body 0");
                    VisualAssert.WroteText(scope.Driver, "Visible styled body 0");
                    VisualAssert.WroteText(scope.Driver, "Visible renderable body");
                    VisualAssert.WroteText(scope.Driver, "Visible factory body 0");
                    Assert.Contains(scope.Driver.Lines, line => line.Contains("Left Action") && line.IndexOf("Left Action", StringComparison.Ordinal) < 8);
                    Assert.Contains(scope.Driver.Lines, line => line.Contains("Right Action") && line.IndexOf("Right Action", StringComparison.Ordinal) > 40);
                    Assert.Contains(scope.Driver.Lines, line => line.Contains("Dynamic Right 0") && line.IndexOf("Dynamic Right 0", StringComparison.Ordinal) > 40);
                    Assert.Contains(scope.Driver.Lines, line => line.Contains("Visible Right") && line.IndexOf("Visible Right", StringComparison.Ordinal) > 40);
                    AssertColoredWrite(scope.Driver, "Right Action", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Styled Center", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Dynamic Right 0", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Visible Styled", ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate read-only property-list sections from the active model metadata.
        /// </summary>
        [Fact]
        public void DynamicModelView_PropertyListSection_RendersCurrentModelMetadata()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                InitializeRoutine("1", "2");
                var model = new SummarySettings
                {
                    Service = "API",
                    Environment = "Development",
                    Port = 5000
                };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .PropertyListSection(
                            "Overview",
                            new PropertyListOptions
                            {
                                Width = 48,
                                LabelWidth = 14
                            },
                            ConsoleColor.Cyan)
                        .Option("Promote", settings =>
                        {
                            settings.Environment = "Production";
                            settings.Port = 443;
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal("Production", model.Environment);
                    Assert.Equal(443, model.Port);
                    VisualAssert.WroteText(scope.Driver, "Overview");
                    VisualAssert.WroteText(scope.Driver, "Service");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Development");
                    VisualAssert.WroteText(scope.Driver, "Production");
                    VisualAssert.WroteText(scope.Driver, "Port");
                    VisualAssert.WroteText(scope.Driver, "443");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Overview") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive table sections from collections owned by the view model.
        /// </summary>
        [Fact]
        public void DynamicModelView_TableSection_RendersCurrentModelCollection()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2");
                var model = new ServicesDashboard();
                model.Services.Add(new ServiceSummary("API", "Ready", 2));
                model.Services.Add(new ServiceSummary("Worker", "Waiting", 0));

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .TableSection(
                            "Services",
                            dashboard => dashboard.Services,
                            new TableRenderOptions
                            {
                                MaximumTableWidth = 56,
                                Expand = true
                            },
                            ConsoleColor.Yellow,
                            RuleTitleAlignment.Left,
                            service => service.Name,
                            service => service.State,
                            service => service.Replicas)
                        .Option("Scale worker", dashboard =>
                        {
                            dashboard.Services[1].State = "Ready";
                            dashboard.Services[1].Replicas = 1;
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal("Ready", model.Services[1].State);
                    Assert.Equal(1, model.Services[1].Replicas);
                    VisualAssert.WroteText(scope.Driver, "Services");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Worker");
                    VisualAssert.WroteText(scope.Driver, "Waiting");
                    VisualAssert.WroteText(scope.Driver, "Ready");
                    VisualAssert.WroteText(scope.Driver, "Replicas");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Services") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated property-list sections can stay hidden until the model reaches the relevant state.
        /// </summary>
        [Fact]
        public void DynamicModelView_PropertyListSectionWhen_RendersAfterPredicateBecomesTrue()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                InitializeRoutine("1", "2");
                var model = new SummarySettings
                {
                    Service = "API",
                    Environment = "Development",
                    Port = 5000
                };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .PropertyListSectionWhenStyled(
                            "Production details",
                            settings => settings.Environment == "Production",
                            new PropertyListOptions
                            {
                                Width = 48,
                                LabelWidth = 14
                            },
                            "aqua")
                        .Option("Promote", settings =>
                        {
                            settings.Environment = "Production";
                            settings.Port = 443;
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal("Production", model.Environment);
                    Assert.Equal(443, model.Port);
                    Assert.DoesNotContain("Development", scope.Driver.Text);
                    Assert.DoesNotContain("5000", scope.Driver.Text);
                    VisualAssert.WroteText(scope.Driver, "Production details");
                    VisualAssert.WroteText(scope.Driver, "Service");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Production");
                    VisualAssert.WroteText(scope.Driver, "443");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production details") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated table sections can stay hidden until the view model has rows to display.
        /// </summary>
        [Fact]
        public void DynamicModelView_TableSectionWhen_RendersAfterRowsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2");
                var model = new ServicesDashboard();

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .TableSectionWhenStyled(
                            "Loaded services",
                            dashboard => dashboard.Services,
                            dashboard => dashboard.Services.Count > 0,
                            new TableRenderOptions
                            {
                                MaximumTableWidth = 56,
                                Expand = true
                            },
                            "yellow",
                            RuleTitleAlignment.Left,
                            service => service.Name,
                            service => service.State,
                            service => service.Replicas)
                        .Option("Load services", dashboard =>
                        {
                            dashboard.Services.Add(new ServiceSummary("API", "Ready", 2));
                            dashboard.Services.Add(new ServiceSummary("Worker", "Waiting", 0));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Services.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded services");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Worker");
                    VisualAssert.WroteText(scope.Driver, "Waiting");
                    VisualAssert.WroteText(scope.Driver, "Replicas");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded services") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive metrics sections from model dashboard state.
        /// </summary>
        [Fact]
        public void DynamicModelView_MetricsSection_RendersCurrentModelMetrics()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2");
                var model = new MetricsDashboard();
                model.Metrics.Add(new DashboardMetric("Queued", 3, "jobs", "waiting", MetricKind.Info));
                model.Metrics.Add(new DashboardMetric("Errors", 0, "items", "healthy", MetricKind.Success));

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .MetricsSection(
                            "Health",
                            dashboard => dashboard.Metrics,
                            metric => metric.Name,
                            metric => metric.Value,
                            metric => metric.Unit,
                            metric => metric.Note,
                            metric => metric.Kind,
                            new MetricsOptions
                            {
                                Width = 56,
                                Columns = 2,
                                MinimumColumnWidth = 16
                            },
                            ConsoleColor.Cyan)
                        .Option("Record error", dashboard =>
                        {
                            DashboardMetric errors = dashboard.Metrics[1];
                            errors.Value = 1;
                            errors.Kind = MetricKind.Error;
                            errors.Note = "needs attention";
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(1, model.Metrics[1].Value);
                    Assert.Equal(MetricKind.Error, model.Metrics[1].Kind);
                    VisualAssert.WroteText(scope.Driver, "Health");
                    VisualAssert.WroteText(scope.Driver, "Queued");
                    VisualAssert.WroteText(scope.Driver, "Errors");
                    VisualAssert.WroteText(scope.Driver, "needs attention");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Health") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated metrics sections can stay hidden until model state has summary values to render.
        /// </summary>
        [Fact]
        public void DynamicModelView_MetricsSectionWhen_RendersAfterMetricsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new MetricsDashboard();

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .MetricsSectionWhenStyled(
                            "Live health",
                            dashboard => dashboard.Metrics,
                            dashboard => dashboard.Metrics.Count > 0,
                            metric => metric.Name,
                            metric => metric.Value,
                            metric => metric.Unit,
                            metric => metric.Note,
                            metric => metric.Kind,
                            new MetricsOptions
                            {
                                Width = 44,
                                Columns = 1,
                                MinimumColumnWidth = 12
                            },
                            "yellow")
                        .Option("Load metrics", dashboard =>
                        {
                            dashboard.Metrics.Add(new DashboardMetric("Latency", 42, "ms", "p95", MetricKind.Success));
                            dashboard.Metrics.Add(new DashboardMetric("Retries", 2, "calls", "watch", MetricKind.Warning));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Metrics.Count);
                    VisualAssert.WroteText(scope.Driver, "Live health");
                    VisualAssert.WroteText(scope.Driver, "Latency");
                    VisualAssert.WroteText(scope.Driver, "Retries");
                    VisualAssert.WroteText(scope.Driver, "watch");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Live health") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive bar-chart sections from model dashboard state.
        /// </summary>
        [Fact]
        public void DynamicModelView_BarChartSection_RendersCurrentModelChart()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2");
                var model = new ChartDashboard();
                model.Items.Add(new DashboardChartItem("Passing", 8, ConsoleColor.Green, "green"));
                model.Items.Add(new DashboardChartItem("Failing", 1, ConsoleColor.Red, "red"));

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .BarChartSection(
                            "Build health",
                            dashboard => dashboard.Items,
                            item => item.Label,
                            item => item.Value,
                            item => item.Color,
                            new BarChartOptions
                            {
                                Label = "Build jobs",
                                Width = 56,
                                MaximumValue = 10
                            },
                            ConsoleColor.Cyan)
                        .Option("Record failure", dashboard => dashboard.Items[1].Value = 2);

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Items[1].Value);
                    VisualAssert.WroteText(scope.Driver, "Build health");
                    VisualAssert.WroteText(scope.Driver, "Build jobs");
                    VisualAssert.WroteText(scope.Driver, "Passing");
                    VisualAssert.WroteText(scope.Driver, "Failing");
                    VisualAssert.WroteText(scope.Driver, "2");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build health") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Red);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated breakdown-chart sections can stay hidden until model state has chart rows to render.
        /// </summary>
        [Fact]
        public void DynamicModelView_BreakdownChartSectionWhen_RendersAfterItemsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new ChartDashboard();

                try
                {
                    var view = Consoul.View("Queue", model)
                        .BreakdownChartSectionWhenStyled(
                            "Queue mix",
                            dashboard => dashboard.Items,
                            dashboard => dashboard.Items.Count > 0,
                            item => item.Label,
                            item => item.Value,
                            item => item.StyleText,
                            new BreakdownChartOptions
                            {
                                Label = "Current queue",
                                Width = 44,
                                ShowPercentages = true
                            },
                            "yellow")
                        .Option("Load queue", dashboard =>
                        {
                            dashboard.Items.Add(new DashboardChartItem("Queued", 12, ConsoleColor.Yellow, "yellow"));
                            dashboard.Items.Add(new DashboardChartItem("Running", 5, ConsoleColor.Cyan, "aqua"));
                            dashboard.Items.Add(new DashboardChartItem("Failed", 1, ConsoleColor.Red, "red"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Items.Count);
                    VisualAssert.WroteText(scope.Driver, "Queue mix");
                    VisualAssert.WroteText(scope.Driver, "Current queue");
                    VisualAssert.WroteText(scope.Driver, "Queued");
                    VisualAssert.WroteText(scope.Driver, "Running");
                    VisualAssert.WroteText(scope.Driver, "Failed");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queue mix") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Red);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive timeline sections from workflow state.
        /// </summary>
        [Fact]
        public void DynamicModelView_TimelineSection_RendersCurrentModelWorkflow()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                InitializeRoutine("1", "2");
                var model = new TimelineDashboard();
                model.Steps.Add(new DashboardTimelineStep(
                    "Build",
                    TimelineItemState.Complete,
                    "Artifacts ready.",
                    new DateTimeOffset(2026, 7, 5, 9, 0, 0, TimeSpan.Zero),
                    "green",
                    "green",
                    "grey"));
                model.Steps.Add(new DashboardTimelineStep(
                    "Review",
                    TimelineItemState.Active,
                    "Waiting for owner.",
                    new DateTimeOffset(2026, 7, 5, 9, 15, 0, TimeSpan.Zero),
                    "aqua",
                    "aqua",
                    "grey"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .TimelineSection(
                            "Workflow",
                            dashboard => dashboard.Steps,
                            step => step.Title,
                            step => step.State,
                            step => step.Description,
                            step => step.Timestamp,
                            new TimelineOptions
                            {
                                Width = 56,
                                TimestampFormat = "HH:mm"
                            },
                            ConsoleColor.Cyan)
                        .Option("Publish", dashboard =>
                        {
                            dashboard.Steps.Add(new DashboardTimelineStep(
                                "Publish",
                                TimelineItemState.Pending,
                                "Waiting for release window.",
                                new DateTimeOffset(2026, 7, 5, 9, 30, 0, TimeSpan.Zero),
                                "yellow",
                                "yellow",
                                "darkgray"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Steps.Count);
                    VisualAssert.WroteText(scope.Driver, "Workflow");
                    VisualAssert.WroteText(scope.Driver, "Build");
                    VisualAssert.WroteText(scope.Driver, "Review");
                    VisualAssert.WroteText(scope.Driver, "Publish");
                    VisualAssert.WroteText(scope.Driver, "09:30");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Workflow") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated timeline sections can stay hidden until model state has workflow steps to render.
        /// </summary>
        [Fact]
        public void DynamicModelView_TimelineSectionWhen_RendersAfterStepsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new TimelineDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .TimelineSectionWhenStyled(
                            "Loaded workflow",
                            dashboard => dashboard.Steps,
                            dashboard => dashboard.Steps.Count > 0,
                            step => step.Title,
                            step => step.State,
                            step => step.Description,
                            step => step.Timestamp,
                            step => step.TitleStyle,
                            step => step.MarkerStyle,
                            step => step.DescriptionStyle,
                            new TimelineOptions
                            {
                                Width = 44,
                                TimestampFormat = "HH:mm"
                            },
                            "yellow")
                        .Option("Load workflow", dashboard =>
                        {
                            dashboard.Steps.Add(new DashboardTimelineStep(
                                "Queued",
                                TimelineItemState.Complete,
                                "Release accepted.",
                                new DateTimeOffset(2026, 7, 5, 10, 0, 0, TimeSpan.Zero),
                                "green",
                                "green",
                                "grey"));
                            dashboard.Steps.Add(new DashboardTimelineStep(
                                "Deploy",
                                TimelineItemState.Warning,
                                "Waiting for approval.",
                                new DateTimeOffset(2026, 7, 5, 10, 15, 0, TimeSpan.Zero),
                                "white on navy",
                                "yellow",
                                "darkgray"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Steps.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded workflow");
                    VisualAssert.WroteText(scope.Driver, "Queued");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "Waiting for approval");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded workflow") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive row composition sections from current model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_RowsSection_RendersCurrentModelRows()
        {
            using (var scope = new ConsoleDriverScope(46))
            {
                InitializeRoutine("1", "2");
                var model = new CompositionDashboard();
                model.Rows.Add(new CompositionItem("Build", "Queued", ConsoleColor.Yellow, "yellow"));
                model.Rows.Add(new CompositionItem("Review", "Waiting for owner", ConsoleColor.Cyan, "aqua"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .RowsSection(
                            "Operations",
                            dashboard => dashboard.Rows,
                            item => item.Label + ": " + item.Value,
                            item => (ConsoleColor?)item.Color,
                            new RowsOptions
                            {
                                Width = 40,
                                Spacing = 0
                            },
                            ConsoleColor.Cyan)
                        .Option("Add deploy row", dashboard => dashboard.Rows.Add(new CompositionItem("Deploy", "Ready", ConsoleColor.Green, "green")));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Rows.Count);
                    VisualAssert.WroteText(scope.Driver, "Operations");
                    VisualAssert.WroteText(scope.Driver, "Build: Queued");
                    VisualAssert.WroteText(scope.Driver, "Review: Waiting");
                    VisualAssert.WroteText(scope.Driver, "Deploy: Ready");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Operations") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy: Ready") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated column composition sections can stay hidden until the model has side-by-side content to render.
        /// </summary>
        [Fact]
        public void DynamicModelView_ColumnsSectionWhen_RendersAfterColumnsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new CompositionDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ColumnsSectionWhenStyled(
                            "Loaded columns",
                            dashboard => dashboard.Columns,
                            dashboard => dashboard.Columns.Count > 0,
                            item => item.Value,
                            item => item.Label,
                            item => item.StyleText,
                            new ColumnsOptions
                            {
                                Width = 40,
                                Columns = 2,
                                MinimumColumnWidth = 12,
                                Gutter = 2
                            },
                            "yellow")
                        .Option("Load columns", dashboard =>
                        {
                            dashboard.Columns.Add(new CompositionItem("API", "Ready", ConsoleColor.White, "white on navy"));
                            dashboard.Columns.Add(new CompositionItem("Worker", "Waiting", ConsoleColor.Yellow, "yellow"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Columns.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded columns");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Ready");
                    VisualAssert.WroteText(scope.Driver, "Worker");
                    VisualAssert.WroteText(scope.Driver, "Waiting");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded columns") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive grid sections from current cell models.
        /// </summary>
        [Fact]
        public void DynamicModelView_GridSection_RendersCurrentModelCells()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new CompositionDashboard();
                model.Cells.Add(new CompositionItem("Service", "API", ConsoleColor.Cyan, "aqua"));
                model.Cells.Add(new CompositionItem("State", "Ready", ConsoleColor.Green, "green"));
                model.Cells.Add(new CompositionItem("Owner", "Platform", ConsoleColor.Yellow, "yellow"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .GridSection(
                            "Service grid",
                            dashboard => dashboard.Cells,
                            item => item.Value,
                            item => item.Label,
                            item => (ConsoleColor?)item.Color,
                            new GridOptions
                            {
                                Width = 44,
                                Columns = 2,
                                MinimumColumnWidth = 12,
                                Gutter = 2
                            },
                            ConsoleColor.Cyan)
                        .Option("Add health", dashboard => dashboard.Cells.Add(new CompositionItem("Health", "Green", ConsoleColor.Green, "green")));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(4, model.Cells.Count);
                    VisualAssert.WroteText(scope.Driver, "Service grid");
                    VisualAssert.WroteText(scope.Driver, "Service");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Health");
                    VisualAssert.WroteText(scope.Driver, "Green");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service grid") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Green") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can build responsive layout dashboards from current model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_LayoutSection_BuildsDashboardFromCurrentModel()
        {
            using (var scope = new ConsoleDriverScope(54))
            {
                InitializeRoutine("1", "2");
                var model = new CompositionDashboard();
                model.Panes.Add(new CompositionItem("Status", "Green and ready", ConsoleColor.Green, "green"));
                model.Panes.Add(new CompositionItem("Pipeline", "Waiting for approval", ConsoleColor.Cyan, "aqua"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .LayoutSection(
                            "Dashboard panes",
                            (dashboard, layout) =>
                            {
                                layout
                                    .Width(50)
                                    .ColumnCount(2)
                                    .MinimumPaneWidth(14)
                                    .AddPanes(
                                        dashboard.Panes,
                                        item => item.Value,
                                        item => item.Label,
                                        item => (ConsoleColor?)item.Color,
                                        captureWidth: 24);
                            },
                            titleColor: ConsoleColor.Cyan)
                        .Option("Add risk pane", dashboard => dashboard.Panes.Add(new CompositionItem("Risk", "Low", ConsoleColor.Yellow, "yellow")));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Panes.Count);
                    VisualAssert.WroteText(scope.Driver, "Dashboard panes");
                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Green and ready");
                    VisualAssert.WroteText(scope.Driver, "Risk");
                    VisualAssert.WroteText(scope.Driver, "Low");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Dashboard panes") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Low") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Conditional model-projected layout sections render after model panes are loaded.
        /// </summary>
        [Fact]
        public void DynamicModelView_LayoutSectionWhen_ProjectsLoadedPanes()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new CompositionDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .LayoutSectionWhenStyled(
                            "Loaded panes",
                            dashboard => dashboard.Panes,
                            dashboard => dashboard.Panes.Count > 0,
                            item => item.Value,
                            item => item.Label,
                            item => item.StyleText,
                            new LayoutOptions
                            {
                                Width = 44,
                                Columns = 2,
                                MinimumPaneWidth = 14
                            },
                            captureWidth: 22,
                            titleStyle: "yellow")
                        .Option("Load panes", dashboard =>
                        {
                            dashboard.Panes.Add(new CompositionItem("API", "Ready", ConsoleColor.White, "white on navy"));
                            dashboard.Panes.Add(new CompositionItem("Worker", "Queued", ConsoleColor.Yellow, "yellow"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Panes.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded panes");
                    VisualAssert.WroteText(scope.Driver, "API");
                    VisualAssert.WroteText(scope.Driver, "Ready");
                    VisualAssert.WroteText(scope.Driver, "Worker");
                    VisualAssert.WroteText(scope.Driver, "Queued");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded panes") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive sparkline sections from trend values.
        /// </summary>
        [Fact]
        public void DynamicModelView_SparklineSection_RendersCurrentModelTrend()
        {
            using (var scope = new ConsoleDriverScope(52))
            {
                InitializeRoutine("1", "2");
                var model = new TrendDashboard();
                model.LatencySamples.AddRange(new[] { 12d, 10d, 11d, 8d, 7d, 9d });

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .SparklineSection(
                            "Latency trend",
                            dashboard => dashboard.LatencySamples,
                            new SparklineOptions
                            {
                                Label = "P95 latency",
                                Width = 44,
                                ShowRange = true,
                                Characters = ".#"
                            },
                            ConsoleColor.Cyan)
                        .Option("Record recovery", dashboard => dashboard.LatencySamples.Add(6));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(7, model.LatencySamples.Count);
                    VisualAssert.WroteText(scope.Driver, "Latency trend");
                    VisualAssert.WroteText(scope.Driver, "P95 latency");
                    VisualAssert.WroteText(scope.Driver, "6..12");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Latency trend") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated sparkline sections can project sample models and stay hidden until data is available.
        /// </summary>
        [Fact]
        public void DynamicModelView_SparklineSectionWhen_RendersAfterSamplesAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new TrendDashboard();

                try
                {
                    var view = Consoul.View("Dashboard", model)
                        .SparklineSectionWhenStyled(
                            "Loaded trend",
                            dashboard => dashboard.Samples,
                            dashboard => dashboard.Samples.Count > 0,
                            sample => sample.Minutes,
                            new SparklineOptions
                            {
                                Label = "Duration",
                                Width = 40,
                                ShowRange = true,
                                Characters = ".#"
                            },
                            "yellow")
                        .Option("Load samples", dashboard =>
                        {
                            dashboard.Samples.Add(new DashboardTrendSample("Build", 12d));
                            dashboard.Samples.Add(new DashboardTrendSample("Test", 18d));
                            dashboard.Samples.Add(new DashboardTrendSample("Deploy", 10d));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Samples.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded trend");
                    VisualAssert.WroteText(scope.Driver, "Duration");
                    VisualAssert.WroteText(scope.Driver, "10..18");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded trend") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive tree sections from nested model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_TreeSection_RendersCurrentModelHierarchy()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new DependencyDashboard
                {
                    Root = new DependencyNode("Deployment", "ready", ConsoleColor.Green, "green")
                };
                var api = new DependencyNode("API", "healthy", ConsoleColor.Cyan, "aqua");
                api.Children.Add(new DependencyNode("Database", "waiting on migration approval", ConsoleColor.Yellow, "yellow"));
                model.Root.Children.Add(api);

                try
                {
                    var view = Consoul.View("Dependencies", model)
                        .TreeSection(
                            "Dependency graph",
                            dashboard => dashboard.Root,
                            node => node.Name + " - " + node.State,
                            node => node.Children,
                            node => node.Color,
                            new TreeOptions
                            {
                                Width = 40,
                                Guide = TreeGuide.Ascii
                            },
                            ConsoleColor.Cyan)
                        .Option("Add worker", dashboard =>
                        {
                            dashboard.Root.Children.Add(new DependencyNode("Worker", "warming up", ConsoleColor.Red, "red"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Root.Children.Count);
                    VisualAssert.WroteText(scope.Driver, "Dependency graph");
                    VisualAssert.WroteText(scope.Driver, "Deployment - ready");
                    VisualAssert.WroteText(scope.Driver, "Database - waiting");
                    VisualAssert.WroteText(scope.Driver, "Worker - warming up");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Dependency graph") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.Red);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated tree sections can stay hidden until nested diagnostic state is available.
        /// </summary>
        [Fact]
        public void DynamicModelView_TreeSectionWhen_RendersAfterHierarchyIsLoaded()
        {
            using (var scope = new ConsoleDriverScope(42))
            {
                InitializeRoutine("1", "2");
                var model = new DependencyDashboard();

                try
                {
                    var view = Consoul.View("Diagnostics", model)
                        .TreeSectionWhenStyled(
                            "Loaded graph",
                            dashboard => dashboard.Root,
                            dashboard => dashboard.Root != null,
                            node => node.Name + " " + node.State,
                            node => node.Children,
                            node => node.StyleText,
                            new TreeOptions
                            {
                                Width = 38,
                                Guide = TreeGuide.Ascii
                            },
                            "yellow")
                        .Option("Load graph", dashboard =>
                        {
                            dashboard.Root = new DependencyNode("Release", "active", ConsoleColor.Green, "green");
                            dashboard.Root.Children.Add(new DependencyNode("Deploy", "blocked", ConsoleColor.White, "white on navy"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.NotNull(model.Root);
                    VisualAssert.WroteText(scope.Driver, "Loaded graph");
                    VisualAssert.WroteText(scope.Driver, "Release active");
                    VisualAssert.WroteText(scope.Driver, "Deploy blocked");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded graph") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive text-block sections from current explanatory text.
        /// </summary>
        [Fact]
        public void DynamicModelView_TextBlockSection_RendersCurrentModelSummary()
        {
            using (var scope = new ConsoleDriverScope(42))
            {
                InitializeRoutine("1", "2");
                var model = new TextBlockDashboard
                {
                    Summary = "Service is waiting for deployment approval."
                };

                try
                {
                    var view = Consoul.View("Release", model)
                        .TextBlockSection(
                            "Summary text",
                            dashboard => dashboard.Summary,
                            new TextBlockOptions
                            {
                                Width = 36,
                                Alignment = TextBlockAlignment.Center,
                                Scheme = new ColorScheme
                                {
                                    Color = ConsoleColor.Green,
                                    BackgroundColor = ConsoleColor.Black
                                }
                            },
                            ConsoleColor.Cyan)
                        .Option("Approve", dashboard => dashboard.Summary = "Service is approved for release.");

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal("Service is approved for release.", model.Summary);
                    VisualAssert.WroteText(scope.Driver, "Summary text");
                    VisualAssert.WroteText(scope.Driver, "Service is approved");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Summary text") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service is approved") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated text-block sections can stay hidden until explanatory text is available.
        /// </summary>
        [Fact]
        public void DynamicModelView_TextBlockSectionWhen_RendersAfterSummaryIsLoaded()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                InitializeRoutine("1", "2");
                var model = new TextBlockDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .TextBlockSectionWhenStyled(
                            "Loaded summary",
                            dashboard => dashboard.Summary,
                            dashboard => dashboard.SummaryLoaded,
                            new TextBlockOptions
                            {
                                Width = 34,
                                Overflow = Overflow.Ellipsis,
                                Scheme = new ColorScheme
                                {
                                    Color = ConsoleColor.Yellow,
                                    BackgroundColor = ConsoleColor.Black
                                }
                            },
                            "yellow")
                        .Option("Load summary", dashboard =>
                        {
                            dashboard.SummaryLoaded = true;
                            dashboard.Summary = "Rollback plan attached to release notes and ready for review.";
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.True(model.SummaryLoaded);
                    VisualAssert.WroteText(scope.Driver, "Loaded summary");
                    VisualAssert.WroteText(scope.Driver, "Rollback plan attached");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded summary") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rollback plan attached") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive panel sections from current detail state.
        /// </summary>
        [Fact]
        public void DynamicModelView_PanelSection_RendersCurrentModelDetails()
        {
            using (var scope = new ConsoleDriverScope(50))
            {
                InitializeRoutine("1", "2");
                var model = new PanelDashboard
                {
                    Details = "Build is waiting for owner approval."
                };

                try
                {
                    var view = Consoul.View("Release", model)
                        .PanelSection(
                            "Detail panel",
                            dashboard => dashboard.Details,
                            new PanelOptions
                            {
                                Title = "Deployment",
                                Footer = "Esc backs out",
                                Width = 42,
                                BorderStyle = PanelBorderStyle.Ascii
                            },
                            ConsoleColor.Cyan)
                        .Option("Approve", dashboard => dashboard.Details = "Build approved and queued.");

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal("Build approved and queued.", model.Details);
                    VisualAssert.WroteText(scope.Driver, "Detail panel");
                    VisualAssert.WroteText(scope.Driver, "Deployment");
                    VisualAssert.WroteText(scope.Driver, "Build approved and queued.");
                    VisualAssert.WroteText(scope.Driver, "Esc backs out");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Detail panel") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated panel sections can stay hidden until detail content is available.
        /// </summary>
        [Fact]
        public void DynamicModelView_PanelSectionWhen_RendersAfterDetailsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new PanelDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .PanelSectionWhenStyled(
                            "Loaded detail",
                            dashboard => dashboard.Details,
                            dashboard => dashboard.DetailsLoaded,
                            new PanelOptions
                            {
                                Title = "Preview",
                                Width = 38,
                                BorderStyle = PanelBorderStyle.Ascii
                            },
                            "yellow")
                        .Option("Load detail", dashboard =>
                        {
                            dashboard.DetailsLoaded = true;
                            dashboard.Details = "Rollback plan attached to the release.";
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.True(model.DetailsLoaded);
                    VisualAssert.WroteText(scope.Driver, "Loaded detail");
                    VisualAssert.WroteText(scope.Driver, "Preview");
                    VisualAssert.WroteText(scope.Driver, "Rollback plan attached");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded detail") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive callout sections from current status state.
        /// </summary>
        [Fact]
        public void DynamicModelView_CalloutSection_RendersCurrentModelStatus()
        {
            using (var scope = new ConsoleDriverScope(50))
            {
                InitializeRoutine("1", "2");
                var model = new CalloutDashboard
                {
                    Message = "Waiting for owner approval.",
                    Kind = CalloutKind.Warning
                };

                try
                {
                    var view = Consoul.View("Release", model)
                        .CalloutSection(
                            "Status message",
                            dashboard => dashboard.Message,
                            dashboard => dashboard.Kind,
                            new CalloutOptions
                            {
                                Title = "Release",
                                Width = 42
                            },
                            ConsoleColor.Cyan)
                        .Option("Approve", dashboard =>
                        {
                            dashboard.Message = "Ready for deploy.";
                            dashboard.Kind = CalloutKind.Success;
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal("Ready for deploy.", model.Message);
                    Assert.Equal(CalloutKind.Success, model.Kind);
                    VisualAssert.WroteText(scope.Driver, "Status message");
                    VisualAssert.WroteText(scope.Driver, "Release");
                    VisualAssert.WroteText(scope.Driver, "Ready for deploy.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status message") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated callout sections can stay hidden until status details are available.
        /// </summary>
        [Fact]
        public void DynamicModelView_CalloutSectionWhen_RendersAfterStatusIsLoaded()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new CalloutDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .CalloutSectionWhenStyled(
                            "Loaded status",
                            dashboard => dashboard.Message,
                            dashboard => dashboard.StatusLoaded,
                            dashboard => dashboard.Kind,
                            new CalloutOptions
                            {
                                Title = "Health",
                                Width = 38
                            },
                            "yellow")
                        .Option("Load status", dashboard =>
                        {
                            dashboard.StatusLoaded = true;
                            dashboard.Message = "Deployment is blocked by validation.";
                            dashboard.Kind = CalloutKind.Error;
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.True(model.StatusLoaded);
                    VisualAssert.WroteText(scope.Driver, "Loaded status");
                    VisualAssert.WroteText(scope.Driver, "Health");
                    VisualAssert.WroteText(scope.Driver, "Deployment is blocked");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded status") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate bounded exception sections from current error state.
        /// </summary>
        [Fact]
        public void DynamicModelView_ExceptionSection_RendersCurrentModelError()
        {
            using (var scope = new ConsoleDriverScope(50))
            {
                InitializeRoutine("1", "2");
                var model = new ExceptionDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ExceptionSection(
                            "Failure",
                            dashboard => dashboard.LastError,
                            dashboard => dashboard.Context,
                            new ExceptionRenderOptions
                            {
                                Title = "Preview failed",
                                Width = 44,
                                IncludeStackTrace = false,
                                Format = ExceptionFormats.ShortenTypes
                            },
                            ConsoleColor.Red)
                        .Option("Run preview", dashboard =>
                        {
                            dashboard.Context = "Latest diagnostic";
                            dashboard.LastError = new InvalidOperationException("Preview failed because owner approval is missing.");
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.NotNull(model.LastError);
                    VisualAssert.WroteText(scope.Driver, "Failure");
                    VisualAssert.WroteText(scope.Driver, "Preview failed");
                    VisualAssert.WroteText(scope.Driver, "Latest diagnostic");
                    VisualAssert.WroteText(scope.Driver, "InvalidOperationException");
                    VisualAssert.WroteText(scope.Driver, "owner");
                    VisualAssert.WroteText(scope.Driver, "approval");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Failure") && write.Color == ConsoleColor.Red);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated exception sections can require additional model state before showing diagnostics.
        /// </summary>
        [Fact]
        public void DynamicModelView_ExceptionSectionWhen_RendersAfterDiagnosticsAreEnabled()
        {
            using (var scope = new ConsoleDriverScope(46))
            {
                InitializeRoutine("1", "2");
                var model = new ExceptionDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ExceptionSectionWhenStyled(
                            "Diagnostics",
                            dashboard => dashboard.LastError,
                            dashboard => dashboard.ShowDiagnostics,
                            dashboard => dashboard.Context,
                            new ExceptionRenderOptions
                            {
                                Title = "Action failed",
                                Width = 40,
                                IncludeStackTrace = false,
                                Format = ExceptionFormats.ShortenTypes
                            },
                            "yellow")
                        .Option("Validate target", dashboard =>
                        {
                            dashboard.ShowDiagnostics = true;
                            dashboard.Context = "Validation failed";
                            dashboard.LastError = new ArgumentException("Target environment is blocked.");
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.True(model.ShowDiagnostics);
                    VisualAssert.WroteText(scope.Driver, "Diagnostics");
                    VisualAssert.WroteText(scope.Driver, "Action failed");
                    VisualAssert.WroteText(scope.Driver, "Validation failed");
                    VisualAssert.WroteText(scope.Driver, "ArgumentException");
                    VisualAssert.WroteText(scope.Driver, "Target environment");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Diagnostics") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive breadcrumb sections from current navigation state.
        /// </summary>
        [Fact]
        public void DynamicModelView_BreadcrumbSection_RendersCurrentModelTrail()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new BreadcrumbDashboard();
                model.Trail.Add(new BreadcrumbSegment("Home", ConsoleColor.DarkGray, "grey"));
                model.Trail.Add(new BreadcrumbSegment("Operations", ConsoleColor.Cyan, "aqua"));
                model.Trail.Add(new BreadcrumbSegment("Deployments", ConsoleColor.Green, "green"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .BreadcrumbSection(
                            "Navigation",
                            dashboard => dashboard.Trail,
                            segment => segment.Label,
                            segment => (ConsoleColor?)segment.Color,
                            new BreadcrumbOptions
                            {
                                Width = 42,
                                Separator = " / "
                            },
                            ConsoleColor.Cyan)
                        .Option("Open target", dashboard => dashboard.Trail.Add(new BreadcrumbSegment("Production", ConsoleColor.Yellow, "yellow")));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(4, model.Trail.Count);
                    VisualAssert.WroteText(scope.Driver, "Navigation");
                    VisualAssert.WroteText(scope.Driver, "Home");
                    VisualAssert.WroteText(scope.Driver, "Operations");
                    VisualAssert.WroteText(scope.Driver, "Production");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Navigation") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated breadcrumb sections can stay hidden until navigation state is available.
        /// </summary>
        [Fact]
        public void DynamicModelView_BreadcrumbSectionWhen_RendersAfterTrailIsLoaded()
        {
            using (var scope = new ConsoleDriverScope(42))
            {
                InitializeRoutine("1", "2");
                var model = new BreadcrumbDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .BreadcrumbSectionWhenStyled(
                            "Loaded navigation",
                            dashboard => dashboard.Trail,
                            dashboard => dashboard.Trail.Count > 0,
                            segment => segment.Label,
                            segment => segment.StyleText,
                            new BreadcrumbOptions
                            {
                                Width = 38,
                                Separator = " > "
                            },
                            "yellow")
                        .Option("Load trail", dashboard =>
                        {
                            dashboard.Trail.Add(new BreadcrumbSegment("Home", ConsoleColor.DarkGray, "grey"));
                            dashboard.Trail.Add(new BreadcrumbSegment("Services", ConsoleColor.Cyan, "aqua"));
                            dashboard.Trail.Add(new BreadcrumbSegment("API", ConsoleColor.White, "white on navy"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Trail.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded navigation");
                    VisualAssert.WroteText(scope.Driver, "Home");
                    VisualAssert.WroteText(scope.Driver, "Services");
                    VisualAssert.WroteText(scope.Driver, "API");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded navigation") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive shortcut bar sections from current action hints.
        /// </summary>
        [Fact]
        public void DynamicModelView_ShortcutBarSection_RendersCurrentModelShortcuts()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new ShortcutDashboard();
                model.Items.Add(ShortcutItem.Create("enter", "Run selected action", keyColor: ConsoleColor.Cyan, descriptionColor: ConsoleColor.White));
                model.Items.Add(ShortcutItem.Create("f5", "Refresh preview", keyColor: ConsoleColor.Green, descriptionColor: ConsoleColor.DarkGray));

                try
                {
                    var view = Consoul.View("Release", model)
                        .ShortcutBarSection(
                            "Action keys",
                            dashboard => dashboard.Items,
                            new ShortcutBarOptions
                            {
                                Width = 42,
                                ItemSeparator = " | "
                            },
                            ConsoleColor.Cyan)
                        .Option("Add rollback", dashboard => dashboard.Items.Add(ShortcutItem.Create("r", "Rollback", keyColor: ConsoleColor.Yellow)));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(3, model.Items.Count);
                    VisualAssert.WroteText(scope.Driver, "Action keys");
                    VisualAssert.WroteText(scope.Driver, "[enter]");
                    VisualAssert.WroteText(scope.Driver, "Run selected action");
                    VisualAssert.WroteText(scope.Driver, "Rollback");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Action keys") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[enter]") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Run selected action") && write.Color == ConsoleColor.White);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[r]") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated shortcut bar sections can stay hidden until action hints are loaded.
        /// </summary>
        [Fact]
        public void DynamicModelView_ShortcutBarSectionWhen_RendersAfterShortcutsAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(42))
            {
                InitializeRoutine("1", "2");
                var model = new ShortcutDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ShortcutBarSectionWhenStyled(
                            "Loaded keys",
                            dashboard => dashboard.Actions,
                            dashboard => dashboard.Actions.Count > 0,
                            item => item.Key,
                            item => item.Label,
                            item => item.Disabled,
                            item => item.KeyStyle,
                            item => item.LabelStyle,
                            new ShortcutBarOptions
                            {
                                Width = 38,
                                ItemSeparator = " / "
                            },
                            "yellow")
                        .Option("Load shortcuts", dashboard =>
                        {
                            dashboard.Actions.Add(new ShortcutAction("d", "Deploy", false, "white on navy", "green"));
                            dashboard.Actions.Add(new ShortcutAction("esc", "Back", false, "grey", "grey"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Actions.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded keys");
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "Back");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded keys") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[d]") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive calendar sections from schedule state.
        /// </summary>
        [Fact]
        public void DynamicModelView_CalendarSection_RendersCurrentModelSchedule()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new ScheduleDashboard
                {
                    Month = new DateTime(2026, 6, 1)
                };
                model.Events.Add(new DashboardScheduleEvent("Release", new DateTime(2026, 6, 18), ConsoleColor.Green, "green"));
                model.Events.Add(new DashboardScheduleEvent("Freeze", new DateTime(2026, 6, 25), ConsoleColor.Yellow, "yellow"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .CalendarSection(
                            "Release calendar",
                            dashboard => dashboard.Month,
                            dashboard => dashboard.Events,
                            item => item.Date,
                            item => item.Label,
                            item => (ConsoleColor?)item.Color,
                            new CalendarOptions
                            {
                                Width = 32,
                                Culture = CultureInfo.InvariantCulture,
                                HighlightToday = false
                            },
                            ConsoleColor.Cyan)
                        .Option("Move release", dashboard => dashboard.Events[0].Date = new DateTime(2026, 6, 19));

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(new DateTime(2026, 6, 19), model.Events[0].Date);
                    VisualAssert.WroteText(scope.Driver, "Release calendar");
                    VisualAssert.WroteText(scope.Driver, "June 2026");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Release calendar") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "19" && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "25" && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated calendar sections can stay hidden until schedule events are available.
        /// </summary>
        [Fact]
        public void DynamicModelView_CalendarSectionWhen_RendersAfterScheduleIsLoaded()
        {
            using (var scope = new ConsoleDriverScope(42))
            {
                InitializeRoutine("1", "2");
                var model = new ScheduleDashboard
                {
                    Month = new DateTime(2026, 7, 1)
                };

                try
                {
                    var view = Consoul.View("Release", model)
                        .CalendarSectionWhenStyled(
                            "Loaded calendar",
                            dashboard => dashboard.Month,
                            dashboard => dashboard.Events.Count > 0,
                            dashboard => dashboard.Events,
                            item => item.Date,
                            item => item.Label,
                            item => item.StyleText,
                            new CalendarOptions
                            {
                                Width = 32,
                                Culture = CultureInfo.InvariantCulture,
                                HighlightToday = false
                            },
                            "yellow")
                        .Option("Load schedule", dashboard =>
                        {
                            dashboard.Events.Add(new DashboardScheduleEvent("Deploy", new DateTime(2026, 7, 8), ConsoleColor.White, "white on navy"));
                            dashboard.Events.Add(new DashboardScheduleEvent("Smoke", new DateTime(2026, 7, 9), ConsoleColor.Green, "green"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Events.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded calendar");
                    VisualAssert.WroteText(scope.Driver, "July 2026");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded calendar") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "8" && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "9" && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive nested bullet-list sections from notes.
        /// </summary>
        [Fact]
        public void DynamicModelView_BulletListSection_RendersCurrentModelNotes()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                var model = new NotesDashboard();
                var release = new DiagnosticNote("Prepare release notes", ConsoleColor.Cyan, ConsoleColor.Yellow, "aqua", "yellow");
                release.Children.Add(new DiagnosticNote("Capture migration caveats", ConsoleColor.Gray, ConsoleColor.Gray, "grey", "grey"));
                model.Notes.Add(release);
                model.Notes.Add(new DiagnosticNote("Notify support", ConsoleColor.Green, ConsoleColor.Green, "green", "green"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .BulletListSection(
                            "Release notes",
                            dashboard => dashboard.Notes,
                            note => note.Text,
                            note => note.Children,
                            note => (ConsoleColor?)note.TextColor,
                            note => (ConsoleColor?)note.MarkerColor,
                            new BulletListOptions
                            {
                                Width = 42
                            },
                            ConsoleColor.Cyan)
                        .Option("Add follow-up", dashboard =>
                        {
                            dashboard.Notes[0].Children.Add(new DiagnosticNote(
                                "Attach rollback runbook",
                                ConsoleColor.Yellow,
                                ConsoleColor.Yellow,
                                "yellow",
                                "yellow"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Notes[0].Children.Count);
                    VisualAssert.WroteText(scope.Driver, "Release notes");
                    VisualAssert.WroteText(scope.Driver, "Prepare release notes");
                    VisualAssert.WroteText(scope.Driver, "Capture migration caveats");
                    VisualAssert.WroteText(scope.Driver, "Attach rollback runbook");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Release notes") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Notify support") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated bullet-list sections can stay hidden until note state is available.
        /// </summary>
        [Fact]
        public void DynamicModelView_BulletListSectionWhen_RendersAfterNotesAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new NotesDashboard();

                try
                {
                    var view = Consoul.View("Diagnostics", model)
                        .BulletListSectionWhenStyled(
                            "Loaded notes",
                            dashboard => dashboard.Notes,
                            dashboard => dashboard.Notes.Count > 0,
                            note => note.Text,
                            note => note.Children,
                            note => note.TextStyle,
                            note => note.MarkerStyle,
                            new BulletListOptions
                            {
                                Width = 40,
                                Style = BulletListStyle.Numbered
                            },
                            "yellow")
                        .Option("Load notes", dashboard =>
                        {
                            var deploy = new DiagnosticNote("Deploy", ConsoleColor.White, ConsoleColor.Yellow, "white on navy", "yellow");
                            deploy.Children.Add(new DiagnosticNote("Run smoke tests", ConsoleColor.Cyan, ConsoleColor.Gray, "aqua", "grey"));
                            dashboard.Notes.Add(deploy);
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Single(model.Notes);
                    VisualAssert.WroteText(scope.Driver, "Loaded notes");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "Run smoke tests");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded notes") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can generate responsive checklist sections from readiness state.
        /// </summary>
        [Fact]
        public void DynamicModelView_ChecklistSection_RendersCurrentModelReadiness()
        {
            using (var scope = new ConsoleDriverScope(50))
            {
                InitializeRoutine("1", "2");
                var model = new ReadinessDashboard();
                model.Checks.Add(new ReadinessCheck(
                    "Build artifacts",
                    ChecklistItemState.Complete,
                    "Package is ready.",
                    "green",
                    "green",
                    "grey"));
                model.Checks.Add(new ReadinessCheck(
                    "Owner approval",
                    ChecklistItemState.Warning,
                    "Waiting for release owner.",
                    "yellow",
                    "yellow",
                    "grey"));

                try
                {
                    var view = Consoul.View("Release", model)
                        .ChecklistSection(
                            "Readiness",
                            dashboard => dashboard.Checks,
                            check => check.Label,
                            check => check.State,
                            check => check.Description,
                            new ChecklistOptions
                            {
                                Width = 44
                            },
                            ConsoleColor.Cyan)
                        .Option("Approve", dashboard =>
                        {
                            dashboard.Checks[1].State = ChecklistItemState.Complete;
                            dashboard.Checks[1].Description = "Owner approved.";
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(ChecklistItemState.Complete, model.Checks[1].State);
                    VisualAssert.WroteText(scope.Driver, "Readiness");
                    VisualAssert.WroteText(scope.Driver, "Build artifacts");
                    VisualAssert.WroteText(scope.Driver, "Owner approval");
                    VisualAssert.WroteText(scope.Driver, "Owner approved.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Readiness") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Generated checklist sections can stay hidden until readiness checks are available.
        /// </summary>
        [Fact]
        public void DynamicModelView_ChecklistSectionWhen_RendersAfterChecksAreLoaded()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                InitializeRoutine("1", "2");
                var model = new ReadinessDashboard();

                try
                {
                    var view = Consoul.View("Release", model)
                        .ChecklistSectionWhenStyled(
                            "Loaded readiness",
                            dashboard => dashboard.Checks,
                            dashboard => dashboard.Checks.Count > 0,
                            check => check.Label,
                            check => check.State,
                            check => check.Description,
                            check => check.LabelStyle,
                            check => check.MarkerStyle,
                            check => check.DescriptionStyle,
                            new ChecklistOptions
                            {
                                Width = 40
                            },
                            "yellow")
                        .Option("Load checks", dashboard =>
                        {
                            dashboard.Checks.Add(new ReadinessCheck(
                                "Deploy",
                                ChecklistItemState.Pending,
                                "Waiting for release window.",
                                "white on navy",
                                "yellow",
                                "grey"));
                            dashboard.Checks.Add(new ReadinessCheck(
                                "Smoke tests",
                                ChecklistItemState.Skipped,
                                "Run after deploy.",
                                "aqua",
                                "grey",
                                "darkgray"));
                        });

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(2, model.Checks.Count);
                    VisualAssert.WroteText(scope.Driver, "Loaded readiness");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "Smoke tests");
                    VisualAssert.WroteText(scope.Driver, "Waiting for release window.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loaded readiness") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic view sections can stay hidden until model state makes them relevant.
        /// </summary>
        [Fact]
        public void DynamicModelView_SectionWhen_RendersAfterPredicateBecomesTrue()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .SectionWhen(
                            "Changed",
                            counter => Consoul.TextBlock("Changed section count " + counter.Count, width: 44),
                            counter => counter.Count > 0,
                            ConsoleColor.Green)
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    Assert.DoesNotContain("Changed section count 0", scope.Driver.Text);
                    VisualAssert.WroteText(scope.Driver, "Changed");
                    VisualAssert.WroteText(scope.Driver, "Changed section count 1");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Conditional model sections can render reusable components from the current model.
        /// </summary>
        [Fact]
        public void DynamicModelView_SectionWhen_AcceptsRenderableFactory()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .SectionWhen(
                            counter => "Snapshot " + counter.Count,
                            (Func<CounterModel, IConsoulRenderable>)(counter => new PanelEntry(
                                "Visible count: " + counter.Count,
                                new PanelOptions { Title = "Conditional panel", Width = 42 })),
                            counter => counter.Count > 0,
                            counter => ConsoleColor.Cyan)
                        .Option("Increment", counter => counter.Count++);

                    view.Render();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    Assert.DoesNotContain("Visible count: 0", scope.Driver.Text);
                    VisualAssert.WroteText(scope.Driver, "Snapshot 1");
                    VisualAssert.WroteText(scope.Driver, "Conditional panel");
                    VisualAssert.WroteText(scope.Driver, "Visible count: 1");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render validation summaries and disable save actions until the model is valid.
        /// </summary>
        [Fact]
        public void DynamicModelView_ValidationSummarySection_AndOptionWhenValid_GuardSaveFlow()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2", "1", "3");
                var model = new ValidatedSettings();
                var options = new ValidationSummaryOptions
                {
                    Width = 56,
                    InvalidTitle = "Settings",
                    ValidTitle = "Settings",
                    ValidMessage = "Settings look good.",
                    ShowValidMessage = true
                };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .ValidationSummarySection(options)
                        .OptionWhenValid(
                            "Save",
                            settings => settings.Saved = true,
                            "fix validation issues first",
                            ConsoleColor.Green,
                            options)
                        .Option("Fix service", settings => settings.Service = "API", ConsoleColor.Cyan);

                    view.Render();

                    Assert.Equal("API", model.Service);
                    Assert.True(model.Saved);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Service is required.");
                    VisualAssert.WroteText(scope.Driver, "disabled: fix validation issues first");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: fix validation issues first");
                    VisualAssert.WroteText(scope.Driver, "Settings look good.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Validation summary sections can use the same titled and conditional section flow as other MVC-style dashboard blocks.
        /// </summary>
        [Fact]
        public void DynamicModelView_ValidationSummarySection_TitledAndConditionalSections_ReevaluateWithModel()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2", "3");
                var model = new ValidatedSettings();
                var validationOptions = new ValidationSummaryOptions
                {
                    Width = 56,
                    InvalidTitle = "Validation",
                    ValidTitle = "Validation",
                    ValidMessage = "All settings valid.",
                    ShowValidMessage = true
                };
                var savedOptions = new ValidationSummaryOptions
                {
                    Width = 56,
                    ValidTitle = "Saved",
                    ValidMessage = "Save checks passed.",
                    ShowValidMessage = true
                };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .ValidationSummarySectionStyled(
                            validationOptions,
                            title: "Validation state",
                            titleStyle: "yellow")
                        .ValidationSummarySectionWhen(
                            settings => settings.Saved,
                            savedOptions,
                            "Saved state",
                            new Style(ConsoleColor.Green))
                        .Option("Fix service", settings => settings.Service = "API", ConsoleColor.Cyan)
                        .OptionWhenValid(
                            "Save",
                            settings => settings.Saved = true,
                            "fix validation issues first",
                            ConsoleColor.Green,
                            validationOptions);

                    view.Render();

                    Assert.Equal("API", model.Service);
                    Assert.True(model.Saved);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Validation state");
                    VisualAssert.WroteText(scope.Driver, "Saved state");
                    VisualAssert.WroteText(scope.Driver, "Service is required.");
                    VisualAssert.WroteText(scope.Driver, "All settings valid.");
                    VisualAssert.WroteText(scope.Driver, "Save checks passed.");
                    AssertColoredWrite(scope.Driver, "Validation state", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Saved state", ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Valid-only dynamic options respect object-level model validation.
        /// </summary>
        [Fact]
        public void DynamicModelView_OptionWhenValid_UsesObjectLevelValidation()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2", "1", "3");
                var model = new CrossFieldValidatedSettings
                {
                    Target = "prod",
                    ConfirmTarget = "staging"
                };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .ValidationSummarySection()
                        .OptionWhenValid(
                            "Save",
                            settings => settings.Saved = true,
                            "targets must match",
                            ConsoleColor.Green)
                        .Option("Match target", settings => settings.ConfirmTarget = settings.Target, ConsoleColor.Cyan);

                    view.Render();

                    Assert.Equal("prod", model.ConfirmTarget);
                    Assert.True(model.Saved);
                    VisualAssert.WroteText(scope.Driver, "Targets must match.");
                    VisualAssert.WroteText(scope.Driver, "disabled: targets must match");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: targets must match");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Valid-only dynamic options can use model-aware labels and asynchronous actions.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_OptionWhenValid_SupportsAsyncDynamicActions()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2", "3");
                var model = new ValidatedSettings();

                try
                {
                    var view = Consoul.View("Settings", model)
                        .ValidationSummarySection()
                        .Option("Fix service", settings => settings.Service = "Worker", ConsoleColor.Cyan)
                        .OptionWhenValid(
                            settings => "Save " + settings.Service,
                            (settings, cancellationToken) =>
                            {
                                settings.Saved = !cancellationToken.IsCancellationRequested;
                                return Task.CompletedTask;
                            },
                            settings => "service is required",
                            settings => ConsoleColor.Green);

                    await view.RenderAsync();

                    Assert.Equal("Worker", model.Service);
                    Assert.True(model.Saved);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Service is required.");
                    VisualAssert.WroteText(scope.Driver, "Save Worker");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Valid-only dynamic options can run Task-returning actions without forcing POC code to accept a cancellation token.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_OptionWhenValid_RunsTaskOptionsWithoutTokenParameter()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("1", "2", "3", "4");
                var model = new ValidatedSettings();
                bool checkedHealth = false;

                try
                {
                    var view = Consoul.View("Settings", model)
                        .ValidationSummarySection()
                        .Option("Fix service", settings => settings.Service = "Worker", ConsoleColor.Cyan)
                        .OptionWhenValid(
                            "Check health",
                            async () =>
                            {
                                await Task.Yield();
                                checkedHealth = true;
                            },
                            "fix validation issues first",
                            ConsoleColor.Yellow)
                        .OptionWhenValid(
                            settings => "Save " + settings.Service,
                            (Func<ValidatedSettings, Task>)(async settings =>
                            {
                                await Task.Yield();
                                settings.Saved = true;
                            }),
                            settings => "service is required",
                            settings => ConsoleColor.Green);

                    await view.RenderAsync();

                    Assert.Equal("Worker", model.Service);
                    Assert.True(checkedHealth);
                    Assert.True(model.Saved);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Service is required.");
                    VisualAssert.WroteText(scope.Driver, "Check health");
                    VisualAssert.WroteText(scope.Driver, "Save Worker");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views pass cancellation tokens into asynchronous model actions.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_RenderAsync_SelectsAsynchronousModelOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .Option("Increment async", (counter, cancellationToken) =>
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                counter.Count++;
                            }

                            return Task.CompletedTask;
                        });

                    await view.RenderAsync();

                    Assert.Equal(1, model.Count);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Increment async");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views execute Task-returning model actions without forcing cancellation-token plumbing.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_RenderAsync_SelectsTaskModelOptionWithoutTokenParameter()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                var model = new CounterModel();

                try
                {
                    var view = Consoul.View("Counter", model)
                        .Option(
                            counter => "Increment async from " + counter.Count,
                            async counter =>
                            {
                                await Task.Yield();
                                counter.Count++;
                            },
                            counter => new Style(ConsoleColor.Cyan),
                            counter => "Current count is " + counter.Count);

                    await view.RenderAsync();

                    Assert.Equal(1, model.Count);
                    VisualAssert.WroteText(scope.Driver, "Current count is 0");
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views expose typed navigation through the existing navigation context.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_Navigate_RequestsTypedNavigation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1");

                try
                {
                    var context = new ViewNavigationContext();
                    var view = Consoul.View("Root", new CounterModel())
                        .Navigate<DestinationView>("Next", new Style(ConsoleColor.Cyan), replace: false);

                    ((INavigationAwareView)view).NavigationContext = context;

                    await view.RenderAsync();

                    NavigationCommand command = context.Consume();
                    Assert.True(command.HasValue);
                    Assert.Equal(NavigationCommandType.Push, command.CommandType);
                    Assert.Equal(typeof(DestinationView), command.TargetViewType);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can expose the MVC-style object editor as a first-class option.
        /// </summary>
        [Fact]
        public void DynamicModelView_EditModel_OpensExistingFormEditor()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "1", "3", "2");
                scope.Driver.EnqueueKey(new ConsoleKeyInfo((char)27, ConsoleKey.Escape, false, false, false));
                var model = new EditableSettings { Name = "Demo" };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .EditModel("Edit settings", new Style(ConsoleColor.Green));

                    view.Render();

                    Assert.Same(model, view.Model);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Edit settings");
                    VisualAssert.WroteText(scope.Driver, "Open document editor");
                    VisualAssert.WroteText(scope.Driver, "Press Esc to return to the object editor.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views can expose the same object editor through form-oriented naming.
        /// </summary>
        [Fact]
        public void DynamicModelView_EditForm_OpensExistingFormEditor()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "1", "3", "2");
                scope.Driver.EnqueueKey(new ConsoleKeyInfo((char)27, ConsoleKey.Escape, false, false, false));
                var model = new EditableSettings { Name = "Demo" };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .EditForm("Edit settings", new Style(ConsoleColor.Green));

                    view.Render();

                    Assert.Same(model, view.Model);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Edit settings");
                    VisualAssert.WroteText(scope.Driver, "Open document editor");
                    VisualAssert.WroteText(scope.Driver, "Press Esc to return to the object editor.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic views expose StaticView-style edit-form aliases over the same editor stack.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_AddEditFormOption_OpensExistingFormEditor()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "1", "Dynamic", "2", "2");
                var model = new EditableSettings { Name = "Demo" };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .AddEditFormOption(
                            "Edit settings",
                            "green",
                            enableDocumentEditor: false,
                            description: "Edits the active model.");

                    await view.RenderAsync();

                    Assert.Equal("Dynamic", model.Name);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Edit settings");
                    VisualAssert.WroteText(scope.Driver, "Edits the active model.");
                    VisualAssert.WroteText(scope.Driver, "Name");
                    AssertColoredWrite(scope.Driver, "Edit settings", ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model edit options pass the active cancellation token into the nested form editor.
        /// </summary>
        [Fact]
        public async Task DynamicModelView_EditModel_PassesCanceledTokenToNestedEditor()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("2");
                var model = new EditableSettings { Name = "Demo" };

                try
                {
                    var view = Consoul.View("Settings", model)
                        .EditModel("Edit settings", color: null, enableDocumentEditor: false);
                    DynamicOption<EditableSettings> option = Assert.Single(view.Options);

                    using (var cancellation = new CancellationTokenSource())
                    {
                        cancellation.Cancel();

                        await option.InvokeAsync(cancellation.Token);
                    }

                    Assert.Equal(0, scope.Driver.ClearCount);
                    Assert.DoesNotContain("Name", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The form document editor option exits before rendering when cancellation is already requested.
        /// </summary>
        [Fact]
        public async Task EditObjectView_DocumentEditorOption_RespectsCanceledToken()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(new ConsoleKeyInfo((char)27, ConsoleKey.Escape, false, false, false));
                var editor = new EditObjectView(new EditableSettings { Name = "Demo" });
                DynamicOption<object>? documentOption = null;

                foreach (DynamicOption<object> option in editor.Options)
                {
                    if (option.Entry.SetMessage() == editor.DocumentEditorOptionLabel)
                    {
                        documentOption = option;
                        break;
                    }
                }

                Assert.NotNull(documentOption);
                if (documentOption == null)
                {
                    throw new InvalidOperationException("The document editor option was not created.");
                }

                using (var cancellation = new CancellationTokenSource())
                {
                    cancellation.Cancel();

                    await documentOption.InvokeAsync(cancellation.Token);
                }

                Assert.Equal(0, scope.Driver.ClearCount);
                Assert.DoesNotContain("Press Esc to return to the object editor.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Legacy object-editor property options pass the active render token into custom property editors.
        /// </summary>
        [Fact]
        public async Task EditObjectView_LegacyPropertyOption_PassesRenderTokenToCustomEditor()
        {
            using (var scope = new ConsoleDriverScope(80))
            using (var cancellation = new CancellationTokenSource())
            {
                InitializeRoutine("1", "2");
                var model = new CancellationAwareEditableSettings { Name = "Existing" };
                ObservingCancellationPropertyEditor.ObservedToken = default;

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    await view.RenderAsync(cancellation.Token);

                    Assert.True(ObservingCancellationPropertyEditor.ObservedToken.CanBeCanceled);
                    Assert.Equal(cancellation.Token, ObservingCancellationPropertyEditor.ObservedToken);
                    Assert.Equal("Edited through custom editor", model.Name);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ObservingCancellationPropertyEditor.ObservedToken = default;
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Built-in object-editor prompts observe cancellation from the current render token.
        /// </summary>
        [Fact]
        public async Task EditObjectView_DefaultEditor_UsesRenderCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(80))
            using (var cancellation = new CancellationTokenSource())
            {
                InitializeRoutine("1", "Updated");
                var model = new CancelingDefaultEditableSettings { Name = "Existing" };
                CancelingDefaultPropertyEditor.Cancellation = cancellation;

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    await view.RenderAsync(cancellation.Token);

                    Assert.True(cancellation.IsCancellationRequested);
                    Assert.True(CancelingDefaultPropertyEditor.DelegatedToDefaultEditor);
                    Assert.Equal("Existing", model.Name);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    CancelingDefaultPropertyEditor.Cancellation = null;
                    CancelingDefaultPropertyEditor.DelegatedToDefaultEditor = false;
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model edit options reject boxed value types because edits would not flow back into the view model.
        /// </summary>
        [Fact]
        public void DynamicModelView_EditModel_RejectsValueTypeModels()
        {
            using (new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");

                try
                {
                    var previousMode = RenderOptions.ViewErrorMode;
                    RenderOptions.ViewErrorMode = RenderOptions.ViewErrorModes.Throw;

                    try
                    {
                        var view = Consoul.View("Value", 7)
                            .EditModel("Edit boxed value");

                        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => view.Render());
                        Assert.Contains("reference-type model", exception.Message);
                    }
                    finally
                    {
                        RenderOptions.ViewErrorMode = previousMode;
                    }
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor uses typed prompt validation metadata for simple properties.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_UsesTypedPromptValidationMetadata()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "ab", "deployment-service", "api", "2", "0", "5", "3");
                var model = new ValidatedEditableSettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal("api", model.ServiceKey);
                    Assert.Equal(5, model.RetryCount);
                    VisualAssert.WroteText(scope.Driver, "Service key must be 3 to 8 characters.");
                    VisualAssert.WroteText(scope.Driver, "Retry count must be between 1 and 5.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor applies generic DataAnnotations validation attributes during text edits.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_UsesGenericValidationAttributes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "api", "svc-api", "2");
                var model = new RegexEditableSettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal("svc-api", model.ServiceKey);
                    VisualAssert.WroteText(scope.Driver, "Service key must start with svc-.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor applies context-aware validation attributes during text edits.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_UsesContextAwareValidationAttributes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("2", "wrong", "secret", "3");
                var model = new PasswordEditableSettings { Password = "secret" };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal("secret", model.ConfirmPassword);
                    VisualAssert.WroteText(scope.Driver, "Passwords must match.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor keeps the current property value when the user presses Enter.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_UsesCurrentValueAsPromptDefault()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", string.Empty, "2");
                var model = new EditableSettings { Name = "Existing" };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal("Existing", model.Name);
                    VisualAssert.WroteText(scope.Driver, "default: Existing");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor treats prompt cancellation as a skipped edit.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_CanceledPromptDoesNotMutateProperty()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "back", "2");
                var model = new EditableSettings { Name = "Existing" };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal("Existing", model.Name);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Boolean properties use confirmation prompts with the current value as the default.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_BooleanPropertyUsesConfirmationPrompt()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", string.Empty, "2");
                var model = new BooleanEditableSettings { EnablePreviewMode = true };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.True(model.EnablePreviewMode);
                    VisualAssert.WroteText(scope.Driver, "Enter new Enable preview mode (Boolean) [Y/n]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum properties use metadata-backed selection prompts instead of raw text input.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_EnumPropertyUsesSelectionPrompt()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2", "2");
                var model = new EnumEditableSettings { Environment = DeploymentSlot.Development };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal(DeploymentSlot.Staging, model.Environment);
                    VisualAssert.WroteText(scope.Driver, "Development slot");
                    VisualAssert.WroteText(scope.Driver, "Staging slot");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Flags enum properties use a multi-selection prompt and combine selected flag values.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_FlagsEnumPropertyUsesMultiSelectionPrompt()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2,4", "2");
                var model = new FlagsEnumEditableSettings { Capabilities = DeploymentCapability.Deploy };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal(DeploymentCapability.Preview | DeploymentCapability.Rollback, model.Capabilities);
                    VisualAssert.WroteText(scope.Driver, "[x]");
                    VisualAssert.WroteText(scope.Driver, "Preview capability");
                    VisualAssert.WroteText(scope.Driver, "Rollback capability");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Nullable boolean properties use a clearable selection prompt instead of raw text input.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_NullableBooleanPropertyCanBeCleared()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "3", "2");
                var model = new NullableBooleanEditableSettings { EnablePreviewMode = true };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Null(model.EnablePreviewMode);
                    VisualAssert.WroteText(scope.Driver, "Yes");
                    VisualAssert.WroteText(scope.Driver, "No");
                    VisualAssert.WroteText(scope.Driver, "Clear value");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Nullable enum properties use a clearable metadata-backed selection prompt.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_NullableEnumPropertyCanBeCleared()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "4", "2");
                var model = new NullableEnumEditableSettings { Environment = DeploymentSlot.Staging };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Null(model.Environment);
                    VisualAssert.WroteText(scope.Driver, "Development slot");
                    VisualAssert.WroteText(scope.Driver, "Production slot");
                    VisualAssert.WroteText(scope.Driver, "Clear value");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Nullable flags enum properties expose an explicit clear option while still using the flags prompt.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_NullableFlagsEnumPropertyCanBeCleared()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "5", "2");
                var model = new NullableFlagsEnumEditableSettings { Capabilities = DeploymentCapability.Preview | DeploymentCapability.Deploy };

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Null(model.Capabilities);
                    VisualAssert.WroteText(scope.Driver, "Clear value");
                    VisualAssert.WroteText(scope.Driver, "Deploy capability");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed text input accepts enum names for quick POC entry points that do not use a selection prompt.
        /// </summary>
        [Fact]
        public void Consoul_Input_ParsesEnumNamesForPocScalarInputs()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("production");

                try
                {
                    var value = Consoul.Input("Environment", typeof(DeploymentSlot));

                    Assert.Equal(DeploymentSlot.Production, value);
                    VisualAssert.WroteText(scope.Driver, "Environment");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Required nullable choice properties do not expose a clear-value option.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_RequiredNullableChoicePropertiesCannotBeCleared()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "y", "2", "2", "3");
                var model = new RequiredNullableChoiceSettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.True(model.EnablePreviewMode);
                    Assert.Equal(DeploymentSlot.Staging, model.Environment);
                    Assert.DoesNotContain("Clear value", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor treats common collection interfaces as editable model collections.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_EditsInterfaceBackedSimpleCollections()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2", "beta", "4", "2", "2", "jules", "4", "3");
                var model = new InterfaceCollectionSettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal(new[] { "alpha", "beta" }, model.Tags);
                    Assert.Equal(new[] { "mira", "jules" }, model.Reviewers);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum collections can be edited by name through the default object editor's typed input path.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_EditsEnumCollectionsWithNames()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2", "Production", "4", "2");
                var model = new EnumCollectionSettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Equal(new[] { DeploymentSlot.Development, DeploymentSlot.Production }, model.Environments);
                    VisualAssert.WroteText(scope.Driver, "Production");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The default object editor can rebuild concrete set collections instead of falling through to nested object editing.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_EditsConcreteSetCollections()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2", "2", "4", "2");
                var model = new SetCollectionSettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.Contains(1, model.Scores);
                    Assert.Contains(2, model.Scores);
                    Assert.Equal(2, model.Scores.Count);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Interface-typed dictionaries get a concrete backing dictionary when a null POC setting is edited.
        /// </summary>
        [Fact]
        public void EditObjectView_DefaultEditor_CreatesInterfaceBackedDictionaries()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "1", "Owner", "1", "Platform", "3", "2");
                var model = new InterfaceDictionarySettings();

                try
                {
                    var view = new EditObjectView(model, enableJsonEditor: false);

                    view.Render();

                    Assert.NotNull(model.Metadata);
                    Assert.Equal("Platform", model.Metadata["Owner"]);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views expose typed navigation through the existing navigation context.
        /// </summary>
        [Fact]
        public async Task DynamicView_Navigate_RequestsTypedNavigation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1");

                try
                {
                    var context = new ViewNavigationContext();
                    var view = Consoul.View("Root")
                        .Navigate<DestinationView>("Next", replace: true);

                    ((INavigationAwareView)view).NavigationContext = context;

                    await view.RenderAsync();

                    NavigationCommand command = context.Consume();
                    Assert.True(command.HasValue);
                    Assert.Equal(NavigationCommandType.Replace, command.CommandType);
                    Assert.Equal(typeof(DestinationView), command.TargetViewType);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views can navigate to a factory-created destination so POC menus can pass constructor state.
        /// </summary>
        [Fact]
        public async Task DynamicView_NavigateFactory_RequestsFactoryNavigation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1");
                var model = new CounterModel
                {
                    Count = 7
                };

                try
                {
                    var context = new ViewNavigationContext();
                    var view = Consoul.View("Root", model)
                        .Navigate<DestinationView>(
                            "Open detail",
                            () => new DestinationView { Title = "Count " + model.Count },
                            "green",
                            replace: false);

                    ((INavigationAwareView)view).NavigationContext = context;

                    await view.RenderAsync();

                    NavigationCommand command = context.Consume();
                    Assert.True(command.HasValue);
                    Assert.Equal(NavigationCommandType.Push, command.CommandType);
                    Assert.True(command.HasFactory);
                    Assert.Null(command.TargetViewType);
                    DestinationView destination = Assert.IsType<DestinationView>(command.TargetFactory());
                    Assert.Equal("Count 7", destination.Title);
                    AssertColoredWrite(scope.Driver, "Open detail", ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views expose StaticView-style navigation aliases while still supporting factory-created destinations.
        /// </summary>
        [Fact]
        public async Task DynamicView_AddNavigationOptionFactory_RendersDescriptionAndRequestsFactoryNavigation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1");
                var model = new CounterModel
                {
                    Count = 3
                };

                try
                {
                    var context = new ViewNavigationContext();
                    var view = Consoul.View("Root", model)
                        .AddNavigationOption<DestinationView>(
                            "Injected detail",
                            () => new DestinationView { Title = "Injected " + model.Count },
                            new Style(ConsoleColor.Cyan),
                            replace: true,
                            description: "Opens a detail view with the active model.");

                    ((INavigationAwareView)view).NavigationContext = context;

                    await view.RenderAsync();

                    NavigationCommand command = context.Consume();
                    Assert.True(command.HasValue);
                    Assert.Equal(NavigationCommandType.Replace, command.CommandType);
                    Assert.True(command.HasFactory);
                    DestinationView destination = Assert.IsType<DestinationView>(command.TargetFactory());
                    Assert.Equal("Injected 3", destination.Title);
                    VisualAssert.WroteText(scope.Driver, "Opens a detail view with the active model.");
                    AssertColoredWrite(scope.Driver, "Injected detail", ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Canceled dynamic renders exit without invoking option actions.
        /// </summary>
        [Fact]
        public async Task DynamicView_RenderAsync_CanceledTokenExitsWithoutAction()
        {
            using (new ConsoleDriverScope(80))
            {
                bool called = false;
                var view = Consoul.View("Cancel")
                    .Option("Run", () => called = true);

                using (var cancellation = new CancellationTokenSource())
                {
                    cancellation.Cancel();

                    await view.RenderAsync(cancellation.Token);
                }

                Assert.False(called);
                Assert.True(view.GoBackRequested);
            }
        }

        /// <summary>
        /// A dynamic POC view can opt into throwing action errors without changing the global view error mode.
        /// </summary>
        [Fact]
        public async Task DynamicView_ThrowOnError_OverridesGlobalContinueMode()
        {
            RenderOptions.ViewErrorModes previousMode = RenderOptions.ViewErrorMode;

            try
            {
                RenderOptions.ViewErrorMode = RenderOptions.ViewErrorModes.RenderAndContinue;

                using (var scope = new ConsoleDriverScope(80))
                {
                    InitializeRoutine("1");

                    var view = Consoul.View("Error POC")
                        .Option("Fail", () => throw new InvalidOperationException("dynamic failure"))
                        .ThrowOnError();

                    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => view.RenderAsync());

                    Assert.Contains("dynamic failure", exception.Message);
                    VisualAssert.WroteText(scope.Driver, "Failed to render 'Error POC[0]' view");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
            finally
            {
                RenderOptions.ViewErrorMode = previousMode;
                ClearRoutine();
            }
        }

        /// <summary>
        /// A dynamic POC view can keep exploratory render-and-continue behavior even when the application default is throw.
        /// </summary>
        [Fact]
        public async Task DynamicView_RenderErrorsAndContinue_OverridesGlobalThrowMode()
        {
            RenderOptions.ViewErrorModes previousMode = RenderOptions.ViewErrorMode;

            try
            {
                RenderOptions.ViewErrorMode = RenderOptions.ViewErrorModes.Throw;

                using (var scope = new ConsoleDriverScope(80))
                {
                    InitializeRoutine("1", "2");

                    var view = Consoul.View("Error POC")
                        .Option("Fail", () => throw new InvalidOperationException("dynamic failure"))
                        .RenderErrorsAndContinue();

                    await view.RenderAsync();

                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Failed to render 'Error POC[0]' view");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
            finally
            {
                RenderOptions.ViewErrorMode = previousMode;
                ClearRoutine();
            }
        }

        /// <summary>
        /// Class-based static MVC views use the same view-local error policy as dynamic POC views.
        /// </summary>
        [Fact]
        public async Task StaticView_ThrowOnError_OverridesGlobalContinueMode()
        {
            RenderOptions.ViewErrorModes previousMode = RenderOptions.ViewErrorMode;

            try
            {
                RenderOptions.ViewErrorMode = RenderOptions.ViewErrorModes.RenderAndContinue;

                using (var scope = new ConsoleDriverScope(80))
                {
                    InitializeRoutine("1");

                    var view = new ErrorHandlingStaticView();
                    view.ThrowOnError();

                    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => view.RenderAsync());

                    Assert.Contains("static failure", exception.Message);
                    VisualAssert.WroteText(scope.Driver, "Failed to render 'Error static[0]' view");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
            finally
            {
                RenderOptions.ViewErrorMode = previousMode;
                ClearRoutine();
            }
        }

        /// <summary>
        /// Canceled renderer calls exit before creating or rendering the target view.
        /// </summary>
        [Fact]
        public async Task ViewRenderer_RenderAsync_CanceledTokenExitsCleanly()
        {
            DestinationView.RenderCount = 0;
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();

                await new ViewRenderer().RenderAsync<DestinationView>(cancellation.Token);
            }

            Assert.Equal(0, DestinationView.RenderCount);
        }

        /// <summary>
        /// Completed one-shot views exit the renderer instead of being rendered forever.
        /// </summary>
        [Fact]
        public async Task ViewRenderer_RenderAsync_CompletingViewExitsWithoutLoop()
        {
            CompletingView.RenderCount = 0;

            await new ViewRenderer().RenderAsync<CompletingView>();

            Assert.Equal(1, CompletingView.RenderCount);
        }

        /// <summary>
        /// Result-first selection returns the prompt result while legacy Prompt still returns the selected index.
        /// </summary>
        [Fact]
        public void Select_AndLegacyPrompt_PreserveExpectedResults()
        {
            using (new ConsoleDriverScope(80))
            {
                InitializeRoutine("2", "1");

                try
                {
                    PromptResult result = Consoul.Select("Pick", false, "Alpha", "Beta");
                    int legacyIndex = Consoul.Prompt("Pick again", false, "Alpha", "Beta");

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal(0, legacyIndex);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed selection returns both the selected index and object.
        /// </summary>
        [Fact]
        public void SelectTyped_ReturnsSelectedItem()
        {
            using (new ConsoleDriverScope(80))
            {
                InitializeRoutine("2");

                try
                {
                    var items = new[]
                    {
                        new SampleChoice("Alpha", 1),
                        new SampleChoice("Beta", 2)
                    };

                    SelectionResult<SampleChoice> result = Consoul.Select(
                        "Pick a choice",
                        choice => choice.Name,
                        false,
                        items);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal(2, result.SelectedItem.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Existing attributed static views still render and run actions.
        /// </summary>
        [Fact]
        public void StaticView_Render_PreservesAttributedOptionBehavior()
        {
            using (new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                CompatibilityStaticView.RunCount = 0;

                try
                {
                    var view = new CompatibilityStaticView();

                    view.Render();

                    Assert.Equal(1, CompatibilityStaticView.RunCount);
                    Assert.True(view.GoBackRequested);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views can use keyboard navigation and typed filtering for POC menu flow.
        /// </summary>
        [Fact]
        public void DynamicView_Render_UsesKeyboardNavigationAndFiltering()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                ClearRoutine();
                scope.Driver.EnqueueKey(Key(ConsoleKey.D, 'd'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Escape));
                bool previewCalled = false;
                bool deployCalled = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => previewCalled = true)
                        .Option("Deploy", () => deployCalled = true);
                    view.PromptKeyboardNavigationEnabled = true;
                    view.PromptFilterEnabled = true;

                    view.Render();

                    Assert.False(previewCalled);
                    Assert.True(deployCalled);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Filter: d");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Programmatic dynamic views can assign shortcuts to options without changing the numbered fallback flow.
        /// </summary>
        [Fact]
        public void DynamicView_ShortcutLastOption_SelectsOptionByShortcut()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("d", "3");
                bool previewCalled = false;
                bool deployCalled = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => previewCalled = true)
                        .ShortcutLastOption("p")
                        .Option("Deploy", () => deployCalled = true, ConsoleColor.Green)
                        .ShortcutLastOption("d");

                    view.Render();

                    Assert.False(previewCalled);
                    Assert.True(deployCalled);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic shortcuts can change with model state across render passes.
        /// </summary>
        [Fact]
        public void DynamicView_ShortcutLastOption_UsesCurrentModelState()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("a", "d", "3");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .Option("Approve", release => release.Approved = true, ConsoleColor.Yellow)
                        .ShortcutLastOption(release => release.Approved ? "r" : "a")
                        .Option("Deploy", release => release.Deployed = true, ConsoleColor.Green)
                        .ShortcutLastOption("d");

                    view.Render();

                    Assert.True(model.Approved);
                    Assert.True(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "[a]");
                    VisualAssert.WroteText(scope.Driver, "[r]");
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Programmatic dynamic views can search options by hidden aliases without changing the rendered MVC action labels.
        /// </summary>
        [Fact]
        public void DynamicView_SearchLastOption_FiltersByHiddenAlias()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("/ship", "2", "3");
                bool previewCalled = false;
                bool deployCalled = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => previewCalled = true, ConsoleColor.Cyan)
                        .Option("Deploy", () => deployCalled = true, ConsoleColor.Green)
                        .SearchLastOption("ship", "publish");
                    view.PromptFilterEnabled = true;

                    view.Render();

                    Assert.False(previewCalled);
                    Assert.True(deployCalled);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Filter: ship (1 matches)");
                    VisualAssert.WroteText(scope.Driver, "2) Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Model-backed dynamic search aliases can change with model state across render passes.
        /// </summary>
        [Fact]
        public void DynamicView_SearchLastOption_UsesCurrentModelState()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "/ship", "2", "3");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .Option("Approve", release => release.Approved = true, ConsoleColor.Yellow)
                        .SearchLastOption(release => release.Approved ? new[] { "ready" } : new[] { "approval" })
                        .Option("Deploy", release => release.Deployed = true, ConsoleColor.Green)
                        .SearchLastOption(release => release.Approved ? new[] { "ship" } : new[] { "blocked" });
                    view.PromptFilterEnabled = true;

                    view.Render();

                    Assert.True(model.Approved);
                    Assert.True(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Filter: ship (1 matches)");
                    VisualAssert.WroteText(scope.Driver, "2) Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic view separators group options without changing selectable option indexes.
        /// </summary>
        [Fact]
        public void DynamicView_Separator_GroupsOptionsAndPreservesSelectionMapping()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("2", "3");
                bool previewCalled = false;
                bool deployCalled = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Separator("Build")
                        .Option("Preview", () => previewCalled = true, ConsoleColor.Cyan)
                        .Separator("Release")
                        .Option("Deploy", () => deployCalled = true, ConsoleColor.Green);

                    view.Render();

                    Assert.False(previewCalled);
                    Assert.True(deployCalled);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "-- Build --");
                    VisualAssert.WroteText(scope.Driver, "-- Release --");
                    VisualAssert.WroteText(scope.Driver, "2) Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views can expose a reusable help surface built from option metadata.
        /// </summary>
        [Fact]
        public void DynamicView_CreateHelp_RendersCurrentOptionMetadata()
        {
            using (var scope = new ConsoleDriverScope(52))
            {
                var model = new ApprovalModel();

                var view = Consoul.View("Release", model)
                    .Separator("Build")
                    .Option("Preview", release => release.Approved = true, ConsoleColor.Cyan)
                    .DescribeLastOption("Runs a dry run before deployment.")
                    .ShortcutLastOption("p")
                    .Separator("Release")
                    .OptionWhen(
                        "Deploy",
                        release => release.Deployed = true,
                        release => release.Approved,
                        "approval required",
                        ConsoleColor.Green)
                    .ShortcutLastOption("d");

                view.CreateHelp().Render();

                VisualAssert.WroteText(scope.Driver, "Release Help");
                VisualAssert.WroteText(scope.Driver, "-- Build");
                VisualAssert.WroteText(scope.Driver, "Preview");
                VisualAssert.WroteText(scope.Driver, "[p]");
                VisualAssert.WroteText(scope.Driver, "Runs a dry run");
                VisualAssert.WroteText(scope.Driver, "-- Release");
                VisualAssert.WroteText(scope.Driver, "Deploy");
                VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                VisualAssert.WroteText(scope.Driver, "Go Back");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dynamic MVC-style views accept style strings for sections, separators, options, help, and command palette entries.
        /// </summary>
        [Fact]
        public void DynamicView_StyleStringOverloads_ColorizeMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .Separator("Build", "grey")
                        .Section("Summary", release => Consoul.Write("Approved: " + release.Approved, ConsoleColor.White), "aqua")
                        .Option("Preview", release => release.Approved = true, "aqua", "Runs a dry run before deployment.")
                        .OptionWhenValid("Save", release => release.Approved = true, "fix validation issues first", "#22c55e")
                        .ConfirmOption("Delete", "Delete release?", release => release.Deployed = true, "yellow")
                        .HelpOptionStyled(style: "grey")
                        .CommandPaletteOptionStyled(style: "magenta");

                    view.Render();
                    view.CreateHelp("Styled Release Help").Render();

                    Assert.True(view.GoBackRequested);
                    AssertColoredWrite(scope.Driver, "-- Build", ConsoleColor.Gray);
                    AssertColoredWrite(scope.Driver, "Summary", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Preview", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Save", Style.ParseForeground("#22c55e", RenderOptions.OptionColor));
                    AssertColoredWrite(scope.Driver, "Delete", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Help", ConsoleColor.Gray);
                    AssertColoredWrite(scope.Driver, "Command palette", ConsoleColor.Magenta);
                    VisualAssert.WroteText(scope.Driver, "Styled Release Help");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic MVC-style views preserve style decorations and RGB metadata when rendered on ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void DynamicView_StyleMetadataHelpers_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("back");

                try
                {
                    DynamicView<object> view = Consoul.View("Release")
                        .Separator("Build", "italic #778899")
                        .HelpOptionStyled(style: "bold #22c55e")
                        .CommandPaletteOptionStyled(style: "italic #abcdef");

                    view._options.Insert(
                        1,
                        new DynamicOption<object>(() => "Preview", () => { })
                            .UseStyle("bold #112233")
                            .Describe("Ship release"));

                    view.Render();
                    view.CreateHelp("True Color Dynamic Help").Render();

                    string output = writer.ToString();
                    Assert.Contains("\u001b[3;38;2;119;136;153m-- Build --", output);
                    Assert.Contains("\u001b[1;38;2;17;34;51m1) Preview", output);
                    Assert.Contains("\u001b[1;38;2;34;197;94m2) Help", output);
                    Assert.Contains("\u001b[3;38;2;171;205;239m3) Command palette", output);
                    Assert.Contains("True Color Dynamic Help", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic MVC-style view option overloads preserve true-color style metadata without manually constructing options.
        /// </summary>
        [Fact]
        public void DynamicView_PublicStyleOverloads_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("back");

                try
                {
                    var model = new StyleDrivenModel
                    {
                        Accent = RgbColor.FromHex("#445566")
                    };

                    DynamicView<StyleDrivenModel> view = Consoul.View("Release", model)
                        .Option("Preview", release => release.Approved = true, "bold #112233", "Ship release")
                        .Option(
                            release => "Dynamic style",
                            release => release.Deployed = true,
                            release => new Style(release.Accent, Decoration.Italic),
                            release => "From model state")
                        .ConfirmOption(
                            release => "Confirm",
                            release => "Confirm release?",
                            release => release.Deployed = true,
                            release => RgbColor.FromHex("#778899"))
                        .Navigate<DestinationView>("Details", "italic #abcdef")
                        .HelpOptionStyled(style: "bold #22c55e");

                    view.Render();
                    view.CreateHelp("True Color Public Overload Help").Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.False(model.Approved);
                    Assert.False(model.Deployed);
                    Assert.Contains("\u001b[1;38;2;17;34;51m1) Preview", output);
                    Assert.Contains("\u001b[3;38;2;68;85;102m2) Dynamic style", output);
                    Assert.Contains("\u001b[38;2;119;136;153m3) Confirm", output);
                    Assert.Contains("\u001b[3;38;2;171;205;239m4) Details", output);
                    Assert.Contains("\u001b[1;38;2;34;197;94m5) Help", output);
                    Assert.Contains("True Color Public Overload Help", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic MVC-style section titles preserve style decorations and RGB metadata on ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void DynamicView_SectionTitleStyles_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("back");

                try
                {
                    var model = new StyleDrivenModel
                    {
                        Accent = RgbColor.FromHex("#445566")
                    };

                    DynamicView<StyleDrivenModel> view = Consoul.View("Release", model)
                        .Section(
                            "Styled Summary",
                            release => Consoul.Write("Summary body", ConsoleColor.White),
                            "bold #112233")
                        .Section(
                            "Direct Color Summary",
                            release => Consoul.Write("Direct color body", ConsoleColor.White),
                            SpectreColor.FromHex("#223344"))
                        .Section(
                            "Direct Rgb Renderable",
                            new Text("Direct renderable body"),
                            RgbColor.FromHex("#334455"))
                        .Section(
                            "Direct Rgb Factory",
                            release => new Text("Direct factory body"),
                            RgbColor.FromHex("#556677"))
                        .Section(
                            release => "Model Style",
                            release => new Text("Renderable body"),
                            release => new Style(release.Accent, Decoration.Italic))
                        .Section(
                            release => "Rgb Title",
                            release => Consoul.Write("RGB body", ConsoleColor.White),
                            release => RgbColor.FromHex("#778899"))
                        .SectionWhen(
                            release => "Visible Rgb",
                            release => Consoul.Write("Visible body", ConsoleColor.White),
                            release => true,
                            release => RgbColor.FromHex("#abcdef"))
                        .SectionWhen(
                            "Direct Visible Color",
                            release => Consoul.Write("Direct visible body", ConsoleColor.White),
                            release => true,
                            SpectreColor.FromHex("#667788"))
                        .SectionWhen(
                            "Direct Visible Renderable",
                            new Text("Visible renderable body"),
                            release => true,
                            RgbColor.FromHex("#8899aa"))
                        .SectionWhen(
                            "Direct Visible Factory",
                            release => new Text("Visible factory body"),
                            release => true,
                            SpectreColor.FromHex("#99aabb"));

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Contains("\u001b[1;38;2;17;34;51mStyled Summary", output);
                    Assert.Contains("\u001b[38;2;34;51;68mDirect Color Summary", output);
                    Assert.Contains("\u001b[38;2;51;68;85mDirect Rgb Renderable", output);
                    Assert.Contains("\u001b[38;2;85;102;119mDirect Rgb Factory", output);
                    Assert.Contains("\u001b[3;38;2;68;85;102mModel Style", output);
                    Assert.Contains("\u001b[38;2;119;136;153mRgb Title", output);
                    Assert.Contains("\u001b[38;2;171;205;239mVisible Rgb", output);
                    Assert.Contains("\u001b[38;2;102;119;136mDirect Visible Color", output);
                    Assert.Contains("\u001b[38;2;136;153;170mDirect Visible Renderable", output);
                    Assert.Contains("\u001b[38;2;153;170;187mDirect Visible Factory", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Specialized MVC-style section helpers preserve true-color and decoration metadata for their rule titles.
        /// </summary>
        [Fact]
        public void DynamicView_SpecializedSectionTitleStyles_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("back");

                try
                {
                    var model = new SpecializedVisualModel();
                    model.Services.Add(new ServiceSummary("Api", "Ready", 3));
                    model.Metrics.Add(new MetricItem("Queued", 2, "jobs", "waiting", MetricKind.Info));

                    DynamicView<SpecializedVisualModel> view = Consoul.View("Dashboard", model)
                        .TableSectionStyled(
                            "Styled Table",
                            dashboard => dashboard.Services,
                            titleStyle: "bold #112233")
                        .MetricsSection(
                            "Style Metrics",
                            dashboard => dashboard.Metrics,
                            options: null,
                            titleStyle: new Style(RgbColor.FromHex("#445566"), Decoration.Italic))
                        .MetricsSectionWhenStyled(
                            "Visible Metrics",
                            dashboard => dashboard.Metrics,
                            dashboard => true,
                            titleStyle: "underline #778899")
                        .ValidationSummarySectionStyled(
                            title: "Styled Validation",
                            titleStyle: "bold #abcdef")
                        .ValidationSummarySectionWhen(
                            dashboard => true,
                            title: "Rgb Validation",
                            titleColor: RgbColor.FromHex("#fedcba"));

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Contains("\u001b[1;38;2;17;34;51mStyled Table", output);
                    Assert.Contains("\u001b[3;38;2;68;85;102mStyle Metrics", output);
                    Assert.Contains("\u001b[4;38;2;119;136;153mVisible Metrics", output);
                    Assert.Contains("\u001b[1;38;2;171;205;239mStyled Validation", output);
                    Assert.Contains("\u001b[38;2;254;220;186mRgb Validation", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// MVC-style composition section helpers preserve true-color and decoration metadata for their rule titles.
        /// </summary>
        [Fact]
        public void DynamicView_CompositionSectionTitleStyles_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer, width: 110))
            {
                InitializeRoutine("back");

                try
                {
                    var model = new SpecializedVisualModel
                    {
                        Summary = "Deployment summary is ready.",
                        Details = "Build is waiting for owner approval.",
                        StatusMessage = "All release checks are green.",
                        StatusKind = CalloutKind.Success
                    };
                    model.Rows.Add(new CompositionItem("Build", "Ready", ConsoleColor.Green, "green"));
                    model.Columns.Add(new CompositionItem("Owner", "Platform", ConsoleColor.Cyan, "aqua"));
                    model.Cells.Add(new CompositionItem("Service", "API", ConsoleColor.Yellow, "yellow"));

                    DynamicView<SpecializedVisualModel> view = Consoul.View("Composition", model)
                        .PropertyListSectionStyled(
                            "Property Summary",
                            dashboard => dashboard.Settings,
                            titleStyle: "bold #102030")
                        .TextBlockSectionStyled(
                            "Text Summary",
                            dashboard => dashboard.Summary,
                            titleStyle: "italic #203040")
                        .PanelSection(
                            "Panel Detail",
                            dashboard => dashboard.Details,
                            options: null,
                            titleStyle: new Style(RgbColor.FromHex("#304050"), Decoration.Underline))
                        .CalloutSectionStyled(
                            "Callout Status",
                            dashboard => dashboard.StatusMessage,
                            dashboard => dashboard.StatusKind,
                            titleStyle: "bold #405060")
                        .RowsSectionStyled(
                            "Rows Title",
                            dashboard => dashboard.Rows.Select(item => (object)(item.Label + ": " + item.Value)),
                            titleStyle: "italic #506070")
                        .ColumnsSection(
                            "Columns Title",
                            dashboard => dashboard.Columns.Select(item => (object)item.Value),
                            options: null,
                            titleStyle: new Style(RgbColor.FromHex("#607080"), Decoration.Bold))
                        .GridSectionStyled(
                            "Grid Title",
                            dashboard => dashboard.Cells.Select(item => (object)item.Value),
                            titleStyle: "underline #708090")
                        .LayoutSectionWhenStyled(
                            "Layout Title",
                            (dashboard, layout) => layout.Width(60).ColumnCount(1).AddPane("Pane content", "Pane"),
                            dashboard => true,
                            titleStyle: "bold #8090a0");

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Contains("\u001b[1;38;2;16;32;48mProperty Summary", output);
                    Assert.Contains("\u001b[3;38;2;32;48;64mText Summary", output);
                    Assert.Contains("\u001b[4;38;2;48;64;80mPanel Detail", output);
                    Assert.Contains("\u001b[1;38;2;64;80;96mCallout Status", output);
                    Assert.Contains("\u001b[3;38;2;80;96;112mRows Title", output);
                    Assert.Contains("\u001b[1;38;2;96;112;128mColumns Title", output);
                    Assert.Contains("\u001b[4;38;2;112;128;144mGrid Title", output);
                    Assert.Contains("\u001b[1;38;2;128;144;160mLayout Title", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// MVC-style chart, navigation, status, and timeline helpers preserve true-color title styles.
        /// </summary>
        [Fact]
        public void DynamicView_RemainingVisualSectionTitleStyles_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer, width: 132))
            {
                InitializeRoutine("back");

                try
                {
                    var model = new RemainingVisualSectionsModel
                    {
                        LastError = new InvalidOperationException("Deployment failed."),
                        ErrorContext = "Release gate diagnostics",
                        Month = new DateTime(2026, 7, 1),
                        DependencyRoot = new DependencyNode("Release", "Ready", ConsoleColor.Green, "green")
                    };
                    model.ChartItems.Add(new DashboardChartItem("Passing", 8, ConsoleColor.Green, "green"));
                    model.ChartItems.Add(new DashboardChartItem("Failing", 1, ConsoleColor.Red, "red"));
                    model.Breadcrumbs.Add(new BreadcrumbSegment("Home", ConsoleColor.Cyan, "aqua"));
                    model.Breadcrumbs.Add(new BreadcrumbSegment("Deploy", ConsoleColor.Yellow, "yellow"));
                    model.Shortcuts.Add(ShortcutItem.Create("enter", "Run", keyColor: ConsoleColor.Cyan, descriptionColor: ConsoleColor.White));
                    model.Actions.Add(new ShortcutAction("esc", "Cancel", false, "gray", "white"));
                    model.Events.Add(new DashboardScheduleEvent("Freeze", new DateTime(2026, 7, 10), ConsoleColor.Magenta, "magenta"));
                    var parentNote = new DiagnosticNote("Check API", ConsoleColor.White, ConsoleColor.Cyan, "white", "aqua");
                    parentNote.Children.Add(new DiagnosticNote("Confirm worker", ConsoleColor.Gray, ConsoleColor.Yellow, "gray", "yellow"));
                    model.Notes.Add(parentNote);
                    model.Checks.Add(new ReadinessCheck("Smoke tests", ChecklistItemState.Complete, "Passed", "green", "green", "gray"));
                    model.Samples.Add(new DashboardTrendSample("Queue", 1.5));
                    model.Samples.Add(new DashboardTrendSample("Build", 2.75));
                    model.DependencyRoot.Children.Add(new DependencyNode("API", "Healthy", ConsoleColor.Cyan, "aqua"));
                    model.Steps.Add(new DashboardTimelineStep(
                        "Package",
                        TimelineItemState.Complete,
                        "Package created",
                        new DateTimeOffset(2026, 7, 1, 8, 0, 0, TimeSpan.Zero),
                        "green",
                        "green",
                        "gray"));

                    DynamicView<RemainingVisualSectionsModel> view = Consoul.View("Visuals", model)
                        .BarChartSectionStyled(
                            "Bar Chart Title",
                            dashboard => dashboard.ChartItems,
                            item => item.Label,
                            item => item.Value,
                            item => item.StyleText,
                            titleStyle: "bold #010203")
                        .BreakdownChartSectionWhenStyled(
                            "Breakdown Title",
                            dashboard => dashboard.ChartItems,
                            dashboard => true,
                            item => item.Label,
                            item => item.Value,
                            item => item.StyleText,
                            titleStyle: "italic #040506")
                        .ExceptionSection(
                            "Exception Title",
                            dashboard => dashboard.LastError,
                            dashboard => dashboard.ErrorContext,
                            options: null,
                            titleStyle: new Style(RgbColor.FromHex("#070809"), Decoration.Underline))
                        .BreadcrumbSectionWhenStyled(
                            "Breadcrumb Title",
                            dashboard => dashboard.Breadcrumbs,
                            dashboard => true,
                            segment => segment.Label,
                            segment => segment.StyleText,
                            titleStyle: "bold #0a0b0c")
                        .ShortcutBarSectionWhenStyled(
                            "Shortcut Title",
                            dashboard => dashboard.Actions,
                            dashboard => true,
                            action => action.Key,
                            action => action.Label,
                            action => action.Disabled,
                            action => action.KeyStyle,
                            action => action.LabelStyle,
                            titleStyle: "italic #0d0e0f")
                        .ShortcutBarSection(
                            "Shortcut Items Title",
                            dashboard => dashboard.Shortcuts,
                            options: null,
                            titleStyle: new Style(RgbColor.FromHex("#101112"), Decoration.Bold))
                        .CalendarSectionWhenStyled(
                            "Calendar Title",
                            dashboard => dashboard.Month,
                            dashboard => true,
                            dashboard => dashboard.Events,
                            calendarEvent => calendarEvent.Date,
                            calendarEvent => calendarEvent.Label,
                            calendarEvent => calendarEvent.StyleText,
                            titleStyle: "underline #131415")
                        .BulletListSectionStyled(
                            "Bullet Title",
                            dashboard => dashboard.Notes,
                            note => note.Text,
                            note => note.Children,
                            note => note.TextStyle,
                            note => note.MarkerStyle,
                            titleStyle: "bold #161718")
                        .ChecklistSectionWhenStyled(
                            "Checklist Title",
                            dashboard => dashboard.Checks,
                            dashboard => true,
                            check => check.Label,
                            check => check.State,
                            check => check.Description,
                            check => check.LabelStyle,
                            check => check.MarkerStyle,
                            check => check.DescriptionStyle,
                            titleStyle: "italic #191a1b")
                        .SparklineSectionWhenStyled(
                            "Sparkline Title",
                            dashboard => dashboard.Samples,
                            dashboard => true,
                            sample => sample.Minutes,
                            titleStyle: "underline #1c1d1e")
                        .TreeSectionWhenStyled(
                            "Tree Title",
                            dashboard => dashboard.DependencyRoot,
                            dashboard => true,
                            dependency => dependency.Name + " " + dependency.State,
                            dependency => dependency.Children,
                            dependency => dependency.StyleText,
                            titleStyle: "bold #1f2021")
                        .TimelineSectionWhenStyled(
                            "Timeline Title",
                            dashboard => dashboard.Steps,
                            dashboard => true,
                            step => step.Title,
                            step => step.State,
                            step => step.Description,
                            step => step.Timestamp,
                            step => step.TitleStyle,
                            step => step.MarkerStyle,
                            step => step.DescriptionStyle,
                            titleStyle: "italic #222324");

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Contains("\u001b[1;38;2;1;2;3mBar Chart Title", output);
                    Assert.Contains("\u001b[3;38;2;4;5;6mBreakdown Title", output);
                    Assert.Contains("\u001b[4;38;2;7;8;9mException Title", output);
                    Assert.Contains("\u001b[1;38;2;10;11;12mBreadcrumb Title", output);
                    Assert.Contains("\u001b[3;38;2;13;14;15mShortcut Title", output);
                    Assert.Contains("\u001b[1;38;2;16;17;18mShortcut Items Title", output);
                    Assert.Contains("\u001b[4;38;2;19;20;21mCalendar Title", output);
                    Assert.Contains("\u001b[1;38;2;22;23;24mBullet Title", output);
                    Assert.Contains("\u001b[3;38;2;25;26;27mChecklist Title", output);
                    Assert.Contains("\u001b[4;38;2;28;29;30mSparkline Title", output);
                    Assert.Contains("\u001b[1;38;2;31;32;33mTree Title", output);
                    Assert.Contains("\u001b[3;38;2;34;35;36mTimeline Title", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic MVC-style views accept reusable style objects for sections, separators, options, help, and command palette entries.
        /// </summary>
        [Fact]
        public void DynamicView_StyleObjectOverloads_ColorizeMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .Separator("Build", new Style(ConsoleColor.DarkGray))
                        .Section("Summary", release => Consoul.Write("Approved: " + release.Approved, ConsoleColor.White), new Style(ConsoleColor.Cyan))
                        .Option("Preview", release => release.Approved = true, new Style(ConsoleColor.Green), "Runs a dry run before deployment.")
                        .Option(release => "Refresh " + release.Approved, release => release.Approved = !release.Approved, release => new Style(ConsoleColor.Blue), release => "Toggles approval.")
                        .OptionWhen(
                            "Deploy",
                            release => release.Deployed = true,
                            release => release.Approved,
                            "approval required",
                            new Style(ConsoleColor.Yellow))
                        .OptionWhenValid("Save", release => release.Approved = true, "fix validation issues first", new Style(ConsoleColor.Magenta))
                        .ConfirmOption("Delete", "Delete release?", release => release.Deployed = true, new Style(ConsoleColor.Red))
                        .HelpOption("Help", null, new Style(ConsoleColor.Gray))
                        .CommandPaletteOption("Command palette", "Command palette", new Style(ConsoleColor.Cyan));

                    view.Render();
                    view.CreateHelp("Styled Object Release Help").Render();

                    Assert.True(view.GoBackRequested);
                    AssertColoredWrite(scope.Driver, "-- Build", ConsoleColor.DarkGray);
                    AssertColoredWrite(scope.Driver, "Summary", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Preview", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Refresh False", ConsoleColor.Blue);
                    AssertColoredWrite(scope.Driver, "Save", ConsoleColor.Magenta);
                    AssertColoredWrite(scope.Driver, "Delete", ConsoleColor.Red);
                    AssertColoredWrite(scope.Driver, "Help", ConsoleColor.Gray);
                    AssertColoredWrite(scope.Driver, "Command palette", ConsoleColor.Cyan);
                    VisualAssert.WroteText(scope.Driver, "Styled Object Release Help");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic MVC-style views accept Spectre-style color selectors from model-backed view state.
        /// </summary>
        [Fact]
        public void DynamicView_ColorSelectorOverloads_ColorizeMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                var model = new ApprovalPaletteModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .Section(
                            release => "Color " + release.Name,
                            release => Consoul.Write("Approved: " + release.Approved, ConsoleColor.White),
                            release => release.SectionColor)
                        .Section(
                            release => "Rgb " + release.Name,
                            release => new Text("RGB section"),
                            release => release.SectionRgb)
                        .SectionWhen(
                            release => "Visible " + release.Name,
                            release => Consoul.Write("Visible section", ConsoleColor.White),
                            release => release.ShowSections,
                            release => release.SectionRgb)
                        .Option(
                            release => "Preview " + release.Name,
                            release => release.Approved = true,
                            release => release.OptionColor,
                            release => "Runs a color-coded preview.")
                        .Option(
                            release => "Refresh " + release.Name,
                            release => Task.CompletedTask,
                            release => release.OptionRgb,
                            release => "Refreshes from an RGB palette.")
                        .OptionWhen(
                            release => "Deploy " + release.Name,
                            release => release.Deployed = true,
                            release => release.Approved,
                            release => "approval required",
                            release => release.DisabledRgb)
                        .ConfirmOption(
                            release => "Delete " + release.Name,
                            release => "Delete release?",
                            release => release.Deployed = true,
                            release => release.ConfirmColor);

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    AssertColoredWrite(scope.Driver, "Color Production", ConsoleColor.Blue);
                    AssertColoredWrite(scope.Driver, "Rgb Production", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Visible Production", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Preview Production", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Refresh Production", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Deploy Production", ConsoleColor.DarkGray);
                    AssertColoredWrite(scope.Driver, "Delete Production", ConsoleColor.Red);
                    VisualAssert.WroteText(scope.Driver, "Runs a color-coded preview.");
                    VisualAssert.WroteText(scope.Driver, "Refreshes from an RGB palette.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic MVC-style views accept shared palette values for simple POC menu helpers.
        /// </summary>
        [Fact]
        public void DynamicView_SharedPaletteSimpleOverloads_ColorizeMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Palette release", model)
                        .Separator("Build", SpectreColor.Aqua)
                        .Option("Preview", release => release.Approved = true, RgbColor.Yellow, "Runs a dry run before deployment.")
                        .AddOption("Async check", async () => await Task.Yield(), SpectreColor.Green, "Runs async work.")
                        .AddOption("Token check", async (CancellationToken cancellationToken) => await Task.Yield(), RgbColor.Red, "Observes cancellation.")
                        .ConfirmOption("Confirm", "Run confirmation?", release => release.Deployed = true, SpectreColor.Magenta)
                        .AddEditFormOption("Edit settings", SpectreColor.Green, enableDocumentEditor: false, description: "Edits the current model.")
                        .Navigate<DestinationView>("Open detail", SpectreColor.Aqua)
                        .AddNavigationOption<DestinationView>("Injected detail", () => new DestinationView { Title = "Injected palette detail" }, RgbColor.Yellow, replace: true, description: "Opens a factory-created detail view.")
                        .HelpOption("Help", null, SpectreColor.Green)
                        .CommandPaletteOption("Command palette", "Command palette", RgbColor.Magenta);

                    view.Render();
                    view.CreateHelp("Palette Dynamic Help").Render();

                    Assert.True(view.GoBackRequested);
                    Assert.False(model.Approved);
                    Assert.False(model.Deployed);
                    AssertColoredWrite(scope.Driver, "-- Build", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Preview", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Async check", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Token check", ConsoleColor.Red);
                    AssertColoredWrite(scope.Driver, "Confirm", ConsoleColor.Magenta);
                    AssertColoredWrite(scope.Driver, "Edit settings", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Open detail", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Injected detail", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Help", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Command palette", ConsoleColor.Magenta);
                    VisualAssert.WroteText(scope.Driver, "Palette Dynamic Help");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic options accept reusable style objects when configured directly.
        /// </summary>
        [Fact]
        public void DynamicOption_UseStyle_AcceptsReusableStyleObjects()
        {
            var option = new DynamicOption<object>(() => "Run", () => { });

            option.UseStyle(new Style(ConsoleColor.Cyan));

            Assert.Equal(ConsoleColor.Cyan, option.Entry.SetColor());

            option.UseStyle(() => new Style(ConsoleColor.Yellow));

            Assert.Equal(ConsoleColor.Yellow, option.Entry.SetColor());
        }

        /// <summary>
        /// Dynamic options accept shared palette values when configured directly.
        /// </summary>
        [Fact]
        public void DynamicOption_UseColor_AcceptsSharedPaletteValues()
        {
            var option = new DynamicOption<object>(() => "Run", () => { });

            option.UseColor(SpectreColor.Aqua);

            Assert.Equal(ConsoleColor.Cyan, option.Entry.SetColor());

            option.UseColor(RgbColor.Yellow);

            Assert.Equal(ConsoleColor.Yellow, option.Entry.SetColor());
            Assert.Equal(ConsoleColor.Magenta, DynamicOption<object>.Separator("Release", SpectreColor.Magenta).Entry.SetColor());
            Assert.Equal(ConsoleColor.Red, DynamicOption<object>.Separator("Release", RgbColor.Red).Entry.SetColor());
        }

        /// <summary>
        /// Shared palette helpers are exposed as explicit public DynamicView and DynamicOption overloads.
        /// </summary>
        [Fact]
        public void DynamicView_SharedPaletteOverloads_AreExplicitPublicApis()
        {
            Type viewType = typeof(DynamicView<ApprovalModel>);
            Type optionType = typeof(DynamicOption<ApprovalModel>);

            AssertPublicMethod(optionType, "UseColor", typeof(SpectreColor));
            AssertPublicMethod(optionType, "UseColor", typeof(RgbColor));
            AssertPublicMethod(optionType, "Separator", typeof(string), typeof(SpectreColor));
            AssertPublicMethod(optionType, "Separator", typeof(string), typeof(RgbColor));
            AssertPublicMethod(viewType, "Option", typeof(string), typeof(Action<ApprovalModel>), typeof(SpectreColor), typeof(string));
            AssertPublicMethod(viewType, "AddOption", typeof(string), typeof(Func<ApprovalModel, CancellationToken, Task>), typeof(RgbColor), typeof(string));
            AssertPublicMethod(viewType, "ConfirmOption", typeof(string), typeof(string), typeof(Action<ApprovalModel>), typeof(SpectreColor), typeof(bool?));
            AssertPublicMethod(viewType, "AddConfirmOption", typeof(string), typeof(string), typeof(Func<ApprovalModel, Task>), typeof(RgbColor), typeof(bool?), typeof(string));
            AssertPublicMethod(viewType, "Navigate", typeof(string), typeof(SpectreColor), typeof(bool));
            AssertPublicMethod(viewType, "AddNavigationOption", typeof(string), typeof(RgbColor), typeof(bool), typeof(string));
            AssertPublicMethod(viewType, "AddEditFormOption", typeof(string), typeof(SpectreColor), typeof(System.Reflection.BindingFlags), typeof(bool), typeof(string));
            AssertPublicMethod(viewType, "HelpOption", typeof(string), typeof(string), typeof(SpectreColor), typeof(string), typeof(ViewHelpOptions), typeof(bool));
            AssertPublicMethod(viewType, "CommandPaletteOption", typeof(string), typeof(string), typeof(RgbColor), typeof(string), typeof(bool));
        }

        /// <summary>
        /// Dynamic help options render help and return to the normal view loop.
        /// </summary>
        [Fact]
        public void DynamicView_HelpOption_RendersHelpAndReturnsToView()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                InitializeRoutine("?", string.Empty, "3");
                bool called = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Run", () => called = true, ConsoleColor.Green)
                        .DescribeLastOption("Runs the release action.")
                        .ShortcutLastOption("r")
                        .HelpOption();

                    view.Render();

                    Assert.False(called);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Release Help");
                    VisualAssert.WroteText(scope.Driver, "Run");
                    VisualAssert.WroteText(scope.Driver, "[r]");
                    VisualAssert.WroteText(scope.Driver, "Help");
                    VisualAssert.WroteText(scope.Driver, "[?]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static views can expose attributed options through the same reusable help surface.
        /// </summary>
        [Fact]
        public void StaticView_CreateHelp_RendersAttributedOptionMetadata()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var view = new HelpStaticView();

                view.CreateHelp().Render();

                VisualAssert.WroteText(scope.Driver, "Static Help");
                VisualAssert.WroteText(scope.Driver, "Inspect");
                VisualAssert.WroteText(scope.Driver, "Shows the current model summary.");
                VisualAssert.WroteText(scope.Driver, "[i]");
                VisualAssert.WroteText(scope.Driver, "Go Back");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Static MVC-style views accept style strings for manually added options, separators, help, and command palette entries.
        /// </summary>
        [Fact]
        public void StaticView_StyleStringHelpers_ColorizeManualMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                StyledStaticView.PreviewCount = 0;

                try
                {
                    var view = new StyledStaticView();

                    view.Render();
                    view.CreateHelp("Styled Static Help").Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(0, StyledStaticView.PreviewCount);
                    AssertColoredWrite(scope.Driver, "-- Build", ConsoleColor.Gray);
                    AssertColoredWrite(scope.Driver, "Preview", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Help", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Command palette", ConsoleColor.Magenta);
                    VisualAssert.WroteText(scope.Driver, "Styled Static Help");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views preserve style decorations and RGB metadata when rendered on ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void StaticView_StyleMetadataHelpers_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("back");
                TrueColorStaticView.PreviewCount = 0;

                try
                {
                    var view = new TrueColorStaticView();

                    view.Render();
                    view.CreateHelp("True Color Static Help").Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Equal(0, TrueColorStaticView.PreviewCount);
                    Assert.Contains("\u001b[3;38;2;119;136;153m-- Build --", output);
                    Assert.Contains("\u001b[1;38;2;17;34;51m1) Preview", output);
                    Assert.Contains("\u001b[1;38;2;34;197;94m2) Help", output);
                    Assert.Contains("\u001b[3;38;2;171;205;239m3) Command palette", output);
                    Assert.Contains("True Color Static Help", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style helper overloads preserve RGB and decoration metadata on ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void StaticView_HelperOverloads_PreserveTrueColorMvcViewSurface()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("back");
                TrueColorHelperStaticView.PreviewCount = 0;

                try
                {
                    var view = new TrueColorHelperStaticView();

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Equal(0, TrueColorHelperStaticView.PreviewCount);
                    Assert.Contains("\u001b[38;2;18;52;86m1) RGB sync", output);
                    Assert.Contains("\u001b[1;38;2;34;197;94m2) Styled confirm", output);
                    Assert.Contains("\u001b[38;2;119;136;153m3) Edit rgb", output);
                    Assert.Contains("\u001b[38;2;171;205;239m4) Nav rgb", output);
                    Assert.Contains("\u001b[3;38;2;254;220;186m5) Factory styled", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views accept reusable style objects for manually added options, separators, help, and command palette entries.
        /// </summary>
        [Fact]
        public void StaticView_StyleObjectHelpers_ColorizeManualMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                StyleObjectStaticView.PreviewCount = 0;

                try
                {
                    var view = new StyleObjectStaticView();

                    view.Render();
                    view.CreateHelp("Styled Object Static Help").Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(0, StyleObjectStaticView.PreviewCount);
                    AssertColoredWrite(scope.Driver, "-- Build", ConsoleColor.DarkGray);
                    AssertColoredWrite(scope.Driver, "Preview", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Help", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Command palette", ConsoleColor.Magenta);
                    VisualAssert.WroteText(scope.Driver, "Styled Object Static Help");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views accept shared palette values for constructor-defined POC menu helpers.
        /// </summary>
        [Fact]
        public void StaticView_SharedPaletteHelpers_ColorizeManualMvcViewSurface()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("back");
                SharedPaletteStaticView.PreviewCount = 0;

                try
                {
                    var view = new SharedPaletteStaticView();

                    view.Render();
                    view.CreateHelp("Palette Static Help").Render();

                    Assert.True(view.GoBackRequested);
                    Assert.Equal(0, SharedPaletteStaticView.PreviewCount);
                    AssertColoredWrite(scope.Driver, "-- Build", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Preview", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Token check", ConsoleColor.Red);
                    AssertColoredWrite(scope.Driver, "Confirm", ConsoleColor.Magenta);
                    AssertColoredWrite(scope.Driver, "Edit settings", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Open detail", ConsoleColor.Cyan);
                    AssertColoredWrite(scope.Driver, "Injected detail", ConsoleColor.Yellow);
                    AssertColoredWrite(scope.Driver, "Help", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Command palette", ConsoleColor.Magenta);
                    VisualAssert.WroteText(scope.Driver, "Palette Static Help");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views can add synchronous and asynchronous options without constructing ViewOption directly.
        /// </summary>
        [Fact]
        public async Task StaticView_AddOptionHelpers_RunSyncAndAsyncOptions()
        {
            using (var scope = new ConsoleDriverScope(72))
            using (var cancellation = new CancellationTokenSource())
            {
                InitializeRoutine("p", "2", "3", "4");
                HelperStaticView.SyncCount = 0;
                HelperStaticView.AsyncCount = 0;
                HelperStaticView.TokenCount = 0;
                HelperStaticView.ObservedToken = default;

                try
                {
                    var view = new HelperStaticView();

                    await view.RenderAsync(cancellation.Token);

                    Assert.Equal(1, HelperStaticView.SyncCount);
                    Assert.Equal(1, HelperStaticView.AsyncCount);
                    Assert.Equal(1, HelperStaticView.TokenCount);
                    Assert.Equal(cancellation.Token, HelperStaticView.ObservedToken);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Preview");
                    VisualAssert.WroteText(scope.Driver, "Runs preview.");
                    VisualAssert.WroteText(scope.Driver, "Async check");
                    VisualAssert.WroteText(scope.Driver, "Token check");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views can guard constructor-defined options with confirmation prompts.
        /// </summary>
        [Fact]
        public async Task StaticView_AddConfirmOptionHelpers_RunConfirmedActionsAndSkipDeclinedActions()
        {
            using (var scope = new ConsoleDriverScope(72))
            using (var cancellation = new CancellationTokenSource())
            {
                InitializeRoutine("1", "no", "2", "yes", "3", "yes", "4");
                ConfirmHelperStaticView.DeclinedCount = 0;
                ConfirmHelperStaticView.AsyncCount = 0;
                ConfirmHelperStaticView.TokenCount = 0;
                ConfirmHelperStaticView.ObservedToken = default;

                try
                {
                    var view = new ConfirmHelperStaticView();

                    await view.RenderAsync(cancellation.Token);

                    Assert.Equal(0, ConfirmHelperStaticView.DeclinedCount);
                    Assert.Equal(1, ConfirmHelperStaticView.AsyncCount);
                    Assert.Equal(1, ConfirmHelperStaticView.TokenCount);
                    Assert.Equal(cancellation.Token, ConfirmHelperStaticView.ObservedToken);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Run declined? [y/N]");
                    VisualAssert.WroteText(scope.Driver, "Run async? [y/N]");
                    VisualAssert.WroteText(scope.Driver, "Run token? [y/N]");
                    VisualAssert.WroteText(scope.Driver, "Should not run.");
                    VisualAssert.WroteText(scope.Driver, "Observes confirmation token.");
                    AssertColoredWrite(scope.Driver, "Confirmed async", ConsoleColor.Green);
                    AssertColoredWrite(scope.Driver, "Confirmed token", ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views can open the shared form editor without creating a separate form-view abstraction.
        /// </summary>
        [Fact]
        public async Task StaticView_AddEditFormOption_OpensExistingFormEditor()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("e", "1", "Static", "2", "2");
                var model = new EditableSettings { Name = "Demo" };

                try
                {
                    var view = new FormHelperStaticView(model);

                    await view.RenderAsync();

                    Assert.Equal("Static", model.Name);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Edit settings");
                    VisualAssert.WroteText(scope.Driver, "Edits the settings object.");
                    VisualAssert.WroteText(scope.Driver, "Name");
                    AssertColoredWrite(scope.Driver, "Edit settings", ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views can add typed navigation options without exposing protected navigation helpers to callers.
        /// </summary>
        [Fact]
        public async Task StaticView_AddNavigationOption_RequestsTypedNavigation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1");

                try
                {
                    var context = new ViewNavigationContext();
                    var view = new NavigationHelperStaticView();
                    ((INavigationAwareView)view).NavigationContext = context;

                    await view.RenderAsync();

                    NavigationCommand command = context.Consume();
                    Assert.True(command.HasValue);
                    Assert.Equal(NavigationCommandType.Push, command.CommandType);
                    Assert.Equal(typeof(DestinationView), command.TargetViewType);
                    VisualAssert.WroteText(scope.Driver, "Open detail");
                    VisualAssert.WroteText(scope.Driver, "Opens the detail view.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static MVC-style views can add factory navigation options when destination views need constructor state.
        /// </summary>
        [Fact]
        public async Task StaticView_AddNavigationOptionFactory_RequestsFactoryNavigation()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1");

                try
                {
                    var context = new ViewNavigationContext();
                    var view = new FactoryNavigationHelperStaticView();
                    ((INavigationAwareView)view).NavigationContext = context;

                    await view.RenderAsync();

                    NavigationCommand command = context.Consume();
                    Assert.True(command.HasValue);
                    Assert.Equal(NavigationCommandType.Replace, command.CommandType);
                    Assert.True(command.HasFactory);
                    Assert.Null(command.TargetViewType);
                    DestinationView destination = Assert.IsType<DestinationView>(command.TargetFactory());
                    Assert.Equal("Injected static detail", destination.Title);
                    VisualAssert.WroteText(scope.Driver, "Injected detail");
                    VisualAssert.WroteText(scope.Driver, "Opens a factory-created detail view.");
                    AssertColoredWrite(scope.Driver, "Injected detail", ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views can render shortcut chrome from option shortcuts and generated navigation.
        /// </summary>
        [Fact]
        public void DynamicView_ShowShortcuts_RendersOptionAndNavigationHints()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                InitializeRoutine("3");

                try
                {
                    var view = Consoul.View("Release")
                        .ShowShortcuts()
                        .Option("Deploy", () => { }, ConsoleColor.Green)
                        .ShortcutLastOption("d")
                        .HelpOption();

                    view.Render();

                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "[?]");
                    VisualAssert.WroteText(scope.Driver, "Help");
                    VisualAssert.WroteText(scope.Driver, "[back]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic view shortcut chrome preserves option RGB and decoration metadata on ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void DynamicView_ShowShortcuts_PreservesOptionTrueColorInShortcutChrome()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("2");

                try
                {
                    DynamicView<object> view = Consoul.View("Release")
                        .ShowShortcuts()
                        .Option("Deploy", () => { }, "bold #112233")
                        .ShortcutLastOption("d");

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Contains("[d]", output);
                    Assert.Contains("\u001b[1;38;2;17;34;51mDeploy", output);
                    Assert.Contains("\u001b[1;38;2;17;34;51m1) Deploy", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static view shortcut chrome preserves option RGB and decoration metadata on ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void StaticView_ShowShortcuts_PreservesOptionTrueColorInShortcutChrome()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            using (UseTrueColorConsole(writer))
            {
                InitializeRoutine("2");

                try
                {
                    var view = new TrueColorShortcutStaticView();

                    view.Render();

                    string output = writer.ToString();
                    Assert.True(view.GoBackRequested);
                    Assert.Contains("[p]", output);
                    Assert.Contains("\u001b[3;38;2;68;85;102mPreview", output);
                    Assert.Contains("\u001b[3;38;2;68;85;102m1) Preview", output);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// View renderer navigation can render breadcrumb chrome for pushed views.
        /// </summary>
        [Fact]
        public void ViewRenderer_Render_RendersBreadcrumbChromeForNavigationStack()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "1", "2");

                try
                {
                    new ViewRenderer().Render<ChromeRootView>();

                    VisualAssert.WroteText(scope.Driver, "Chrome Root");
                    VisualAssert.WroteText(scope.Driver, " > ");
                    VisualAssert.WroteText(scope.Driver, "Chrome Detail");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic views can run a searchable command palette directly from current option metadata.
        /// </summary>
        [Fact]
        public void DynamicView_ShowCommandPalette_RunsSelectedAction()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("/deploy", "2");
                bool previewCalled = false;
                bool deployCalled = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => previewCalled = true, ConsoleColor.Cyan)
                        .DescribeLastOption("Runs validation only.")
                        .Option("Deploy", () => deployCalled = true, ConsoleColor.Green)
                        .DescribeLastOption("Publishes the release.")
                        .ShortcutLastOption("d");

                    PromptResult result = view.ShowCommandPalette(clear: false);

                    Assert.True(result.HasSelection);
                    Assert.False(previewCalled);
                    Assert.True(deployCalled);
                    VisualAssert.WroteText(scope.Driver, "Command palette");
                    VisualAssert.WroteText(scope.Driver, "Filter: deploy");
                    VisualAssert.WroteText(scope.Driver, "Publishes the release.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic command palettes reuse hidden search aliases from view options.
        /// </summary>
        [Fact]
        public void DynamicView_ShowCommandPalette_FiltersByHiddenAlias()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("/ship", "2");
                bool previewCalled = false;
                bool deployCalled = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => previewCalled = true, ConsoleColor.Cyan)
                        .Option("Deploy", () => deployCalled = true, ConsoleColor.Green)
                        .SearchLastOption("ship", "publish");

                    PromptResult result = view.ShowCommandPalette(clear: false);

                    Assert.True(result.HasSelection);
                    Assert.False(previewCalled);
                    Assert.True(deployCalled);
                    VisualAssert.WroteText(scope.Driver, "Command palette");
                    VisualAssert.WroteText(scope.Driver, "Filter: ship (1 matches)");
                    VisualAssert.WroteText(scope.Driver, "2) Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic command palette options behave like regular view options and return to the view loop.
        /// </summary>
        [Fact]
        public void DynamicView_CommandPaletteOption_RunsSelectedActionAndReturnsToView()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("cmd", "d", "4");
                bool deployed = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => { }, ConsoleColor.Cyan)
                        .Option("Deploy", () => deployed = true, ConsoleColor.Green)
                        .ShortcutLastOption("d")
                        .CommandPaletteOption();

                    view.Render();

                    Assert.True(deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Command palette");
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "[cmd]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static views can run attributed actions through the same command palette path.
        /// </summary>
        [Fact]
        public void StaticView_ShowCommandPalette_RunsAttributedAction()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("i");
                PaletteStaticView.InspectCount = 0;

                try
                {
                    var view = new PaletteStaticView();

                    PromptResult result = view.ShowCommandPalette(clear: false);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, PaletteStaticView.InspectCount);
                    VisualAssert.WroteText(scope.Driver, "Command palette");
                    VisualAssert.WroteText(scope.Driver, "Inspect");
                    VisualAssert.WroteText(scope.Driver, "Shows the current model summary.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static command palettes reuse hidden search aliases declared on MVC-style view attributes.
        /// </summary>
        [Fact]
        public void StaticView_ShowCommandPalette_FiltersByAttributedSearchTerms()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                InitializeRoutine("/summary", "1");
                PaletteStaticView.InspectCount = 0;

                try
                {
                    var view = new PaletteStaticView();

                    PromptResult result = view.ShowCommandPalette(clear: false);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, PaletteStaticView.InspectCount);
                    VisualAssert.WroteText(scope.Driver, "Command palette");
                    VisualAssert.WroteText(scope.Driver, "Filter: summary (1 matches)");
                    VisualAssert.WroteText(scope.Driver, "Inspect");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Programmatic dynamic views can render helper text under options without creating a custom prompt.
        /// </summary>
        [Fact]
        public void DynamicView_DescribeLastOption_RendersOptionDescription()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                InitializeRoutine("1", "2");
                bool called = false;

                try
                {
                    var view = Consoul.View("Release")
                        .Option("Preview", () => called = true, ConsoleColor.Green)
                        .DescribeLastOption("Runs a dry run before the deployment changes state.");

                    view.Render();

                    Assert.True(called);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Preview");
                    VisualAssert.WroteText(scope.Driver, "Runs a dry run");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Dynamic option descriptions can be evaluated from model state each render pass.
        /// </summary>
        [Fact]
        public void DynamicView_DescribeLastOption_UsesCurrentModelState()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("2", "1", "3");
                var model = new ApprovalModel();

                try
                {
                    var view = Consoul.View("Release", model)
                        .Option("Deploy", release => release.Deployed = true, ConsoleColor.Green)
                        .DescribeLastOption(release => release.Approved ? "Approval is ready." : "Approval is still pending.")
                        .Option("Approve", release => release.Approved = true, ConsoleColor.Yellow);

                    view.Render();

                    Assert.True(model.Approved);
                    Assert.True(model.Deployed);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Approval is still pending.");
                    VisualAssert.WroteText(scope.Driver, "Approval is ready.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Attributed static views can render option descriptions for class-based MVC menus.
        /// </summary>
        [Fact]
        public void StaticView_Render_RendersAttributedOptionDescription()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                DescribedStaticView.RunCount = 0;

                try
                {
                    var view = new DescribedStaticView();

                    view.Render();

                    Assert.Equal(1, DescribedStaticView.RunCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Runs the described static action.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Attributed static views can expose shortcut text through view prompts.
        /// </summary>
        [Fact]
        public void StaticView_Render_SelectsAttributedShortcut()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("r", "2");
                ShortcutStaticView.RunCount = 0;

                try
                {
                    var view = new ShortcutStaticView();

                    view.Render();

                    Assert.Equal(1, ShortcutStaticView.RunCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "[r]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Static view separators group options without changing selectable option indexes.
        /// </summary>
        [Fact]
        public void StaticView_AddSeparator_GroupsOptionsAndPreservesSelectionMapping()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("2", "3");
                GroupedStaticView.PreviewCount = 0;
                GroupedStaticView.DeployCount = 0;

                try
                {
                    var view = new GroupedStaticView();

                    view.Render();

                    Assert.Equal(0, GroupedStaticView.PreviewCount);
                    Assert.Equal(1, GroupedStaticView.DeployCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "-- Build --");
                    VisualAssert.WroteText(scope.Driver, "-- Release --");
                    VisualAssert.WroteText(scope.Driver, "2) Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Attributed dynamic views can render helper text from a companion method.
        /// </summary>
        [Fact]
        public void DynamicView_Render_RendersAttributedOptionDescription()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                DescribedDynamicView.RunCount = 0;

                try
                {
                    var view = new DescribedDynamicView();

                    view.Render();

                    Assert.Equal(1, DescribedDynamicView.RunCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Runs the described dynamic action.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Attributed dynamic views can expose hidden search aliases through companion methods.
        /// </summary>
        [Fact]
        public void DynamicView_Render_FiltersByAttributedSearchTerms()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("/ship", "1", "2");
                SearchableDynamicView.RunCount = 0;

                try
                {
                    var view = new SearchableDynamicView
                    {
                        PromptFilterEnabled = true
                    };

                    view.Render();

                    Assert.Equal(1, SearchableDynamicView.RunCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Filter: ship (1 matches)");
                    VisualAssert.WroteText(scope.Driver, "1) Release");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// View prompt render options flow into newly constructed static views.
        /// </summary>
        [Fact]
        public void StaticView_Render_UsesGlobalPromptDefaults()
        {
            bool previousKeyboard = RenderOptions.ViewPromptKeyboardNavigationEnabled;
            bool previousFilter = RenderOptions.ViewPromptFilterEnabled;
            int? previousPageSize = RenderOptions.ViewPromptPageSize;

            try
            {
                RenderOptions.ViewPromptKeyboardNavigationEnabled = true;
                RenderOptions.ViewPromptFilterEnabled = true;
                RenderOptions.ViewPromptPageSize = 1;

                using (var scope = new ConsoleDriverScope(80))
                {
                    ClearRoutine();
                    scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));
                    scope.Driver.EnqueueKey(Key(ConsoleKey.Escape));
                    CompatibilityStaticView.RunCount = 0;

                    var view = new CompatibilityStaticView();

                    Assert.True(view.PromptKeyboardNavigationEnabled);
                    Assert.True(view.PromptFilterEnabled);
                    Assert.Equal(1, view.PromptPageSize);

                    view.Render();

                    Assert.Equal(1, CompatibilityStaticView.RunCount);
                    Assert.True(view.GoBackRequested);
                    VisualAssert.WroteText(scope.Driver, "Use Up/Down to move");
                    VisualAssert.WroteText(scope.Driver, "Showing options 1-1 of 2");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
            finally
            {
                RenderOptions.ViewPromptKeyboardNavigationEnabled = previousKeyboard;
                RenderOptions.ViewPromptFilterEnabled = previousFilter;
                RenderOptions.ViewPromptPageSize = previousPageSize;
                ClearRoutine();
            }
        }

        /// <summary>
        /// Existing attributed dynamic views still render and run actions.
        /// </summary>
        [Fact]
        public void DynamicView_Render_PreservesAttributedOptionBehavior()
        {
            using (new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "2");
                CompatibilityDynamicView.RunCount = 0;

                try
                {
                    var view = new CompatibilityDynamicView();

                    view.Render();

                    Assert.Equal(1, CompatibilityDynamicView.RunCount);
                    Assert.True(view.GoBackRequested);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The object form document editor uses the fake console driver for key input, cursor visibility, clears, and writes.
        /// </summary>
        [Fact]
        public void EditObjectView_DocumentEditor_UsesConsoleDriver()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("1", "3");
                scope.Driver.EnqueueKey(new ConsoleKeyInfo((char)27, ConsoleKey.Escape, false, false, false));

                try
                {
                    var view = new EditObjectView(new EditableSettings { Name = "Demo" });

                    view.Render();

                    Assert.True(scope.Driver.CursorVisible);
                    VisualAssert.WroteText(scope.Driver, "Press Esc to return to the object editor.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The object editor renders property metadata descriptions in its generated menu.
        /// </summary>
        [Fact]
        public void EditObjectView_Render_RendersPropertyDescriptionsInMenu()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InitializeRoutine("2");

                try
                {
                    var view = new EditObjectView(new DescribedEditableSettings(), enableJsonEditor: false);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Used to group preview deployments.");
                    VisualAssert.WroteText(scope.Driver, "Type /text to filter options, or /clear to reset.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The object editor helper creates the existing object editor implementation.
        /// </summary>
        [Fact]
        public void ConsoulEditObject_CreatesObjectEditorView()
        {
            var model = new EditableSettings { Name = "Demo" };

            EditObjectView editor = Consoul.EditObject(model);

            Assert.Same(model, editor.Model);
            Assert.Equal("Open document editor", editor.DocumentEditorOptionLabel);
            Assert.True(editor.IsDocumentEditorEnabled);
        }

        /// <summary>
        /// The form helper uses the same object editor implementation rather than introducing a parallel editor stack.
        /// </summary>
        [Fact]
        public void ConsoulForm_CreatesExistingObjectEditorView()
        {
            var model = new EditableSettings { Name = "Demo" };

            EditObjectView editor = Consoul.Form(model, enableDocumentEditor: false);

            Assert.IsType<EditObjectView>(editor);
            Assert.Same(model, editor.Model);
            Assert.Equal("Open document editor", editor.DocumentEditorOptionLabel);
            Assert.False(editor.IsDocumentEditorEnabled);
        }

        /// <summary>
        /// The older JSON serializer name remains a compatibility alias over the form serializer.
        /// </summary>
        [Fact]
        public void EditObjectView_UsesSameSerializerAsCompatibilityJsonAlias()
        {
            var previousSerializer = EditObjectView.FormValueSerializer;

            try
            {
                Func<object, Type, string> jsonAliasSerializer = (value, type) => "\"json-alias\"";
                EditObjectView.JsonValueSerializer = jsonAliasSerializer;

                Assert.Same(jsonAliasSerializer, EditObjectView.FormValueSerializer);
                Assert.Same(EditObjectView.FormValueSerializer, EditObjectView.JsonValueSerializer);

                Func<object, Type, string> formSerializer = (value, type) => "\"form\"";
                EditObjectView.FormValueSerializer = formSerializer;

                Assert.Same(formSerializer, EditObjectView.JsonValueSerializer);
            }
            finally
            {
                EditObjectView.FormValueSerializer = previousSerializer;
            }
        }

        /// <summary>
        /// The form document editor passes declared property types, including null values, to the configured serializer hook.
        /// </summary>
        [Fact]
        public void EditObjectView_DocumentEditor_UsesDeclaredPropertyTypeForFormSerialization()
        {
            var previousSerializer = EditObjectView.FormValueSerializer;
            var calls = new List<string>();

            try
            {
                EditObjectView.FormValueSerializer = (value, type) =>
                {
                    calls.Add((value == null ? "<null>" : value.ToString()) + "|" + type.FullName);
                    return "\"" + type.Name + "\"";
                };

                using (var scope = new ConsoleDriverScope(80))
                {
                    InitializeRoutine("1", "5");
                    scope.Driver.EnqueueKey(new ConsoleKeyInfo((char)27, ConsoleKey.Escape, false, false, false));

                    try
                    {
                        var view = new EditObjectView(new SerializerAwareSettings());

                        view.Render();

                        Assert.Contains(calls, call => call == "<null>|" + typeof(string).FullName);
                        Assert.Contains(calls, call => call == "17|" + typeof(object).FullName);
                        Assert.Contains(calls, call => call.EndsWith("|" + typeof(ISerializerShape).FullName, StringComparison.Ordinal));
                        VisualAssert.WroteText(scope.Driver, "String");
                        VisualAssert.WroteText(scope.Driver, "Object");
                        VisualAssert.WroteText(scope.Driver, "ISerializerShape");
                        VisualAssert.NoOverflow(scope.Driver);
                    }
                    finally
                    {
                        ClearRoutine();
                    }
                }
            }
            finally
            {
                EditObjectView.FormValueSerializer = previousSerializer;
            }
        }

        private static void InitializeRoutine(params string[] inputs)
        {
            Routines.InitializeRoutine(new TestRoutine(inputs));
        }

        private static void ClearRoutine()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
        }

        private static IDisposable UseTrueColorConsole(StringWriter writer, int width = 90, int height = 48)
        {
            var output = new AnsiConsoleOutput(writer, width: width, height: height, isTerminal: true);
            var settings = new AnsiConsoleSettings
            {
                Out = output,
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor,
                Interactive = InteractionSupport.Yes
            };

            return Consoul.UseConsoleDriver(new TextWriterConsoleDriver(output, settings));
        }

        private static void AssertColoredWrite(FakeConsoleDriver driver, string expectedText, ConsoleColor expectedColor)
        {
            Assert.Contains(
                driver.Writes,
                write => write.Text.Contains(expectedText) && write.Color == expectedColor);
        }

        private static void AssertPublicMethod(Type type, string methodName, params Type[] parameterTypes)
        {
            Assert.Contains(
                type.GetMethods(),
                method => method.Name == methodName &&
                    method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameterTypes));
        }

        private static ConsoleKeyInfo Key(ConsoleKey key, char keyChar = '\0')
        {
            return new ConsoleKeyInfo(keyChar, key, false, false, false);
        }

        /// <summary>
        /// Destination view used to verify navigation and cancellation.
        /// </summary>
        public sealed class DestinationView : IView
        {
            /// <summary>
            /// Gets the number of times this view was rendered.
            /// </summary>
            public static int RenderCount { get; set; }

            /// <summary>
            /// Gets or sets the title of the view.
            /// </summary>
            public string Title { get; set; } = string.Empty;

            /// <summary>
            /// Gets a value indicating whether a request to navigate back has been made.
            /// </summary>
            public bool GoBackRequested { get; private set; }

            /// <summary>
            /// Signals a request to navigate back from the current view.
            /// </summary>
            public void GoBack()
            {
                GoBackRequested = true;
            }

            /// <summary>
            /// Renders the view synchronously.
            /// </summary>
            public void Render()
            {
                RenderAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Renders the view asynchronously.
            /// </summary>
            /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
            /// <returns>A task representing the asynchronous render operation.</returns>
            public Task RenderAsync(CancellationToken cancellationToken = default)
            {
                RenderCount++;
                GoBack();
                return Task.CompletedTask;
            }
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(IEnumerable<string> inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }

        private sealed class CompletingView : IView
        {
            public static int RenderCount { get; set; }

            public string Title { get; set; } = "Complete";

            public bool GoBackRequested { get; private set; }

            public void GoBack()
            {
                GoBackRequested = true;
            }

            public void Render()
            {
                RenderAsync().GetAwaiter().GetResult();
            }

            public Task RenderAsync(CancellationToken cancellationToken = default)
            {
                RenderCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class SampleChoice
        {
            public SampleChoice(string name, int value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }

            public int Value { get; }
        }

        private sealed class ViewActionDescriptor
        {
            public ViewActionDescriptor(string label, int amount, ConsoleColor color, string description, string shortcut)
            {
                Label = label;
                Amount = amount;
                Color = color;
                Description = description;
                Shortcut = shortcut;
            }

            public string Label { get; }

            public int Amount { get; }

            public ConsoleColor Color { get; }

            public string Description { get; }

            public string Shortcut { get; }
        }

        private sealed class CounterModel
        {
            public int Count { get; set; }
        }

        private sealed class SummarySettings
        {
            [Display(Order = 0)]
            public string Service { get; set; } = string.Empty;

            [Display(Order = 1)]
            public string Environment { get; set; } = string.Empty;

            [Display(Order = 2)]
            public int Port { get; set; }
        }

        private sealed class ServicesDashboard
        {
            public List<ServiceSummary> Services { get; } = new List<ServiceSummary>();
        }

        private sealed class ServiceSummary
        {
            public ServiceSummary(string name, string state, int replicas)
            {
                Name = name;
                State = state;
                Replicas = replicas;
            }

            [Display(Order = 0)]
            public string Name { get; set; }

            [Display(Order = 1)]
            public string State { get; set; }

            [Display(Order = 2)]
            public int Replicas { get; set; }
        }

        private sealed class MetricsDashboard
        {
            public List<DashboardMetric> Metrics { get; } = new List<DashboardMetric>();
        }

        private sealed class DashboardMetric
        {
            public DashboardMetric(string name, int value, string unit, string note, MetricKind kind)
            {
                Name = name;
                Value = value;
                Unit = unit;
                Note = note;
                Kind = kind;
            }

            public string Name { get; }

            public int Value { get; set; }

            public string Unit { get; }

            public string Note { get; set; }

            public MetricKind Kind { get; set; }
        }

        private sealed class ChartDashboard
        {
            public List<DashboardChartItem> Items { get; } = new List<DashboardChartItem>();
        }

        private sealed class DashboardChartItem
        {
            public DashboardChartItem(string label, double value, ConsoleColor color, string styleText)
            {
                Label = label;
                Value = value;
                Color = color;
                StyleText = styleText;
            }

            public string Label { get; }

            public double Value { get; set; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }
        }

        private sealed class TimelineDashboard
        {
            public List<DashboardTimelineStep> Steps { get; } = new List<DashboardTimelineStep>();
        }

        private sealed class DashboardTimelineStep
        {
            public DashboardTimelineStep(
                string title,
                TimelineItemState state,
                string description,
                DateTimeOffset timestamp,
                string titleStyle,
                string markerStyle,
                string descriptionStyle)
            {
                Title = title;
                State = state;
                Description = description;
                Timestamp = timestamp;
                TitleStyle = titleStyle;
                MarkerStyle = markerStyle;
                DescriptionStyle = descriptionStyle;
            }

            public string Title { get; }

            public TimelineItemState State { get; }

            public string Description { get; }

            public DateTimeOffset Timestamp { get; }

            public string TitleStyle { get; }

            public string MarkerStyle { get; }

            public string DescriptionStyle { get; }
        }

        private sealed class CompositionDashboard
        {
            public List<CompositionItem> Rows { get; } = new List<CompositionItem>();

            public List<CompositionItem> Columns { get; } = new List<CompositionItem>();

            public List<CompositionItem> Cells { get; } = new List<CompositionItem>();

            public List<CompositionItem> Panes { get; } = new List<CompositionItem>();
        }

        private sealed class CompositionItem
        {
            public CompositionItem(string label, string value, ConsoleColor color, string styleText)
            {
                Label = label;
                Value = value;
                Color = color;
                StyleText = styleText;
            }

            public string Label { get; }

            public string Value { get; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }
        }

        private sealed class TrendDashboard
        {
            public List<double> LatencySamples { get; } = new List<double>();

            public List<DashboardTrendSample> Samples { get; } = new List<DashboardTrendSample>();
        }

        private sealed class DashboardTrendSample
        {
            public DashboardTrendSample(string stage, double minutes)
            {
                Stage = stage;
                Minutes = minutes;
            }

            public string Stage { get; }

            public double Minutes { get; }
        }

        private sealed class DependencyDashboard
        {
            public DependencyNode Root { get; set; } = null!;
        }

        private sealed class DependencyNode
        {
            public DependencyNode(string name, string state, ConsoleColor color, string styleText)
            {
                Name = name;
                State = state;
                Color = color;
                StyleText = styleText;
            }

            public string Name { get; }

            public string State { get; set; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }

            public List<DependencyNode> Children { get; } = new List<DependencyNode>();
        }

        private sealed class TextBlockDashboard
        {
            public bool SummaryLoaded { get; set; }

            public string Summary { get; set; } = string.Empty;
        }

        private sealed class PanelDashboard
        {
            public bool DetailsLoaded { get; set; }

            public string Details { get; set; } = string.Empty;
        }

        private sealed class CalloutDashboard
        {
            public bool StatusLoaded { get; set; }

            public string Message { get; set; } = string.Empty;

            public CalloutKind Kind { get; set; } = CalloutKind.Info;
        }

        private sealed class ExceptionDashboard
        {
            public bool ShowDiagnostics { get; set; }

            public Exception LastError { get; set; } = null!;

            public string Context { get; set; } = string.Empty;
        }

        private sealed class BreadcrumbDashboard
        {
            public List<BreadcrumbSegment> Trail { get; } = new List<BreadcrumbSegment>();
        }

        private sealed class BreadcrumbSegment
        {
            public BreadcrumbSegment(string label, ConsoleColor color, string styleText)
            {
                Label = label;
                Color = color;
                StyleText = styleText;
            }

            public string Label { get; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }
        }

        private sealed class ShortcutDashboard
        {
            public List<ShortcutItem> Items { get; } = new List<ShortcutItem>();

            public List<ShortcutAction> Actions { get; } = new List<ShortcutAction>();
        }

        private sealed class ShortcutAction
        {
            public ShortcutAction(string key, string label, bool disabled, string keyStyle, string labelStyle)
            {
                Key = key;
                Label = label;
                Disabled = disabled;
                KeyStyle = keyStyle;
                LabelStyle = labelStyle;
            }

            public string Key { get; }

            public string Label { get; }

            public bool Disabled { get; }

            public string KeyStyle { get; }

            public string LabelStyle { get; }
        }

        private sealed class ScheduleDashboard
        {
            public DateTime Month { get; set; }

            public List<DashboardScheduleEvent> Events { get; } = new List<DashboardScheduleEvent>();
        }

        private sealed class DashboardScheduleEvent
        {
            public DashboardScheduleEvent(string label, DateTime date, ConsoleColor color, string styleText)
            {
                Label = label;
                Date = date;
                Color = color;
                StyleText = styleText;
            }

            public string Label { get; }

            public DateTime Date { get; set; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }
        }

        private sealed class NotesDashboard
        {
            public List<DiagnosticNote> Notes { get; } = new List<DiagnosticNote>();
        }

        private sealed class DiagnosticNote
        {
            public DiagnosticNote(string text, ConsoleColor textColor, ConsoleColor markerColor, string textStyle, string markerStyle)
            {
                Text = text;
                TextColor = textColor;
                MarkerColor = markerColor;
                TextStyle = textStyle;
                MarkerStyle = markerStyle;
            }

            public string Text { get; }

            public ConsoleColor TextColor { get; }

            public ConsoleColor MarkerColor { get; }

            public string TextStyle { get; }

            public string MarkerStyle { get; }

            public List<DiagnosticNote> Children { get; } = new List<DiagnosticNote>();
        }

        private sealed class ReadinessDashboard
        {
            public List<ReadinessCheck> Checks { get; } = new List<ReadinessCheck>();
        }

        private sealed class ReadinessCheck
        {
            public ReadinessCheck(
                string label,
                ChecklistItemState state,
                string description,
                string labelStyle,
                string markerStyle,
                string descriptionStyle)
            {
                Label = label;
                State = state;
                Description = description;
                LabelStyle = labelStyle;
                MarkerStyle = markerStyle;
                DescriptionStyle = descriptionStyle;
            }

            public string Label { get; }

            public ChecklistItemState State { get; set; }

            public string Description { get; set; }

            public string LabelStyle { get; }

            public string MarkerStyle { get; }

            public string DescriptionStyle { get; }
        }

        private sealed class ApprovalModel
        {
            public bool Approved { get; set; }

            public bool Deployed { get; set; }
        }

        private sealed class StyleDrivenModel
        {
            public bool Approved { get; set; }

            public bool Deployed { get; set; }

            public RgbColor Accent { get; set; }
        }

        private sealed class RemainingVisualSectionsModel
        {
            public List<DashboardChartItem> ChartItems { get; } = new List<DashboardChartItem>();

            public Exception LastError { get; set; } = null!;

            public string ErrorContext { get; set; } = string.Empty;

            public List<BreadcrumbSegment> Breadcrumbs { get; } = new List<BreadcrumbSegment>();

            public List<ShortcutItem> Shortcuts { get; } = new List<ShortcutItem>();

            public List<ShortcutAction> Actions { get; } = new List<ShortcutAction>();

            public DateTime Month { get; set; }

            public List<DashboardScheduleEvent> Events { get; } = new List<DashboardScheduleEvent>();

            public List<DiagnosticNote> Notes { get; } = new List<DiagnosticNote>();

            public List<ReadinessCheck> Checks { get; } = new List<ReadinessCheck>();

            public List<DashboardTrendSample> Samples { get; } = new List<DashboardTrendSample>();

            public DependencyNode DependencyRoot { get; set; } = null!;

            public List<DashboardTimelineStep> Steps { get; } = new List<DashboardTimelineStep>();
        }

        private sealed class SpecializedVisualModel
        {
            public SummarySettings Settings { get; } = new SummarySettings
            {
                Service = "Api",
                Environment = "Production",
                Port = 443
            };

            public List<ServiceSummary> Services { get; } = new List<ServiceSummary>();

            public List<MetricItem> Metrics { get; } = new List<MetricItem>();

            public List<CompositionItem> Rows { get; } = new List<CompositionItem>();

            public List<CompositionItem> Columns { get; } = new List<CompositionItem>();

            public List<CompositionItem> Cells { get; } = new List<CompositionItem>();

            public string Summary { get; set; } = string.Empty;

            public string Details { get; set; } = string.Empty;

            public string StatusMessage { get; set; } = string.Empty;

            public CalloutKind StatusKind { get; set; }
        }

        private sealed class ApprovalPaletteModel
        {
            public string Name { get; set; } = "Production";

            public bool Approved { get; set; } = true;

            public bool Deployed { get; set; }

            public bool ShowSections { get; set; } = true;

            public SpectreColor SectionColor { get; set; } = SpectreColor.Blue;

            public RgbColor SectionRgb { get; set; } = RgbColor.Aqua;

            public SpectreColor OptionColor { get; set; } = SpectreColor.Green;

            public RgbColor OptionRgb { get; set; } = RgbColor.Yellow;

            public RgbColor DisabledRgb { get; set; } = RgbColor.DarkGray;

            public SpectreColor ConfirmColor { get; set; } = SpectreColor.Red;
        }

        private sealed class ValidatedSettings
        {
            [Required(ErrorMessage = "Service is required.")]
            public string Service { get; set; } = string.Empty;

            public bool Saved { get; set; }
        }

        private sealed class CrossFieldValidatedSettings : IValidatableObject
        {
            public string Target { get; set; } = string.Empty;

            [Display(Name = "Confirm target")]
            public string ConfirmTarget { get; set; } = string.Empty;

            public bool Saved { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!string.Equals(Target, ConfirmTarget, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        "Targets must match.",
                        new[] { nameof(ConfirmTarget) });
                }
            }
        }

        private sealed class CompatibilityStaticView : StaticView
        {
            public static int RunCount { get; set; }

            public CompatibilityStaticView()
            {
                Title = "Compatibility static";
            }

            [ViewOption("Run")]
            public void Run()
            {
                RunCount++;
            }
        }

        private sealed class DescribedStaticView : StaticView
        {
            public static int RunCount { get; set; }

            public DescribedStaticView()
            {
                Title = "Described static";
            }

            [ViewOption("Run", Description = "Runs the described static action.")]
            public void Run()
            {
                RunCount++;
            }
        }

        private sealed class ShortcutStaticView : StaticView
        {
            public static int RunCount { get; set; }

            public ShortcutStaticView()
            {
                Title = "Shortcut static";
            }

            [ViewOption("Run", Shortcut = "r")]
            public void Run()
            {
                RunCount++;
            }
        }

        private sealed class GroupedStaticView : StaticView
        {
            public static int PreviewCount { get; set; }

            public static int DeployCount { get; set; }

            public GroupedStaticView()
            {
                Title = "Grouped static";
                AddSeparator("Build");
                Options.Add(new ViewOption("Preview", () => PreviewCount++, ConsoleColor.Cyan));
                AddSeparator("Release");
                Options.Add(new ViewOption("Deploy", () => DeployCount++, ConsoleColor.Green));
            }
        }

        private sealed class StyledStaticView : StaticView
        {
            public static int PreviewCount { get; set; }

            public StyledStaticView()
            {
                Title = "Styled static";
                AddSeparator("Build", "grey");
                Options.Add(new ViewOption("Preview", () => PreviewCount++, "aqua", "Runs a dry run before deployment."));
                AddHelpOptionStyled(style: "green");
                AddCommandPaletteOptionStyled(style: "magenta");
            }
        }

        private sealed class TrueColorStaticView : StaticView
        {
            public static int PreviewCount { get; set; }

            public TrueColorStaticView()
            {
                Title = "True color static";
                AddSeparator("Build", "italic #778899");
                Options.Add(new ViewOption("Preview", () => PreviewCount++, "bold #112233", "Ship release"));
                AddHelpOptionStyled(style: "bold #22c55e");
                AddCommandPaletteOptionStyled(style: "italic #abcdef");
            }
        }

        private sealed class TrueColorHelperStaticView : StaticView
        {
            public static int PreviewCount { get; set; }

            public TrueColorHelperStaticView()
            {
                Title = "True color helper static";
                AddOption("RGB sync", () => PreviewCount++, RgbColor.FromHex("#123456"));
                AddConfirmOption("Styled confirm", "Run styled confirm?", () => PreviewCount++, "bold #22c55e");
                AddEditFormOption("Edit rgb", new EditableSettings(), RgbColor.FromHex("#778899"), enableDocumentEditor: false);
                AddNavigationOption<DestinationView>("Nav rgb", RgbColor.FromHex("#abcdef"), replace: false);
                AddNavigationOption<DestinationView>(
                    "Factory styled",
                    () => new DestinationView { Title = "Styled factory detail" },
                    "italic #fedcba",
                    replace: true);
            }
        }

        private sealed class TrueColorShortcutStaticView : StaticView
        {
            public TrueColorShortcutStaticView()
            {
                Title = "True color shortcut static";
                ShowShortcuts();
                AddOption("Preview", () => { }, "italic #445566")
                    .UseShortcut("p");
            }
        }

        private sealed class StyleObjectStaticView : StaticView
        {
            public static int PreviewCount { get; set; }

            public StyleObjectStaticView()
            {
                Title = "Styled object static";
                AddSeparator("Build", new Style(ConsoleColor.DarkGray));
                Options.Add(new ViewOption("Preview", () => PreviewCount++, new Style(ConsoleColor.Cyan), "Runs a dry run before deployment."));
                AddHelpOption("Help", null, new Style(ConsoleColor.Green));
                AddCommandPaletteOption("Command palette", "Command palette", new Style(ConsoleColor.Magenta));
            }
        }

        private sealed class SharedPaletteStaticView : StaticView
        {
            public static int PreviewCount { get; set; }

            public SharedPaletteStaticView()
            {
                Title = "Palette static";
                AddSeparator("Build", SpectreColor.Aqua);
                Options.Add(new ViewOption("Preview", () => PreviewCount++, RgbColor.Yellow, "Runs a dry run before deployment."));
                AddOption("Async check", async () => await Task.Yield(), SpectreColor.Green, "Runs async work.");
                AddOption("Token check", async cancellationToken => await Task.Yield(), RgbColor.Red, "Observes cancellation.");
                AddConfirmOption("Confirm", "Run confirmation?", () => PreviewCount++, SpectreColor.Magenta, description: "Confirms before running.");
                AddEditFormOption("Edit settings", new EditableSettings(), RgbColor.Green, enableDocumentEditor: false, description: "Edits the settings object.");
                AddNavigationOption<DestinationView>("Open detail", SpectreColor.Aqua, replace: false, description: "Opens the detail view.");
                AddNavigationOption<DestinationView>("Injected detail", () => new DestinationView { Title = "Injected palette detail" }, RgbColor.Yellow, replace: true, description: "Opens a factory-created detail view.");
                AddHelpOption("Help", null, SpectreColor.Green);
                AddCommandPaletteOption("Command palette", "Command palette", RgbColor.Magenta);
            }
        }

        private sealed class HelperStaticView : StaticView
        {
            public static int SyncCount { get; set; }

            public static int AsyncCount { get; set; }

            public static int TokenCount { get; set; }

            public static CancellationToken ObservedToken { get; set; }

            public HelperStaticView()
            {
                Title = "Helper static";
                AddOption("Preview", () => SyncCount++, "aqua", "Runs preview.")
                    .UseShortcut("p")
                    .AddSearchTerms("dry-run");
                AddOption("Async check", async () =>
                {
                    await Task.Yield();
                    AsyncCount++;
                }, new Style(ConsoleColor.Green), "Runs async work.");
                AddOption("Token check", async cancellationToken =>
                {
                    await Task.Yield();
                    ObservedToken = cancellationToken;
                    TokenCount++;
                }, ConsoleColor.Yellow, "Observes cancellation.");
            }
        }

        private sealed class ConfirmHelperStaticView : StaticView
        {
            public static int DeclinedCount { get; set; }

            public static int AsyncCount { get; set; }

            public static int TokenCount { get; set; }

            public static CancellationToken ObservedToken { get; set; }

            public ConfirmHelperStaticView()
            {
                Title = "Confirm helper static";
                AddConfirmOption(
                    "Declined",
                    "Run declined?",
                    () => DeclinedCount++,
                    ConsoleColor.Red,
                    description: "Should not run.");
                AddConfirmOption(
                    "Confirmed async",
                    "Run async?",
                    async () =>
                    {
                        await Task.Yield();
                        AsyncCount++;
                    },
                    "green",
                    description: "Runs async.");
                AddConfirmOption(
                    "Confirmed token",
                    "Run token?",
                    async cancellationToken =>
                    {
                        await Task.Yield();
                        ObservedToken = cancellationToken;
                        TokenCount++;
                    },
                    new Style(ConsoleColor.Cyan),
                    description: "Observes confirmation token.");
            }
        }

        private sealed class FormHelperStaticView : StaticView
        {
            public FormHelperStaticView(EditableSettings settings)
            {
                Title = "Form helper static";
                AddEditFormOption(
                    "Edit settings",
                    settings,
                    "green",
                    enableDocumentEditor: false,
                    description: "Edits the settings object.")
                    .UseShortcut("e");
            }
        }

        private sealed class NavigationHelperStaticView : StaticView
        {
            public NavigationHelperStaticView()
            {
                Title = "Navigation helper static";
                AddNavigationOption<DestinationView>(
                    "Open detail",
                    new Style(ConsoleColor.Cyan),
                    replace: false,
                    description: "Opens the detail view.");
            }
        }

        private sealed class FactoryNavigationHelperStaticView : StaticView
        {
            public FactoryNavigationHelperStaticView()
            {
                Title = "Factory navigation static";
                AddNavigationOption<DestinationView>(
                    "Injected detail",
                    () => new DestinationView { Title = "Injected static detail" },
                    new Style(ConsoleColor.Cyan),
                    replace: true,
                    description: "Opens a factory-created detail view.");
            }
        }

        private sealed class ErrorHandlingStaticView : StaticView
        {
            public ErrorHandlingStaticView()
            {
                Title = "Error static";
                AddOption("Fail", () => throw new InvalidOperationException("static failure"));
            }
        }

        private sealed class CompatibilityDynamicView : DynamicView<object>
        {
            public static int RunCount { get; set; }

            public CompatibilityDynamicView()
            {
                Title = "Compatibility dynamic";
            }

            public string RunMessage()
            {
                return "Run";
            }

            public ConsoleColor RunColor()
            {
                return ConsoleColor.Green;
            }

            [DynamicViewOption(nameof(RunMessage), nameof(RunColor))]
            public void Run()
            {
                RunCount++;
            }
        }

        private sealed class DescribedDynamicView : DynamicView<object>
        {
            public static int RunCount { get; set; }

            public DescribedDynamicView()
            {
                Title = "Described dynamic";
            }

            public string RunMessage()
            {
                return "Run";
            }

            public ConsoleColor RunColor()
            {
                return ConsoleColor.Green;
            }

            public string RunDescription()
            {
                return "Runs the described dynamic action.";
            }

            [DynamicViewOption(nameof(RunMessage), nameof(RunColor), nameof(RunDescription))]
            public void Run()
            {
                RunCount++;
            }
        }

        private sealed class SearchableDynamicView : DynamicView<object>
        {
            public static int RunCount { get; set; }

            public SearchableDynamicView()
            {
                Title = "Searchable dynamic";
            }

            public string ReleaseMessage()
            {
                return "Release";
            }

            public ConsoleColor ReleaseColor()
            {
                return ConsoleColor.Green;
            }

            public IEnumerable<string> ReleaseSearchTerms()
            {
                return new[] { "ship", "publish" };
            }

            [DynamicViewOption(nameof(ReleaseMessage), nameof(ReleaseColor), null, null, nameof(ReleaseSearchTerms))]
            public void Release()
            {
                RunCount++;
            }
        }

        [View("Static")]
        private sealed class HelpStaticView : StaticView
        {
            [ViewOption("Inspect", Description = "Shows the current model summary.", Shortcut = "i")]
            public void Inspect()
            {
            }
        }

        private sealed class PaletteStaticView : StaticView
        {
            public static int InspectCount { get; set; }

            [ViewOption("Inspect", Description = "Shows the current model summary.", Shortcut = "i", SearchTerms = "details;summary")]
            public void Inspect()
            {
                InspectCount++;
            }
        }

        /// <summary>
        /// Root view used to verify renderer-provided breadcrumb chrome.
        /// </summary>
        [View("Chrome Root")]
        public sealed class ChromeRootView : StaticView
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChromeRootView"/> class.
            /// </summary>
            public ChromeRootView()
            {
                ShowBreadcrumbs();
            }

            /// <summary>
            /// Navigates to the detail view.
            /// </summary>
            [ViewOption("Open detail")]
            public void OpenDetail()
            {
                NavigateTo<ChromeDetailView>(replace: false);
            }
        }

        /// <summary>
        /// Detail view used to verify renderer-provided breadcrumb chrome.
        /// </summary>
        [View("Chrome Detail")]
        public sealed class ChromeDetailView : StaticView
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChromeDetailView"/> class.
            /// </summary>
            public ChromeDetailView()
            {
                ShowBreadcrumbs();
            }
        }

        private sealed class EditableSettings
        {
            public string Name { get; set; } = string.Empty;
        }

        private sealed class CancellationAwareEditableSettings
        {
            [PropertyEditor(typeof(ObservingCancellationPropertyEditor))]
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Test editor that records the cancellation token supplied by the object editor.
        /// </summary>
        public sealed class ObservingCancellationPropertyEditor : IPropertyEditor
        {
            /// <summary>
            /// Gets or sets the token observed by the most recent edit.
            /// </summary>
            public static CancellationToken ObservedToken { get; set; }

            /// <inheritdoc />
            public bool TryEdit(PropertyEditContext context, out object value)
            {
                ObservedToken = context.CancellationToken;
                value = "Edited through custom editor";
                return true;
            }
        }

        private sealed class CancelingDefaultEditableSettings
        {
            [PropertyEditor(typeof(CancelingDefaultPropertyEditor))]
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Test editor that cancels the active render token before delegating to the default editor.
        /// </summary>
        public sealed class CancelingDefaultPropertyEditor : IPropertyEditor
        {
            /// <summary>
            /// Gets or sets the cancellation source canceled by the next edit.
            /// </summary>
            public static CancellationTokenSource? Cancellation { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the editor delegated to the default editor.
            /// </summary>
            public static bool DelegatedToDefaultEditor { get; set; }

            /// <inheritdoc />
            public bool TryEdit(PropertyEditContext context, out object value)
            {
                Cancellation?.Cancel();
                DelegatedToDefaultEditor = true;
                return new DefaultPropertyEditor().TryEdit(context, out value);
            }
        }

        private sealed class ValidatedEditableSettings
        {
            [Display(Name = "Service key")]
            [Required(ErrorMessage = "Service key is required.")]
            [StringLength(8, MinimumLength = 3, ErrorMessage = "Service key must be 3 to 8 characters.")]
            public string ServiceKey { get; set; } = string.Empty;

            [Display(Name = "Retry count")]
            [Range(1, 5, ErrorMessage = "Retry count must be between 1 and 5.")]
            public int RetryCount { get; set; } = 1;
        }

        private sealed class RegexEditableSettings
        {
            [Display(Name = "Service key")]
            [RegularExpression("^svc-[a-z]+$", ErrorMessage = "Service key must start with svc-.")]
            public string ServiceKey { get; set; } = string.Empty;
        }

        private sealed class PasswordEditableSettings
        {
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Confirm password")]
            [Compare(nameof(Password), ErrorMessage = "Passwords must match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        private sealed class BooleanEditableSettings
        {
            [Display(Name = "Enable preview mode")]
            public bool EnablePreviewMode { get; set; }
        }

        private sealed class NullableBooleanEditableSettings
        {
            [Display(Name = "Enable preview mode")]
            public bool? EnablePreviewMode { get; set; }
        }

        private sealed class EnumEditableSettings
        {
            [Display(Name = "Environment")]
            public DeploymentSlot Environment { get; set; }
        }

        private sealed class FlagsEnumEditableSettings
        {
            [Display(Name = "Capabilities")]
            public DeploymentCapability Capabilities { get; set; }
        }

        private sealed class NullableEnumEditableSettings
        {
            [Display(Name = "Environment")]
            public DeploymentSlot? Environment { get; set; }
        }

        private sealed class NullableFlagsEnumEditableSettings
        {
            [Display(Name = "Capabilities")]
            public DeploymentCapability? Capabilities { get; set; }
        }

        private sealed class RequiredNullableChoiceSettings
        {
            [Display(Name = "Enable preview mode")]
            [Required(ErrorMessage = "Preview mode is required.")]
            public bool? EnablePreviewMode { get; set; }

            [Display(Name = "Environment")]
            [Required(ErrorMessage = "Environment is required.")]
            public DeploymentSlot? Environment { get; set; }
        }

        private sealed class InterfaceCollectionSettings
        {
            [Display(Order = 1)]
            public IList<string> Tags { get; set; } = new List<string> { "alpha" };

            [Display(Order = 2)]
            public IReadOnlyList<string> Reviewers { get; set; } = new List<string> { "mira" };
        }

        private sealed class EnumCollectionSettings
        {
            public IList<DeploymentSlot> Environments { get; set; } = new List<DeploymentSlot> { DeploymentSlot.Development };
        }

        private sealed class SetCollectionSettings
        {
            public HashSet<int> Scores { get; set; } = new HashSet<int> { 1 };
        }

        private sealed class InterfaceDictionarySettings
        {
            public IDictionary<string, string> Metadata { get; set; } = null!;
        }

        private sealed class DescribedEditableSettings
        {
            [Display(Name = "Service key", Description = "Used to group preview deployments.")]
            public string ServiceKey { get; set; } = "api";
        }

        private sealed class SerializerAwareSettings
        {
            public string OptionalName { get; set; } = null!;

            public object UntypedValue { get; set; } = 17;

            public ISerializerShape Shape { get; set; } = new SerializerShape { Name = "API" };
        }

        private interface ISerializerShape
        {
            string Name { get; set; }
        }

        private sealed class SerializerShape : ISerializerShape
        {
            public string Name { get; set; } = string.Empty;
        }

        private enum DeploymentSlot
        {
            [Display(Name = "Development slot")]
            Development,

            [Display(Name = "Staging slot")]
            Staging,

            [Display(Name = "Production slot")]
            Production
        }

        [Flags]
        private enum DeploymentCapability
        {
            [Display(Name = "No capabilities")]
            None = 0,

            [Display(Name = "Preview capability")]
            Preview = 1,

            [Display(Name = "Deploy capability")]
            Deploy = 2,

            [Display(Name = "Rollback capability")]
            Rollback = 4,

            All = Preview | Deploy | Rollback
        }
    }
}
