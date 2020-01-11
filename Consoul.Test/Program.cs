using System;
using ConsoulLibrary.Test.Views;
using ConsoulLibrary;
using System.Linq;

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
            Routines.MonitorInputs = true;
            Routines.InitializeRoutine(args);
            //Routines.UseDelays = true; // Showcases the usecase of reusing input delays to simulate user response

            var view1 = new Welcome();
            view1.Run(
                (choice) => System.Threading.Tasks.Task.Run(() =>
                    {
                        Consoul.Write("DONE!", ConsoleColor.Green);
                    }
                )
            );

            var xRoutine = new XmlRoutine();
            xRoutine.SaveInputs("Test.xml");
        }

    }
}
