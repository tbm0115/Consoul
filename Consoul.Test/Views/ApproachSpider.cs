using System;
using System.Linq;
using ConsoulLibrary.Views;

namespace ConsoulLibrary.Test.Views
{
    [View("Do you try to fight it?")]
    public class ApproachSpider : StoryView
    {
        public ApproachSpider(Story story) : base(story)
        {
            Title = "Do you try to fight it?";
        }

        private string _fightMessage() => "\"Death to Spider!\"";
        private ConsoleColor _fightColor() => ConsoleColor.White;
        [DynamicViewOption(nameof(_fightMessage), nameof(_fightColor))]
        public void Fight()
        {
            int hitThreshold = 5;
            if (Source.Hero.Inventory.Items.Any(o => o is Weapon))
            {
                Consoul.Write($"{String.Join("", Enumerable.Repeat("*", 20))}", ConsoleColor.Gray);
                Consoul.Write($"You must hit above {hitThreshold} to kill the spider", ConsoleColor.Gray);
                Consoul.Write($"If the spider hits higher than you, you will lose.", ConsoleColor.Gray);
                Consoul.Write($"{String.Join("", Enumerable.Repeat("*", 20))}", ConsoleColor.Gray);

                var fang = new Fang();
                int spiderHit = fang.Hit();
                int youHit = (Source.Hero.Inventory.Items.First(o => o is Weapon) as Weapon).Hit();
                Consoul.Write($"You strike: {youHit}", ConsoleColor.Gray);
                Consoul.Write($"Spider strike: {spiderHit}", ConsoleColor.Gray);
                if (youHit < spiderHit)
                {
                    Consoul.Write("The spider acts quickly and sticks its fangs deep into your gut!", ConsoleColor.Red);
                }else if (youHit < hitThreshold)
                {
                    Consoul.Write("You strike fast, but ultimately miss the mark. But, your quick hit distracted the spider enough for you to escape.", ConsoleColor.Yellow);
                }
                else
                {
                    Consoul.Write("You strike fast and true, thrusting the stick deep into the eye socket of the spider. It writhes in pain and scurries deeper into the cave.", ConsoleColor.Green);
                }
            }
            else
            {
                Consoul.Write("You have no weapon with you and the spider takes advantage!", ConsoleColor.Red);
            }
            Consoul.Wait();
        }

        private string _runMessage() => "Run Away";
        private ConsoleColor _runColor() => ConsoleColor.Yellow;
        [DynamicViewOption(nameof(_runMessage), nameof(_runColor))]
        public void RunAway()
        {
            Consoul.Write("As you turn away, it ambushes you and impales you with its fangs!!!", ConsoleColor.Red);
            Consoul.Wait();
            GoBack();
        }
    }
    
}
