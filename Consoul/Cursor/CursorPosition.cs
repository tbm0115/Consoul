using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Structure for a position of the cursor in the console.
    /// </summary>
    public struct CursorPosition
    {
        /// <summary>
        /// <see cref="Console.CursorLeft"/>
        /// </summary>
        public int Left;

        /// <summary>
        /// <see cref="Console.CursorTop"/>
        /// </summary>
        public int Top;
    }
}
