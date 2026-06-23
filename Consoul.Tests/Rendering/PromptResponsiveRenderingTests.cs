using System;
using System.Collections.Generic;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for selection prompts.
    /// </summary>
    public class PromptResponsiveRenderingTests
    {
        /// <summary>
        /// Invalid prompt input re-renders under narrow widths without overflowing.
        /// </summary>
        [Theory]
        [InlineData(24)]
        [InlineData(80)]
        public void Render_InvalidSelectionThenValidSelection_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "9", "2" }));

                try
                {
                    var prompt = new SelectionPrompt(
                        "Choose a deployment target with a long prompt message",
                        clear: true,
                        "Alpha target with extended descriptive text",
                        "Beta target",
                        "Gamma target");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.True(scope.Driver.ClearCount >= 2);
                    VisualAssert.WroteText(scope.Driver, "Invalid selection!");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Default and escape prompt inputs preserve behavior while rendering headlessly.
        /// </summary>
        [Fact]
        public void Render_DefaultAndEscapeInputs_DoNotRequireRealConsole()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new SelectionPrompt(
                        "Pick a default",
                        false,
                        new SelectOption(0, "Default option with a long label", ConsoleColor.Green, isDefault: true),
                        new SelectOption(1, "Other option", ConsoleColor.White));

                    PromptResult defaultResult = prompt.Render();

                    Assert.True(defaultResult.HasSelection);
                    Assert.Equal(0, defaultResult.Index);

                    Routines.InitializeRoutine(new TestRoutine(new[] { "exit" }));
                    PromptResult escapeResult = new SelectionPrompt("Pick anything", false, "One", "Two").Render();

                    Assert.True(escapeResult.IsCanceled);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
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
    }
}
