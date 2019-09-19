using System;
using Consoul.Test.Views;

namespace Consoul.Test
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
