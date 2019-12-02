using System;
using ConsoulLibrary.Test.Views;

namespace ConsoulLibrary.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // **************************************************************************
            // **********************************Credit**********************************
            //
            // Notes:
            //     
            //     https://codereview.stackexchange.com/questions/36768/tiny-text-adventure 
            //
            // **************************************************************************
            var view1 = new Welcome();
            view1.Run();
        }
    }
}
