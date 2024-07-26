using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary.Table
{
    /// <summary>
    /// Options for how a <see cref="TableView"/> is rendered.
    /// </summary>
    public class TableRenderOptions {
        public bool IncludeChoices { get; set; } = false;

        /// <summary>
        /// Render options for the row and column lines.
        /// </summary>
        public TableLineDisplayOptions Lines { get; set; } = new TableLineDisplayOptions();

        /// <summary>
        /// Color for the header.
        /// </summary>
        public ConsoleColor HeaderColor { get; set; } = RenderOptions.DefaultColor;

        /// <summary>
        /// Text color option 1.
        /// </summary>
        public ConsoleColor ContentColor1 { get; set; } = RenderOptions.DefaultColor;

        /// <summary>
        /// Text color option 2.
        /// </summary>
        public ConsoleColor ContentColor2 { get; set; } = RenderOptions.DefaultColor;

        /// <summary>
        /// Text color for row(s) that are selected.
        /// </summary>
        public ConsoleColor SelectionColor { get; set; } = RenderOptions.OptionColor;

        /// <summary>
        /// Left padding.
        /// </summary>
        public string LeftPad { get; private set; }

        /// <summary>
        /// Right padding.
        /// </summary>
        public string RightPad { get; private set; }

        /// <summary>
        /// Width of column.
        /// </summary>
        public int? ColumnSize { get; private set; }

        /// <summary>
        /// Maximum width of the table.
        /// </summary>
        public int? MaximumTableWidth { get; private set; }

        public bool IsNormalized { get; private set; } = false;

        private decimal _tableWidthPercentage { get; set; } = 0.8m;
        /// <summary>
        /// Ratio of table width to the console width.
        /// </summary>
        public decimal TableWidthPercentage {
            get{
                return _tableWidthPercentage;
            }
            set{
                if (value > 1 || value < 0)
                    throw new ArgumentOutOfRangeException("Value must be between 0.0 and 1.0.");
                _tableWidthPercentage = value;
            }
        }

        /// <summary>
        /// Constructs a new instance of the render options.
        /// </summary>
        public TableRenderOptions() { }

        public void Normalize(IEnumerable<IEnumerable<string>> contents)
        {
            MaximumTableWidth = (int)(Console.BufferWidth * TableWidthPercentage);
            int widthRemainder = Console.BufferWidth - (int)MaximumTableWidth;
            int marginLeft, marginRight;
            marginRight = widthRemainder / 2;
            marginLeft = marginRight;
            if (widthRemainder % 2 != 0)
                marginLeft = (widthRemainder + 1) / 2;

            int columnCount = contents.Max(o => o.Count()) + (IncludeChoices ? 1 : 0);
            ColumnSize = MaximumTableWidth / columnCount;
            int maximumColumnSize = contents.SelectMany(o => o).Max(o => o.Length);
            int minimumTableWidth = (maximumColumnSize + 3) * columnCount;

            LeftPad = new string(' ', marginLeft);
            RightPad = new string(' ', marginRight);

            IsNormalized = true;
        }

        /// <summary>
        /// Contains settings for how <see cref="TableView"/> lines are rendered.
        /// </summary>
        public class TableLineDisplayOptions {
            /// <summary>
            /// Character used to render horizontal lines.
            /// </summary>
            public char HorizontalCharacter { get; set; } = '-';

            /// <summary>
            /// Character used to render vertical lines.
            /// </summary>
            public char VerticalCharacter { get; set; } = '|';

            /// <summary>
            /// Flags whether to render a horizontal line for the header of the table.
            /// </summary>
            public bool HeaderHorizontal { get; set; } = true;

            /// <summary>
            /// Flags whether to render vertical lines for the header of the table.
            /// </summary>
            public bool HeaderVertical { get; set; } = true;

            /// <summary>
            /// Flags whether to render horizontal lines for the contents of the table.
            /// </summary>
            public bool ContentHorizontal { get; set; } = true;

            /// <summary>
            /// Flags whether to render vertical lines for the contents of the table.
            /// </summary>
            public bool ContentVertical { get; set; } = true;

            /// <summary>
            /// Color of the lines.
            /// </summary>
            public ConsoleColor Color { get; set; } = RenderOptions.SubnoteColor;

            /// <summary>
            /// Constructs a new instance of <see cref="TableLineDisplayOptions"/>
            /// </summary>
            public TableLineDisplayOptions() { }
        }
    }
}
