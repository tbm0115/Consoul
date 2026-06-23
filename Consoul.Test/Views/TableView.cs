using System;
using System.Collections.Generic;

namespace ConsoulLibrary.Test.Views
{
    public class TableView : StaticView
    {
        private List<Hero> Heroes { get; set; }

        public TableView()
        {
            Title = "Testing Table View";
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
            var table = ConsoulLibrary.DynamicTableView<Hero>.Create(Heroes, o => o.Name, o => o.HitPoints);
            table.QueryYieldsNoResults += Table_QueryYieldsNoResults;

            var heroChoice = table.Prompt();
            if (heroChoice != null)
            {
                ConsoulLibrary.Consoul.Write($"Hero: {heroChoice?.Name ?? "Unknown"}");
            } else
            {
                Consoul.Write("Invalid selection");
            }
            ConsoulLibrary.Consoul.Wait();
        }

        private void Table_QueryYieldsNoResults(object sender, ConsoulLibrary.TableQueryYieldsNoResultsEventArgs e)
        {
            throw new NotImplementedException();
        }

        [ViewOption("Dynamic Table Test")]
        public void DynamicTable()
        {
            var table = ConsoulLibrary.DynamicTableView<Actor>.Create(Heroes, o => o.Name, o => o.HitPoints);
            table.AddHeader(o => o.Inventory.Items.Count, "Inventory Count");
            table.QueryYieldsNoResults += Table_QueryYieldsNoResults;

            var heroChoice = table.Prompt($"Test Message");

            ConsoulLibrary.Consoul.Write($"Actor: {heroChoice?.Name ?? "Unknown"}");
            ConsoulLibrary.Consoul.Wait();
        }

        [ViewOption("Dynamic Table Test (AllowEmpty)")]
        public void DynamicTableEmpty()
        {
            var table = ConsoulLibrary.DynamicTableView<Actor>.Create(Heroes, o => o.Name, o => o.HitPoints);
            table.AddHeader(o => o.Inventory.Items.Count);// new ConsoulLibrary.DynamicTableView<Actor>(Heroes, new ConsoulLibrary.TableRenderOptions() { });
            table.QueryYieldsNoResults += Table_QueryYieldsNoResults;

            var heroChoice = table.Prompt($"Test Message", allowEmpty: true);

            if (heroChoice != null)
            {
                ConsoulLibrary.Consoul.Write($"Actor: {heroChoice?.Name ?? "Unknown"}");
            } else
            {
                ConsoulLibrary.Consoul.Write($"No Actor selected...");
            }

            ConsoulLibrary.Consoul.Wait();
        }
    }
}
