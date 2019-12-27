using System;
using System.Xml;

namespace ConsoulLibrary {
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

        public RoutineInput() {
            RequestTime = DateTime.UtcNow;
        }

        public RoutineInput(XmlNode xNode) : base() {
            Value = xNode.SelectSingleNode("Value").InnerText;
            if (!string.IsNullOrEmpty(xNode.SelectSingleNode("RequestTime")?.InnerText))
                RequestTime = DateTime.Parse(xNode.SelectSingleNode("RequestTime").InnerText);
            if (!string.IsNullOrEmpty(xNode.SelectSingleNode("ResponseTime")?.InnerText))
                ResponseTime = DateTime.Parse(xNode.SelectSingleNode("ResponseTime").InnerText);
            if (!string.IsNullOrEmpty(xNode.SelectSingleNode("Description")?.InnerText))
                Description = xNode.SelectSingleNode("Description").InnerText;
        }

        public XmlNode ToXmlNode(XmlDocument xDoc) {
            XmlNode xInput = xDoc.CreateElement("Input");
            // TODO: Add Description and Groupings
            xInput.AppendChild(xDoc.CreateElement("Value")).InnerText = Value;
            xInput.AppendChild(xDoc.CreateElement("RequestTime")).InnerText = RequestTime.ToString();
            xInput.AppendChild(xDoc.CreateElement("ResponseTime")).InnerText = ResponseTime.ToString();
            xInput.AppendChild(xDoc.CreateElement("Delay")).InnerText = Delay.Value.Ticks.ToString();
            xInput.AppendChild(xDoc.CreateElement("Description")).AppendChild(xDoc.CreateCDataSection(Description));

            return xInput;
        }
    }
}
