using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoulLibrary.Table
{
    public delegate void TableQueryYieldsNoResults(object sender, TableQueryYieldsNoResultsEventArgs e);

    public class TableQueryYieldsNoResultsEventArgs : EventArgs
    {
        public string Message { get; set; }

        public string Query { get; set; }

        public TableQueryYieldsNoResultsEventArgs(string message, string query)
        {
            Message = message;
            Query = query;
        }
    }
    public class TableView
    {
        public TableRenderOptions RenderOptions { get; set; }

        public List<List<string>> Contents { get; set; } = new List<List<string>>();

        public List<string> Headers { get; set; } = new List<string>();

        public int? Selection { get; set; }

        public int? CurrentRow { get; private set; }

        public string HorizontalLineString { get; set; }

        public string VerticalLineString { get; set; }

        public event TableQueryYieldsNoResults QueryYieldsNoResults;

        public TableView(TableRenderOptions options = null)
        {
            RenderOptions = options ?? new TableRenderOptions();
        }

        public TableView(string[][] contents, TableRenderOptions options = null) : this(contents.Select(o => o.ToList()).ToList(), options) 
        {

        }

        public TableView(IEnumerable<IEnumerable<string>> contents, TableRenderOptions options = null) : this(options)
        {
            Contents = contents.Select(o => o.ToList()).ToList();
            if (Contents.Count > 0){
                Headers = Contents[0];
                Contents.RemoveAt(0); // Remove Header Row
            }
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



        public void Write(bool clearConsole = true){
            RenderOptions.Normalize(Contents);

            if (clearConsole)
                Console.Clear(); // Clear view
            CurrentRow = 0;
            HorizontalLineString = new string(RenderOptions.Lines.HorizontalCharacter, (int)RenderOptions.MaximumTableWidth);
            VerticalLineString = RenderOptions.Lines.VerticalCharacter.ToString();

            Append(Headers);

            foreach (IEnumerable<string> row in Contents) 
            {
                Append(row);
            }
        }

        public int Prompt(string message = "", ConsoleColor? color = null, bool allowEmpty = false)
        {
            var prevRenderOptionChoice = RenderOptions.IncludeChoices;
            RenderOptions.IncludeChoices = true;

            int selection = -1;
            do
            {
                Write();
                if (!string.IsNullOrEmpty(message))
                {
                    Consoul.Write(message, color);
                }
                if (allowEmpty)
                {
                    Consoul.Write("Press Enter to continue", ConsoulLibrary.RenderOptions.SubnoteColor);
                }

                string input = Consoul.Read();
                if (string.IsNullOrEmpty(input) && allowEmpty)
                {
                    selection = Contents.Count + 1;
                    break;
                }
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
                                raiseQueryYieldsNoResults("Invalid Query! Query yielded multiple results. Try a more refined search.", input);
                            } 
                            else if (results.Count == 0)
                            {
                                raiseQueryYieldsNoResults("Invalid Query! Query yielded no results.", input);
                            }
                        }
                        else
                        {
                            raiseQueryYieldsNoResults($"Invalid Header reference! Could not find Header '{queryParts[0]}'.", input);
                        }
                    }
                    else
                    {
                        raiseQueryYieldsNoResults("Query-based selection not formatted correctly. Must be in {Header Name}={Value} format", input);
                    }
                } 
                else if (!int.TryParse(input, out selection) || selection <= 0 || selection > Contents.Count)
                {
                    Consoul.Write("Invalid selection!", ConsoleColor.Red);
                    Consoul.Wait();
                    selection = -1;
                }
            } while (selection < 0);

            RenderOptions.IncludeChoices = prevRenderOptionChoice;

            return selection - 1;
        }

        public void Append(IEnumerable<string> row, bool addToCache = false)
        {
            if (!RenderOptions.IsNormalized)
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
                Consoul.Write(RenderOptions.LeftPad + HorizontalLineString, RenderOptions.Lines.Color);

            Consoul.Write(RenderOptions.LeftPad, writeLine: false);
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
                    RenderOptions.ColumnSize.Value,
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


        private void raiseQueryYieldsNoResults(string message, string query)
        {
            if (QueryYieldsNoResults != null)
                QueryYieldsNoResults(this, new TableQueryYieldsNoResultsEventArgs(message, query));

            Consoul.Write(message, ConsoleColor.Red);
            Consoul.Wait();
        }
    }
}
