using Consoul.Attributes;
using System;

namespace Consoul.Test.Views
{
    [View("As you proceed further into the cave, you see a small glowing object.")]
    public class GlowingObject : StoryView
    {
        public GlowingObject(Story story) : base(story)
        {
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
            Story.Progress(typeof(ApproachSpider));
        }

        [ViewOption("Do not proceed (Leave the cave)")]
        public void Leave()
        {
            Consoul.Write("You turn away from the glowing object, and attempt to leave the cave...");
            Consoul.Wait();
            Consoul.Write("But, something won't let you leave...", ConsoleColor.Red);
            Consoul.Wait();
            GoBack();
        }
    }
    
}
