using System.Threading;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Smoke tests for smaller visual rendering primitives.
    /// </summary>
    public class VisualRenderingSmokeTests
    {
        /// <summary>
        /// Banner, line, and progress visuals render within a narrow fake terminal.
        /// </summary>
        [Theory]
        [InlineData(24)]
        [InlineData(80)]
        public void Render_PrimitiveVisuals_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                BannerEntry.Render("Responsive Visual");
                using (var line = new LineEntry("Line entry with longer content than the terminal width"))
                {
                    line.Message = "Updated line entry content";

                    Thread.Sleep(20);
                    using (var progress = new ProgressBar("Starting responsive render"))
                    {
                        Thread.Sleep(20);
                        progress.Update(0.75, "Three quarters complete");
                    }
                }

                VisualAssert.NoOverflow(scope.Driver);
                Assert.True(scope.Driver.Writes.Count > 0);
            }
        }
    }
}
