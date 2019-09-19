using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Consoul.Test.Views
{
    public class Welcome : View
    {
        public Inventory Inventory { get; set; }

        public Welcome() : base()
        {
            string message = "Welcome to the cavern of secrets!";
            int padding = 5;
            Title = $"{String.Join("", Enumerable.Repeat("*", (padding * 2) + message.Length))}\r\n" +
                $"{String.Join("", Enumerable.Repeat(" ", padding))}" + 
                $"{message}" +
                $"{String.Join("", Enumerable.Repeat(" ", padding))}\r\n" + 
                $"{String.Join("", Enumerable.Repeat("*", (padding * 2) + message.Length))}\r\n\r\nWould you like to begin?";

            Inventory = new Inventory();
        }

        [ViewOption("Yes, let's start!")]
        public void Yes()
        {
            (new PickUpStick(Inventory)).Run();
        }

    }
    public class PickUpStick : View
    {
        public Inventory Inventory { get; set; }

        public PickUpStick(Inventory inventory) : base()
        {
            Inventory = inventory;
            Title = "You enter a dark cavern out of curiosity. " +
                "It is dark and you can only make out a small stick on the floor.\r\n\r\n" +
                "Do you take it?";
        }

        [ViewOption("Take it with you")]
        public void TakeIt()
        {
            Consoul.Write("You have taken the stick!", ConsoleColor.Green);
            Consoul.Wait();
            Inventory.Items.Add(new Stick());
            (new GlowingObject(Inventory)).Run();
        }

        [ViewOption("Leave it alone")]
        public void LeaveIt()
        {
            Consoul.Write("You did not take the stick!");
            Consoul.Wait();
        }
    }
    public class GlowingObject : View
    {
        public Inventory Inventory { get; set; }

        public GlowingObject(Inventory inventory) : base()
        {
            Inventory = inventory;
            Title = "As you proceed further into the cave, you see a small glowing object.";
        }

        [ViewOption("What is this? (proceed)")]
        public void Approach()
        {
            Consoul.Write("You approach the object...");
            Consoul.Wait();
            Consoul.Write("As you draw closer you begin to make out the object as an eye!");
            Consoul.Wait();
            Consoul.Write("The eye belongs to a giant spider!");
            Consoul.Wait();
            (new ApproachSpider(Inventory)).Run();
        }

        [ViewOption("Do not proceed (Leave the cave)")]
        public void Leave()
        {
            Consoul.Write("You turn away from the glowing object, and attempt to leave the cave...");
            Consoul.Wait();
            Consoul.Write("But, something won't let you leave...", ConsoleColor.Red);
            Consoul.Wait();
        }
    }
    public class ApproachSpider : View
    {
        public Inventory Inventory { get; set; }

        public ApproachSpider(Inventory inventory) : base()
        {
            Inventory = inventory;
            Title = "Do you try to fight it?";
        }

        [ViewOption("\"Death to Spider!\"")]
        public void Fight()
        {
            int hitThreshold = 5;
            if (Inventory.Items.Any(o => o.Name == "Stick"))
            {
                Consoul.Write($"{String.Join("", Enumerable.Repeat("*", 20))}", ConsoleColor.Gray);
                Consoul.Write($"You must hit above {hitThreshold} to kill the spider", ConsoleColor.Gray);
                Consoul.Write($"If the spider hits higher than you, you will lose.", ConsoleColor.Gray);
                Consoul.Write($"{String.Join("", Enumerable.Repeat("*", 20))}", ConsoleColor.Gray);

                var fang = new Fang();
                int spiderHit = fang.Hit();
                int youHit = (Inventory.Items.First(o => o.Name == "Stick") as Weapon).Hit();
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

        [ViewOption("Run Away", Color = ConsoleColor.Yellow)]
        public void RunAway()
        {
            Consoul.Write("As you turn away, it ambushes you and impales you with its fangs!!!", ConsoleColor.Red);
            Consoul.Wait();
        }
    }

    public class Inventory
    {
        public List<Item> Items { get; set; } = new List<Item>();

        public Inventory()
        {

        }
    }
    public abstract class Item
    {
        public string Name { get; set; }

        public Item(string name)
        {
            Name = name;
        }
    }
    public abstract class Weapon : Item
    {
        protected Random random { get; set; } = new Random(3);
        public int BaseDamage { get; set; }

        public Weapon(string name, int damage) : base(name)
        {
            BaseDamage = damage;
        }

        public int Hit()
        {
            return random.Next(0, BaseDamage);
        }
    }
    public class Fang : Weapon
    {
        public Fang() : base("Fang", 6)
        {

        }
    }
    public class Stick : Weapon
    {
        public Stick() : base("Stick", 10)
        {

        }
    }
}
