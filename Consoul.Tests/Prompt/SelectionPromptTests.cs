using System;
using System.Collections.Generic;
using ConsoulLibrary;
using Xunit;

namespace Consoul.Tests.Prompt
{
    public class SelectionPromptTests
    {
        [Fact]
        public void Render_RepeatsWhenSelectionExceedsOptionCount()
        {
            var routine = new TestRoutine(new[]
            {
                new RoutineInput { Value = "4" },
                new RoutineInput { Value = "1" }
            });

            Routines.InitializeRoutine(routine);

            try
            {
                var prompt = new SelectionPrompt("Pick an option", clear: false, "Alpha", "Beta", "Gamma");

                PromptResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(0, result.Index);
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
            }
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(IEnumerable<RoutineInput> inputs)
            {
                foreach (var input in inputs)
                {
                    Enqueue(input);
                }
            }
        }
    }
}
