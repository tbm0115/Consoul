using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// A decoder retains multibyte and surrogate-pair state between the one-byte reads.
        /// </summary>
        [Theory]
        [InlineData("utf-8")]
        [InlineData("utf-16")]
        [InlineData("utf-32")]
        public void ReadFromStream_PreservesUnicodeAcrossByteBoundaries(string encodingName)
        {
            Encoding encoding = Encoding.GetEncoding(encodingName);
            const string expected = "caf\u00e9 \u6771\u4eac \ud83d\ude80";
            using (var stream = new MemoryStream(encoding.GetBytes(expected + "\nnext")))
            {
                Assert.Equal(expected, Consoul.ReadFromStream(stream, encoding, CancellationToken.None, "\n"));
                Assert.Equal("next", Consoul.ReadFromStream(stream, encoding, CancellationToken.None, "\n"));
            }
        }

        /// <summary>
        /// Non-ASCII custom terminators are recognized after decoding without consuming the next response.
        /// </summary>
        [Fact]
        public void ReadFromStream_RecognizesUnicodeTerminator()
        {
            Assert.Equal("first", ReadFromStream("first\u7d42tail", "\u7d42"));
        }

        /// <summary>
        /// An unfinished trailing sequence is flushed through the encoding's configured fallback.
        /// </summary>
        [Fact]
        public void ReadFromStream_FlushesIncompleteSequenceAtEndOfStream()
        {
            var encoding = (Encoding)Encoding.UTF8.Clone();
            encoding.DecoderFallback = new DecoderReplacementFallback("[incomplete]");
            using (var stream = new MemoryStream(new byte[] { (byte)'a', 0xe2, 0x82 }))
            {
                Assert.Equal("a[incomplete]", Consoul.ReadFromStream(stream, encoding, CancellationToken.None, "\n"));
            }
        }

        /// <summary>
        /// Strict decoding rejects an incomplete trailing sequence when the stream ends.
        /// </summary>
        [Fact]
        public void ReadFromStream_RespectsStrictFallbackAtEndOfStream()
        {
            using (var stream = new MemoryStream(new byte[] { 0xe2, 0x82 }))
            {
                Assert.Throws<DecoderFallbackException>(() =>
                    Consoul.ReadFromStream(stream, new UTF8Encoding(false, true), CancellationToken.None, "\n"));
            }
        }

        /// <summary>
        /// A real blank line and exhausted input remain distinct to prompt callers.
        /// </summary>
        [Fact]
        public void ReadFromStream_DistinguishesBlankLineFromEndOfStream()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("\n")))
            {
                Assert.Equal(string.Empty, Consoul.ReadFromStream(stream, Encoding.UTF8, CancellationToken.None, "\n"));
                Assert.Null(Consoul.ReadFromStream(stream, Encoding.UTF8, CancellationToken.None, "\n"));
            }
        }

        /// <summary>
        /// Cancellation returns the accumulated response without treating it as end of input.
        /// </summary>
        [Fact]
        public void ReadFromStream_PreCanceledReadDoesNotConsumeInput()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("next\n")))
            {
                Assert.Equal(string.Empty, Consoul.ReadFromStream(stream, Encoding.UTF8, new CancellationToken(true), "\n"));
                Assert.Equal(0, stream.Position);
            }
        }

        /// <summary>
        /// A canceled individual read does not submit partial input or discard the decoder's pending bytes.
        /// </summary>
        [Fact]
        public void ReadFromStream_TransientReadCancellationPreservesDecoderState()
        {
            using (var stream = new InterruptedReadStream(Encoding.UTF8.GetBytes("\u00e9\nnext")))
            {
                Assert.Equal("\u00e9", Consoul.ReadFromStream(stream, Encoding.UTF8, CancellationToken.None, "\n"));
                Assert.Equal(3, stream.Position);
                Assert.Equal("next", Consoul.ReadFromStream(stream, Encoding.UTF8, CancellationToken.None, "\n"));
            }
        }

        /// <summary>
        /// Cancellation reaches an outstanding read, preserving complete characters without consuming later input.
        /// </summary>
        [Fact]
        public void ReadFromStream_CancellationDuringReadReturnsAccumulatedText()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var stream = new CancelDuringReadStream(cancellation))
            {
                Assert.Equal("a", Consoul.ReadFromStream(stream, Encoding.UTF8, cancellation.Token, "\n"));
                Assert.True(cancellation.IsCancellationRequested);
                Assert.Equal(2, stream.ReadCount);
                Assert.Equal(1, stream.Position);
            }
        }

        private static string ReadFromStream(string input, string exitCode)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                return Consoul.ReadFromStream(stream, Encoding.UTF8, CancellationToken.None, exitCode);
            }
        }

        private sealed class InterruptedReadStream : MemoryStream
        {
            private int _readCount;

            public InterruptedReadStream(byte[] input) : base(input)
            {
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (++_readCount == 2)
                {
                    return Task.FromException<int>(new OperationCanceledException("The individual read was interrupted."));
                }

                return base.ReadAsync(buffer, offset, count, cancellationToken);
            }
        }

        private sealed class CancelDuringReadStream : MemoryStream
        {
            private readonly CancellationTokenSource _cancellation;

            public CancelDuringReadStream(CancellationTokenSource cancellation)
                : base(Encoding.UTF8.GetBytes("az\n"))
            {
                _cancellation = cancellation;
            }

            public int ReadCount { get; private set; }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                ReadCount++;
                if (ReadCount == 1)
                {
                    return base.ReadAsync(buffer, offset, count, cancellationToken);
                }

                var pending = new TaskCompletionSource<int>();
                using (cancellationToken.Register(() => pending.TrySetCanceled(cancellationToken)))
                {
                    _cancellation.Cancel();
                    if (!pending.Task.IsCompleted)
                    {
                        // Fail immediately if cancellation was not propagated, rather than leaving a hanging read.
                        return Task.FromException<int>(new InvalidOperationException("The caller's cancellation did not reach the pending read."));
                    }

                    return pending.Task;
                }
            }
        }
    }
}
