using Consoul.Views;

namespace Consoul.Test.Views
{
    public abstract class StoryView : DynamicView<Story>{

        public StoryView(Story story) : base()
        {
            Source = story;
        }
    }
}
