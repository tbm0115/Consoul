using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace ConsoulLibrary {
    public class XmlRoutine : Routine
    {
        private XmlDocument _xml { get; set; }

        public XmlRoutine() {
            _xml = new XmlDocument();
        }

        public XmlRoutine(string filepath) : this() {
            _xml.Load(filepath);
            read();
        }

        private void read(){
            XmlNodeList xInputs = _xml.SelectNodes("//Input");
            foreach (XmlNode xInput in xInputs) {
                base.Enqueue(xInput.InnerText);
            }
        }

        public void SaveInputs(string filepath){
            _xml = new XmlDocument();
            _xml.AppendChild(_xml.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            XmlNode xRoot = _xml.AppendChild(_xml.CreateElement("Inputs"));

            string[] userInputs = Routines.UserInputs.ToArray().Reverse().ToArray();
            foreach (string userInput in userInputs) {
                XmlNode xInput = xRoot.AppendChild(_xml.CreateElement("Input"));
                xInput.InnerText = userInput;
            }

            _xml.Save(filepath);
        }
    }
}
