using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary.Test.Views
{
    public class Story
    {
        public Hero Hero { get; set; } = new Hero();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();

        public List<StoryView> Stages { get; set; }

        public Story()
        {
            Stages = new List<StoryView>()
            {
                new PickUpStick(this),
                new GlowingObject(this),
                new ApproachSpider(this)
            };
        }

        public void Progress(Type viewType)
        {
            var nextStage = Stages.FirstOrDefault(o => o.GetType() == viewType);
            if (nextStage != null)
            {
                nextStage.Model = this;
                nextStage.Render();
            }
            else
            {
                Consoul.Write("Invalid Story Stage type!", ConsoleColor.Red);
                Consoul.Wait();
            }
        }
    }
}
