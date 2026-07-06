using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Tests terminal profile snapshots exposed through Consoul.
    /// </summary>
    public class TerminalProfileTests
    {
        /// <summary>
        /// Terminal profile snapshots reflect the active console driver dimensions and virtual status.
        /// </summary>
        [Fact]
        public void Terminal_ReflectsActiveConsoleDriver()
        {
            using (var scope = new ConsoleDriverScope(42, 12))
            {
                TerminalProfile profile = Consoul.Terminal;

                Assert.Equal(42, profile.Width);
                Assert.Equal(12, profile.Height);
                Assert.True(profile.IsVirtual);
                Assert.True(profile.IsInteractive);
                Assert.True(profile.SupportsColor);
                Assert.True(profile.Capabilities.Ansi);
                Assert.True(profile.Capabilities.Unicode);
                Assert.True(profile.Capabilities.Links);
                Assert.True(profile.Capabilities.Interactive);
                Assert.False(profile.Capabilities.Legacy);
                Assert.Equal(ColorSystem.Standard, profile.Capabilities.ColorSystem);
                Assert.True(profile.IsNarrow());
                Assert.False(profile.IsWide());
                Assert.Equal(30, profile.ResolveWidth(30));
                Assert.Equal(42, profile.ResolveWidth(100));
                Assert.Equal(42, profile.ResolveWidth(null, minimumWidth: 80));

                scope.Driver.Resize(120, 30);
                TerminalProfile resized = Consoul.Terminal;

                Assert.Equal(120, resized.Width);
                Assert.Equal(30, resized.Height);
                Assert.False(resized.IsNarrow());
                Assert.True(resized.IsWide());
            }
        }

        /// <summary>
        /// Terminal profiles respect explicit redirection metadata on the active driver.
        /// </summary>
        [Fact]
        public void Terminal_ReportsRedirectedDriverAsNonInteractive()
        {
            using (var scope = new ConsoleDriverScope(80, 20))
            {
                scope.Driver.IsInputRedirected = true;
                scope.Driver.IsOutputRedirected = true;
                scope.Driver.IsErrorRedirected = true;

                TerminalProfile profile = Consoul.Terminal;

                Assert.False(profile.IsInteractive);
                Assert.True(profile.IsInputRedirected);
                Assert.True(profile.IsOutputRedirected);
                Assert.True(profile.IsErrorRedirected);
                Assert.True(profile.IsVirtual);
                Assert.True(profile.SupportsColor);
                Assert.True(profile.Capabilities.Ansi);
                Assert.True(profile.Capabilities.Unicode);
                Assert.True(profile.Capabilities.Links);
                Assert.False(profile.Capabilities.Interactive);
                Assert.False(profile.Capabilities.Legacy);
                Assert.Equal(ColorSystem.Standard, profile.Capabilities.ColorSystem);
                Assert.Equal("80x20 virtual", profile.ToString());
            }
        }

        /// <summary>
        /// Capture exposes a virtual non-interactive terminal profile to the captured render action and restores the outer profile afterward.
        /// </summary>
        [Fact]
        public void Capture_UsesVirtualTerminalProfileAndRestoresOuterProfile()
        {
            using (var scope = new ConsoleDriverScope(72, 18))
            {
                TerminalProfile capturedProfile = new TerminalProfile(1, 1);

                string output = Consoul.Capture(() =>
                {
                    capturedProfile = Consoul.Terminal;
                    Consoul.Write("Captured width: " + capturedProfile.Width);
                }, width: 33, height: 9);

                Assert.Equal(33, capturedProfile.Width);
                Assert.Equal(9, capturedProfile.Height);
                Assert.True(capturedProfile.IsVirtual);
                Assert.False(capturedProfile.IsInteractive);
                Assert.True(capturedProfile.IsInputRedirected);
                Assert.True(capturedProfile.IsOutputRedirected);
                Assert.True(capturedProfile.Capabilities.Ansi);
                Assert.False(capturedProfile.Capabilities.Interactive);
                Assert.Contains("Captured width: 33", output);

                TerminalProfile restored = Consoul.Terminal;
                Assert.Equal(72, restored.Width);
                Assert.Equal(18, restored.Height);
                Assert.True(restored.IsVirtual);
                Assert.True(restored.IsInteractive);
            }
        }

        /// <summary>
        /// Standalone terminal profiles normalize dimensions and helper calculations.
        /// </summary>
        [Fact]
        public void TerminalProfile_NormalizesDimensionsAndWidthHelpers()
        {
            var profile = new TerminalProfile(0, -4, isOutputRedirected: true);

            Assert.Equal(1, profile.Width);
            Assert.Equal(1, profile.Height);
            Assert.False(profile.IsInteractive);
            Assert.False(profile.SupportsColor);
            Assert.False(profile.Capabilities.Ansi);
            Assert.False(profile.Capabilities.Unicode);
            Assert.False(profile.Capabilities.Links);
            Assert.False(profile.Capabilities.Interactive);
            Assert.True(profile.Capabilities.Legacy);
            Assert.Equal(ColorSystem.NoColors, profile.Capabilities.ColorSystem);
            Assert.True(profile.IsNarrow(2));
            Assert.True(profile.IsWide(1));
            Assert.Equal(1, profile.ResolveWidth(120, minimumWidth: 20));
            Assert.Equal("1x1 redirected", profile.ToString());
        }
    }
}
