using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Consoul.Properties;

namespace ConsoulLibrary {
    public static class Consoul
    {
        public const int EscapeIndex = -100;

        /// <summary>
        /// Waits for the user to press "Enter". Performs Console.ReadLine()
        /// <paramref name="silent">Flags whether or not to show continue message.</paramref>
        /// </summary>
        public static void Wait(bool silent = false)
        {
            if (!silent)
                Consoul._write(RenderOptions.ContinueMessage, RenderOptions.SubnoteColor);
            Read();
        }

        /// <summary>
        /// Prompts the user to provide a string input.
        /// </summary>
        /// <param name="message">Prompt Message</param>
        /// <param name="color">Override the Prompt Message color</param>
        /// <param name="allowEmpty">Specifies whether the user can provide an empty response. Can result in string.Empty</param>
        /// <returns>User response (string)</returns>
        public static string Input(string message, ConsoleColor? color = null, bool allowEmpty = false)
        {
            string output = string.Empty;
            bool valid = false;
            do
            {
                Consoul._write(message, RenderOptions.GetColor(color));
                output = Read();
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
        /// Waits for user input and reads the user response.
        /// </summary>
        /// <returns>Value from the user</returns>
        public static string Read()
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
                input.Value = Console.ReadLine();
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
        public static bool Ask(string message, bool clear = false, bool allowEmpty = false, bool defaultIsNo = true)
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
                input = Read();// Console.ReadLine();
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
        /// <returns></returns>
        public static int Prompt(string message, bool clear = false, params string[] options)
        {
            return (new Prompt(message, clear, options)).Run();
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
        public static string PromptForFilepath(string message, bool checkExists, ConsoleColor? color = null) {
            string path;
            do
            {
                Consoul.Write(message, color);
                path = Consoul.Read();
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
        public static string PromptForFilepath(string defaultPath, string message, bool checkExists, ConsoleColor? color = null) {
            string path = defaultPath;
            if (!File.Exists(path) || !Consoul.Ask($"Would you like to use the default path:\r\n{path}", defaultIsNo: false)) {
                path = PromptForFilepath(message, checkExists, color);
            }
            return path;
        }
    }
}
