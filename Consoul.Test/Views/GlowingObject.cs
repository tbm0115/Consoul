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

        private string _approachMessage() => "What is this? (proceed)";
        private ConsoleColor _approachColor() => ConsoleColor.White;
        [DynamicViewOption(nameof(_approachMessage), nameof(_approachColor))]
        public void Approach()
        {
            Consoul.Write("You approach the object...");
            Consoul.Wait();
            Consoul.Write("As you draw closer you begin to make out the object as an eye!");
            Consoul.Wait();
            Consoul.Write("The eye belongs to a giant spider!");
            Consoul.Wait();
            Source.Progress(typeof(ApproachSpider));
        }

        private string _leaveMessage() => "Do not proceed (Leave the cave)";
        private ConsoleColor _leaveColor() => ConsoleColor.White;
        [DynamicViewOption(nameof(_leaveMessage), nameof(_leaveColor))]
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
