using System;

namespace ConsoulLibrary
{
    public class TableInputHandler
    {
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

            var keyInfo = Console.ReadKey(true);
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
