using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    internal sealed class TextWriterConsoleDriver : IConsoleDriver, IConsoleDriverProfileProvider
    {
        private readonly IAnsiConsoleOutput _output;
        private readonly TextReader _input;
        private readonly TerminalProfile _profile;
        private int _cursorLeft;
        private int _cursorTop;

        public TextWriterConsoleDriver(IAnsiConsoleOutput output)
            : this(output, null)
        {
        }

        public TextWriterConsoleDriver(IAnsiConsoleOutput output, AnsiConsoleSettings settings)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _input = settings == null ? null : settings.In;
            _profile = CreateProfile(_output, settings);
        }

        public int BufferWidth => Math.Max(1, _output.Width);

        public int BufferHeight => Math.Max(1, _output.Height);

        public int CursorLeft => _cursorLeft;

        public int CursorTop => _cursorTop;

        public bool CursorVisible { get; set; } = true;

        public ConsoleColor ForegroundColor { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor BackgroundColor { get; set; } = RenderOptions.DefaultScheme.BackgroundColor;

        public Encoding InputEncoding => Encoding.UTF8;

        public string Title { get; set; } = string.Empty;

        public bool IsInputRedirected => _profile.IsInputRedirected;

        public bool IsOutputRedirected => _profile.IsOutputRedirected;

        public bool IsErrorRedirected => _profile.IsErrorRedirected;

        public bool IsVirtual => true;

        public TerminalProfile Profile => _profile;

        public void Write(string value)
        {
            value = value ?? string.Empty;
            _output.Writer.Write(value);
            Track(value);
        }

        public void WriteLine(string value)
        {
            value = value ?? string.Empty;
            _output.Writer.WriteLine(value);
            Track(value);
            NewLine();
        }

        public void WriteControl(string value)
        {
            _output.Writer.Write(value ?? string.Empty);
        }

        public void Clear()
        {
            _cursorLeft = 0;
            _cursorTop = 0;
        }

        public void ClearLine(int top)
        {
            _cursorLeft = 0;
            _cursorTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
        }

        public void SetCursorPosition(int left, int top)
        {
            _cursorLeft = Math.Max(0, Math.Min(left, BufferWidth - 1));
            _cursorTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (_input == null)
            {
                return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
            }

            int value = _input.Read();
            if (value < 0)
            {
                return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
            }

            ConsoleKeyInfo key = ToKeyInfo((char)value);
            if (key.Key == ConsoleKey.Enter && value == '\r' && _input.Peek() == '\n')
            {
                _input.Read();
            }

            if (!intercept && key.Key != ConsoleKey.Enter)
            {
                Write(key.KeyChar.ToString());
            }

            return key;
        }

        public Stream OpenStandardInput()
        {
            if (_input == null)
            {
                return new MemoryStream();
            }

            return new TextReaderInputStream(_input, InputEncoding);
        }

        private void Track(string value)
        {
            foreach (char character in value)
            {
                if (character == '\r')
                {
                    continue;
                }

                if (character == '\n')
                {
                    NewLine();
                    continue;
                }

                _cursorLeft++;
                if (_cursorLeft >= BufferWidth)
                {
                    NewLine();
                }
            }
        }

        private void NewLine()
        {
            _cursorLeft = 0;
            _cursorTop = Math.Min(BufferHeight - 1, _cursorTop + 1);
        }

        private static TerminalProfile CreateProfile(IAnsiConsoleOutput output, AnsiConsoleSettings settings)
        {
            settings = settings ?? new AnsiConsoleSettings();

            bool outputRedirected = !output.IsTerminal;
            bool inputRedirected = settings.In == null;
            bool errorRedirected = !output.IsTerminal;

            if (settings.Interactive == InteractionSupport.Yes)
            {
                inputRedirected = false;
                outputRedirected = false;
                errorRedirected = false;
            }
            else if (settings.Interactive == InteractionSupport.No)
            {
                inputRedirected = true;
            }

            bool detectedAnsi = true;
            bool ansi = settings.Ansi == AnsiSupport.Yes || (settings.Ansi == AnsiSupport.Detect && detectedAnsi);
            ColorSystem colorSystem = ResolveColorSystem(settings.ColorSystem);
            bool supportsColor = colorSystem != ColorSystem.NoColors;
            bool interactive = !inputRedirected && !outputRedirected;

            var capabilities = new TerminalCapabilities(
                ansi,
                unicode: ansi,
                links: ansi,
                interactive: interactive,
                legacy: !ansi,
                colorSystem: colorSystem);

            return new TerminalProfile(
                output.Width,
                output.Height,
                inputRedirected,
                outputRedirected,
                errorRedirected,
                isVirtual: true,
                supportsColor: supportsColor,
                capabilities: capabilities);
        }

        private static ColorSystem ResolveColorSystem(ColorSystemSupport support)
        {
            switch (support)
            {
                case ColorSystemSupport.NoColors:
                    return ColorSystem.NoColors;
                case ColorSystemSupport.Legacy:
                    return ColorSystem.Legacy;
                case ColorSystemSupport.Standard:
                    return ColorSystem.Standard;
                case ColorSystemSupport.EightBit:
                    return ColorSystem.EightBit;
                case ColorSystemSupport.TrueColor:
                    return ColorSystem.TrueColor;
                default:
                    return ColorSystem.Standard;
            }
        }

        private static ConsoleKeyInfo ToKeyInfo(char character)
        {
            if (character == '\r' || character == '\n')
            {
                return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
            }

            if (character == '\b' || character == (char)127)
            {
                return new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false);
            }

            if (character == '\u001b')
            {
                return new ConsoleKeyInfo('\u001b', ConsoleKey.Escape, false, false, false);
            }

            return new ConsoleKeyInfo(character, ResolveConsoleKey(character), false, false, false);
        }

        private static ConsoleKey ResolveConsoleKey(char character)
        {
            char upper = char.ToUpperInvariant(character);
            if (upper >= 'A' && upper <= 'Z')
            {
                return (ConsoleKey)((int)ConsoleKey.A + (upper - 'A'));
            }

            if (character >= '0' && character <= '9')
            {
                return (ConsoleKey)((int)ConsoleKey.D0 + (character - '0'));
            }

            if (character == ' ')
            {
                return ConsoleKey.Spacebar;
            }

            return ConsoleKey.A;
        }

        private sealed class TextReaderInputStream : Stream
        {
            private readonly TextReader _reader;
            private readonly Encoding _encoding;
            private readonly Queue<byte> _buffer = new Queue<byte>();

            public TextReaderInputStream(TextReader reader, Encoding encoding)
            {
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
                _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || count < 0 || offset + count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                int read = 0;
                while (read < count)
                {
                    EnsureBufferedByte();
                    if (_buffer.Count == 0)
                    {
                        break;
                    }

                    buffer[offset + read] = _buffer.Dequeue();
                    read++;
                }

                return read;
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromResult(0);
                }

                return Task.FromResult(Read(buffer, offset, count));
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            private void EnsureBufferedByte()
            {
                if (_buffer.Count > 0)
                {
                    return;
                }

                int value = _reader.Read();
                if (value < 0)
                {
                    return;
                }

                foreach (byte item in _encoding.GetBytes(new[] { (char)value }))
                {
                    _buffer.Enqueue(item);
                }
            }
        }
    }
}
