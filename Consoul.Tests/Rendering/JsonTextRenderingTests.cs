using System;
using System.Text.Json;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for optional System.Text.Json view helpers.
    /// </summary>
    public class JsonTextRenderingTests
    {
        /// <summary>
        /// Raw JSON text is formatted, colorized, and kept inside the virtual terminal width.
        /// </summary>
        [Fact]
        public void JsonText_RendersFormattedColoredTokensWithoutOverflow()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                ConsoulTextJsonExtensions.Json("{\"name\":\"Consoul\",\"count\":3,\"ready\":true,\"missing\":null}");

                VisualAssert.ContainsText(scope.Driver, "\"name\"");
                VisualAssert.ContainsText(scope.Driver, "\"Consoul\"");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"name\"") && write.Color == RenderOptions.PromptColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"Consoul\"") && write.Color == RenderOptions.OptionColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("true") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style JsonText aliases customize token colors while staying inside the optional JSON package.
        /// </summary>
        [Fact]
        public void JsonText_FluentBuilder_CustomizesTokenColors()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var json = new JsonText("{\"status\":\"ready\",\"count\":3,\"ok\":true,\"missing\":null,\"items\":[1,2]}")
                    .Width(64)
                    .Indentation("..")
                    .MemberColor(Colors.Aqua)
                    .StringStyle(new Style(Decoration.Italic, Colors.Green))
                    .NumberColor(Colors.Blue)
                    .BooleanStyle(new Style(Colors.Yellow))
                    .NullStyle(new Style(Colors.Red))
                    .BracesStyle(new Style(Colors.Magenta))
                    .BracketsStyle(new Style(Colors.DarkMagenta))
                    .ColonStyle(new Style(Colors.Cyan))
                    .CommaStyle(new Style(Colors.Gray));

                AnsiConsole.Write(json);

                VisualAssert.ContainsText(scope.Driver, "\"status\"");
                VisualAssert.ContainsText(scope.Driver, "\"ready\"");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("..\"status\"", StringComparison.Ordinal));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"status\"") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"ready\"") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("true") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("null") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("{") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[") && write.Color == ConsoleColor.DarkMagenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(":") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(",") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style JsonText properties support initializer configuration for reusable renderables.
        /// </summary>
        [Fact]
        public void JsonText_PropertyStyleConfiguration_WorksWithObjectInitializers()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var json = new JsonText("{\"status\":\"ready\",\"items\":[1]}")
                {
                    Indentation = "--",
                    MemberStyle = new Style(Colors.Aqua),
                    StringStyle = new Style(Colors.Green),
                    NumberStyle = new Style(Colors.Blue),
                    BracesStyle = new Style(Colors.Magenta),
                    BracketsStyle = new Style(Colors.DarkMagenta),
                    ColonStyle = new Style(Colors.Cyan),
                    CommaStyle = new Style(Colors.Gray)
                };

                AnsiConsole.Write(json);

                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("--\"status\"", StringComparison.Ordinal));
                VisualAssert.ContainsText(scope.Driver, "\"items\"");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"status\"") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"ready\"") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("1") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("{") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[") && write.Color == ConsoleColor.DarkMagenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(":") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model snapshots can be serialized into the Spectre-style JsonText builder and then configured fluently.
        /// </summary>
        [Fact]
        public void JsonText_FromValue_ReturnsFluentBuilder()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                JsonText json = JsonText
                    .FromValue(
                        new DeploymentStatus { EnvironmentName = "Prod", RetryCount = 2 },
                        serializerOptions: new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                    .Width(40)
                    .MemberColor(Colors.Aqua)
                    .NumberColor(Colors.Blue);

                AnsiConsole.Write(json);

                VisualAssert.ContainsText(scope.Driver, "\"environmentName\"");
                VisualAssert.ContainsText(scope.Driver, "\"retryCount\"");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\"environmentName\"") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.Blue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// JSON value rendering uses System.Text.Json serializer options before Consoul renders tokens.
        /// </summary>
        [Fact]
        public void JsonValue_UsesSerializerOptions()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                ConsoulTextJsonExtensions.JsonValue(
                    new DeploymentStatus { EnvironmentName = "Prod", RetryCount = 2 },
                    serializerOptions: new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                VisualAssert.ContainsText(scope.Driver, "\"environmentName\"");
                VisualAssert.ContainsText(scope.Driver, "\"retryCount\"");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Injected console facades can render optional JSON helpers without adding System.Text.Json to the core package.
        /// </summary>
        [Fact]
        public void IAnsiConsoleJsonExtensions_RenderRawJsonAndValues()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                IAnsiConsole console = AnsiConsole.Console;

                console.Json("{\"status\":\"ready\",\"count\":3}", new JsonTextOptions { Width = 36 });
                console.JsonValue(
                    new DeploymentStatus { EnvironmentName = "Prod", RetryCount = 2 },
                    new JsonTextOptions { Width = 36 },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                VisualAssert.ContainsText(scope.Driver, "\"status\"");
                VisualAssert.ContainsText(scope.Driver, "\"ready\"");
                VisualAssert.ContainsText(scope.Driver, "\"environmentName\"");
                VisualAssert.ContainsText(scope.Driver, "\"retryCount\"");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Injected console JSON helpers validate the console facade before rendering.
        /// </summary>
        [Fact]
        public void IAnsiConsoleJsonExtensions_RejectNullConsole()
        {
            IAnsiConsole console = null!;

            Assert.Throws<ArgumentNullException>(() => console.Json("{}"));
            Assert.Throws<ArgumentNullException>(() => console.JsonValue(new DeploymentStatus()));
        }

        /// <summary>
        /// The form-first System.Text.Json hook and the older object-editor alias configure the same form value serializer.
        /// </summary>
        [Fact]
        public void SystemTextJson_FormValueConfiguration_UsesSharedObjectEditorSerializer()
        {
            var previousSerializer = EditObjectView.FormValueSerializer;

            try
            {
                ConsoulTextJsonExtensions.UseSystemTextJsonForFormValues(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string formJson = EditObjectView.FormValueSerializer(new DeploymentStatus { EnvironmentName = "QA", RetryCount = 4 }, typeof(DeploymentStatus));

                Assert.Contains("\"environmentName\"", formJson);
                Assert.Contains("\"retryCount\"", formJson);

                ConsoulTextJsonExtensions.UseSystemTextJsonForObjectEditor(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

                string aliasJson = EditObjectView.FormValueSerializer(new DeploymentStatus { EnvironmentName = "QA", RetryCount = 4 }, typeof(DeploymentStatus));

                Assert.Contains("\"EnvironmentName\"", aliasJson);
                Assert.Contains("\"RetryCount\"", aliasJson);
            }
            finally
            {
                EditObjectView.FormValueSerializer = previousSerializer;
            }
        }

        /// <summary>
        /// Invalid JSON renders a friendly message and bounded raw content by default.
        /// </summary>
        [Fact]
        public void JsonText_InvalidJson_RendersFriendlyMessage()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                ConsoulTextJsonExtensions.Json("{\"name\":\"unterminated", new JsonTextOptions
                {
                    InvalidJsonMessage = "JSON problem"
                });

                VisualAssert.ContainsText(scope.Driver, "JSON problem");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("JSON problem") && write.Color == RenderOptions.InvalidColor);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid JSON can be configured to throw for callers that want strict parsing.
        /// </summary>
        [Fact]
        public void JsonText_InvalidJson_CanThrow()
        {
            Assert.ThrowsAny<JsonException>(() =>
                ConsoulTextJsonExtensions.Json("{broken", new JsonTextOptions { ThrowOnInvalidJson = true }));
        }

        /// <summary>
        /// JSON text entries can be reused as renderable components in captured layouts.
        /// </summary>
        [Fact]
        public void JsonTextEntry_Renderable_CanBeCapturedAndComposed()
        {
            IConsoulRenderable json = new JsonTextEntry(
                "{\"status\":\"ready\",\"count\":3}",
                new JsonTextOptions { Width = 32 });

            string captured = Consoul.Capture(json, width: 36);

            Assert.Contains("\"status\"", captured);
            Assert.Contains("\"ready\"", captured);
            Assert.Contains("\"count\"", captured);

            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(json, title: "Snapshot", width: 44, captureWidth: 36);

                VisualAssert.ContainsText(scope.Driver, "Snapshot");
                VisualAssert.ContainsText(scope.Driver, "\"status\"");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render JSON snapshots as MVC-style diagnostics.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderJsonTextSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("JSON", new DeploymentStatus { EnvironmentName = "Dev", RetryCount = 1 })
                        .RenderModel(model => JsonTextEntry.FromValue(model, options: new JsonTextOptions { Width = 36 }))
                        .Option("Increment", model => model.RetryCount++);

                    view.Render();

                    VisualAssert.ContainsText(scope.Driver, "\"EnvironmentName\"");
                    VisualAssert.ContainsText(scope.Driver, "\"RetryCount\"");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class DeploymentStatus
        {
            public string EnvironmentName { get; set; } = string.Empty;

            public int RetryCount { get; set; }
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(string[] inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }
    }
}
