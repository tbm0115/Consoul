using System.Xml;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a prompt option captured for routine playback or recording.
    /// </summary>
    public class RegisteredOption
    {
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
            Prompt = xNode["Prompt"].FirstChild.InnerText;
            Text = xNode["Text"].FirstChild.InnerText;
            Index = int.Parse(xNode["Index"]?.InnerText);
        }

        /// <summary>
        /// Serializes this option to an XML node.
        /// </summary>
        /// <param name="xDoc">XML document used to create nodes.</param>
        /// <returns>An XML node representing this registered option.</returns>
        public XmlNode ToXmlNode(XmlDocument xDoc)
        {
            XmlNode xInput = xDoc.CreateElement("RegisteredOption");
            // TODO: Add Description and Groupings
            xInput.AppendChild(xDoc.CreateElement("Prompt")).AppendChild(xDoc.CreateCDataSection("\r\n" + Prompt + "\r\n"));
            xInput.AppendChild(xDoc.CreateElement("Choice")).InnerText = Index.ToString();
            xInput.AppendChild(xDoc.CreateElement("Text")).AppendChild(xDoc.CreateCDataSection(Text));

            return xInput;
        }
    }
}
