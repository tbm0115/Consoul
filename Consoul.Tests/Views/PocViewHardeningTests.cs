using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using ConsoulLibrary.Views;
using Xunit;

namespace ConsoulLibrary.Tests.Views
{
    /// <summary>
    /// Tests proof-of-concept view APIs and compatibility behavior.
    /// </summary>
    public class PocViewHardeningTests
    {
        /// <summary>
        /// Fluent views execute synchronous options and then continue until the user exits.
        /// </summary>
        [Fact]
        public void FluentView_Render_SelectsSynchronousOption()
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
        /// Fluent views execute asynchronous options with the active cancellation token.
        /// </summary>
        [Fact]
        public async Task FluentView_RenderAsync_SelectsAsynchronousOption()
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
        /// Fluent views expose typed navigation through the existing navigation context.
        /// </summary>
        [Fact]
        public async Task FluentView_Navigate_RequestsTypedNavigation()
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
        /// Canceled fluent renders exit without invoking option actions.
        /// </summary>
        [Fact]
        public async Task FluentView_RenderAsync_CanceledTokenExitsWithoutAction()
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
        /// The JSON object editor uses the fake console driver for key input, cursor visibility, clears, and writes.
        /// </summary>
        [Fact]
        public void EditObjectView_JsonEditor_UsesConsoleDriver()
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
                    VisualAssert.WroteText(scope.Driver, "Press Esc to return to the editor.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
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

        private sealed class EditableSettings
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
