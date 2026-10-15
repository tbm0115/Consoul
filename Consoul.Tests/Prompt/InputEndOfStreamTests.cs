using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using ConsoulLibrary.Views.Editing;
using Xunit;

namespace ConsoulLibrary.Tests.Prompt
{
    /// <summary>
    /// Exercises real stream and key input exhaustion without allowing retry regressions to hang the test process.
    /// </summary>
    public class InputEndOfStreamTests
    {
        /// <summary>
        /// Exhausted input cancels each prompt instead of retrying or silently accepting a default.
        /// </summary>
        [Theory]
        [InlineData("text")]
        [InlineData("text-default")]
        [InlineData("editable-default")]
        [InlineData("text-secret")]
        [InlineData("secret")]
        [InlineData("confirm")]
        [InlineData("confirm-default")]
        [InlineData("confirmation")]
        [InlineData("confirmation-key")]
        [InlineData("selection")]
        [InlineData("selection-key")]
        [InlineData("multi-selection")]
        [InlineData("multi-selection-key")]
        public void Render_EndOfInputReturnsCanceled(string kind)
        {
            using (UseInput(string.Empty))
            {
                bool canceled;
                switch (kind)
                {
                    case "text":
                        canceled = new TextPrompt<int>("Number").Render().IsCanceled;
                        break;
                    case "text-default":
                        canceled = new TextPrompt<int>("Number").WithDefault(7).Render().IsCanceled;
                        break;
                    case "editable-default":
                        var editable = new TextPrompt<string>("Name").WithDefault("default");
                        editable.EditableDefaultValue = true;
                        canceled = editable.Render().IsCanceled;
                        break;
                    case "text-secret":
                        canceled = new TextPrompt<string>("Secret") { IsSecret = true }.Render().IsCanceled;
                        break;
                    case "secret":
                        canceled = new SecretPrompt("Secret").Render().IsCanceled;
                        break;
                    case "confirm":
                        canceled = new ConfirmPrompt("Continue?").Render().IsCanceled;
                        break;
                    case "confirm-default":
                        canceled = new ConfirmPrompt("Continue?").WithDefault(true).Render().IsCanceled;
                        break;
                    case "confirmation":
                    case "confirmation-key":
                        canceled = new ConfirmationPrompt("Continue?")
                        {
                            RequireEnter = kind == "confirmation"
                        }.Render().IsCanceled;
                        break;
                    case "selection":
                    case "selection-key":
                        canceled = new SelectionPrompt("Choose")
                            .AddChoices("one", "two")
                            .UseKeyboardNavigation(kind == "selection-key")
                            .Render().IsCanceled;
                        break;
                    default:
                        canceled = new MultiSelectionPrompt("Choose")
                            .AddChoices("one", "two")
                            .UseKeyboardNavigation(kind == "multi-selection-key")
                            .Render().IsCanceled;
                        break;
                }

                Assert.True(canceled);
            }
        }

        /// <summary>
        /// A final invalid response is rejected once, then EOF cancels instead of retrying forever.
        /// </summary>
        [Theory]
        [InlineData("invalid")]
        [InlineData("invalid\n")]
        public void TextPrompt_InvalidFinalInputThenEndOfInputCancels(string input)
        {
            using (UseInput(input))
            {
                Assert.True(new TextPrompt<int>("Number").Render().IsCanceled);
            }
        }

        /// <summary>
        /// Blank lines still accept defaults, while valid final responses do not require a trailing newline.
        /// </summary>
        [Theory]
        [InlineData("\n", 7)]
        [InlineData("\r\n", 7)]
        [InlineData("42", 42)]
        public void TextPrompt_PreservesBlankLineDefaultsAndFinalValues(string input, int expected)
        {
            using (UseInput(input))
            {
                TextPromptResult<int> result = new TextPrompt<int>("Number").WithDefault(7).Render();
                Assert.False(result.IsCanceled);
                Assert.Equal(expected, result.Value);
            }
        }

