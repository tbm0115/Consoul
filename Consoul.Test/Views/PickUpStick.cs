using Consoul.Attributes;
using System;

namespace Consoul.Test.Views
{
    public class PickUpStick : StoryView
    {
        public PickUpStick(Story story) : base(story)
        {
            Title = "You enter a dark cavern out of curiosity. " +
                "It is dark and you can only make out a small stick on the floor.\r\n\r\n" +
                "Do you take it?";
        }

        [ViewOption("Take it with you")]
        public void TakeIt()
        {
            Consoul.Write("You have taken the stick!", ConsoleColor.Green);
            Consoul.Wait();
            Story.Hero.Inventory.Items.Add(new Stick());
            Story.Progress(typeof(GlowingObject));
        }

        [ViewOption("Leave it alone")]
        public void LeaveIt()
        {
            Consoul.Write("You did not take the stick!");
            Consoul.Wait();
            GoBack();
        }
    }
    
}
