using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoulLibrary.Table {
    public class TableView
    {
        public TableRenderOptions RenderOptions { get; set; }

        public string[][] Contents { get; set; }

        public int? Selection { get; set; }

        public TableView(string[][] contents, TableRenderOptions options = null) 
        {
            Contents = contents;
            RenderOptions = options ?? new TableRenderOptions();
        }

        public TableView(IEnumerable<IEnumerable<string>> contents, TableRenderOptions options = null) : this(contents.Select(o => o.ToArray()).ToArray(), options)
        {

        }

        public TableView(IEnumerable<object> source, string[] properties, TableRenderOptions options = null)
        {
            RenderOptions = options ?? new TableRenderOptions();

            Type enumerableType = source.GetType().GetGenericArguments()[0];
            IEnumerable<PropertyInfo> columns = enumerableType.GetProperties().Where(o => properties.Contains(o.Name));

            List<List<string>> contents = new List<List<string>>();
            contents.Add(columns.Select(o => o.Name).ToList()); // Add Column Header Row
            foreach (object sourceItem in source) {
                List<string> row = new List<string>();
                foreach (PropertyInfo property in columns) {
                    row.Add(property.GetValue(sourceItem).ToString());
                }
                contents.Add(row);
            }
            Contents = contents.Select(o => o.ToArray()).ToArray();
        }

        public void Run(){
            List<List<string>> contents = Contents.Select(o => o.ToList()).ToList();
            if (RenderOptions.IncludeChoices){
                contents[0].Insert(0, "Choose");
                for (int i = 1; i < contents.Count; i++) {
                    contents[i].Insert(0, i.ToString());
                }
            }

            int maximumTableWidth = (int)(Console.BufferWidth * RenderOptions.TableWidthPercentage);
            int widthRemainder = Console.BufferWidth - maximumTableWidth;
            int marginLeft, marginRight;
            marginRight = widthRemainder / 2;
            marginLeft = marginRight;
            if (widthRemainder % 2 != 0)
                marginLeft = (widthRemainder + 1) / 2;

            int columnCount = contents.Max(o => o.Count);
            int columnSize = maximumTableWidth / columnCount;
            int maximumColumnSize = contents.SelectMany(o => o).Max(o => o.Length);
            int minimumTableWidth = (maximumColumnSize + 3) * columnCount;

            string leftPad = new string(' ', marginLeft);
            string rightPad = new string(' ', marginRight);

            int rowIndex = 0;
            string horizontalLine = new string(RenderOptions.Lines.HorizontalCharacter, maximumTableWidth);
            string verticalLine = RenderOptions.Lines.VerticalCharacter.ToString();
            foreach (IEnumerable<string> row in contents) 
            {
                if ((rowIndex <= 1 && RenderOptions.Lines.HeaderHorizontal) || (rowIndex > 1 && RenderOptions.Lines.ContentHorizontal))
                    Consoul.Write(leftPad + horizontalLine, RenderOptions.Lines.Color);

                Consoul.Write(leftPad, writeLine: false);
                int columnIndex = 0;
                foreach (string column in row) 
                {
                    if ((rowIndex == 0 && RenderOptions.Lines.HeaderVertical) || (rowIndex != 0 && RenderOptions.Lines.ContentVertical))
                    {
                        Consoul.Write(verticalLine, RenderOptions.Lines.Color, false);
                    }
                    else
                    {
                        Consoul.Write(" ", RenderOptions.Lines.Color, false);
                    }
                    Consoul.Center(
                        column, 
                        columnSize, 
                        rowIndex == 0 
                            ? RenderOptions.HeaderColor 
                            : rowIndex == Selection
                                ? RenderOptions.SelectionColor
                                : (rowIndex % 2) == 0 
                                    ? RenderOptions.ContentColor1
                                    : RenderOptions.ContentColor2, 
                        false
                    );
                    columnIndex++;
                }
                if ((rowIndex == 0 && RenderOptions.Lines.HeaderVertical) || (rowIndex != 0 && RenderOptions.Lines.ContentVertical))
                {
                    Consoul.Write(verticalLine, RenderOptions.Lines.Color);
                }
                else 
                {
                    Consoul.Write(" ", RenderOptions.Lines.Color);
                }
                rowIndex++;
            }
            if (RenderOptions.Lines.ContentHorizontal)
                Consoul.Write(leftPad + horizontalLine, RenderOptions.Lines.Color);
            Consoul.Wait();
        }
    }
    public class TableRenderOptions{
        public bool IncludeChoices { get; set; } = false;

        public TableLineDisplayOptions Lines { get; set; } = new TableLineDisplayOptions();

        public ConsoleColor HeaderColor { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor ContentColor1 { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor ContentColor2 { get; set; } = RenderOptions.DefaultColor;

        public ConsoleColor SelectionColor { get; set; } = RenderOptions.OptionColor;


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
