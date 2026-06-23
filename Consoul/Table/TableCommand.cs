namespace ConsoulLibrary
{
    /// <summary>
    /// Defines normalized table navigation commands.
    /// </summary>
    public enum TableCommand
    {
        /// <summary>
        /// Move the table cursor to the previous row.
        /// </summary>
        MoveUp,

        /// <summary>
        /// Move the table cursor to the next row.
        /// </summary>
        MoveDown,

        /// <summary>
        /// Toggle selection for the current row.
        /// </summary>
        ToggleSelection,

        /// <summary>
        /// Confirm the current row or typed row number.
        /// </summary>
        Confirm,

        /// <summary>
        /// Exit the table prompt without selecting a row.
        /// </summary>
        Exit,

        /// <summary>
        /// Represents an input that does not map to a table command.
        /// </summary>
        Invalid,
    }
}
