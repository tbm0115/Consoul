using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ConsoulLibrary {
    /// <summary>
    /// Loads and saves scripted Consoul routines in XML format.
    /// </summary>
    public class XmlRoutine : Routine
    {
        private XmlDocument _xml { get; set; }

        /// <summary>
        /// Gets or sets the routine document name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the time the routine document was created.
        /// </summary>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the named routine inputs stored by the XML document.
        /// </summary>
        public Dictionary<string, List<RoutineInput>> XmlRoutines { get; set; } = new Dictionary<string, List<RoutineInput>>();

        /// <summary>
        /// Gets or sets variables associated with the XML routine document.
        /// </summary>
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new XML routine document.
        /// </summary>
        public XmlRoutine() {
            _xml = new XmlDocument();
            Name = Guid.NewGuid().ToString("n");
            if (Routines.UserInputs.Any())
                XmlRoutines.Add(Guid.NewGuid().ToString("n"), Routines.UserInputs.ToList());
        }

        /// <summary>
        /// Loads an XML routine document from disk.
        /// </summary>
        /// <param name="filepath">Path to the XML routine file.</param>
        /// <param name="routineName">Optional named routine to enqueue from the document.</param>
        public XmlRoutine(string filepath, string routineName = "") : this() {
            _xml.Load(filepath);
            XmlRoutines.Clear();

            XmlNode xMeta = _xml.SelectSingleNode("//Meta");
            Name = xMeta["Name"].InnerText;
            DateCreated = DateTime.Parse(xMeta["DateCreated"].InnerText);
            if (xMeta.SelectSingleNode("UseDelays") != null)
                UseDelays = bool.Parse(xMeta["UseDelays"].InnerText);
            Variables = ReadVariables(xMeta);

            string xPath = "//Routine";

            XmlNodeList xRoutines = _xml.SelectNodes(xPath);
            for (int i = 0; i < xRoutines.Count; i++)
            {
                string xRoutineName = xRoutines[i].Attributes["name"]?.Value ?? Guid.NewGuid().ToString("n");
                if (!XmlRoutines.ContainsKey(xRoutineName))
                    XmlRoutines.Add(xRoutineName, new List<RoutineInput>());
                Dictionary<string, string> routineVariables = MergeVariables(
                    Variables,
                    ReadVariables(xRoutines[i].SelectSingleNode("Meta")));
                XmlNodeList xInputs = xRoutines[i].SelectNodes("Inputs/Input");
                foreach (XmlNode xInput in xInputs)
                    XmlRoutines[xRoutineName].Add(ApplyVariables(new RoutineInput(xInput), routineVariables));
            }

            List<RoutineInput> selectedRoutineInputs;
            if (!string.IsNullOrEmpty(routineName))
                XmlRoutines.TryGetValue(routineName, out selectedRoutineInputs);
            else
                selectedRoutineInputs = XmlRoutines.Values.First();

            if (selectedRoutineInputs != null)
                foreach (RoutineInput selectedRoutineInput in selectedRoutineInputs)
                    base.Enqueue(selectedRoutineInput);
        }

        /// <summary>
        /// Saves recorded routine inputs to an XML file.
        /// </summary>
        /// <param name="filepath">Destination XML file path.</param>
        public void SaveInputs(string filepath) {
            XmlNode xRoot, xMeta;
            if (_xml != null)
            {
                _xml = new XmlDocument();
                _xml.AppendChild(_xml.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                xRoot = _xml.AppendChild(_xml.CreateElement("XmlRoutineDoc"));
                xMeta = xRoot.AppendChild(_xml.CreateElement("Meta"));
                xMeta.AppendChild(_xml.CreateElement("DateCreated"));
                xMeta.AppendChild(_xml.CreateElement("Name"));
                xMeta.AppendChild(_xml.CreateElement("UseDelays"));
            }
            else
            {
                xRoot = _xml.SelectSingleNode("//XmlRoutineDoc");
                xMeta = xRoot.SelectSingleNode("Meta");
            }
            xMeta["DateCreated"].InnerText = DateCreated.ToString();
            xMeta["Name"].InnerText = Name;
            xMeta["UseDelays"].InnerText = (UseDelays || Routines.UseDelays).ToString();
            AppendVariables(_xml, xMeta, Variables);

            XmlNode xRoutines = xRoot["Routines"] ?? xRoot.AppendChild(_xml.CreateElement("Routines"));
            xRoutines.RemoveAll(); // Clear all Routines to avoid duplications
            foreach (KeyValuePair<string, List<RoutineInput>> routine in XmlRoutines)
            {
                XmlNode xRoutine = xRoutines.AppendChild(_xml.CreateElement("Routine"));
                XmlAttribute xRoutineName = _xml.CreateAttribute("name");
                xRoutineName.Value = routine.Key;
                xRoutine.Attributes.Append(xRoutineName);
                XmlNode xRoutineMeta = xRoutine.AppendChild(_xml.CreateElement("Meta"));
                xRoutineMeta.AppendChild(_xml.CreateElement("Description")).AppendChild(_xml.CreateCDataSection("{Describe this Routine}"));

                XmlNode xInputs = xRoutine.AppendChild(_xml.CreateElement("Inputs"));
                RoutineInput[] userInputs = Routines.UserInputs.ToArray().Reverse().ToArray();
                foreach (RoutineInput userInput in userInputs)
                    xInputs.AppendChild(userInput.ToXmlNode(_xml));
            }

            _xml.Save(filepath);
        }

        private static RoutineInput ApplyVariables(RoutineInput input, IDictionary<string, string> variables)
        {
            if (input == null || variables == null || variables.Count == 0)
            {
                return input;
            }

            var transforms = input.Transforms == null
                ? new List<InputTransform>()
                : input.Transforms.Where(transform => transform != null).ToList();
            var existingKeys = new HashSet<string>(
                transforms
                    .Where(transform => !string.IsNullOrWhiteSpace(transform.Key))
                    .Select(transform => transform.Key),
                StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> variable in variables)
            {
                if (string.IsNullOrWhiteSpace(variable.Key) || existingKeys.Contains(variable.Key))
                {
                    continue;
                }

                transforms.Add(new InputTransform
                {
                    Key = variable.Key,
                    Value = variable.Value ?? string.Empty,
                    UseAppSettings = false
                });
            }

            input.Transforms = transforms.ToArray();
            return input;
        }

        private static Dictionary<string, string> MergeVariables(
            IDictionary<string, string> documentVariables,
            IDictionary<string, string> routineVariables)
        {
            var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (documentVariables != null)
            {
                foreach (KeyValuePair<string, string> variable in documentVariables)
                {
                    if (!string.IsNullOrWhiteSpace(variable.Key))
                    {
                        merged[variable.Key] = variable.Value ?? string.Empty;
                    }
                }
            }

            if (routineVariables != null)
            {
                foreach (KeyValuePair<string, string> variable in routineVariables)
                {
                    if (!string.IsNullOrWhiteSpace(variable.Key))
                    {
                        merged[variable.Key] = variable.Value ?? string.Empty;
                    }
                }
            }

            return merged;
        }

        private static Dictionary<string, string> ReadVariables(XmlNode parent)
        {
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            XmlNode xVariables = parent?.SelectSingleNode("Variables");
            if (xVariables == null)
            {
                return variables;
            }

            foreach (XmlNode xVariable in xVariables.ChildNodes)
            {
                if (xVariable.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                string key;
                string value;
                if (string.Equals(xVariable.Name, "Variable", StringComparison.OrdinalIgnoreCase))
                {
                    key = ReadVariableKey(xVariable);
                    value = ReadVariableValue(xVariable);
                }
                else
                {
                    key = xVariable.Name;
                    value = xVariable.InnerText;
                }

                if (!string.IsNullOrWhiteSpace(key))
                {
                    variables[key.Trim()] = value ?? string.Empty;
                }
            }

            return variables;
        }

        private static string ReadVariableKey(XmlNode xVariable)
        {
            return xVariable.Attributes?["key"]?.Value
                ?? xVariable.Attributes?["name"]?.Value
                ?? xVariable["Key"]?.InnerText
                ?? xVariable["Name"]?.InnerText;
        }

        private static string ReadVariableValue(XmlNode xVariable)
        {
            return xVariable.Attributes?["value"]?.Value
                ?? xVariable["Value"]?.InnerText
                ?? xVariable.InnerText
                ?? string.Empty;
        }

        private static void AppendVariables(XmlDocument xDoc, XmlNode parent, IDictionary<string, string> variables)
        {
            if (xDoc == null || parent == null || variables == null || variables.Count == 0)
            {
                return;
            }

            XmlNode existingVariables = parent.SelectSingleNode("Variables");
            if (existingVariables != null)
            {
                parent.RemoveChild(existingVariables);
            }

            XmlNode xVariables = parent.AppendChild(xDoc.CreateElement("Variables"));
            foreach (KeyValuePair<string, string> variable in variables
                .Where(variable => !string.IsNullOrWhiteSpace(variable.Key))
                .OrderBy(variable => variable.Key, StringComparer.OrdinalIgnoreCase))
            {
                XmlNode xVariable = xVariables.AppendChild(xDoc.CreateElement("Variable"));
                xVariable.AppendChild(xDoc.CreateElement("Key")).InnerText = variable.Key;
                xVariable.AppendChild(xDoc.CreateElement("Value")).InnerText = variable.Value ?? string.Empty;
            }
        }
    }
}
