using ConsoulLibrary.Views;
using ConsoulLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using ConsoulLibrary.Test.Views;
using System.Linq.Expressions;

namespace ConsoulLibrary.Test.Views
{
    public class TableView : StaticView
    {
        private List<Hero> Heroes { get; set; }

        public TableView()
        {
            Title = (new BannerEntry($"Testing Table View")).Message;
            Heroes = new List<Hero>()
            {
                new Hero()
                {
                    Name = "Trais McAllister",
                    HitPoints = 100,
                    Inventory = new Inventory()
                    {
                        Items = new List<Item>()
                        {
                            new Stick()
                        }
                    }
                },
                new Hero()
                {
                    Name = "Samantha McAllister",
                    HitPoints = 100
                },
                new Hero()
                {
                    Name = "Kylo McAllister",
                    HitPoints = 50,
                    Inventory = new Inventory()
                    {
                        Items = new List<Item>()
                        {
                            new Fang(),
                            new Collar()
                        }
                    }
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
        }

        [ViewOption("Prompt Test")]
        public void PromptTest()
        {
            var table = new ConsoulLibrary.Table.TableView(
                Heroes, 
                new string[] 
                { 
                    "Name", 
                    "HitPoints" 
                }, 
                new ConsoulLibrary.Table.TableRenderOptions() { }
            );
            table.QueryYieldsNoResults += Table_QueryYieldsNoResults;

            int idxChoice = table.Prompt();

            ConsoulLibrary.Consoul.Write($"Hero: {Heroes[idxChoice].Name}");
            ConsoulLibrary.Consoul.Wait();
        }

        private void Table_QueryYieldsNoResults(object sender, Table.TableQueryYieldsNoResultsEventArgs e)
        {
            throw new NotImplementedException();
        }

        [ViewOption("Dynamic Table Test")]
        public void DynamicTable()
        {
            var table = new ConsoulLibrary.Table.DynamicTableView<Actor>(Heroes, new Table.TableRenderOptions() { });
            table.AddHeader(o => o.Name);
            table.AddHeader(o => o.HitPoints);
            table.AddHeader(o => o.Inventory.Items.Count, "Inventory Size");
            table.Build();
            table.QueryYieldsNoResults += Table_QueryYieldsNoResults;

            int idxChoice = table.Prompt($"Test Message");

            ConsoulLibrary.Consoul.Write($"Actor: {Heroes[idxChoice].Name}");
            ConsoulLibrary.Consoul.Wait();
        }

        [ViewOption("Dynamic Table Test (AllowEmpty)")]
        public void DynamicTableEmpty()
        {
            var table = new ConsoulLibrary.Table.DynamicTableView<Actor>(Heroes, new Table.TableRenderOptions() { });
            table.AddHeader(o => o.Name);
            table.AddHeader(o => o.HitPoints);
            table.AddHeader(o => o.Inventory.Items.Count, "Inventory Size");
            table.Build();
            table.QueryYieldsNoResults += Table_QueryYieldsNoResults;

            int idxChoice = table.Prompt($"Test Message", allowEmpty: true);

            if (idxChoice >= 0)
            {
                ConsoulLibrary.Consoul.Write($"Actor: {Heroes[idxChoice].Name}");
            } else
            {
                ConsoulLibrary.Consoul.Write($"No Actor selected...");
            }

            ConsoulLibrary.Consoul.Wait();
        }
    }
}
