using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Prompt
{
    /// <summary>
    /// Tests legacy yes/no prompt behavior.
    /// </summary>
    public class AskPromptTests
    {
        /// <summary>
        /// Invalid non-empty answers are rejected until the user enters a valid yes/no value.
        /// </summary>
        [Fact]
        public void Ask_RepeatsAfterInvalidInput()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "maybe", "y" }));

                try
                {
                    bool result = Consoul.Ask("Continue?");

                    Assert.True(result);
                    VisualAssert.WroteText(scope.Driver, "Invalid input!");
                    Assert.True(scope.Driver.Writes.Count(write => write.Text.Contains("Continue?")) >= 2);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Empty answers are accepted only when explicitly allowed and then map to the configured default.
        /// </summary>
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Ask_AllowEmptyUsesConfiguredDefault(bool defaultIsNo, bool expected)
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    bool result = Consoul.Ask("Continue?", allowEmpty: true, defaultIsNo: defaultIsNo);

                    Assert.Equal(expected, result);
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
