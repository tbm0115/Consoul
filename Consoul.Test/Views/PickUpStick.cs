using System;
using ConsoulLibrary.Views;

namespace ConsoulLibrary.Test.Views
{
    public class PickUpStick : StoryView
    {
        public PickUpStick(Story story) : base(story)
        {
            Title = "You enter a dark cavern out of curiosity. " +
                "It is dark and you can only make out a small stick on the floor.\r\n\r\n" +
                "Do you take it?";
        }

        private string _takeItMessage() => "Take it with you";
        private ConsoleColor _takeItColor() => ConsoleColor.White;
        [DynamicViewOption(nameof(_takeItMessage), nameof(_takeItColor))]
        public void TakeIt()
        {
            Consoul.Write("You have taken the stick!", ConsoleColor.Green);
            Consoul.Wait();
            Source.Hero.Inventory.Items.Add(new Stick());
            Source.Progress(typeof(GlowingObject));
        }

        private string _leaveItMessage() => "Leave it alone.";
        private ConsoleColor _leaveItColor() => ConsoleColor.White;
        [DynamicViewOption(nameof(_leaveItMessage), nameof(_leaveItColor))]
        public void LeaveIt()
        {
            Consoul.Write("You did not take the stick!");
            Consoul.Wait();
            Source.Progress(typeof(GlowingObject));
        }
    }
    
}
