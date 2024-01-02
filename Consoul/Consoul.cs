using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoulLibrary {
    public static class Consoul
    {
        public const int EscapeIndex = -100;

        /// <summary>
        /// Waits for the user to press "Enter". Performs Console.ReadLine()
        /// <paramref name="silent">Flags whether or not to show continue message.</paramref>
        /// </summary>
        public static void Wait(bool silent = false, CancellationToken cancellationToken = default)
        {
            if (!silent) Consoul._write(RenderOptions.ContinueMessage, RenderOptions.SubnoteColor);
            Read(cancellationToken);
        }

        /// <summary>
        /// Prompts the user to provide a string input.
        /// </summary>
        /// <param name="message">Prompt Message</param>
        /// <param name="color">Override the Prompt Message color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in string.Empty</param>
        /// <returns>User response (string)</returns>
        public static string Input(string message, ConsoleColor? color = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            string output = string.Empty;
            bool valid = false;
            do
            {
                Consoul._write(message, RenderOptions.GetColor(color));
                output = Read(cancellationToken);
                if (allowEmpty)
                {
                    valid = true;
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    valid = true;
                }
            } while (!valid);
            return output;
        }

        /// <summary>
        /// Internal Console.WriteLine() wrapper
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="color">Color for Message</param>
        /// <param name="writeLine">Specifies whether to use Console.WriteLine() or Console.Write().</param>
        internal static void _write(string message, ConsoleColor color, bool writeLine = true)
        {
            Console.ForegroundColor = color;
            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
        }

        /// <summary>
        /// Writes a message to the Console. Rendering depends on RenderOptions.WriteMode
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="color">Color for Message. Defaults to RenderOptions.DefaultColor</param>
        /// <param name="writeLine">Specifies whether to use Console.WriteLine() or Console.Write()</param>
        public static void Write(string message, ConsoleColor? color = null, bool writeLine = true)
        {
            switch (RenderOptions.WriteMode)
            {
                case RenderOptions.WriteModes.SuppressAll:
                    // Do nothing
                    break;
                case RenderOptions.WriteModes.SuppressBlacklist:
                    if (!RenderOptions.BlacklistColors.Any(c => c == color))
                        _write(message, RenderOptions.GetColor(color), writeLine);
                    break;
                default: // Include WriteAll
                    _write(message, RenderOptions.GetColor(color), writeLine);
                    break;
            }
        }

        /// <summary>
        /// Reads user input from the console.
        /// </summary>
        /// <returns>Response from the user.</returns>
        public static string Read() => Read("\r\n");

        /// <summary>
        /// Waits for user input and reads the user response.
        /// </summary>
        /// <param name="exitCode">Reference to the string that indicates the end of stream.</param>
        /// <returns>Value from the user</returns>
        public static string Read(string exitCode = "\r\n")
        {
            using (var cancelSource = new CancellationTokenSource())
            {
                return Read(cancelSource.Token, exitCode);
            }
        }

        /// <summary>
        /// Asynchronously reads any input from the user and allows the operation to be cancelled at any time.
        /// </summary>
        /// <param name="cancellationToken">Reference to the cancellation token to stop the read operation.</param>
        /// <param name="exitCode">Reference to the string that indicates the end of stream.</param>
        /// <returns>Response from the user.</returns>
        public static string Read(CancellationToken cancellationToken = default, string exitCode = "\r\n")
        {
            bool keyControl = false, keyAlt = false, keyShift = false;
            RoutineInput input = new RoutineInput();
            if (Routines.HasBuffer())
            {
                input = Routines.Next();

                long delayTicks = 0;
                if (Routines.UseDelays && input.Delay.Value != null)
                    delayTicks = input.Delay.Value.Ticks / 2;
                TimeSpan delay = new TimeSpan(delayTicks);
                if (!string.IsNullOrEmpty(input.Description))
                    Write(input.Description, RenderOptions.SubnoteColor);

                System.Threading.Thread.Sleep(delay);
                Write(input.Value, ConsoleColor.Cyan);
                System.Threading.Thread.Sleep(delay);
            }
            else
            {
                string userInput = string.Empty;
                using (var stream = Console.OpenStandardInput())
                {
                    byte[] data = new byte[1];
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        using (var readCanceller = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)))
                        {
                            try
                            {
                                stream.ReadAsync(data, 0, data.Length, readCanceller.Token).Wait(cancellationToken);
                            }
                            catch (OperationCanceledException cancelled)
                            {
                                break;
                            }

                            if (data.Length > 0 && data[0] >= 0)
                            {
                                userInput += Console.InputEncoding.GetString(data);
                            }

                            if (userInput.EndsWith(exitCode))
                            {
                                userInput = userInput.Substring(0, userInput.Length - exitCode.Length);
                                break;
                            }
                        }
                    }
                    stream.Close();
                }
                input.Value = userInput;
            }

            if (Routines.PromptRegistry.Any())
            {
                input.OptionReference = Routines.PromptRegistry.FirstOrDefault(o => (o.Index + 1).ToString() == input.Value);
                //if (keyControl)
                //{
                //    input.Method = RoutineInput.InputMethod.OptionText;
                //}
            }

            // Check if we should save the input to the Routine Stack
            if (Routines.MonitorInputs)
                Routines.UserInputs.Push(input);

            return input.Value;
        }

        /// <summary>
        /// Method to read password without showing it in the console
        /// </summary>
        /// <returns>Entered password</returns>
        public static string ReadPassword(ConsoleColor? color = null, CancellationToken cancellationToken = default)
        {
            bool keyControl = false, keyAlt = false, keyShift = false;
            RoutineInput input = new RoutineInput();
            if (Routines.HasBuffer())
            {
                input = Routines.Next();

                long delayTicks = 0;
                if (Routines.UseDelays && input.Delay.Value != null)
                    delayTicks = input.Delay.Value.Ticks / 2;
                TimeSpan delay = new TimeSpan(delayTicks);
                if (!string.IsNullOrEmpty(input.Description))
                    Write(input.Description, RenderOptions.SubnoteColor);

                System.Threading.Thread.Sleep(delay);
                Write(input.Value, ConsoleColor.Cyan);
                System.Threading.Thread.Sleep(delay);
            }
            else
            {
                string password = "";
                ConsoleKeyInfo key;

                while(!cancellationToken.IsCancellationRequested)
                {
                    key = Console.ReadKey(true);

                    // Ignore any key other than Enter
                    if (key.Key == ConsoleKey.Enter)
                    {
                        //password = password.Substring(0, password.Length - 1);
                        Console.WriteLine();

                        break;
                    }
                     else if (key.Key != ConsoleKey.Backspace)
                    {
                        password += key.KeyChar;
                        Consoul.Write("*", color, false);
                    }
                    else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                input.Value = password;
            }

            if (Routines.PromptRegistry.Any())
            {
                input.OptionReference = Routines.PromptRegistry.FirstOrDefault(o => (o.Index + 1).ToString() == input.Value);
                //if (keyControl)
                //{
                //    input.Method = RoutineInput.InputMethod.OptionText;
                //}
            }

            // Check if we should save the input to the Routine Stack
            if (Routines.MonitorInputs)
                Routines.UserInputs.Push(input);

            return input.Value;
        }

        /// <summary>
        /// Writes a message centered in the window in its current size.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="maxWidth">Maximum width that the text can render (in character length)</param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="writeLine"><inheritdoc cref="Write" path="/param[@name='writeLine']"/></param>
        public static void Center(string message, int maxWidth, ConsoleColor? color = null, bool writeLine = true)
        {
            string text = message.Length > maxWidth ? message.Substring(0, maxWidth - 3) + "..." : message;

            int remainder = maxWidth - text.Length - 1;
            int left, right;
            right = remainder / 2;
            left = right;
            if (remainder % 2 != 0)
                left = (remainder + 1) / 2;


            text = $"{(new string(' ', left))}{text}{(new string(' ', right))}";

            Write(text, color, writeLine);
        }

        /// <summary>
        /// Prompts the user to acknowledge a Prompt.
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="clear">Option to clear the Console buffer. If true, can make the prompt more prominant.</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Default is typically the 'No', but can be overriden</param>
        /// <param name="defaultIsNo">Specifies whether the default entry should be 'No'. This only applies if 'allowEmpty' is true.</param>
        /// <returns>Boolean of users response relative to 'Yes' or 'No'</returns>
        public static bool Ask(string message, bool clear = false, bool allowEmpty = false, bool defaultIsNo = true, CancellationToken cancellationToken = default)
        {
            string input = "";
            string orEmpty = $" or Press Enter";
            string[] options = new string[] {
                $"Y{(allowEmpty && !defaultIsNo ? orEmpty : string.Empty)}=Yes",
                $"N{(allowEmpty && defaultIsNo ? orEmpty : string.Empty)}=No"
            };
            string optionMessage = $"({string.Join(", ", options)})";

            do
            {
                if (clear)
                {
                    Console.Clear();
                }
                Consoul._write(message, RenderOptions.PromptColor);
                Consoul._write(optionMessage, RenderOptions.SubnoteColor);
                input = Read(cancellationToken);
                if (input.ToLower() != "y" && input.ToLower() != "n" && !string.IsNullOrEmpty(input))
                {
                    Consoul._write("Invalid input!", RenderOptions.InvalidColor);
                }
            } while ((allowEmpty ? false : string.IsNullOrEmpty(input)) && input.ToLower() != "y" && input.ToLower() != "n");
            if (allowEmpty && string.IsNullOrEmpty(input))
                input = defaultIsNo ? "n" : "y";
            return input.ToLower() == "y";
        }

        /// <summary>
        /// Prompts the user with a simple list of choices.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>Index of the option that was chosen. Returns -1 if selection was invalid.</returns>
        public static int Prompt(string message, bool clear = false, params string[] options)
        {
            return Prompt(message, clear, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts the user with a simple list of choices.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="clear">Indicates whether or not to clear the console window.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>Index of the option that was chosen. Returns -1 if selection was invalid.</returns>
        public static int Prompt(string message, bool clear = false, CancellationToken cancellationToken = default, params string[] options)
        {
            return (new Prompt(message, clear, options)).Run(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a complex list of choices.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="options">Array of complex options.</param>
        /// <param name="clear"><inheritdoc cref="Prompt(string, bool, string[])" path="/param[@name='clear']"/></param>
        /// <returns></returns>
        public static int Prompt(string message, PromptOption[] options, bool clear = false)
        {
            return (new Prompt(message, clear, options)).Run();
        }

        /// <summary>
        /// Prompts the user to input a file path.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="checkExists">Indicates whether to check the file exists before allowing the user exit the loop.</param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <returns></returns>
        public static string PromptForFilepath(string message, bool checkExists, ConsoleColor? color = null, CancellationToken cancellationToken = default) {
            string path;
            do
            {
                Write(message, color);
                path = Read(cancellationToken);
            } while (string.IsNullOrEmpty(path) && (checkExists ? !File.Exists(path) : true));
            if (path.StartsWith("\"") && path.EndsWith("\"")) path = path.Substring(1, path.Length - 2);
            return path;
        }

        /// <summary>
        /// Prompts the user to input a file path with a suggested default path.
        /// </summary>
        /// <param name="defaultPath">The default file path the user must accept.</param>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="checkExists"><inheritdoc cref="PromptForFilepath(string, bool, ConsoleColor?)" path="/param[@name='checkExists']"/></param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <returns></returns>
        public static string PromptForFilepath(string defaultPath, string message, bool checkExists, ConsoleColor? color = null, CancellationToken cancellationToken = default) {
            string path = defaultPath;
            if (!File.Exists(path) || !Ask($"Would you like to use the default path:\r\n{path}", defaultIsNo: false)) {
                path = PromptForFilepath(message, checkExists, color, cancellationToken);
            }
            return path;
        }

        /// <summary>
        /// Plays the BEL character in the console. See <see href="https://en.wikipedia.org/wiki/Bell_character">Wikipedia</see> for more details.
        /// </summary>
        public static void Ding()
        {
            Console.Write((char)7);
        }

        /// <summary>
        /// Displays a message in the console and plays <see cref="Ding"/>.
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="color">Color for Message. Defaults to RenderOptions.DefaultColor</param>
        /// <param name="writeLine">Specifies whether to use Console.WriteLine() or Console.Write()</param>
        public static void Alert(string message, ConsoleColor? color = null, bool writeLine = true)
        {
            Write(message, color, writeLine);
            Ding();
        }
    }
}
