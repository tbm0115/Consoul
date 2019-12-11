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

            var table = new Table.TableView(
                people.ToList(), 
                new string[]{
                    nameof(Person.FirstName),
                    nameof(Person.LastName),
                    nameof(Person.Age)
                },
                new Table.TableRenderOptions(){
                    IncludeChoices = false,
                    SelectionColor = ConsoleColor.Green,
                    HeaderColor = ConsoleColor.DarkCyan,
                    ContentColor1 = ConsoleColor.White,
                    ContentColor2 = ConsoleColor.Gray,
                    Lines = new Table.TableRenderOptions.TableLineDisplayOptions(){
                        ContentVertical = false
                    }
                }
            );
            table.Append(
                new Person() {
                    FirstName = "Toph",
                    LastName = "Beifong",
                    Age = 12
                }, 
                true
            );
            Consoul.Wait();

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
