using System;
using System.Xml;

namespace ConsoulLibrary
{
    public class RoutineInput {
        private string _value { get; set; }
        public string Value {
            get{
                return _value;
            }
            set{
                ResponseTime = DateTime.UtcNow;
                _value = value;
            }
        }

        public TimeSpan? Delay => ResponseTime != null ? ResponseTime - RequestTime : null;

        public DateTime RequestTime { get; private set; }

        public DateTime? ResponseTime { get; private set; }

        public string Description { get; set; } = string.Empty;

        public RegisteredOption OptionReference { get; set; }

        public InputMethod Method { get; set; } = InputMethod.Value;

        public enum InputMethod
        {
            Value,
            OptionText
        }

        public RoutineInput() {
            RequestTime = DateTime.UtcNow;
        }

        public RoutineInput(XmlNode xNode) : this() {
            Value = xNode["Value"]?.InnerText;
            if (!string.IsNullOrEmpty(xNode["RequestTime"]?.InnerText))
                RequestTime = DateTime.Parse(xNode["RequestTime"].InnerText);
            if (!string.IsNullOrEmpty(xNode["ResponseTime"]?.InnerText))
                ResponseTime = DateTime.Parse(xNode["ResponseTime"].InnerText);
            if (!string.IsNullOrEmpty(xNode["Method"]?.InnerText))
            {
                Method = (InputMethod)Enum.Parse(typeof(InputMethod), xNode["Method"].InnerText);
            }
            Description = xNode["Description"]?.InnerText;
        }

        public XmlNode ToXmlNode(XmlDocument xDoc) {
            XmlNode xInput = xDoc.CreateElement("Input");
            // TODO: Add Description and Groupings
            xInput.AppendChild(xDoc.CreateElement("Value")).InnerText = Value;
            xInput.AppendChild(xDoc.CreateElement("RequestTime")).InnerText = RequestTime.ToString();
            xInput.AppendChild(xDoc.CreateElement("ResponseTime")).InnerText = ResponseTime.ToString();
            xInput.AppendChild(xDoc.CreateElement("Delay")).InnerText = Delay.Value.Ticks.ToString();
            xInput.AppendChild(xDoc.CreateElement("Method")).InnerText = Method.ToString();
            xInput.AppendChild(xDoc.CreateElement("Description")).AppendChild(xDoc.CreateCDataSection(Description));
            if (OptionReference != null)
                xInput.AppendChild(OptionReference.ToXmlNode(xDoc));

            return xInput;
        }
    }
}
