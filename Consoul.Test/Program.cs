using System;
using ConsoulLibrary.Test.Views;
using ConsoulLibrary;
using System.Linq;

namespace ConsoulLibrary.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // **************************************************************************
            // **********************************Credit**********************************
            //
            // Notes:
            //     
            //     https://codereview.stackexchange.com/questions/36768/tiny-text-adventure 
            //
            // **************************************************************************
            Routines.MonitorInputs = true;
            Routines.InitializeRoutine(args);
            //Routines.UseDelays = true; // Showcases the usecase of reusing input delays to simulate user response

            // Person info from https://avatar.fandom.com/wiki
            Person[] people = new Person[]{
                new Person(){
                    FirstName = "Aang",
                    LastName = "",
                    Age = 112
                },
                new Person(){
                    FirstName = "Katara",
                    LastName = "",
                    Age = 14
                },
                new Person(){
                    FirstName = "Sokka",
                    LastName = "",
                    Age = 15
                },
                new Person(){
                    FirstName = "Zuko",
                    LastName = "",
                    Age = 16
                }
            };

            if (Consoul.Ask("Would you like to view the people of Avatar: The Last Airbender?"))
            {
                var table = new Table.TableView(
                    people.ToList(),
                    new string[]{
                    nameof(Person.FirstName),
                    nameof(Person.LastName),
                    nameof(Person.Age)
                    },
                    new Table.TableRenderOptions()
                    {
                        IncludeChoices = false,
                        SelectionColor = ConsoleColor.Green,
                        HeaderColor = ConsoleColor.DarkCyan,
                        ContentColor1 = ConsoleColor.White,
                        ContentColor2 = ConsoleColor.Gray,
                        Lines = new Table.TableRenderOptions.TableLineDisplayOptions()
                        {
                            ContentVertical = false
                        }
                    }
                );
                table.Append(
                    new Person()
                    {
                        FirstName = "Toph",
                        LastName = "Beifong",
                        Age = 12
                    },
                    true
                );
                Consoul.Wait();
            }

            var view1 = new Welcome();
            view1.Run();

            var xRoutine = new XmlRoutine();
            xRoutine.SaveInputs("Test.xml");
        }

        public class TestRoutine : Routine
        {
            public TestRoutine() : base(new string[]
            {
                "y", // Yes, show list of Persons
                string.Empty, // Skip Wait,
                "1", // Yes, let's start!
                "1", // Take it with you
                string.Empty, // Skip message
                "1", // What is this?
                string.Empty, // Skip message
                "1" // "Death to Spider!"
            })
            {
                // Should land on begining of Text game
            }
        }

        public class Person{
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }

            public Person() {

            }
        }
    }
}
