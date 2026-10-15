using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Tests theme application across render options and markup helpers.
    /// </summary>
    public class ThemeRenderingTests
    {
        /// <summary>
        /// Theme presets return independent instances so callers can safely customize them.
        /// </summary>
        [Fact]
        public void ThemePresets_ReturnIndependentInstances()
        {
            ConsoulTheme modern = ConsoulTheme.Modern;
            modern.PromptScheme = Scheme(ConsoleColor.Red, ConsoleColor.Black);

            Assert.Equal(ConsoleColor.Cyan, ConsoulTheme.Modern.PromptScheme.Color);
        }

        /// <summary>
        /// Applying a theme updates render options and named markup schemes.
        /// </summary>
        [Fact]
        public void ApplyTheme_UpdatesRenderOptionsAndNamedMarkupSchemes()
        {
            using (RenderOptions.UseTheme(RenderOptions.CurrentTheme))
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.ApplyTheme(ConsoulTheme.Modern);

                Consoul.MarkupLine("[prompt]Question[/] [subnote]hint[/] [invalid]error[/] [option]choice[/] [routine]input[/]");

                Assert.Equal(ConsoleColor.Cyan, RenderOptions.PromptColor);
                Assert.Equal(ConsoleColor.DarkGray, RenderOptions.SubnoteColor);
                Assert.Equal(ConsoleColor.Green, RenderOptions.OptionColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Question") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("hint") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("error") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("choice") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("input") && write.Color == ConsoleColor.Magenta);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Scoped theme application restores the previous render options when disposed.
        /// </summary>
        [Fact]
        public void UseTheme_RestoresPreviousRenderOptionsWhenDisposed()
        {
            using (RenderOptions.UseTheme(RenderOptions.CurrentTheme))
            {
                ConsoulTheme baseline = ConsoulTheme.Classic;
                RenderOptions.ApplyTheme(baseline);

                using (RenderOptions.UseTheme(ConsoulTheme.HighContrast))
                {
                    Assert.Equal(ConsoleColor.Black, RenderOptions.PromptColor);
                    Assert.Equal(ConsoleColor.White, RenderOptions.PromptScheme.BackgroundColor);
                    Assert.Equal(ConsoleColor.Yellow, RenderOptions.OptionScheme.BackgroundColor);
                }

                AssertThemeEqual(baseline, RenderOptions.CurrentTheme);
            }
        }

        /// <summary>
        /// Theme instances can be applied directly and still restore through their scope helper.
        /// </summary>
        [Fact]
        public void ThemeApplyScoped_RestoresPreviousRenderOptionsWhenDisposed()
        {
            using (RenderOptions.UseTheme(RenderOptions.CurrentTheme))
            {
                RenderOptions.ApplyTheme(ConsoulTheme.Monochrome);
                ConsoulTheme before = RenderOptions.CurrentTheme;

                using (ConsoulTheme.Modern.ApplyScoped())
                {
                    Assert.Equal(ConsoleColor.Cyan, RenderOptions.PromptColor);
                    Assert.Equal(ConsoleColor.Green, RenderOptions.OptionColor);
                }

                AssertThemeEqual(before, RenderOptions.CurrentTheme);
            }
        }

        /// <summary>
        /// Scoped render option changes restore global defaults and seed newly created MVC-style views while active.
        /// </summary>
        [Fact]
        public void UseRenderOptions_RestoresScopedRenderOptionsAndSeedsViews()
        {
            using (Consoul.UseRenderOptions(() =>
            {
                RenderOptions.ApplyTheme(ConsoulTheme.Classic);
                RenderOptions.BlacklistColors = new List<ConsoleColor> { ConsoleColor.DarkGray };
                RenderOptions.WriteMode = RenderOptions.WriteModes.WriteAll;
                RenderOptions.DefaultGoBackMessage = "Back";
                RenderOptions.ContinueMessage = "Continue";
                RenderOptions.WaitOnError = false;
                RenderOptions.ViewErrorMode = RenderOptions.ViewErrorModes.RenderAndContinue;
                RenderOptions.ViewPromptKeyboardNavigationEnabled = false;
                RenderOptions.ViewPromptFilterEnabled = false;
                RenderOptions.ViewPromptPageSize = null;
            }))
            {
                ColorScheme baselinePromptScheme = RenderOptions.PromptScheme;
                ColorScheme baselineOptionScheme = RenderOptions.OptionScheme;
                List<ConsoleColor> baselineBlacklistColors = new List<ConsoleColor>(RenderOptions.BlacklistColors);
                RenderOptions.WriteModes baselineWriteMode = RenderOptions.WriteMode;
                string baselineGoBackMessage = RenderOptions.DefaultGoBackMessage;
                string baselineContinueMessage = RenderOptions.ContinueMessage;
                bool baselineWaitOnError = RenderOptions.WaitOnError;
                RenderOptions.ViewErrorModes baselineViewErrorMode = RenderOptions.ViewErrorMode;
                bool baselineKeyboardNavigation = RenderOptions.ViewPromptKeyboardNavigationEnabled;
                bool baselinePromptFilter = RenderOptions.ViewPromptFilterEnabled;
                int? baselinePromptPageSize = RenderOptions.ViewPromptPageSize;

                using (Consoul.UseRenderOptions(() =>
                {
                    RenderOptions.PromptScheme = Scheme(ConsoleColor.Cyan, ConsoleColor.Black);
                    RenderOptions.OptionScheme = Scheme(ConsoleColor.Green, ConsoleColor.Black);
                    RenderOptions.BlacklistColors.Add(ConsoleColor.Red);
                    RenderOptions.WriteMode = RenderOptions.WriteModes.SuppressBlacklist;
                    RenderOptions.DefaultGoBackMessage = "Return";
                    RenderOptions.ContinueMessage = "Continue now";
                    RenderOptions.WaitOnError = true;
                    RenderOptions.ViewErrorMode = RenderOptions.ViewErrorModes.Throw;
                    RenderOptions.ViewPromptKeyboardNavigationEnabled = true;
                    RenderOptions.ViewPromptFilterEnabled = true;
                    RenderOptions.ViewPromptPageSize = 4;
                }))
                {
                    DynamicView<object> view = Consoul.View("Scoped defaults");

                    Assert.Equal(ConsoleColor.Cyan, RenderOptions.PromptColor);
                    Assert.Equal(ConsoleColor.Green, RenderOptions.OptionColor);
                    Assert.Contains(ConsoleColor.Red, RenderOptions.BlacklistColors);
                    Assert.Equal(RenderOptions.WriteModes.SuppressBlacklist, RenderOptions.WriteMode);
                    Assert.Equal("Return", RenderOptions.DefaultGoBackMessage);
                    Assert.Equal("Continue now", RenderOptions.ContinueMessage);
                    Assert.True(RenderOptions.WaitOnError);
                    Assert.Equal(RenderOptions.ViewErrorModes.Throw, RenderOptions.ViewErrorMode);
                    Assert.True(view.PromptKeyboardNavigationEnabled);
                    Assert.True(view.PromptFilterEnabled);
                    Assert.Equal(4, view.PromptPageSize);
                }

                AssertSchemeEqual(baselinePromptScheme, RenderOptions.PromptScheme);
                AssertSchemeEqual(baselineOptionScheme, RenderOptions.OptionScheme);
                Assert.Equal(baselineBlacklistColors, RenderOptions.BlacklistColors);
                Assert.Equal(baselineWriteMode, RenderOptions.WriteMode);
                Assert.Equal(baselineGoBackMessage, RenderOptions.DefaultGoBackMessage);
                Assert.Equal(baselineContinueMessage, RenderOptions.ContinueMessage);
                Assert.Equal(baselineWaitOnError, RenderOptions.WaitOnError);
                Assert.Equal(baselineViewErrorMode, RenderOptions.ViewErrorMode);
                Assert.Equal(baselineKeyboardNavigation, RenderOptions.ViewPromptKeyboardNavigationEnabled);
                Assert.Equal(baselinePromptFilter, RenderOptions.ViewPromptFilterEnabled);
                Assert.Equal(baselinePromptPageSize, RenderOptions.ViewPromptPageSize);
            }
        }

        /// <summary>
        /// Scoped render option changes restore immediately when configuration fails.
        /// </summary>
        [Fact]
        public void UseRenderOptions_RestoresPreviousRenderOptionsWhenConfigureThrows()
        {
            using (Consoul.UseRenderOptions(() =>
            {
                RenderOptions.ApplyTheme(ConsoulTheme.Classic);
                RenderOptions.BlacklistColors = new List<ConsoleColor> { ConsoleColor.DarkGray };
                RenderOptions.WriteMode = RenderOptions.WriteModes.WriteAll;
            }))
            {
                ColorScheme baselinePromptScheme = RenderOptions.PromptScheme;
                List<ConsoleColor> baselineBlacklistColors = new List<ConsoleColor>(RenderOptions.BlacklistColors);
                RenderOptions.WriteModes baselineWriteMode = RenderOptions.WriteMode;

                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    Consoul.UseRenderOptions(() =>
                    {
                        RenderOptions.PromptScheme = Scheme(ConsoleColor.Red, ConsoleColor.Black);
                        RenderOptions.BlacklistColors.Add(ConsoleColor.Yellow);
                        RenderOptions.WriteMode = RenderOptions.WriteModes.SuppressAll;
                        throw new InvalidOperationException("Scope setup failed.");
                    }));

                Assert.Equal("Scope setup failed.", exception.Message);
                AssertSchemeEqual(baselinePromptScheme, RenderOptions.PromptScheme);
                Assert.Equal(baselineBlacklistColors, RenderOptions.BlacklistColors);
                Assert.Equal(baselineWriteMode, RenderOptions.WriteMode);
            }
        }

        private static void AssertThemeEqual(ConsoulTheme expected, ConsoulTheme actual)
        {
            AssertSchemeEqual(expected.DefaultScheme, actual.DefaultScheme);
            AssertSchemeEqual(expected.PromptScheme, actual.PromptScheme);
            AssertSchemeEqual(expected.SubnoteScheme, actual.SubnoteScheme);
            AssertSchemeEqual(expected.InvalidScheme, actual.InvalidScheme);
            AssertSchemeEqual(expected.OptionScheme, actual.OptionScheme);
            AssertSchemeEqual(expected.RoutineInputScheme, actual.RoutineInputScheme);
        }

        private static void AssertSchemeEqual(ColorScheme expected, ColorScheme actual)
        {
            Assert.Equal(expected.Color, actual.Color);
            Assert.Equal(expected.BackgroundColor, actual.BackgroundColor);
        }

        private static ColorScheme Scheme(ConsoleColor color, ConsoleColor backgroundColor)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = backgroundColor
            };
        }
    }
}
