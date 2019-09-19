using System;
using Consoul.Test.Views;

namespace Consoul.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            View view1 = new Welcome();
            view1.Run();
        }
    }
}
