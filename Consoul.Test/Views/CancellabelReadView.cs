using ConsoulLibrary.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary.Test.Views
{
    public class CancellabelReadView : StaticView
    {
        public CancellabelReadView()
        {
            Title = (new BannerEntry("Testing the Read(CancellationToken) method")).Message;
        }

        [ViewOption("Test Read()")]
        public void Test()
        {
            Consoul.Write("Input some text:", ConsoleColor.Yellow);
            string input = Consoul.Read();
            Consoul.Write("Read the following input:");
            Consoul.Write(input, ConsoleColor.Gray);

            Consoul.Wait();
        }

        [ViewOption("Test Read(CancellationToken)")]
        public void TestCancellable()
        {
            const int TIMEOUT_SECONDS = 5;
            string input = string.Empty;
            Consoul.Write("Input some text:", ConsoleColor.Yellow);
            using (var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SECONDS)))
            {
                cancelSource.Token.Register(() => Consoul.Write("Consoul.Read(CancellationToken) Timed Out!", ConsoleColor.Red));
                input = Consoul.Read(cancelSource.Token);
            }
            Consoul.Write("Read the following input:");
            Consoul.Write(input, ConsoleColor.Gray);

            Consoul.Wait();
        }
    }
}
