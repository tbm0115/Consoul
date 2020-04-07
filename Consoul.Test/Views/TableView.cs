using ConsoulLibrary.Views;
using ConsoulLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using ConsoulLibrary.Test.Views;

namespace ConsoulLibrary.Test.Views
{
    public class TableView : StaticView
    {
        public TableView()
        {
            Title = (new BannerEntry($"Testing Table View")).Message;
        }

        [ViewOption("Prompt Test")]
        public void PromptTest()
        {
            List<Actor> actors = new List<Actor>()
            {
                new Hero()
                {
                    Name = "Trais McAllister",
                    HitPoints = 100
                },
                new Hero()
                {
                    Name = "Samantha McAllister",
                    HitPoints = 100
                },
                new Hero()
                {
                    Name = "Kylo McAllister",
                    HitPoints = 50
                },
                new Hero()
                {
                    Name = "Victoria McAllister",
                    HitPoints = 10
                },
                new Hero()
                {
                    Name = "William McAllister",
                    HitPoints = 10
                }
            };
            var table = new ConsoulLibrary.Table.TableView(
                actors, 
                new string[] 
                { 
                    "Name", 
                    "HitPoints" 
                }, 
                new ConsoulLibrary.Table.TableRenderOptions() { }
            );

            int idxChoice = table.Prompt();

            ConsoulLibrary.Consoul.Write($"Actor: {actors[idxChoice].Name}");
            ConsoulLibrary.Consoul.Wait();
        }
    }
}
