using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using ConsoulLibrary.Color;
using ConsoulLibrary.Views.Editing;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for model validation summaries.
    /// </summary>
    public class ValidationSummaryRenderingTests
    {
        /// <summary>
        /// Validation summaries render DataAnnotations-style issues with model metadata and stay bounded.
        /// </summary>
        [Fact]
        public void ValidationSummary_InvalidModel_RendersMetadataBackedIssues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = string.Empty,
                    RetryCount = 8,
                    Secret = string.Empty,
                    InternalNote = string.Empty
                };

                ValidationSummaryResult result = Consoul.ValidationSummary(
                    model,
                    new ValidationSummaryOptions { Width = 50, InvalidTitle = "Settings" });

                Assert.False(result.IsValid);
                Assert.Equal(2, result.Count);
                Assert.Contains(result.Issues, issue => issue.PropertyName == nameof(DeploymentSettings.ServiceName));
                Assert.Contains(result.Issues, issue => issue.PropertyName == nameof(DeploymentSettings.RetryCount));
                VisualAssert.WroteText(scope.Driver, "Settings");
                VisualAssert.WroteText(scope.Driver, "Service");
                VisualAssert.WroteText(scope.Driver, "Service is required.");
                VisualAssert.WroteText(scope.Driver, "Retry count");
                VisualAssert.WroteText(scope.Driver, "Retry count must be between 0 and 3.");
                Assert.DoesNotContain("Secret", scope.Driver.Text);
                Assert.DoesNotContain("Secret is required.", scope.Driver.Text);
                Assert.DoesNotContain("Internal note", scope.Driver.Text);
                Assert.DoesNotContain("Internal note is required.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model validation can run without rendering output so controller-style actions can branch on it.
        /// </summary>
        [Fact]
        public void ValidateModel_ReturnsIssuesWithoutRendering()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = string.Empty,
                    RetryCount = 1
                };

                ValidationSummaryResult result = Consoul.ValidateModel(model);

                Assert.False(result.IsValid);
                Assert.Single(result.Issues);
                Assert.Equal("Service", result.Issues[0].DisplayName);
                Assert.Equal(string.Empty, scope.Driver.Text);
            }
        }

        /// <summary>
        /// Model validation includes object-level <see cref="IValidatableObject"/> issues.
        /// </summary>
        [Fact]
        public void ValidateModel_IncludesObjectLevelValidationIssues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var model = new CrossFieldSettings
                {
                    Target = "prod",
                    ConfirmTarget = "staging"
                };

                ValidationSummaryResult result = Consoul.ValidateModel(model);

                Assert.False(result.IsValid);
                Assert.Single(result.Issues);
                Assert.Equal(nameof(CrossFieldSettings.ConfirmTarget), result.Issues[0].PropertyName);
                Assert.Equal("Confirm target", result.Issues[0].DisplayName);
                Assert.Equal("Targets must match.", result.Issues[0].Message);
                Assert.Equal(string.Empty, scope.Driver.Text);
            }
        }

        /// <summary>
        /// Model validation includes context-aware property validation attributes.
        /// </summary>
        [Fact]
        public void ValidateModel_IncludesContextAwarePropertyValidation()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var model = new PasswordSettings
                {
                    Password = "secret",
                    ConfirmPassword = "different"
                };

                ValidationSummaryResult result = Consoul.ValidateModel(model);

                Assert.False(result.IsValid);
                Assert.Single(result.Issues);
                Assert.Equal(nameof(PasswordSettings.ConfirmPassword), result.Issues[0].PropertyName);
                Assert.Equal("Confirm password", result.Issues[0].DisplayName);
                Assert.Equal("Passwords must match.", result.Issues[0].Message);
                Assert.Equal(string.Empty, scope.Driver.Text);
            }
        }

        /// <summary>
        /// Validation summaries can append ad hoc controller-rule issues to model validation.
        /// </summary>
        [Fact]
        public void ValidationSummaryEntry_AddIssues_AppendsControllerRuleIssues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = string.Empty,
                    RetryCount = 2
                };

                ValidationSummaryResult result = new ValidationSummaryEntry(
                    model,
                    new ValidationSummaryOptions
                    {
                        Width = 56,
                        InvalidTitle = "Before Save"
                    })
                    .IssueColor("yellow")
                    .AddIssue("Approval", "Approval", "Owner approval is required.")
                    .AddIssues(new ValidationIssue("ReleaseWindow", "Release window", "Release window has closed."))
                    .Render();

                Assert.False(result.IsValid);
                Assert.Equal(3, result.Count);
                Assert.Equal(2, result.Issues.Count(issue => issue.PropertyName != nameof(DeploymentSettings.ServiceName)));
                VisualAssert.WroteText(scope.Driver, "Before Save");
                VisualAssert.WroteText(scope.Driver, "Service is required.");
                VisualAssert.WroteText(scope.Driver, "Approval");
                VisualAssert.WroteText(scope.Driver, "Owner approval is required.");
                VisualAssert.WroteText(scope.Driver, "Release window");
                VisualAssert.WroteText(scope.Driver, "Release window has closed.");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Owner approval") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model validation honors display order and auto-generation metadata.
        /// </summary>
        [Fact]
        public void ValidateModel_HonorsDisplayOrderAndAutoGenerationMetadata()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var model = new OrderedValidationSettings();

                ValidationSummaryResult result = Consoul.ValidateModel(model);

                Assert.False(result.IsValid);
                Assert.Equal(3, result.Count);
                Assert.Equal(
                    new[] { "First", "Second", "Third" },
                    result.Issues.Select(issue => issue.DisplayName).ToArray());
                Assert.DoesNotContain(result.Issues, issue => issue.PropertyName == nameof(OrderedValidationSettings.Hidden));
                Assert.Equal(string.Empty, scope.Driver.Text);
            }
        }

        /// <summary>
        /// Valid model summaries can optionally render a success callout.
        /// </summary>
        [Fact]
        public void ValidationSummary_ValidModel_CanRenderSuccessMessage()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = "Worker",
                    RetryCount = 2
                };

                ValidationSummaryResult result = Consoul.ValidationSummary(
                    model,
                    new ValidationSummaryOptions
                    {
                        Width = 42,
                        ShowValidMessage = true,
                        ValidTitle = "Ready",
                        ValidMessage = "Settings look good."
                    });

                Assert.True(result.IsValid);
                Assert.Empty(result.Issues);
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Settings look good.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Validation summary entries can be reused as renderable components inside captured layouts.
        /// </summary>
        [Fact]
        public void ValidationSummaryEntry_Renderable_CanBeCapturedAndComposed()
        {
            var model = new DeploymentSettings
            {
                ServiceName = string.Empty,
                RetryCount = 9
            };
            IConsoulRenderable renderable = new ValidationSummaryEntry(
                model,
                new ValidationSummaryOptions { Width = 44, InvalidTitle = "Validation" });

            string captured = Consoul.Capture(renderable, width: 48);

            Assert.Contains("Validation", captured);
            Assert.Contains("Service is required.", captured);
            Assert.Contains("Retry count", captured);

            using (var scope = new ConsoleDriverScope(56))
            {
                Consoul.Panel(renderable, title: "Before Save", width: 54, captureWidth: 48);

                VisualAssert.WroteText(scope.Driver, "Before Save");
                VisualAssert.WroteText(scope.Driver, "Validation");
                VisualAssert.WroteText(scope.Driver, "Service is required.");
                VisualAssert.WroteText(scope.Driver, "Retry count");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Validation summary options can style invalid chrome and issue content for MVC-style view sections.
        /// </summary>
        [Fact]
        public void ValidationSummary_StyleOptions_ColorInvalidChromeAndIssueContent()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = string.Empty,
                    RetryCount = 9
                };

                ValidationSummaryOptions options = new ValidationSummaryOptions
                {
                    Width = 56,
                    InvalidTitle = "Review"
                }
                    .InvalidColor(ConsoleColor.White, ConsoleColor.DarkRed)
                    .IssueColor(new Style(ConsoleColor.Yellow, ConsoleColor.DarkBlue));

                ValidationSummaryResult result = Consoul.ValidationSummary(model, options);

                Assert.False(result.IsValid);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Review") &&
                    write.Color == ConsoleColor.White &&
                    write.BackgroundColor == ConsoleColor.DarkRed);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Service is required.") &&
                    write.Color == ConsoleColor.Yellow &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Validation summary options accept shared palette and RGB colors.
        /// </summary>
        [Fact]
        public void ValidationSummary_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = string.Empty,
                    RetryCount = 9
                };

                ValidationSummaryOptions options = new ValidationSummaryOptions
                {
                    Width = 56,
                    InvalidTitle = "Palette Review"
                }
                    .InvalidColor(SpectreColor.White)
                    .IssueColor(RgbColor.Yellow);

                ValidationSummaryResult result = Consoul.ValidationSummary(model, options);

                Assert.False(result.IsValid);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Palette Review") &&
                    write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Service is required.") &&
                    write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid validation summary style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void ValidationSummary_InvalidStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            var model = new DeploymentSettings
            {
                ServiceName = string.Empty,
                RetryCount = 9
            };
            ValidationSummaryOptions options = new ValidationSummaryOptions
            {
                Width = 56,
                InvalidTitle = "Review"
            }
                .InvalidColor("bold #22c55e")
                .IssueColor("underline #336699");

            console.Write(new ValidationSummaryEntry(model, options));

            string output = writer.ToString();
            Assert.Contains("Review", output);
            Assert.Contains("\u001b[1;38;2;34;197;94m+", output);
            Assert.Contains("\u001b[4;38;2;51;102;153m", output);
            Assert.Contains("Service is required.", output);
        }

        /// <summary>
        /// Valid validation summary style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void ValidationSummary_ValidStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            var model = new DeploymentSettings
            {
                ServiceName = "Worker",
                RetryCount = 2
            };
            ValidationSummaryOptions options = new ValidationSummaryOptions
            {
                Width = 48,
                ShowValidMessage = true,
                ValidTitle = "Ready",
                ValidMessage = "Settings look good."
            }
                .ValidColor("bold #22c55e")
                .ValidMessageColor("underline #336699");

            console.Write(new ValidationSummaryEntry(model, options));

            string output = writer.ToString();
            Assert.Contains("Ready", output);
            Assert.Contains("\u001b[1;38;2;34;197;94m+", output);
            Assert.Contains("\u001b[4;38;2;51;102;153mSettings look good.", output);
        }

        /// <summary>
        /// Validation summary RGB helpers use 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void ValidationSummary_RgbHelpersEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });
            var model = new DeploymentSettings
            {
                ServiceName = string.Empty,
                RetryCount = 9
            };
            ValidationSummaryOptions options = new ValidationSummaryOptions
            {
                Width = 56,
                InvalidTitle = "Review"
            }
                .InvalidColor(RgbColor.FromHex("#445566"))
                .IssueColor(RgbColor.FromHex("#336699"));

            console.Write(new ValidationSummaryEntry(model, options));

            string output = writer.ToString();
            Assert.Contains("\u001b[38;5;239m+", output);
            Assert.Contains("\u001b[38;5;60m", output);
            Assert.Contains("Service is required.", output);
            Assert.DoesNotContain("\u001b[38;2", output);
        }

        /// <summary>
        /// Validation summary entries expose fluent style helpers for quick ad hoc POC summaries.
        /// </summary>
        [Fact]
        public void ValidationSummaryEntry_FluentStyleHelpers_ColorValidSummary()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = "Worker",
                    RetryCount = 2
                };

                ValidationSummaryResult result = new ValidationSummaryEntry(model)
                    .Width(42)
                    .ValidTitle("Ready")
                    .ValidMessage("Settings look good.")
                    .ShowValidMessage()
                    .ValidColor("green")
                    .ValidMessageColor("cyan")
                    .Render();

                Assert.True(result.IsValid);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Ready") &&
                    write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Settings look good.") &&
                    write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Validation summary entries expose shared palette helpers for ad hoc valid summaries.
        /// </summary>
        [Fact]
        public void ValidationSummaryEntry_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var model = new DeploymentSettings
                {
                    ServiceName = "Worker",
                    RetryCount = 2
                };

                ValidationSummaryResult result = new ValidationSummaryEntry(model)
                    .Width(42)
                    .ValidTitle("Palette Ready")
                    .ValidMessage("Palette looks good.")
                    .ShowValidMessage()
                    .ValidColor(RgbColor.Green)
                    .ValidMessageColor(SpectreColor.Cyan)
                    .Render();

                Assert.True(result.IsValid);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Palette Ready") &&
                    write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Palette looks good.") &&
                    write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class DeploymentSettings
        {
            [Display(Name = "Service")]
            [Required(ErrorMessage = "Service is required.")]
            public string ServiceName { get; set; } = string.Empty;

            [DisplayName("Retry count")]
            [Range(0, 3, ErrorMessage = "Retry count must be between 0 and 3.")]
            public int RetryCount { get; set; }

            [PropertyEditorIgnore]
            [Required(ErrorMessage = "Secret is required.")]
            public string Secret { get; set; } = string.Empty;

            [Browsable(false)]
            [Required(ErrorMessage = "Internal note is required.")]
            public string InternalNote { get; set; } = string.Empty;
        }

        private sealed class CrossFieldSettings : IValidatableObject
        {
            [Display(Name = "Target")]
            public string Target { get; set; } = string.Empty;

            [Display(Name = "Confirm target")]
            public string ConfirmTarget { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!string.Equals(Target, ConfirmTarget, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        "Targets must match.",
                        new[] { nameof(ConfirmTarget) });
                }
            }
        }

        private sealed class PasswordSettings
        {
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Confirm password")]
            [Compare(nameof(Password), ErrorMessage = "Passwords must match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        private sealed class OrderedValidationSettings
        {
            [Display(Name = "Third", Order = 30)]
            [Required(ErrorMessage = "Third is required.")]
            public string Third { get; set; } = string.Empty;

            [Display(Name = "First", Order = 10)]
            [Required(ErrorMessage = "First is required.")]
            public string First { get; set; } = string.Empty;

            [Display(Name = "Second", Order = 20)]
            [Required(ErrorMessage = "Second is required.")]
            public string Second { get; set; } = string.Empty;

            [Display(AutoGenerateField = false)]
            [Required(ErrorMessage = "Hidden is required.")]
            public string Hidden { get; set; } = string.Empty;
        }
    }
}
