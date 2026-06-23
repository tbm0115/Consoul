using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Converts keyboard input into table navigation commands.
    /// </summary>
    public class TableInputHandler
    {
        /// <summary>
        /// Reads or maps input into a <see cref="TableCommand"/>.
        /// </summary>
        /// <param name="input">Optional key name to map without reading from the console.</param>
        /// <returns>The mapped table command.</returns>
        public TableCommand GetCommand(string input = null)
        {
            if (input != null)
            {
                // Handle keyboard input from user
                if (input == "UpArrow") return TableCommand.MoveUp;
                if (input == "DownArrow") return TableCommand.MoveDown;
                if (input == "Spacebar") return TableCommand.ToggleSelection;
                if (input == "Enter") return TableCommand.Confirm;
                if (input == "Escape") return TableCommand.Exit;
            }

            var keyInfo = Consoul.ConsoleDriver.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: return TableCommand.MoveUp;
                case ConsoleKey.DownArrow: return TableCommand.MoveDown;
                case ConsoleKey.Spacebar: return TableCommand.ToggleSelection;
                case ConsoleKey.Enter: return TableCommand.Confirm;
                case ConsoleKey.Escape: return TableCommand.Exit;
                default:
                    return TableCommand.Invalid;
            }
        }
    }
}
