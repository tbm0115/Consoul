using Consoul.Views;

namespace Consoul.Test.Views
{
    public abstract class StoryView : StaticView{
        public Story Story { get; set; }

        public StoryView(Story story) : base()
        {

        }
    }
}
