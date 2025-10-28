using ConsoulLibrary.Color;
using ConsoulLibrary.Views;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary {
    /// <summary>
    /// Static extensions to <see cref="Console"/>
    /// </summary>
    public static class Consoul
    {
        [Obsolete("Use PromptResult.IsCanceled instead of sentinel values.")]
        public const int EscapeIndex = -100;

        #region Window Resize Listener
        private static Timer _resizeCheckTimer;
        private static int _lastKnownWidth = Console.BufferWidth;
        /// <summary>
        /// Event that is triggered whenever the console window is resized.
        /// </summary>
        public static event EventHandler WindowResized
        {
            add
            {
                if (WindowResizedInternal == null)
                {
                    StartResizeListener();
                }
                WindowResizedInternal += value;
            }
            remove
            {
                WindowResizedInternal -= value;
                if (WindowResizedInternal == null)
                {
                    StopResizeListener();
                }
            }
        }

        private static event EventHandler WindowResizedInternal;

        /// <summary>
        /// Starts a timer to check for window resize events and raises the event if a change is detected.
        /// </summary>
        private static void StartResizeListener()
        {
            if (_resizeCheckTimer == null)
            {
                _resizeCheckTimer = new Timer(CheckForResize, null, 500, 500); // Check every 500 ms
            }
        }

        /// <summary>
        /// Stops the timer that checks for window resize events.
        /// </summary>
        private static void StopResizeListener()
        {
            _resizeCheckTimer?.Dispose();
            _resizeCheckTimer = null;
        }

        /// <summary>
        /// Checks if the console window size has changed.
        /// </summary>
        private static void CheckForResize(object state)
        {
            int newWidth = Console.BufferWidth;
            if (newWidth != _lastKnownWidth)
            {
                _lastKnownWidth = newWidth;
                WindowResizedInternal?.Invoke(null, EventArgs.Empty);
            }
        }
        #endregion

        /// <summary>
        /// Waits for the user to press "Enter". Performs Console.ReadLine()
        /// <paramref name="silent">Flags whether or not to show continue message.</paramref>
        /// </summary>
        public static void Wait(bool silent = false, CancellationToken cancellationToken = default)
        {
            if (!silent)
                Consoul.WriteCore(RenderOptions.ContinueMessage, RenderOptions.SubnoteScheme);
            Read(cancellationToken);
        }

        /// <summary>
        /// Writes a new line as a break
        /// </summary>
        public static void LineBreak()
            => Console.WriteLine();

        /// <summary>
        /// Prompts the user to provide a string input.
        /// </summary>
        /// <param name="message">Prompt Message</param>
        /// <param name="color">Override the Prompt Message color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in <see cref="string.Empty"/> .</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User response (string)</returns>
        public static string Input(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            string output;
            bool valid = false;
            do
            {
                Consoul.WriteCore(message, RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor));
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
        /// Prompts the user to provide a string input and converts it to the expected type.
        /// </summary>
        /// <param name="message">Prompt message</param>
        /// <param name="expectedType">The expected type of the return value</param>
        /// <param name="color">Override the prompt message color</param>
        /// <param name="backgroundColor">Override the prompt background color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in string.Empty</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>User response converted to the expected type</returns>
        public static object Input(string message, Type expectedType, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            string output = string.Empty;
            bool valid = false;
            object result = null;
            do
            {
                Consoul.WriteCore(message, RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor));
                output = Read(cancellationToken);
                if (allowEmpty && string.IsNullOrEmpty(output))
                {
                    valid = true;
                    result = string.Empty;
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        result = Convert.ChangeType(output, expectedType);
                        valid = true;
                    }
                    catch (Exception ex)
                    {
                        Consoul.WriteCore($"Invalid input. Please enter a value of type {expectedType.Name}. Error: {ex.Message}", RenderOptions.InvalidScheme);
                    }
                }
            } while (!valid);

            return result;
        }

        /// <summary>
        /// Prompts the user to provide a string input and converts it to the expected type "T".
        /// </summary>
        /// <typeparam name="T">The expected return type</typeparam>
        /// <param name="message">Prompt message</param>
        /// <param name="color">Override the prompt message color</param>
        /// <param name="backgroundColor">Override the prompt background color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in string.Empty</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>User response converted to type T</returns>
        public static T Input<T>(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            return (T)Input(message, typeof(T), color: color, backgroundColor: backgroundColor, allowEmpty: allowEmpty, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Internal Console.WriteLine() wrapper
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="color">Color for Message. Defaults to <see cref="RenderOptions.DefaultColor"/></param>
        /// <param name="backgroundColor">Color for the background. Defaults to <see cref="RenderOptions.BackgroundColor"/></param>
        /// <param name="writeLine">Specifies whether to use <see cref="Console.WriteLine()"/> or <see cref="Console.Write(string)"/></param>
        internal static void WriteCore(string message, ConsoleColor color, ConsoleColor? backgroundColor = null, bool writeLine = true)
        {
            using (var colors = new ColorMemory(color, backgroundColor))
            {
                if (writeLine)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                }
            }
        }

        /// <summary>
        /// Internal Console.WriteLine() wrapper
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="colorScheme">Color scheme for Message. Defaults to <see cref="RenderOptions.DefaultColor"/></param>
        /// <param name="writeLine">Specifies whether to use <see cref="Console.WriteLine()"/> or <see cref="Console.Write(string)"/></param>
        internal static void WriteCore(string message, ColorScheme? colorScheme = null, bool writeLine = true)
        {
            if (colorScheme == null)
                colorScheme = RenderOptions.DefaultScheme;
            WriteCore(message, colorScheme.Value.Color, backgroundColor: colorScheme.Value.BackgroundColor, writeLine: writeLine);
        }

        /// <summary>
        /// Writes a message to the <see cref="Console"/>. Rendering depends on <see cref="RenderOptions.WriteMode"/>
        /// </summary>
        /// <param name="message"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='message']"/></param>
        /// <param name="color"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='color']"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='backgroundColor']"/></param>
        /// <param name="writeLine"><inheritdoc cref="WriteCore(string, ConsoleColor, ConsoleColor?, bool)" path="/param[@name='writeLine']"/></param>
        public static void Write(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true)
        {
            switch (RenderOptions.WriteMode)
            {
                case RenderOptions.WriteModes.SuppressAll:
                    // Do nothing
                    break;
                case RenderOptions.WriteModes.SuppressBlacklist:
                    if (!RenderOptions.BlacklistColors.Any(c => c == color))
                        WriteCore(message, color: RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor), writeLine: writeLine);
                    break;
                default: // Include WriteAll
                    WriteCore(message, color: RenderOptions.GetColorOrDefault(color), backgroundColor: RenderOptions.GetBackgroundColorOrDefault(backgroundColor), writeLine: writeLine);
                    break;
            }
        }

        /// <summary>
        /// Writes a message to the <see cref="Console"/>. Rendering depends on <see cref="RenderOptions.WriteMode"/>
        /// </summary>
        /// <param name="message"><inheritdoc cref="WriteCore(string, ColorScheme?, bool)" path="/param[@name='message']"/></param>
        /// <param name="colorScheme"><inheritdoc cref="WriteCore(string, ColorScheme?, bool)" path="/param[@name='colorScheme']"/></param>
        /// <param name="writeLine"><inheritdoc cref="WriteCore(string, ColorScheme?, bool)" path="/param[@name='writeLine']"/></param>
        public static void Write(string message, ColorScheme colorScheme, bool writeLine = true)
            => WriteCore(message, colorScheme, writeLine);

        /// <summary>
        /// Writes a formatted string to the console with flexible color formatting.
        /// </summary>
        /// <param name="template">The template string containing placeholders in the form "{PropertyName:Color}".</param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="Write" path="/param[@name='backgroundColor']"/></param>
        /// <param name="writeLine"><inheritdoc cref="Write" path="/param[@name='writeLine']"/></param>
        /// <param name="args">The values for the placeholders in the template.</param>
        public static void Write(string template, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true, params object[] args)
        {
            // Regular expression to match placeholders with optional color specification using named capture groups
            Regex regex = new Regex(@"\{(?<PropertyName>\w+)(:(?<Color>\w+))?\}");
            MatchCollection matches = regex.Matches(template);

            int currentIndex = 0;
            foreach (Match match in matches)
            {
                // Write the text before the match
                if (match.Index > currentIndex)
                {
                    Consoul.Write(template.Substring(currentIndex, match.Index - currentIndex), color: color, backgroundColor: backgroundColor, writeLine: false);
                }

                // Extract property name and optional color using named capture groups
                string propertyName = match.Groups["PropertyName"].Value;
                string colorName = match.Groups["Color"].Success ? match.Groups["Color"].Value : null;

                // Find the value corresponding to the propertyName
                object value = null;
                foreach (var arg in args)
                {
                    var prop = arg.GetType().GetProperty(propertyName);
                    if (prop != null)
                    {
                        value = prop.GetValue(arg);
                        break;
                    }
                }

                // If the value is found, write it with the specified color
                ConsoleColor subtextColor;
                if (string.IsNullOrEmpty(colorName) || !Enum.TryParse(colorName, true, out subtextColor))
                {
                    subtextColor = color ?? RenderOptions.GetColorOrDefault();
                }

                if (value != null)
                {
                    Consoul.Write(value.ToString(), color: subtextColor, backgroundColor: backgroundColor, writeLine: false);
                }
                else
                {
                    // If the value is not found, write the placeholder as-is
                    Consoul.Write(match.Value, color: subtextColor, backgroundColor: backgroundColor, writeLine: false);
                }

                // Update currentIndex to the end of the current match
                currentIndex = match.Index + match.Length;
            }

            // Write any remaining text after the last placeholder
            if (currentIndex < template.Length)
            {
                Consoul.Write(template.Substring(currentIndex), color: color, backgroundColor: backgroundColor, writeLine: writeLine);
            } else if(writeLine)
            {
                Consoul.Write(string.Empty);
            }
        }

        /// <summary>
        /// Writes a formatted message for exceptions.
        /// </summary>
        /// <param name="ex">Exception to be written</param>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="Write" path="/param[@name='backgroundColor']"/></param>
        public static void Write(Exception ex, string message = null, bool includeStackTrace = true, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            if (!string.IsNullOrEmpty(message))
                Write(message);
            Write(ex.Message, RenderOptions.InvalidScheme);
            if (includeStackTrace)
                Write($"\t{ex.StackTrace}", RenderOptions.InvalidScheme);
            if (ex.InnerException != null)
                Write(ex.InnerException, null, includeStackTrace, color: color, backgroundColor: backgroundColor);
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
            RoutineInput input = new RoutineInput();
            if (Routines.HasBuffer())
            {
                input = Routines.Next();

                long delayTicks = 0;
                if (Routines.UseDelays && input.Delay.Value != null)
                    delayTicks = input.Delay.Value.Ticks / 2;
                TimeSpan delay = new TimeSpan(delayTicks);
                if (!string.IsNullOrEmpty(input.Description))
                    Write(input.Description, RenderOptions.SubnoteScheme);

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
                            catch (OperationCanceledException)
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
            RoutineInput input = new RoutineInput();
            if (Routines.HasBuffer())
            {
                input = Routines.Next();

                long delayTicks = 0;
                if (Routines.UseDelays && input.Delay.Value != null)
                    delayTicks = input.Delay.Value.Ticks / 2;
                TimeSpan delay = new TimeSpan(delayTicks);
                if (!string.IsNullOrEmpty(input.Description))
                    Write(input.Description, RenderOptions.SubnoteScheme);

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
                        Consoul.Write("*", color: color, writeLine: false);
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
            }

            // Check if we should save the input to the Routine Stack
            if (Routines.MonitorInputs)
                Routines.UserInputs.Push(input);

            return input.Value;
        }

        /// <summary>
        /// Renders a Consoul view using the <see cref="ViewRenderer"/> to chain renderings.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IView"/> to render.</typeparam>
        /// <returns>Reference to the created <see cref="ViewRenderer"/> to chain renderings.</returns>
        public static ViewRenderer Render<T>() where T : IView
        {
            var renderer = new ViewRenderer();
            return renderer.Render<T>();
        }

        /// <summary>
        /// Renders a Consoul view using the <see cref="ViewRenderer"/> to chain renderings.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IView"/> to render.</typeparam>
        /// <returns>Reference to the created <see cref="ViewRenderer"/> to chain renderings.</returns>
        public static async Task<ViewRenderer> RenderAsync<T>() where T : IView
        {
            var renderer = new ViewRenderer();
            return await renderer.RenderAsync<T>();
        }

        /// <summary>
        /// Writes a message centered in the window in its current size.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="maxWidth">Maximum width that the text can render (in character length)</param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="Write" path="/param[@name='backgroundColor']"/></param>
        /// <param name="writeLine"><inheritdoc cref="Write" path="/param[@name='writeLine']"/></param>
        /// <param name="whitespace">Character used to fill space around the <paramref name="message"/></param>
        public static void Center(string message, int maxWidth, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true, char whitespace = ' ')
        {
            if (maxWidth == 0)
                maxWidth = Console.BufferWidth;
            string text = message.Length > maxWidth ? message.Substring(0, maxWidth - 1) + "…" : message;

            int remainder = maxWidth - text.Length - 1;
            int left, right;
            right = remainder / 2;
            left = right;
            if (remainder % 2 != 0)
                left = (remainder + 1) / 2;


            text = $"{(new string(whitespace, left))}{text}{(new string(whitespace, right))}";

            Write(text, color: color, backgroundColor: backgroundColor, writeLine: writeLine);
        }

        /// <summary>
        /// <inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)"/>
        /// </summary>
        /// <param name="message"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='message']"/></param>
        /// <param name="maxWidth"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='maxWidth']"/></param>
        /// <param name="colorScheme">Color scheme used to write message and background</param>
        /// <param name="writeLine"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='writeLine']"/></param>
        /// <param name="whitespace"><inheritdoc cref="Center(string, int, ConsoleColor?, ConsoleColor?, bool, char)" path="/param[@name='whitespace']"/></param>
        public static void Center(string message, int maxWidth, ColorScheme colorScheme, bool writeLine = true, char whitespace = ' ')
            => Center(message, maxWidth, colorScheme.Color, colorScheme.BackgroundColor, writeLine, whitespace);

        /// <summary>
        /// Prompts the user to acknowledge a Prompt.
        /// </summary>
        /// <param name="message">Display message</param>
        /// <param name="clear">Option to clear the Console buffer. If true, can make the prompt more prominant.</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Default is typically the 'No', but can be overriden</param>
        /// <param name="defaultIsNo">Specifies whether the default entry should be 'No'. This only applies if 'allowEmpty' is true.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
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
                Consoul.WriteCore(message, RenderOptions.PromptScheme);
                Consoul.WriteCore(optionMessage, RenderOptions.SubnoteScheme);
                input = Read(cancellationToken);
                if (input.ToLower() != "y" && input.ToLower() != "n" && !string.IsNullOrEmpty(input))
                {
                    Consoul.WriteCore("Invalid input!", RenderOptions.InvalidScheme);
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Index of the option that was chosen. Returns -1 if selection was invalid.</returns>
        public static int Prompt(string message, bool clear = false, CancellationToken cancellationToken = default, params string[] options)
        {
            return (new SelectionPrompt(message, clear, options)).Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user with a complex list of choices.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="options">Array of complex options.</param>
        /// <param name="clear"><inheritdoc cref="Prompt(string, bool, string[])" path="/param[@name='clear']"/></param>
        /// <returns></returns>
        public static int Prompt(string message, SelectOption[] options, bool clear = false, CancellationToken cancellationToken = default)
        {
            return (new SelectionPrompt(message, clear, options)).Render(cancellationToken);
        }

        /// <summary>
        /// Prompts the user to input a file path.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="checkExists">Indicates whether to check the file exists before allowing the user exit the loop.</param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filepath string</returns>
        public static string PromptForFilepath(string message, bool checkExists, CancellationToken cancellationToken = default) {
            string path;
            do
            {
                Write(message, RenderOptions.PromptScheme);
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
        /// <param name="checkExists"><inheritdoc cref="PromptForFilepath(string, bool, ConsoleColor?, CancellationToken)" path="/param[@name='checkExists']"/></param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filepath string</returns>
        public static string PromptForFilepath(string defaultPath, string message, bool checkExists, CancellationToken cancellationToken = default) {
            string path = defaultPath;
            if (!File.Exists(path) || !Ask($"Would you like to use the default path:\r\n{path}", defaultIsNo: false)) {
                path = PromptForFilepath(message, checkExists, cancellationToken);
            }
            return path;
        }

        /// <summary>
        /// Plays the BEL character in the console. See <see href="https://en.wikipedia.org/wiki/Bell_character">Wikipedia</see> for more details.
        /// </summary>
        public static void Ding()
        {
            const char BEL = (char)7;
            Console.Write(BEL);
        }

        /// <summary>
        /// Displays a message in the console and plays <see cref="Ding"/>.
        /// </summary>
        /// <param name="message"><inheritdoc cref="Write" path="/param[@name='message']"/></param>
        /// <param name="color"><inheritdoc cref="Write" path="/param[@name='color']"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="Write" path="/param[@name='backgroundColor']"/></param>
        /// <param name="writeLine"><inheritdoc cref="Write" path="/param[@name='writeLine']"/></param>
        public static void Alert(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null, bool writeLine = true)
        {
            Write(message, color: color, backgroundColor: backgroundColor, writeLine: writeLine);
            Ding();
        }

        /// <summary>
        /// Saves the current cursor position. NOTE: You <b>MUST</b> dispose this variable.
        /// </summary>
        /// <returns>Reference to a <see cref="CursorScope"/> position</returns>
        public static CursorScope SaveCursor()
            => new CursorScope();
    }
}
