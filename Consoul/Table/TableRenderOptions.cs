using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary.Table
{
    public class TableRenderOptions{
        public bool IncludeChoices { get; set; } = false;

        public TableLineDisplayOptions Lines { get; set; } = new TableLineDisplayOptions();

        public ConsoleColor HeaderColor { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor ContentColor1 { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor ContentColor2 { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor SelectionColor { get; set; } = RenderOptions.OptionColor;

        public string LeftPad { get; private set; }

        public string RightPad { get; private set; }


        public int? ColumnSize { get; private set; }

        public int? MaximumTableWidth { get; private set; }

        public bool IsNormalized { get; private set; } = false;

        private decimal _tableWidthPercentage { get; set; } = 0.8m;
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

        public TableRenderOptions() {

        }

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

        public class TableLineDisplayOptions {
            public char HorizontalCharacter { get; set; } = '-';

            public char VerticalCharacter { get; set; } = '|';

            public bool HeaderHorizontal { get; set; } = true;

            public bool HeaderVertical { get; set; } = true;

            public bool ContentHorizontal { get; set; } = true;

            public bool ContentVertical { get; set; } = true;

            public ConsoleColor Color { get; set; } = RenderOptions.SubnoteColor;

            public TableLineDisplayOptions() {

            }
        }
    }
}
