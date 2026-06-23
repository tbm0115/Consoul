using ConsoulLibrary.Tests.Rendering;
using System.Linq;
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
    }
}
