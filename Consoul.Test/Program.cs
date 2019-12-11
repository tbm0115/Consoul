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

            Person[] people = new Person[]{
                new Person(){
                    FirstName = "Trais",
                    LastName = "McAllister",
                    Age = 28
                },
                new Person(){
                    FirstName = "Samantha",
                    LastName = "McAllister",
                    Age = 27
                },
                new Person(){
                    FirstName = "Taran",
                    LastName = "McAllister",
                    Age = 18
                },
                new Person(){
                    FirstName = "Scott",
                    LastName = "McAllister",
                    Age = 52
                }
            };

            var table = new Table.TableView(
                people.ToList(), 
                new string[]{
                    nameof(Person.FirstName),
                    nameof(Person.LastName),
                    nameof(Person.Age)
                },
                new Table.TableRenderOptions(){
                    IncludeChoices = true,
                    SelectionColor = ConsoleColor.Green,
                    HeaderColor = ConsoleColor.DarkCyan,
                    ContentColor1 = ConsoleColor.White,
                    ContentColor2 = ConsoleColor.Gray,
                    Lines = new Table.TableRenderOptions.TableLineDisplayOptions(){
                        ContentVertical = false
                    }
                }
            );
            table.Selection = 2;
            table.Run();

            var view1 = new Welcome();
            view1.Run();
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
