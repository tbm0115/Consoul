using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a prompt option captured for routine playback or recording.
    /// </summary>
    public class RegisteredOption
    {
        private readonly List<string> _searchTerms = new List<string>();

        /// <summary>
        /// Gets or sets the prompt message that exposed the option.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the option display text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the zero-based option index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets optional helper text associated with the prompt option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets optional shortcut text associated with the prompt option.
        /// </summary>
        public string Shortcut { get; set; }

        /// <summary>
        /// Gets hidden search terms associated with the prompt option.
        /// </summary>
        public IList<string> SearchTerms => _searchTerms;

        /// <summary>
        /// Gets or sets a value indicating whether the prompt option was disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets optional text explaining why the prompt option was disabled.
        /// </summary>
        public string DisabledReason { get; set; }

        /// <summary>
        /// Initializes an empty registered option.
        /// </summary>
        public RegisteredOption()
        {
        }

        /// <summary>
        /// Initializes a registered option from an XML node.
        /// </summary>
        /// <param name="xNode">XML node containing option data.</param>
        public RegisteredOption(XmlNode xNode) : this()
        {
            Prompt = ReadText(xNode, "Prompt");
            Text = ReadText(xNode, "Text");
            Description = ReadText(xNode, "Description");
            Shortcut = ReadText(xNode, "Shortcut");
            DisabledReason = ReadText(xNode, "DisabledReason");
            IsDisabled = bool.TryParse(ReadText(xNode, "IsDisabled"), out bool isDisabled) && isDisabled;

            string indexText = ReadText(xNode, "Index");
            if (string.IsNullOrWhiteSpace(indexText))
            {
                indexText = ReadText(xNode, "Choice");
            }

            Index = int.TryParse(indexText, out int parsedIndex) ? parsedIndex : -1;

            XmlNodeList searchTermNodes = xNode == null ? null : xNode.SelectNodes("SearchTerms/Term");
            if (searchTermNodes == null)
            {
                return;
            }

            foreach (XmlNode searchTermNode in searchTermNodes)
            {
                string term = searchTermNode.InnerText;
                if (!string.IsNullOrWhiteSpace(term))
                {
                    _searchTerms.Add(term.Trim());
                }
            }
        }

        /// <summary>
        /// Serializes this option to an XML node.
        /// </summary>
        /// <param name="xDoc">XML document used to create nodes.</param>
        /// <returns>An XML node representing this registered option.</returns>
        public XmlNode ToXmlNode(XmlDocument xDoc)
        {
            XmlNode xInput = xDoc.CreateElement("RegisteredOption");
            xInput.AppendChild(xDoc.CreateElement("Prompt")).AppendChild(xDoc.CreateCDataSection(Prompt ?? string.Empty));
            xInput.AppendChild(xDoc.CreateElement("Index")).InnerText = Index.ToString();
            xInput.AppendChild(xDoc.CreateElement("Choice")).InnerText = Index.ToString();
            xInput.AppendChild(xDoc.CreateElement("Text")).AppendChild(xDoc.CreateCDataSection(Text ?? string.Empty));
            xInput.AppendChild(xDoc.CreateElement("Description")).AppendChild(xDoc.CreateCDataSection(Description ?? string.Empty));
            xInput.AppendChild(xDoc.CreateElement("Shortcut")).InnerText = Shortcut ?? string.Empty;
            xInput.AppendChild(xDoc.CreateElement("IsDisabled")).InnerText = IsDisabled.ToString();
            xInput.AppendChild(xDoc.CreateElement("DisabledReason")).AppendChild(xDoc.CreateCDataSection(DisabledReason ?? string.Empty));

            XmlNode searchTerms = xInput.AppendChild(xDoc.CreateElement("SearchTerms"));
            foreach (string term in SearchTerms.Where(term => !string.IsNullOrWhiteSpace(term)))
            {
                searchTerms.AppendChild(xDoc.CreateElement("Term")).AppendChild(xDoc.CreateCDataSection(term.Trim()));
            }

            return xInput;
        }

        private static string ReadText(XmlNode node, string elementName)
        {
            XmlNode child = node == null ? null : node[elementName];
            return child == null ? string.Empty : child.InnerText;
        }
    }
}
