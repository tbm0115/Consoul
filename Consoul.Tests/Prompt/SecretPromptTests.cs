using System;
using System.Collections.Generic;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests masked secret prompt behavior.
    /// </summary>
    public class SecretPromptTests
    {
        /// <summary>
        /// Secret prompts return typed key input while rendering only mask characters.
        /// </summary>
        [Fact]
        public void Render_ReturnsSecretAndMasksTypedCharacters()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                EnqueueText(scope.Driver, "secret");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("API key").Render();

                Assert.True(result.HasValue);
                Assert.Equal("secret", result.Value);
                VisualAssert.WroteText(scope.Driver, "API key");
                Assert.Contains(scope.Driver.Writes, write => write.Text == "*");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("secret"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Secret prompts honor backspace while editing masked input.
        /// </summary>
        [Fact]
        public void Render_BackspaceRemovesPreviousCharacter()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                EnqueueText(scope.Driver, "ab");
                scope.Driver.EnqueueKey(Backspace());
                EnqueueText(scope.Driver, "c");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("Token").Render();

                Assert.True(result.HasValue);
                Assert.Equal("ac", result.Value);
                Assert.Contains(scope.Driver.Writes, write => write.Text == "\b \b");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Secret prompts can hide input without rendering mask characters.
        /// </summary>
        [Fact]
        public void Render_NullMaskCharacterHidesInputWithoutMask()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                EnqueueText(scope.Driver, "hidden");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("Secret")
                {
                    MaskCharacter = null
                }.Render();

                Assert.True(result.HasValue);
                Assert.Equal("hidden", result.Value);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text == "*");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hidden"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Secret prompts repeat for empty input until a value is provided.
        /// </summary>
        [Fact]
        public void Render_EmptyInputRepeatsUntilValueProvided()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                scope.Driver.EnqueueKey(Enter());
                EnqueueText(scope.Driver, "value");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("Secret", clear: true).Render();

                Assert.True(result.HasValue);
                Assert.Equal("value", result.Value);
                Assert.True(scope.Driver.ClearCount >= 2);
                VisualAssert.WroteText(scope.Driver, "A value is required.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Secret prompts can accept empty input when explicitly allowed.
        /// </summary>
        [Fact]
        public void Render_AllowEmptyReturnsEmptySecret()
        {
            using (new ConsoleDriverScope(80))
            {
                SecretResult result = new SecretPrompt("Optional secret")
                    .AllowEmptyInput()
                    .Render();

                Assert.True(result.HasValue);
                Assert.Equal(string.Empty, result.Value);
            }
        }

        /// <summary>
        /// Secret prompts can validate masked values and repeat without leaking rejected input.
        /// </summary>
        [Fact]
        public void Render_ValidationRepeatsUntilSecretAccepted()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                EnqueueText(scope.Driver, "short");
                scope.Driver.EnqueueKey(Enter());
                EnqueueText(scope.Driver, "long-secret");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("API key")
                    .MinimumLength(8, "API key must contain at least 8 characters.")
                    .Render();

                Assert.True(result.HasValue);
                Assert.Equal("long-secret", result.Value);
                VisualAssert.WroteText(scope.Driver, "API key must contain at least 8 characters.");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("short"));
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("long-secret"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Secret prompts support predicate validators for common POC checks.
        /// </summary>
        [Fact]
        public void Render_PredicateValidatorRepeatsUntilValid()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                EnqueueText(scope.Driver, "abc123");
                scope.Driver.EnqueueKey(Enter());
                EnqueueText(scope.Driver, "tok_abc123");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("Token")
                    .Validate(value => value.StartsWith("tok_", StringComparison.Ordinal), "Token must start with tok_.")
                    .Render();

                Assert.True(result.HasValue);
                Assert.Equal("tok_abc123", result.Value);
                VisualAssert.WroteText(scope.Driver, "Token must start with tok_.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The static secret helper returns a result-first secret outcome.
        /// </summary>
        [Fact]
        public void ConsoulReadSecret_ReturnsSecretResult()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                EnqueueText(scope.Driver, "token");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = ConsoulLibrary.Consoul.ReadSecret("Token");

                Assert.True(result.HasValue);
                Assert.Equal("token", result.Value);
                VisualAssert.WroteText(scope.Driver, "Token");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Routine-backed password reads render masks instead of secret values.
        /// </summary>
        [Fact]
        public void ReadPassword_RoutineInputDoesNotEchoSecretValue()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "routine-secret" }));

                try
                {
                    string value = ConsoulLibrary.Consoul.ReadPassword();

                    Assert.Equal("routine-secret", value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("**************"));
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("routine-secret"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        private static void EnqueueText(FakeConsoleDriver driver, string text)
        {
            foreach (char ch in text)
            {
                driver.EnqueueKey(new ConsoleKeyInfo(ch, ConsoleKey.A, false, false, false));
            }
        }

        private static ConsoleKeyInfo Enter()
        {
            return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
        }

        private static ConsoleKeyInfo Backspace()
        {
            return new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false);
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
