using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using ConsoulLibrary.Views.Editing;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for property-list view helpers.
    /// </summary>
    public class PropertyListRenderingTests
    {
        /// <summary>
        /// Property lists wrap long values and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        [InlineData(80)]
        public void PropertyList_LongLabelsAndValues_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.PropertyList(list =>
                {
                    list.Add("Environment", "Production", ConsoleColor.Cyan);
                    list.Add("Deployment Owner", "Platform tooling with verylongtokenwithoutnaturalbreaks");
                    list.Add("Empty", string.Empty);
                }, new PropertyListOptions { Width = width, LabelWidth = 12 });

                VisualAssert.WroteText(scope.Driver, "Environment");
                VisualAssert.WroteText(scope.Driver, "Production".Substring(0, Math.Min(3, Math.Max(1, width - 1))));
                VisualAssert.WroteText(scope.Driver, "Empty");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property lists honor configured separators, width, and color schemes.
        /// </summary>
        [Fact]
        public void PropertyList_CustomOptions_RenderColorsAndWrappedValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new PropertyListOptions
                {
                    Width = 34,
                    LabelWidth = 10,
                    Separator = " = ",
                    LabelScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    SeparatorScheme = new ColorScheme
                    {
                        Color = ConsoleColor.DarkGray,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ValueScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.PropertyList(list =>
                {
                    list.Add("Status", "Ready for release", ConsoleColor.Green);
                    list.Add("Owner", "Platform");
                }, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Owner") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Platform") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 34, "Property-list line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property lists measure raw markup by visible text so styled labels and values stay responsive.
        /// </summary>
        /// <param name="width">The configured property-list width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void PropertyList_RawMarkupLabelsAndValues_WrapByVisibleTextAndDoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width + 4))
            {
                Consoul.PropertyList(list =>
                {
                    list.Add("[cyan]Service[/]", "[green]Worker[/]");
                    list.Add(
                        "[yellow]ApprovalClusterWithVeryLongNameWithoutNaturalBreaks[/]",
                        "[white on navy]Owner approval[/] required for [[prod]]");
                }, new PropertyListOptions
                {
                    Width = width,
                    LabelWidth = Math.Min(12, Math.Max(6, width / 3)),
                    LabelScheme = new ColorScheme { Color = ConsoleColor.White },
                    ValueScheme = new ColorScheme { Color = ConsoleColor.DarkGray }
                });

                VisualAssert.WroteText(scope.Driver, "Service");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.WroteText(scope.Driver, "Approval");
                VisualAssert.WroteText(scope.Driver, "Owner");
                VisualAssert.WroteText(scope.Driver, "approval");
                VisualAssert.WroteText(scope.Driver, "[prod]");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Worker") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Owner") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[prod]") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Property-list line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property list item and model-summary helpers accept Spectre-style style strings.
        /// </summary>
        [Fact]
        public void PropertyList_StyleStringHelpers_ColorizeItemsAndModelValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var model = new DashboardModel
                {
                    Status = "Green",
                    Jobs = 4,
                    Owner = "Platform"
                };

                Consoul.PropertyList(list =>
                {
                    list.Add("Environment", "Production", "white on navy", "grey");
                    list.AddModel(model, "#22c55e", "yellow");
                }, new PropertyListOptions { Width = 44, LabelWidth = 14 });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Environment") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Green") && write.Color == ConsoleColor.DarkCyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property-list style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void PropertyList_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            Style defaultLabel = Style.Parse("bold #22c55e");
            Style defaultValue = Style.Parse("#778899");
            var entry = new PropertyListEntry(new PropertyListOptions
            {
                Width = 60,
                LabelWidth = 14,
                LabelScheme = defaultLabel.ToColorScheme(RenderOptions.DefaultScheme),
                LabelDecoration = defaultLabel.Decoration,
                LabelForegroundRgb = defaultLabel.ForegroundRgb,
                LabelBackgroundRgb = defaultLabel.BackgroundRgb,
                ValueScheme = defaultValue.ToColorScheme(RenderOptions.DefaultScheme),
                ValueDecoration = defaultValue.Decoration,
                ValueForegroundRgb = defaultValue.ForegroundRgb,
                ValueBackgroundRgb = defaultValue.BackgroundRgb
            });

            entry.Add("Default", "Inherited");
            entry.Add("Build", "Ready", "underline #112233", "#336699");

            console.Write(entry);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94mDefault", output);
            Assert.Contains("\u001b[38;2;119;136;153mInherited", output);
            Assert.Contains("\u001b[38;2;51;102;153mBuild", output);
            Assert.Contains("\u001b[4;38;2;17;34;51mReady", output);
        }

        /// <summary>
        /// Property-list RGB selectors use 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void PropertyList_RgbSelectorsEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });

            var items = new[]
            {
                new
                {
                    Label = "API",
                    Value = "Ready",
                    LabelColor = RgbColor.FromHex("#445566"),
                    ValueColor = RgbColor.FromHex("#336699")
                }
            };

            var entry = new PropertyListEntry(new PropertyListOptions { Width = 60, LabelWidth = 14 });
            entry.AddItems(
                items,
                item => item.Label,
                item => item.Value,
                item => item.ValueColor,
                item => item.LabelColor);

            console.Write(entry);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;5;239mAPI", output);
            Assert.Contains("\u001b[38;5;60mReady", output);
            Assert.DoesNotContain("\u001b[38;2", output);
        }

        /// <summary>
        /// Key/value helpers make dictionary-backed settings summaries readable without custom model types.
        /// </summary>
        [Fact]
        public void PropertyList_AddKeyValues_RendersDictionaryEntriesWithStyles()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var settings = new Dictionary<string, object>
                {
                    { "Environment", "Development" },
                    { "Preview", true },
                    { "Retries", 2 }
                };

                Consoul.PropertyList(list =>
                {
                    list.AddKeyValues(settings, "green", "grey");
                }, new PropertyListOptions { Width = 42, LabelWidth = 14 });

                VisualAssert.WroteText(scope.Driver, "Environment");
                VisualAssert.WroteText(scope.Driver, "Development");
                VisualAssert.WroteText(scope.Driver, "Preview");
                VisualAssert.WroteText(scope.Driver, "Yes");
                VisualAssert.WroteText(scope.Driver, "Retries");
                VisualAssert.WroteText(scope.Driver, "2");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Environment") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Development") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property lists can project model collections into labeled values with per-item styles.
        /// </summary>
        [Fact]
        public void PropertyList_AddItems_ProjectsModelCollectionsWithStyles()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var states = new[]
                {
                    new StatusSummary("Environment", "Production", ConsoleColor.Cyan, ConsoleColor.DarkGray, "white on navy", "grey"),
                    new StatusSummary("Approval", "Pending", ConsoleColor.Yellow, ConsoleColor.DarkGray, "yellow", "grey")
                };

                Consoul.PropertyList(list =>
                {
                    list.AddItems(
                        new[] { states[0] },
                        state => state.Label,
                        state => state.Value,
                        state => (ConsoleColor?)state.ValueColor,
                        state => (ConsoleColor?)state.LabelColor);
                    list.AddItemsStyled(
                        new[] { states[1] },
                        state => "Styled " + state.Label,
                        state => state.Value,
                        state => state.ValueStyle,
                        state => state.LabelStyle);
                }, new PropertyListOptions { Width = 50, LabelWidth = 16 });

                VisualAssert.WroteText(scope.Driver, "Environment");
                VisualAssert.WroteText(scope.Driver, "Production");
                VisualAssert.WroteText(scope.Driver, "Styled Approval");
                VisualAssert.WroteText(scope.Driver, "Pending");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Environment") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Approval") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pending") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property lists accept prepared item params for quick MVC state summaries.
        /// </summary>
        [Fact]
        public void PropertyList_AddItems_AcceptsPreparedItemParams()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                PropertyListItem environment = PropertyListItem.Create("Environment", "Production", "white on navy", "grey");
                PropertyListItem approval = PropertyListItem.Create("Approval", "Pending", "yellow", "grey");

                var entry = new PropertyListEntry(new PropertyListOptions { Width = 42, LabelWidth = 14 })
                    .AddItems(environment, approval);

                AnsiConsole.Write(entry);

                Assert.Equal(2, entry.Items.Count);
                VisualAssert.WroteText(scope.Driver, "Environment");
                VisualAssert.WroteText(scope.Driver, "Production");
                VisualAssert.WroteText(scope.Driver, "Approval");
                VisualAssert.WroteText(scope.Driver, "Pending");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Environment") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pending") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Non-generic dictionaries can still be rendered as property lists for legacy settings bags.
        /// </summary>
        [Fact]
        public void PropertyList_AddDictionary_RendersNonGenericDictionaryEntries()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var values = new Hashtable
                {
                    { "Queue", "active" },
                    { "Workers", 4 }
                };

                Consoul.PropertyList(list =>
                {
                    list.AddDictionary(values, ConsoleColor.Cyan, ConsoleColor.Yellow);
                }, new PropertyListOptions { Width = 36, LabelWidth = 10 });

                VisualAssert.WroteText(scope.Driver, "Queue");
                VisualAssert.WroteText(scope.Driver, "active");
                VisualAssert.WroteText(scope.Driver, "Workers");
                VisualAssert.WroteText(scope.Driver, "4");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queue") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("active") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property lists fall back to stacked rendering when the terminal cannot fit a label/value layout.
        /// </summary>
        [Fact]
        public void PropertyList_NarrowWidth_StacksWithoutOverflow()
        {
            using (var scope = new ConsoleDriverScope(2))
            {
                Consoul.PropertyList(list =>
                {
                    list.Add("Label", "Value");
                }, new PropertyListOptions { Width = 2, LabelWidth = 20 });

                VisualAssert.WroteText(scope.Driver, "L");
                VisualAssert.WroteText(scope.Driver, "V");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed property lists use form metadata, readable names, and editor ignore attributes.
        /// </summary>
        [Fact]
        public void PropertyList_ModelSummary_UsesMetadataAndSkipsIgnoredProperties()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var model = new MetadataDashboardModel
                {
                    ServiceName = "Deploy",
                    Status = "Ready",
                    EnablePreviewMode = true,
                    Jobs = new List<string> { "Build", "Test" },
                    Secret = "do-not-render",
                    InternalNote = "hidden-note"
                };

                Consoul.PropertyList(model, new PropertyListOptions { Width = 48, LabelWidth = 16 });

                VisualAssert.WroteText(scope.Driver, "Service");
                VisualAssert.WroteText(scope.Driver, "Deploy");
                VisualAssert.WroteText(scope.Driver, "Current state");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Enable Preview");
                VisualAssert.WroteText(scope.Driver, "Yes");
                VisualAssert.WroteText(scope.Driver, "2 items");
                Assert.DoesNotContain("Secret", scope.Driver.Text);
                Assert.DoesNotContain("do-not-render", scope.Driver.Text);
                Assert.DoesNotContain("Internal Note", scope.Driver.Text);
                Assert.DoesNotContain("hidden-note", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model summaries honor display order and auto-generation metadata.
        /// </summary>
        [Fact]
        public void PropertyList_ModelSummary_HonorsDisplayOrderAndAutoGenerationMetadata()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var model = new OrderedMetadataDashboardModel
                {
                    Third = "third",
                    First = "first",
                    Second = "second",
                    Hidden = "hidden"
                };

                Consoul.PropertyList(model, new PropertyListOptions { Width = 48, LabelWidth = 10 });

                string text = scope.Driver.Text;
                Assert.True(text.IndexOf("First", StringComparison.Ordinal) < text.IndexOf("Second", StringComparison.Ordinal));
                Assert.True(text.IndexOf("Second", StringComparison.Ordinal) < text.IndexOf("Third", StringComparison.Ordinal));
                Assert.DoesNotContain("Hidden", text);
                Assert.DoesNotContain("hidden", text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Property list entries can add model summaries inside composed render helpers.
        /// </summary>
        [Fact]
        public void PropertyListEntry_AddModel_ComposesInsidePanels()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var model = new DashboardModel
                {
                    Status = "Green",
                    Jobs = 4,
                    Owner = "Platform"
                };

                Consoul.Panel(
                    () => Consoul.PropertyList(list => list.AddModel(model), new PropertyListOptions { Width = 36, LabelWidth = 8 }),
                    title: "Model",
                    width: 44);

                VisualAssert.WroteText(scope.Driver, "Model");
                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.WroteText(scope.Driver, "Green");
                VisualAssert.WroteText(scope.Driver, "Owner");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render property lists as compact model summaries.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderPropertyListSections()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DashboardModel
                    {
                        Status = "Green",
                        Jobs = 4,
                        Owner = "Platform"
                    };

                    var view = Consoul.View("Dashboard", model)
                        .RenderModel(dashboard =>
                        {
                            Consoul.PropertyList(list =>
                            {
                                list.Add("Status", dashboard.Status, ConsoleColor.Green);
                                list.Add("Jobs", dashboard.Jobs, ConsoleColor.Cyan);
                                list.Add("Owner", dashboard.Owner, ConsoleColor.Yellow);
                            }, new PropertyListOptions { Width = 36, LabelWidth = 8 });
                        })
                        .Option("Refresh", dashboard => dashboard.Jobs++);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Green");
                    VisualAssert.WroteText(scope.Driver, "Owner");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class DashboardModel
        {
            public string Status { get; set; } = string.Empty;

            public int Jobs { get; set; }

            public string Owner { get; set; } = string.Empty;
        }

        private sealed class StatusSummary
        {
            public StatusSummary(string label, string value, ConsoleColor valueColor, ConsoleColor labelColor, string valueStyle, string labelStyle)
            {
                Label = label;
                Value = value;
                ValueColor = valueColor;
                LabelColor = labelColor;
                ValueStyle = valueStyle;
                LabelStyle = labelStyle;
            }

            public string Label { get; }

            public string Value { get; }

            public ConsoleColor ValueColor { get; }

            public ConsoleColor LabelColor { get; }

            public string ValueStyle { get; }

            public string LabelStyle { get; }
        }

        private sealed class MetadataDashboardModel
        {
            [Display(Name = "Service")]
            public string ServiceName { get; set; } = string.Empty;

            [Description("Current state")]
            public string Status { get; set; } = string.Empty;

            public bool EnablePreviewMode { get; set; }

            public List<string> Jobs { get; set; } = new List<string>();

            [PropertyEditorIgnore]
            public string Secret { get; set; } = string.Empty;

            [Browsable(false)]
            public string InternalNote { get; set; } = string.Empty;
        }

        private sealed class OrderedMetadataDashboardModel
        {
            [Display(Order = 30)]
            public string Third { get; set; } = string.Empty;

            [Display(Order = 10)]
            public string First { get; set; } = string.Empty;

            [Display(Order = 20)]
            public string Second { get; set; } = string.Empty;

            [Display(AutoGenerateField = false)]
            public string Hidden { get; set; } = string.Empty;
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
