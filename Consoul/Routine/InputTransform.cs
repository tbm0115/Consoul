using System.Xml;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a token replacement applied to a routine input value.
    /// </summary>
    public class InputTransform
    {
        /// <summary>
        /// Gets or sets the transform token key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets whether the replacement value should be read from routine settings.
        /// </summary>
        public bool UseAppSettings { get; set; }

        /// <summary>
        /// Gets or sets the explicit replacement value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes an empty input transform.
        /// </summary>
        public InputTransform()
        {

        }

        /// <summary>
        /// Initializes an input transform from an XML node.
        /// </summary>
        /// <param name="xNode">XML node containing transform data.</param>
        public InputTransform(XmlNode xNode)
        {
            Key = xNode["Key"].InnerText;
            Value = xNode["Value"].InnerText;
            UseAppSettings = bool.Parse(xNode["UseAppSettings"].InnerText);
        }

        /// <summary>
        /// Serializes this transform to an XML node.
        /// </summary>
        /// <param name="xDoc">XML document used to create nodes.</param>
        /// <returns>An XML node representing this transform.</returns>
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
