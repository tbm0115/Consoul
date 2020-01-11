using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoulLibrary.Views
{
    public delegate Task ChoiceCallback(int choiceIndex);

    public interface IView
    {
        string Title { get; set; }

        bool GoBackRequested { get; }

        Task RunAsync(ChoiceCallback callback = null);
        void Run(ChoiceCallback callback = null);

        void GoBack();
    }
}
