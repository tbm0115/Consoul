using System.Xml;

namespace ConsoulLibrary
{
    public class InputTransform
    {
        public string Key { get; set; }

        public bool UseAppSettings { get; set; }

        public string Value { get; set; }

        public InputTransform()
        {

        }

        public InputTransform(XmlNode xNode)
        {
            Key = xNode["Key"].InnerText;
            Value = xNode["Value"].InnerText;
            UseAppSettings = bool.Parse(xNode["UseAppSettings"].InnerText);
        }

        public XmlNode ToXmlNode(XmlDocument xDoc)
        {
            XmlNode xTransform = xDoc.CreateElement("Transform");
            xTransform.AppendChild(xDoc.CreateElement("Key")).InnerText = Key;
            xTransform.AppendChild(xDoc.CreateElement("Value")).InnerText = UseAppSettings ? string.Empty : Value;
            xTransform.AppendChild(xDoc.CreateElement("UseAppSettings")).InnerText = UseAppSettings.ToString();

            return xTransform;
        }
    }
}
