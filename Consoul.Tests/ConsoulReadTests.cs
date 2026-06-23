using System.IO;
using System.Text;
using System.Threading;
using ConsoulLibrary;
using Xunit;

namespace ConsoulLibrary.Tests
{
    /// <summary>
    /// Tests for the stream-backed Consoul read loop.
    /// </summary>
    public class ConsoulReadTests
    {
        /// <summary>
        /// Verifies LF input ends a read configured with the legacy CRLF terminator.
        /// </summary>
        [Fact]
        public void ReadFromStream_TrimsLfWhenExitCodeIsCrLf()
        {
            string result = ReadFromStream("hello\nnext", "\r\n");

            Assert.Equal("hello", result);
        }

        /// <summary>
        /// Verifies CRLF input keeps the existing Windows terminator behavior.
        /// </summary>
        [Fact]
        public void ReadFromStream_TrimsCrLfWhenExitCodeIsCrLf()
        {
            string result = ReadFromStream("hello\r\nnext", "\r\n");

            Assert.Equal("hello", result);
        }

        /// <summary>
        /// Verifies CRLF input trims the actual line ending when LF is configured.
        /// </summary>
        [Fact]
        public void ReadFromStream_TrimsCrLfWhenExitCodeIsLf()
        {
            string result = ReadFromStream("hello\r\nnext", "\n");

            Assert.Equal("hello", result);
        }

        /// <summary>
        /// Verifies custom non-newline terminators do not stop on line endings.
        /// </summary>
        [Fact]
        public void ReadFromStream_UsesExactMatchForCustomExitCode()
        {
            string result = ReadFromStream("alpha\nomegaENDtail", "END");

            Assert.Equal("alpha\nomega", result);
        }

        /// <summary>
        /// Verifies EOF returns the input accumulated before the stream ends.
        /// </summary>
        [Fact]
        public void ReadFromStream_ReturnsAccumulatedInputAtEndOfStream()
        {
            string result = ReadFromStream("partial", "\r\n");

            Assert.Equal("partial", result);
        }

        private static string ReadFromStream(string input, string exitCode)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                return Consoul.ReadFromStream(stream, Encoding.UTF8, CancellationToken.None, exitCode);
            }
        }
    }
}
