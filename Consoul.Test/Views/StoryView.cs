namespace Consoul.Test.Views
{
    public abstract class StoryView : View{
        public Story Story { get; set; }

        public StoryView(Story story) : base()
        {

        }
    }
}
