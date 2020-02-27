using System.Xml;

namespace ConsoulLibrary
{
    public class RegisteredOption
    {
        public string Prompt { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }

        public RegisteredOption()
        {

        }

        public RegisteredOption(XmlNode xNode) : this()
        {
            Prompt = xNode["Prompt"].FirstChild.InnerText;
            Text = xNode["Text"].FirstChild.InnerText;
            Index = int.Parse(xNode["Index"]?.InnerText);
        }

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
