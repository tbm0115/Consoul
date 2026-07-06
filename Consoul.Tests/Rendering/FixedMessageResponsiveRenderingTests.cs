using ConsoulLibrary.Tests.Rendering;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for fixed-position messages.
    /// </summary>
    public class FixedMessageResponsiveRenderingTests
    {
        /// <summary>
        /// Long fixed messages wrap within the configured max width and terminal width.
        /// </summary>
        [Theory]
        [InlineData(12)]
        [InlineData(20)]
        [InlineData(40)]
        public void Render_LongMessage_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                using (var message = new FixedMessage(width)
                {
                    Height = 4,
                    MaxWidth = width
                })
                {

                    message.Render("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");

                    VisualAssert.NoOverflow(scope.Driver);
                    Assert.All(scope.Driver.Writes, write => Assert.True(write.Text.Length <= width || write.Text.Contains("\n")));
                }
            }
        }

        /// <summary>
        /// Re-rendering after a resize uses the current terminal width.
        /// </summary>
        [Fact]
        public void Render_AfterResize_UsesCurrentTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (var message = new FixedMessage
                {
                    Height = 3
                })
                {

                    message.Render("A fixed message that initially has plenty of room.");
                    VisualAssert.NoOverflow(scope.Driver);

                    scope.Driver.Resize(16);
                    scope.Driver.Clear();
                    int writesBeforeResizeRender = scope.Driver.Writes.Count;
                    message.Render("A fixed message that now has much less room.");

                    VisualAssert.NoOverflow(scope.Driver);
                    Assert.All(scope.Driver.Writes.Skip(writesBeforeResizeRender), write => Assert.True(write.Text.Length <= 16 || write.Text.Contains("\n")));
                }
            }
        }

        /// <summary>
        /// Re-rendering a fixed message after narrowing the terminal clamps its saved cursor position.
        /// </summary>
        [Fact]
        public void Render_AfterResize_ClampsSavedCursorPosition()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Write(new string(' ', 30), writeLine: false);

                using (var message = new FixedMessage
                {
                    Height = 2,
                    RefreshRate = 0
                })
                {
                    message.Render("Pinned near the old right edge.");
                    scope.Driver.Resize(16);
                    scope.Driver.Clear();
                    int writesBeforeResizeRender = scope.Driver.Writes.Count;

                    message.Render("Pinned after resize.");

                    VisualAssert.NoOverflow(scope.Driver);
                    Assert.All(scope.Driver.Writes.Skip(writesBeforeResizeRender), write => Assert.True(write.Text.Length <= 16 || write.Text.Contains("\n")));
                }
            }
        }

        /// <summary>
        /// Styled fixed messages apply reusable foreground and background styles through the fixed renderer.
        /// </summary>
        [Fact]
        public void RenderStyled_RendersReusableStyle()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                using (var message = new FixedMessage
                {
                    Height = 1,
                    RefreshRate = 0
                })
                {
                    message.RenderStyled("Styled fixed", new Style(ConsoleColor.Yellow, ConsoleColor.DarkBlue));
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Styled fixed") &&
                    write.Color == ConsoleColor.Yellow &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Delayed fixed-message refreshes do not leak into a later console driver scope.
        /// </summary>
        [Fact]
        public void Render_PendingUpdateAfterConsoleScopeChange_DoesNotWriteToNewDriver()
        {
            FixedMessage? message = null;

            try
            {
                using (new ConsoleDriverScope(24))
                {
                    message = new FixedMessage
                    {
                        Height = 1,
                        RefreshRate = 100
                    };

                    message.Render("initial");
                    message.Render("pending-from-old-scope");
                }

                using (var scope = new ConsoleDriverScope(28))
                {
                    Thread.Sleep(150);

                    Consoul.Panel(
                        "Ready",
                        new PanelOptions
                        {
                            BorderStyle = PanelBorderStyle.Heavy,
                            Width = 28
                        });

                    Assert.DoesNotContain("pending-from-old-scope", scope.Driver.Text);
                    VisualAssert.ContainsText(scope.Driver, "┏");
                    VisualAssert.ContainsText(scope.Driver, "Ready");
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
            finally
            {
                message?.Dispose();
            }
        }
    }
}
