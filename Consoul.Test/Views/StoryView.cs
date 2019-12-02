using ConsoulLibrary.Views;

namespace ConsoulLibrary.Test.Views
{
    public abstract class StoryView : DynamicView<Story>{

        public StoryView(Story story) : base()
        {
            Source = story;
        }
    }
}
