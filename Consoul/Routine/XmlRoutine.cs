using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ConsoulLibrary {
    public class XmlRoutine : Routine
    {
        private XmlDocument _xml { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public Dictionary<string, List<RoutineInput>> XmlRoutines { get; set; } = new Dictionary<string, List<RoutineInput>>();

        public XmlRoutine() {
            _xml = new XmlDocument();
            Name = Guid.NewGuid().ToString("n");
            if (Routines.UserInputs.Any())
                XmlRoutines.Add(Guid.NewGuid().ToString("n"), Routines.UserInputs.ToList());
        }

        public XmlRoutine(string filepath, string routineName = "") : this() {
            _xml.Load(filepath);

            XmlNode xMeta = _xml.SelectSingleNode("//Meta");
            Name = xMeta.InnerText;
            DateCreated = DateTime.Parse(xMeta.SelectSingleNode("DateCreated").InnerText);
            if (xMeta.SelectSingleNode("UseDelays") != null)
                UseDelays = bool.Parse(xMeta.SelectSingleNode("UseDelays").InnerText);

            string xPath = "//Routine";

            XmlNodeList xRoutines = _xml.SelectNodes(xPath);
            for (int i = 0; i < xRoutines.Count; i++)
            {
                string xRoutineName = xRoutines[i].Attributes["name"]?.Value ?? Guid.NewGuid().ToString("n");
                if (!XmlRoutines.ContainsKey(xRoutineName))
                    XmlRoutines.Add(xRoutineName, new List<RoutineInput>());
                XmlNodeList xInputs = xRoutines[i].SelectNodes("Inputs/Input");
                foreach (XmlNode xInput in xInputs)
                    XmlRoutines[xRoutineName].Add(new RoutineInput(xInput));
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

            XmlNode xRoutines = xRoot["Routines"] ?? xRoot.AppendChild(_xml.CreateElement("Routines"));
            xRoutines.RemoveAll(); // Clear all Routines to avoid duplications
            foreach (KeyValuePair<string, List<RoutineInput>> routine in XmlRoutines)
            {
                XmlNode xRoutine = xRoutines.AppendChild(_xml.CreateElement("Routine"));
                XmlNode xInputs = xRoutine.AppendChild(_xml.CreateElement("Inputs"));
                RoutineInput[] userInputs = Routines.UserInputs.ToArray().Reverse().ToArray();
                foreach (RoutineInput userInput in userInputs)
                    xInputs.AppendChild(userInput.ToXmlNode(_xml));
            }

            _xml.Save(filepath);
        }
    }
}
