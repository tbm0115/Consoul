using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace ConsoulLibrary {
    public class XmlRoutine : Routine
    {
        private XmlDocument _xml { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;


        public XmlRoutine() {
            _xml = new XmlDocument();
            Name = Guid.NewGuid().ToString("n");
        }

        public XmlRoutine(string filepath) : this() {
            _xml.Load(filepath);

            XmlNode xMeta = _xml.SelectSingleNode("//Meta");
            Name = xMeta.InnerText;
            DateCreated = DateTime.Parse(xMeta.SelectSingleNode("DateCreated").InnerText);
            if (xMeta.SelectSingleNode("UseDelays") != null)
                UseDelays = bool.Parse(xMeta.SelectSingleNode("UseDelays").InnerText);

            XmlNodeList xRoutines = _xml.SelectNodes("//Routine");
            if (xRoutines.Count > 0)
                read(xRoutines[0]);
        }

        private void read(XmlNode xRoutine) {
            XmlNodeList xInputs = xRoutine.SelectNodes("Inputs/Input");
            foreach (XmlNode xInput in xInputs)
                base.Enqueue(new RoutineInput(xInput));
        }

        public void SaveInputs(string filepath) {
            _xml = new XmlDocument();
            _xml.AppendChild(_xml.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            XmlNode xRoot = _xml.AppendChild(_xml.CreateElement("XmlRoutineDoc"));
            XmlNode xMeta = xRoot.AppendChild(_xml.CreateElement("Meta"));
            xMeta.AppendChild(_xml.CreateElement("DateCreated")).InnerText = DateCreated.ToString();
            xMeta.AppendChild(_xml.CreateElement("Name")).InnerText = Name;
            xMeta.AppendChild(_xml.CreateElement("UseDelays")).InnerText = (UseDelays || Routines.UseDelays).ToString();

            XmlNode xRoutines = xRoot.AppendChild(_xml.CreateElement("Routines"));
            XmlNode xRoutine = xRoutines.AppendChild(_xml.CreateElement("Routine"));
            XmlNode xInputs = xRoutine.AppendChild(_xml.CreateElement("Inputs"));
            RoutineInput[] userInputs = Routines.UserInputs.ToArray().Reverse().ToArray();
            foreach (RoutineInput userInput in userInputs)
                xInputs.AppendChild(userInput.ToXmlNode(_xml));

            _xml.Save(filepath);
        }
    }
}