        /// <summary>
        /// StringReader-backed consoles encode surrogate pairs as one Unicode character before decoding.
        /// </summary>
        [Fact]
        public void CreatedConsole_PreservesUnicodeInputAcrossPrompts()
        {
            const string expected = "caf\u00e9 \u6771\u4eac \ud83d\ude80";
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                In = new BoundedReader(expected + "\nnext\n"),
                Out = new AnsiConsoleOutput(new BoundedWriter()),
                Ansi = AnsiSupport.No
            });

            Assert.Equal(expected, console.Prompt(new TextPrompt<string>("Name")).Value);
            Assert.Equal("next", console.Prompt(new TextPrompt<string>("Next")).Value);
            Assert.True(console.Prompt(new TextPrompt<string>("End")).IsCanceled);
        }

        /// <summary>
        /// Secret key input preserves a final unterminated response and still distinguishes EOF afterward.
        /// </summary>
        [Fact]
        public void SecretPrompt_FinalUnterminatedValueIsAccepted()
        {
            using (UseInput("secret"))
            {
                Assert.Equal("secret", new SecretPrompt("Token").Render().Value);
                Assert.True(new SecretPrompt("Next token").Render().IsCanceled);
            }
        }

        /// <summary>
        /// Table navigation exits on exhausted keys without confirming the currently highlighted row.
        /// </summary>
        [Fact]
        public void TableInput_EndOfInputCancelsSelection()
        {
            using (UseInput(string.Empty))
            using (var table = new TableView())
            {
                table.AddRow(new[] { "one" });
                Assert.True(table.Select(clearConsole: false).IsCanceled);
                Assert.Equal(TableCommand.Exit, new TableInputHandler().GetCommand());
            }
        }

        /// <summary>
        /// Legacy retry helpers terminate for both exhausted input and pre-requested cancellation.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LegacyHelpers_EndOrCancellationReturnsWithoutRetry(bool cancel)
        {
            using (UseInput(string.Empty))
            {
                var token = new CancellationToken(cancel);
                Assert.Null(Consoul.Input("Name", cancellationToken: token));
                Assert.Null(Consoul.Input("Number", typeof(int), cancellationToken: token));
                Assert.Equal(0, Consoul.Input<int>("Number", cancellationToken: token));
                Assert.False(Consoul.Ask("Continue?", allowEmpty: true, defaultIsNo: false, cancellationToken: token));
                Assert.Null(Consoul.PromptForFilepath("Path", false, token));
            }
        }

        /// <summary>
        /// A canceled path edit reports failure and leaves the existing model value intact.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FilePathEditor_EndOrCancellationPreservesExistingPath(bool cancel)
        {
            using (UseInput(string.Empty))
            {
                var model = new InputModel();
                var property = typeof(InputModel).GetProperty(nameof(InputModel.FilePath))!;
                var context = new PropertyEditContext(model, property, new PropertyDocumentation(property), model.FilePath, new CancellationToken(cancel));

                Assert.False(new FilePathPropertyEditor().TryEdit(context, out _));
                EditProperty(model, nameof(InputModel.FilePath), context.CancellationToken);

                Assert.Equal("existing.txt", model.FilePath);
                Assert.Equal("existing.txt", context.CurrentValue);
            }
        }

        /// <summary>
        /// A pre-canceled key confirmation does not wait for or consume a key.
        /// </summary>
        [Fact]
        public void ConfirmationPrompt_PreCanceledKeyReadDoesNotReadInput()
        {
            var driver = new TextWriterConsoleDriver(new AnsiConsoleOutput(TextWriter.Null), new AnsiConsoleSettings
            {
                In = new UnexpectedReadReader(),
                Ansi = AnsiSupport.No
            });
            using (Consoul.UseConsoleDriver(driver))
            {
                Assert.True(new ConfirmationPrompt("Continue?") { RequireEnter = false }
                    .Render(new CancellationToken(true)).IsCanceled);
            }
        }

        /// <summary>
        /// Input exhaustion during either an add or replacement preserves string and nonnullable collection values.
        /// </summary>
        [Theory]
        [InlineData(nameof(InputModel.Names), "1\n")]
        [InlineData(nameof(InputModel.Names), "2\n")]
        [InlineData(nameof(InputModel.Counts), "1\n")]
        [InlineData(nameof(InputModel.Counts), "2\n")]
        public void CollectionEditor_EndOfInputPreservesItems(string propertyName, string input)
        {
            using (UseInput(input))
            {
                var model = new InputModel();
                EditProperty(model, propertyName);

                Assert.Equal(new[] { "keep" }, model.Names);
                Assert.Equal(new[] { 42 }, model.Counts);
            }
        }

        /// <summary>
        /// Exhausting a dictionary value edit retains declared and runtime-typed values instead of assigning null.
        /// </summary>
        [Theory]
        [InlineData(nameof(InputModel.NamesByKey))]
        [InlineData(nameof(InputModel.CountsByKey))]
        [InlineData(nameof(InputModel.ObjectsByKey))]
        public void DictionaryEditor_EndOfInputPreservesValues(string propertyName)
        {
            using (UseInput("1\n"))
            {
                var model = new InputModel();
                EditProperty(model, propertyName);

                Assert.Equal("keep", Assert.Single(model.NamesByKey).Value);
                Assert.Equal(42, Assert.Single(model.CountsByKey).Value);
                Assert.Equal(42, Assert.Single(model.ObjectsByKey).Value);
            }
        }

        private static void EditProperty(object model, string propertyName, CancellationToken cancellationToken = default)
        {
            var property = model.GetType().GetProperty(propertyName)!;
            var descriptor = new EditablePropertyDescriptor(property, new PropertyDocumentation(property));
            var method = typeof(EditObjectView).GetMethod("EditDescriptorUsingDefaultEditor", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(new EditObjectView(model), new object[] { descriptor, cancellationToken });
        }

        private static IDisposable UseInput(string input)
        {
            var output = new AnsiConsoleOutput(new BoundedWriter());
            return Consoul.UseConsoleDriver(new TextWriterConsoleDriver(output, new AnsiConsoleSettings
            {
                In = new BoundedReader(input),
                Ansi = AnsiSupport.No
            }));
        }

        private sealed class BoundedReader : StringReader
        {
            private readonly int _limit;
            private int _readCount;

            public BoundedReader(string input) : base(input)
            {
                _limit = input.Length + 16;
            }

            public override int Read()
            {
                if (++_readCount > _limit)
                {
                    throw new InvalidOperationException("The prompt retried exhausted input instead of terminating.");
                }

                return base.Read();
            }
        }

        private sealed class UnexpectedReadReader : TextReader
        {
            public override int Read()
            {
                throw new InvalidOperationException("Canceled prompt must not read input.");
            }
        }

        private sealed class InputModel
        {
            public string FilePath { get; set; } = "existing.txt";

            public List<string> Names { get; set; } = new List<string> { "keep" };

            public List<int> Counts { get; set; } = new List<int> { 42 };

            public Dictionary<string, string> NamesByKey { get; set; } = new Dictionary<string, string> { ["one"] = "keep" };

            public Dictionary<string, int> CountsByKey { get; set; } = new Dictionary<string, int> { ["one"] = 42 };

            public Dictionary<string, object> ObjectsByKey { get; set; } = new Dictionary<string, object> { ["one"] = 42 };
        }

        private sealed class BoundedWriter : StringWriter
        {
            public override void Write(string? value)
            {
                CheckLimit();
                base.Write(value);
            }

            public override void WriteLine(string? value)
            {
                CheckLimit();
                base.WriteLine(value);
            }

            private void CheckLimit()
            {
                if (GetStringBuilder().Length > 8192)
                {
                    throw new InvalidOperationException("The prompt kept writing after input ended or was canceled.");
                }
            }
        }
    }
}
