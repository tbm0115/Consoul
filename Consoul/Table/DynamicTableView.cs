using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ConsoulLibrary.Table
{
    public class DynamicTableView<TSource>
    {
        private TableRenderOptions _initialOptions { get; set; }
        public TableRenderOptions RenderOptions
        {
            get
            {
                return _table?.RenderOptions ?? _initialOptions;
            }
            set
            {
                if (_table == null)
                {
                    _initialOptions = value;
                }
                else
                {
                    _table.RenderOptions = value;
                }
            }
        }
        public List<TSource> Contents { get; set; } = new List<TSource>();

        public int? Selection
        {
            get
            {
                return _table?.Selection;
            }
            set
            {
                if (_table != null)
                {
                    _table.Selection = value;
                }
            }
        }

        private Dictionary<string, Delegate> propertyReferences = new Dictionary<string, Delegate>();

        private TableView _table { get; set; }

        public DynamicTableView(TableRenderOptions options = null)
        {
            RenderOptions = options ?? new TableRenderOptions();
        }

        public DynamicTableView(IEnumerable<TSource> source, TableRenderOptions options = null) : this(options)
        {
            Contents.AddRange(source);
        }

        public void AddHeader<T>(Expression<Func<TSource, T>> key, string label = "")
        {
            MemberExpression member = key.Body as MemberExpression;
            if (member != null)
            {
                PropertyInfo property = member.Member as PropertyInfo;
                if (property != null)
                {
                    Type keyType = property.PropertyType;
                    string keyName = label;
                    if (string.IsNullOrEmpty(keyName))
                    {
                        keyName = property.Name;
                    }
                    if (!propertyReferences.ContainsKey(keyName))
                    {
                        propertyReferences.Add(keyName, key.Compile());
                    }
                }
            }
        }

        /// <summary>
        /// Uses the dynamic property references to build the TableView
        /// </summary>
        public bool Build()
        {
            if (propertyReferences.Count <= 0)
            {
                Consoul.Write("Table Headers are not defined. Cannot build table without them.", ConsoleColor.Red);
                Consoul.Wait();
                return false;
            }
            _table = new TableView(RenderOptions);
            _table.Headers = propertyReferences.Keys.ToList();

            Contents.ForEach(o => Append(o));

            return true;
        }

        public void Append(TSource sourceItem, bool addToCache = false)
        {
            if (addToCache)
            {
                Contents.Add(sourceItem);
            }
            List<string> row = new List<string>();
            foreach (var propertyReference in propertyReferences)
            {
                object result = propertyReference.Value.DynamicInvoke(new object[] { sourceItem });
                string value = Convert.ToString(result);// string.Empty;
                row.Add(value);
            }
            _table.Contents.Add(row);
            //_table.Contents.Add(propertyReferences.Select(p => (p.Value.Compile().Invoke(sourceItem) as object)?.ToString()).ToList());
        }

        public void Write()
        {
            _table?.Write();
        }

        public int Prompt(bool allowEmpty = false)
        {
            if (_table == null)
            {
                if (!Build())
                {
                    return -1;
                }
            }
            return _table?.Prompt(allowEmpty) ?? -1;
        }

    }
}
