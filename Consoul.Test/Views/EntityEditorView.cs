using ConsoulLibrary;
using ConsoulLibrary.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary.Test.Views
{
    public class EntityEditorView : StaticView
    {
        public EntityEditorView()
        {
            Title = (new BannerEntry("Edit Object View")).Message;
        }

        [ViewOption("Edit Person")]
        internal void EditPerson()
        {
            var person = new Person();
            var view = new EditObjectView(person);
            view.Run();
            Consoul.Write($"{person.LastName}, {person.FirstName} created on {person.DateOfBirth?.ToString("MM/dd/yyyy")}!", ConsoleColor.Green);
            if (person.Spouse != null)
            {
                Consoul.Write($"\tMarried to {person.Spouse!.Spouse.FirstName} on {person.Spouse!.Anniversary.ToString("MM/dd/yyyy")}");
            }
            if (person.Children != null)
            {
                Consoul.Write("\tHas Children");
                foreach (var child in person.Children)
                {
                    Consoul.Write($"\t\t{child.Value.FirstName}, created on {child.Value.DateOfBirth?.ToString("MM/dd/yyyy")}");
                }
            }
            Consoul.Wait();
        }
    }
    public class Person {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public Marriage? Spouse { get; set; }

        public Dictionary<string, Person> Children { get; set; }
    }
    public class Marriage
    {
        public DateTime Anniversary { get; set; }

        public Person Spouse { get; set; }
    }
}
