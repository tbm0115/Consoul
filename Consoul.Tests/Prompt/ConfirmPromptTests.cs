using System;
using System.Collections.Generic;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests confirmation prompt behavior.
    /// </summary>
    public class ConfirmPromptTests
    {
        /// <summary>
        /// Confirmation prompts parse common affirmative input.
        /// </summary>
        [Theory]
        [InlineData("y")]
        [InlineData("yes")]
        [InlineData("true")]
        [InlineData("1")]
        [InlineData("on")]
        public void Render_AffirmativeInputReturnsTrue(string input)
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { input }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("Deploy now?").Render();

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    Assert.Equal(input, result.RawInput);
                    VisualAssert.WroteText(scope.Driver, "Deploy now? [y/n]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmation prompts parse common negative input.
        /// </summary>
        [Theory]
        [InlineData("n")]
        [InlineData("no")]
        [InlineData("false")]
        [InlineData("0")]
        [InlineData("off")]
        public void Render_NegativeInputReturnsFalse(string input)
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { input }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("Deploy now?").Render();

                    Assert.True(result.HasValue);
                    Assert.False(result.Value);
                    Assert.Equal(input, result.RawInput);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmation prompts use configured defaults when the user presses Enter.
        /// </summary>
        [Fact]
        public void Render_EmptyInputUsesDefaultValue()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("Deploy now?")
                        .WithDefault(true)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    Assert.Equal(string.Empty, result.RawInput);
                    VisualAssert.WroteText(scope.Driver, "Deploy now? [Y/n]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmation prompts repeat for invalid input before returning a valid result.
        /// </summary>
        [Fact]
        public void Render_InvalidInputRepeats()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "maybe", "no" }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("Deploy now?", clear: true).Render();

                    Assert.True(result.HasValue);
                    Assert.False(result.Value);
                    Assert.True(scope.Driver.ClearCount >= 2);
                    VisualAssert.WroteText(scope.Driver, "Please answer yes or no.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Confirmation prompts return canceled results for prompt cancellation phrases.
        /// </summary>
        [Fact]
        public void Render_CancellationPhraseReturnsCanceled()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "back" }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("Deploy now?").Render();

                    Assert.True(result.IsCanceled);
                    Assert.False(result.HasValue);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The static confirmation helper returns a result-first confirmation outcome.
        /// </summary>
        [Fact]
        public void ConsoulConfirm_ReturnsConfirmationResult()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    ConfirmResult result = ConsoulLibrary.Consoul.Confirm("Run migrations?", defaultValue: false);

                    Assert.True(result.HasValue);
                    Assert.False(result.Value);
                    VisualAssert.WroteText(scope.Driver, "Run migrations? [y/N]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// The static confirmation builder can be customized before rendering.
        /// </summary>
        [Fact]
        public void ConsoulConfirmation_ReturnsConfigurablePrompt()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "bad", "on" }));

                try
                {
                    ConfirmResult result = ConsoulLibrary.Consoul.Confirmation("Enable preview?")
                        .WithDefault(false)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    VisualAssert.WroteText(scope.Driver, "Enable preview? [y/N]");
                    VisualAssert.WroteText(scope.Driver, "Please answer yes or no.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        private static void ClearRoutine()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
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
