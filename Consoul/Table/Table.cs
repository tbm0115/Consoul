using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoulLibrary.Table {
    public class TableView
    {
        public TableRenderOptions RenderOptions { get; set; }

        public List<List<string>> Contents { get; set; } = new List<List<string>>();

        public List<string> Headers { get; set; } = new List<string>();

        public int? Selection { get; set; }

        public int? CurrentRow { get; private set; }

        public string LeftPad { get; private set; }
        public string RightPad { get; private set; }
        public string HorizontalLineString { get; private set; }
        public string VerticalLineString { get; private set; }
        public int? ColumnSize { get; private set; }
        public int? MaximumTableWidth { get; private set; }
        public bool IsNormalized { get; private set; } = false;

        public TableView(string[][] contents, TableRenderOptions options = null) : this(contents.Select(o => o.ToList()).ToList(), options) 
        {

        }

        public TableView(IEnumerable<IEnumerable<string>> contents, TableRenderOptions options = null)
        {
            Contents = contents.Select(o => o.ToList()).ToList();
            if (Contents.Count > 0){
                Headers = Contents[0];
                Contents.RemoveAt(0); // Remove Header Row
            }
            RenderOptions = options ?? new TableRenderOptions();
        }

        public TableView(IEnumerable<object> source, string[] properties, TableRenderOptions options = null)
        {
            RenderOptions = options ?? new TableRenderOptions();


            Headers = properties.ToList(); // Add Column Header Row

            Type enumerableType = source.GetType().GetGenericArguments()[0];
            var typeProps = enumerableType.GetProperties();
            List<PropertyInfo> columns = new List<PropertyInfo>();
            foreach (string header in Headers)
            {
                PropertyInfo typeProp = typeProps.FirstOrDefault(o => header.Equals(o.Name, StringComparison.OrdinalIgnoreCase));
                if (typeProp != null)
                {
                    columns.Add(typeProp);
                }
                else
                {
                    Consoul.Write($"Couldn't find property by the name '{header}'.", ConsoleColor.Red);
                }
            }
            foreach (object sourceItem in source) {
                List<string> row = new List<string>();
                foreach (PropertyInfo property in columns) {
                    row.Add(property.GetValue(sourceItem).ToString());
                }
                Contents.Add(row);
            }
        }

        public void Normalize() {
            MaximumTableWidth = (int)(Console.BufferWidth * RenderOptions.TableWidthPercentage);
            int widthRemainder = Console.BufferWidth - (int)MaximumTableWidth;
            int marginLeft, marginRight;
            marginRight = widthRemainder / 2;
            marginLeft = marginRight;
            if (widthRemainder % 2 != 0)
                marginLeft = (widthRemainder + 1) / 2;

            int columnCount = Contents.Max(o => o.Count) + (RenderOptions.IncludeChoices ? 1 : 0);
            ColumnSize = MaximumTableWidth / columnCount;
            int maximumColumnSize = Contents.SelectMany(o => o).Max(o => o.Length);
            int minimumTableWidth = (maximumColumnSize + 3) * columnCount;

            LeftPad = new string(' ', marginLeft);
            RightPad = new string(' ', marginRight);

            IsNormalized = true;
        }

        public void Write(){
            Normalize();

            Console.Clear(); // Clear view
            CurrentRow = 0;
            HorizontalLineString = new string(RenderOptions.Lines.HorizontalCharacter, (int)MaximumTableWidth);
            VerticalLineString = RenderOptions.Lines.VerticalCharacter.ToString();

            Append(Headers);

            foreach (IEnumerable<string> row in Contents) 
            {
                Append(row);
            }
            //if (RenderOptions.Lines.ContentHorizontal)
            //    Consoul.Write(LeftPad + HorizontalLineString, RenderOptions.Lines.Color);
        }

        public int Prompt()
        {
            var prevRenderOptionChoice = RenderOptions.IncludeChoices;
            RenderOptions.IncludeChoices = true;

            int selection = -1;
            do
            {
                Write();

                string input = Consoul.Read();
                if (input.Contains("="))
                {
                    string[] queryParts = input.Split('=');
                    if (queryParts.Length == 2)
                    {
                        int columnIndex = Headers.IndexOf(queryParts[0]);
                        if (columnIndex >= 0)
                        {
                            List<int> results = new List<int>();
                            for (int i = 0; i < Contents.Count; i++)
                            {
                                if (Contents[i][columnIndex] == queryParts[1])
                                {
                                    results.Add(i);
                                }
                            }
                            if (results.Count == 1)
                            {
                                selection = results.First() + 1; // selection is expected as one-based
                            }
                            else if(results.Count > 1)
                            {
                                Consoul.Write("Invalid Query! Query yielded multiple results. Try a more refined search.", ConsoleColor.Red);
                                Consoul.Wait();
                            } 
                            else if (results.Count == 0)
                            {
                                Consoul.Write("Invalid Query! Query yielded no results.", ConsoleColor.Red);
                                Consoul.Wait();
                            }
                        }
                        else
                        {
                            Consoul.Write($"Invalid Header reference! Could not find Header '{queryParts[0]}'.", ConsoleColor.Red);
                            Consoul.Wait();
                        }
                    }
                    else
                    {
                        Consoul.Write("Query-based selection not formatted correctly. Must be in {Header Name}={Value} format", ConsoleColor.Red);
                        Consoul.Wait();
                    }
                } 
                else if (!int.TryParse(input, out selection) || selection >= Contents.Count)
                {
                    Consoul.Write("Invalid selection!", ConsoleColor.Red);
                    Consoul.Wait();
                }
            } while (selection < 0);

            RenderOptions.IncludeChoices = prevRenderOptionChoice;

            return selection - 1;
        }

        public void Append(IEnumerable<string> row, bool addToCache = false)
        {
            if (!IsNormalized)
                Write();

            List<string> rowContents = row.ToList();
            if (RenderOptions.IncludeChoices){
                if (CurrentRow == 0){
                    rowContents.Insert(0, "Choose");
                }else{
                    rowContents.Insert(0, (CurrentRow).ToString());
                }
            }
            if ((CurrentRow <= 1 && RenderOptions.Lines.HeaderHorizontal) || (CurrentRow > 1 && RenderOptions.Lines.ContentHorizontal))
                Consoul.Write(LeftPad + HorizontalLineString, RenderOptions.Lines.Color);

            Consoul.Write(LeftPad, writeLine: false);
            int columnIndex = 0;
            foreach (string column in rowContents) 
            {
                if ((CurrentRow == 0 && RenderOptions.Lines.HeaderVertical) || (CurrentRow != 0 && RenderOptions.Lines.ContentVertical))
                {
                    Consoul.Write(VerticalLineString, RenderOptions.Lines.Color, false);
                }
                else
                {
                    Consoul.Write(" ", RenderOptions.Lines.Color, false);
                }
                Consoul.Center(
                    column, 
                    (int)ColumnSize,
                    CurrentRow == 0 
                        ? RenderOptions.HeaderColor 
                        : CurrentRow == Selection
                            ? RenderOptions.SelectionColor
                            : (CurrentRow % 2) == 0 
                                ? RenderOptions.ContentColor1
                                : RenderOptions.ContentColor2, 
                    false
                );
                columnIndex++;
            }
            if ((CurrentRow == 0 && RenderOptions.Lines.HeaderVertical) || (CurrentRow != 0 && RenderOptions.Lines.ContentVertical))
            {
                Consoul.Write(VerticalLineString, RenderOptions.Lines.Color);
            }
            else 
            {
                Consoul.Write(" ", RenderOptions.Lines.Color);
            }
            if (addToCache)
                Contents.Add(row.ToList());
            CurrentRow++;
        }

        public void Append(object source, bool addToCache = false) {
            Type enumerableType = source.GetType();
            List<PropertyInfo> columns = enumerableType.GetProperties().Where(o => Headers.Contains(o.Name)).ToList();
            List<string> row = new List<string>();
            foreach (PropertyInfo property in columns) {
                row.Add(property.GetValue(source).ToString());
            }
            Append(row, addToCache);
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
