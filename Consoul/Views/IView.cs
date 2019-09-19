using System.Text;

namespace Consoul.Views
{
    public delegate void ChoiceCallback(int choiceIndex);

    public interface IView
    {
        string Title { get; set; }
        bool GoBackRequested { get; }

        void Run(ChoiceCallback callback = null);
        void GoBack();
    }
}
