using System;
using System.Collections.Generic;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests typed text prompt behavior.
    /// </summary>
    public class TextPromptTests
    {
        /// <summary>
        /// Text prompts repeat for parse and validation failures before returning a typed value.
        /// </summary>
        [Fact]
        public void Render_RepeatsUntilParsedAndValidated()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "abc", "-1", "3" }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Retry count", clear: true)
                        .Validate(value => value >= 0 ? null : "Retry count cannot be negative.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(3, result.Value);
                    Assert.True(scope.Driver.ClearCount >= 3);
                    VisualAssert.WroteText(scope.Driver, "Invalid input!");
                    VisualAssert.WroteText(scope.Driver, "Retry count cannot be negative.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts use configured defaults when the user presses Enter.
        /// </summary>
        [Fact]
        public void Render_EmptyInputUsesDefaultValue()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Retry count")
                        .WithDefault(5)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(5, result.Value);
                    Assert.Equal(string.Empty, result.RawInput);
                    VisualAssert.WroteText(scope.Driver, "default: 5");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts return canceled results for prompt cancellation phrases.
        /// </summary>
        [Fact]
        public void Render_CancellationPhraseReturnsCanceled()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "exit" }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Name").Render();

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
        /// Text prompts support empty string input when explicitly allowed.
        /// </summary>
        [Fact]
        public void Render_AllowEmptyStringReturnsEmptyValue()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Optional label")
                        .AllowEmptyInput()
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(string.Empty, result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts accept common boolean shorthand.
        /// </summary>
        [Fact]
        public void Render_BooleanShorthandParses()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "yes" }));

                try
                {
                    TextPromptResult<bool> result = ConsoulLibrary.Consoul.Text<bool>("Enable preview").Render();

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse GUID values without requiring a custom parser.
        /// </summary>
        [Fact]
        public void Render_GuidParsesWithoutCustomParser()
        {
            Guid expected = Guid.Parse("d3f49e8a-8c36-45fc-9d35-0ea40669a44a");

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    "not-a-guid",
                    expected.ToString()
                }));

                try
                {
                    TextPromptResult<Guid> result = ConsoulLibrary.Consoul.Text<Guid>("Operation id").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(expected, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Invalid input!");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse nullable GUID values and allow empty input to return null.
        /// </summary>
        [Fact]
        public void Render_NullableGuidAllowsEmptyValue()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    TextPromptResult<Guid?> result = ConsoulLibrary.Consoul.Text<Guid?>("Optional operation id")
                        .AllowEmptyInput()
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Null(result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse time spans without requiring a custom parser.
        /// </summary>
        [Fact]
        public void Render_TimeSpanParsesWithoutCustomParser()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "00:05:30" }));

                try
                {
                    TextPromptResult<TimeSpan> result = ConsoulLibrary.Consoul.Text<TimeSpan>("Timeout").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(TimeSpan.FromSeconds(330), result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse DateTime values without requiring a custom parser.
        /// </summary>
        [Fact]
        public void Render_DateTimeParsesWithoutCustomParser()
        {
            DateTime expected = DateTime.Parse("2026-06-23T14:30:00");

            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { expected.ToString("O") }));

                try
                {
                    TextPromptResult<DateTime> result = ConsoulLibrary.Consoul.Text<DateTime>("Start date").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(expected, result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse DateTimeOffset values without requiring a custom parser.
        /// </summary>
        [Fact]
        public void Render_DateTimeOffsetParsesWithoutCustomParser()
        {
            DateTimeOffset expected = DateTimeOffset.Parse("2026-06-23T14:30:00-04:00");

            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { expected.ToString("O") }));

                try
                {
                    TextPromptResult<DateTimeOffset> result = ConsoulLibrary.Consoul.Text<DateTimeOffset>("Start time").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(expected, result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse URI values without requiring a custom parser.
        /// </summary>
        [Fact]
        public void Render_UriParsesWithoutCustomParser()
        {
            var expected = new Uri("https://example.test/api");

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    "http://[:::]",
                    expected.ToString()
                }));

                try
                {
                    TextPromptResult<Uri> result = ConsoulLibrary.Consoul.Text<Uri>("Endpoint").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(expected, result.Value);
                    Assert.Contains("Invalid input!", scope.Driver.Lines);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts parse version values without requiring a custom parser.
        /// </summary>
        [Fact]
        public void Render_VersionParsesWithoutCustomParser()
        {
            var expected = new Version(2, 1, 3);

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    "v2",
                    expected.ToString()
                }));

                try
                {
                    TextPromptResult<Version> result = ConsoulLibrary.Consoul.Text<Version>("Version").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(expected, result.Value);
                    Assert.Contains("Invalid input!", scope.Driver.Lines);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts support custom parsers.
        /// </summary>
        [Fact]
        public void Render_CustomParserCanMapDomainValues()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "prod" }));

                try
                {
                    TextPromptResult<DeployTarget> result = ConsoulLibrary.Consoul.Text<DeployTarget>("Target")
                        .ParseWith((string text, out DeployTarget target) =>
                        {
                            target = new DeployTarget(text.ToUpperInvariant());
                            return true;
                        })
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("PROD", result.Value.Name);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts can restrict parsed values to a known set.
        /// </summary>
        [Fact]
        public void Render_AcceptOnlyRepeatsUntilValueIsAllowed()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "qa", "prod" }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Environment")
                        .AcceptOnly(
                            new[] { "dev", "prod" },
                            comparer: StringComparer.OrdinalIgnoreCase)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Value must be one of: dev, prod.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Accepted-value prompts show choices and defaults in the prompt message.
        /// </summary>
        [Fact]
        public void Render_AcceptOnlyShowsChoicesAndDefault()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Environment")
                        .AcceptOnly(new[] { "dev", "staging", "prod" })
                        .WithDefault("staging")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("staging", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Environment (dev/staging/prod, default: staging)");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Accepted-value prompts can hide the choice hint while keeping validation.
        /// </summary>
        [Fact]
        public void Render_AcceptOnlyCanHideChoiceHint()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "prod" }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Environment")
                        .AcceptOnly(new[] { "dev", "prod" })
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    Assert.Contains("Environment (dev/prod)", scope.Driver.Text);
                }
                finally
                {
                    ClearRoutine();
                }
            }

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "prod" }));

                try
                {
                    var prompt = ConsoulLibrary.Consoul.Text<string>("Environment")
                        .AcceptOnly(new[] { "dev", "prod" });
                    prompt.ShowChoices = false;

                    TextPromptResult<string> result = prompt.Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Environment");
                    Assert.DoesNotContain("Environment (dev/prod)", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Accepted-value prompts can customize the rendered choice separator.
        /// </summary>
        [Fact]
        public void Render_AcceptOnlyUsesCustomChoiceSeparator()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "prod" }));

                try
                {
                    var prompt = ConsoulLibrary.Consoul.Text<string>("Environment")
                        .AcceptOnly(new[] { "dev", "staging", "prod" });
                    prompt.ChoicesSeparator = " | ";

                    TextPromptResult<string> result = prompt.Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Environment (dev | staging | prod)");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts can use a custom message for parse failures.
        /// </summary>
        [Fact]
        public void Render_WithInvalidMessageUsesCustomParseFailureMessage()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "abc", "7" }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Retry count")
                        .WithInvalidMessage("Enter a whole number.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(7, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Enter a whole number.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Predicate validators provide a compact common-case validation form.
        /// </summary>
        [Fact]
        public void Render_PredicateValidatorRepeatsUntilValid()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2", "3" }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Odd value")
                        .Validate(value => value % 2 != 0, "Value must be odd.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(3, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Value must be odd.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Result-shaped validators support a Spectre-style success/error validation flow.
        /// </summary>
        [Fact]
        public void Render_PromptValidationResultValidatorRepeatsUntilSuccessful()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2", "3" }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Odd value")
                        .Validate(value => value % 2 != 0
                            ? PromptValidationResult.Success()
                            : PromptValidationResult.Error("Value must be odd."))
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(3, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Value must be odd.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompts can normalize input before parsing while preserving the submitted raw text.
        /// </summary>
        [Fact]
        public void Render_TrimInputNormalizesBeforeParsingAndPreservesRawInput()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "  prod  " }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Environment")
                        .TrimInput()
                        .AcceptOnly(
                            new[] { "dev", "prod" },
                            comparer: StringComparer.OrdinalIgnoreCase)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    Assert.Equal("  prod  ", result.RawInput);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Required prompts reject missing string values before returning.
        /// </summary>
        [Fact]
        public void Render_RequiredRejectsEmptyOrWhitespaceString()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "   ", "ops" }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Service owner")
                        .Required("Service owner is required.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("ops", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Service owner is required.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Comparable prompt values support inclusive range validation without custom lambdas.
        /// </summary>
        [Fact]
        public void Render_RangeRepeatsUntilComparableValueIsInsideBounds()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "0", "11", "5" }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Retry count")
                        .Range(1, 10)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(5, result.Value);
                    Assert.Equal(2, CountLines(scope.Driver.Lines, "Value must be between 1 and 10."));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Comparable prompt values can use separate minimum and maximum validators.
        /// </summary>
        [Fact]
        public void Render_MinimumAndMaximumRepeatUntilValuePassesBothBounds()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1", "99", "3" }));

                try
                {
                    TextPromptResult<int> result = ConsoulLibrary.Consoul.Text<int>("Worker count")
                        .Minimum(2)
                        .Maximum(8)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(3, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Value must be at least 2.");
                    VisualAssert.WroteText(scope.Driver, "Value must be at most 8.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// String prompts can use common length validation helpers.
        /// </summary>
        [Fact]
        public void Render_LengthBetweenRepeatsUntilStringLengthIsInsideBounds()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "ab", "deployment-service", "api" }));

                try
                {
                    TextPromptResult<string> result = ConsoulLibrary.Consoul.Text<string>("Service key")
                        .TrimInput()
                        .Required()
                        .LengthBetween(3, 8)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("api", result.Value);
                    Assert.Equal(2, CountLines(scope.Driver.Lines, "Value must be between 3 and 8 characters."));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Text prompt helper methods validate impossible bounds at configuration time.
        /// </summary>
        [Fact]
        public void RangeAndLengthHelpers_ImpossibleBoundsThrow()
        {
            Assert.Throws<ArgumentException>(() => ConsoulLibrary.Consoul.Text<int>("Retry count").Range(10, 1));
            Assert.Throws<ArgumentException>(() => ConsoulLibrary.Consoul.Text<string>("Service key").LengthBetween(8, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => ConsoulLibrary.Consoul.Text<string>("Service key").MinimumLength(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ConsoulLibrary.Consoul.Text<string>("Service key").MaximumLength(-1));
        }

        /// <summary>
        /// Accepted-value prompts require at least one allowed value.
        /// </summary>
        [Fact]
        public void AcceptOnly_EmptyAllowedValuesThrows()
        {
            var prompt = ConsoulLibrary.Consoul.Text<string>("Environment");

            Assert.Throws<ArgumentException>(() => prompt.AcceptOnly(Array.Empty<string>()));
        }

        private static void ClearRoutine()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
        }

        private static int CountLines(IEnumerable<string> lines, string expected)
        {
            int count = 0;
            foreach (string line in lines)
            {
                if (line == expected)
                {
                    count++;
                }
            }

            return count;
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

        private sealed class DeployTarget
        {
            public DeployTarget(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
