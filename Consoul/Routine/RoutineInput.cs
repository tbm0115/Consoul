using System;
using System.Collections.Generic;
using System.Xml;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a single scripted routine input and its playback metadata.
    /// </summary>
    public class RoutineInput {
        private string _value { get; set; }

        /// <summary>
        /// Gets or sets the input value, applying configured transforms when read.
        /// </summary>
        public string Value {
            get{
                string value = _value;
                if (Transforms?.Length > 0)
                {
                    RoutineSettingsSection transforms = Routines.getAppSettings().GetSection("Transforms");
                    foreach (InputTransform transform in Transforms)
                    {
                        if (transform.UseAppSettings)
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                value = value.Replace("{{" + transform.Key + "}}", transforms[transform.Key] ?? string.Empty);
                            }
                            else
                            {
                                value = transforms[transform.Key] ?? string.Empty;
                            }
                        }
                        else
                        {
                            value = value.Replace("{{" + transform.Key + "}}", transform.Value);
                        }
                    }
                }

                return value;
            }
            set{
                ResponseTime = DateTime.UtcNow;
                _value = value;
            }
        }

        /// <summary>
        /// Gets the elapsed time between the input request and response, when available.
        /// </summary>
        public TimeSpan? Delay => ResponseTime != null ? ResponseTime - RequestTime : null;

        /// <summary>
        /// Gets the time the input was requested.
        /// </summary>
        public DateTime RequestTime { get; private set; }

        /// <summary>
        /// Gets the time the input value was supplied.
        /// </summary>
        public DateTime? ResponseTime { get; private set; }

        /// <summary>
        /// Gets or sets descriptive text for the routine input.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the registered prompt option associated with this input.
        /// </summary>
        public RegisteredOption OptionReference { get; set; }

        /// <summary>
        /// Gets or sets transforms applied to the input value during playback.
        /// </summary>
        public InputTransform[] Transforms { get; set; }

        /// <summary>
        /// Gets or sets how this routine input should be interpreted.
        /// </summary>
        public InputMethod Method { get; set; } = InputMethod.Value;

        /// <summary>
        /// Defines how a routine input is matched to a prompt.
        /// </summary>
        public enum InputMethod
        {
            /// <summary>
            /// Use the input value directly.
            /// </summary>
            Value,

            /// <summary>
            /// Resolve the input by matching option text.
            /// </summary>
            OptionText
        }

        /// <summary>
        /// Initializes a routine input and records the request time.
        /// </summary>
        public RoutineInput() {
            RequestTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a routine input from an XML node.
        /// </summary>
        /// <param name="xNode">XML node containing serialized routine input data.</param>
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
            if (xNode["Transforms"] != null)
            {
                XmlNodeList xTransforms = xNode.SelectNodes("Transforms/Transform");
                List<InputTransform> transforms = new List<InputTransform>();
                foreach (XmlNode xTransform in xTransforms)
                {
                    transforms.Add(new InputTransform(xTransform));
                }
                Transforms = transforms.ToArray();
            }
        }

        /// <summary>
        /// Serializes this input to an XML node.
        /// </summary>
        /// <param name="xDoc">XML document used to create nodes.</param>
        /// <returns>An XML node representing this routine input.</returns>
        public XmlNode ToXmlNode(XmlDocument xDoc) {
            XmlNode xInput = xDoc.CreateElement("Input");
            // TODO: Add Description and Groupings
            xInput.AppendChild(xDoc.CreateElement("Value")).InnerText = _value;
            xInput.AppendChild(xDoc.CreateElement("RequestTime")).InnerText = RequestTime.ToString();
            xInput.AppendChild(xDoc.CreateElement("ResponseTime")).InnerText = ResponseTime.ToString();
            xInput.AppendChild(xDoc.CreateElement("Delay")).InnerText = Delay.Value.Ticks.ToString();
            xInput.AppendChild(xDoc.CreateElement("Method")).InnerText = Method.ToString();
            xInput.AppendChild(xDoc.CreateElement("Description")).AppendChild(xDoc.CreateCDataSection(Description));
            if (OptionReference != null)
                xInput.AppendChild(OptionReference.ToXmlNode(xDoc));
            var xInputTransforms = xInput.AppendChild(xDoc.CreateElement("Transforms"));
            if (Transforms?.Length > 0)
            {
                foreach (InputTransform inputTransform in Transforms)
                {
                    xInputTransforms.AppendChild(inputTransform.ToXmlNode(xDoc));
                }
            }

            return xInput;
        }
    }
}
