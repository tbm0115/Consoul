using System;
using ConsoulLibrary.Test.Views;

namespace ConsoulLibrary.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConsoleColor title = ConsoleColor.Green, carrot = ConsoleColor.Blue, underscore = ConsoleColor.Red;
            //var sb = new System.Text.StringBuilder();
            //sb.AppendLine("        CCCCCCCCCCCCC                                                                                       lllllll ");
            //sb.AppendLine("     CCC::::::::::::C                                                                                       l:::::l ");
            //sb.AppendLine("   CC:::::::::::::::C                                                                                       l:::::l ");
            //sb.AppendLine("  C:::::CCCCCCCC::::C                                                                                       l:::::l ");
            //sb.AppendLine(" C:::::C       CCCCCC   ooooooooooo   nnnn  nnnnnnnn        ssssssssss      ooooooooooo   uuuuuu    uuuuuu   l::::l ");
            //sb.AppendLine("C:::::C               oo:::::::::::oo n:::nn::::::::nn    ss::::::::::s   oo:::::::::::oo u::::u    u::::u   l::::l ");
            //sb.AppendLine("C:::::C              o:::::::::::::::on::::::::::::::nn ss:::::::::::::s o:::::::::::::::ou::::u    u::::u   l::::l ");
            //sb.AppendLine("C:::::C              o:::::ooooo:::::onn:::::::::::::::ns::::::ssss:::::so:::::ooooo:::::ou::::u    u::::u   l::::l ");
            //sb.AppendLine("C:::::C              o::::o     o::::o  n:::::nnnn:::::n s:::::s  ssssss o::::o     o::::ou::::u    u::::u   l::::l ");
            //sb.AppendLine("C:::::C              o::::o     o::::o  n::::n    n::::n   s::::::s      o::::o     o::::ou::::u    u::::u   l::::l ");
            //sb.AppendLine("C:::::C              o::::o     o::::o  n::::n    n::::n      s::::::s   o::::o     o::::ou::::u    u::::u   l::::l ");
            //sb.AppendLine(" C:::::C       CCCCCCo::::o     o::::o  n::::n    n::::nssssss   s:::::s o::::o     o::::ou:::::uuuu:::::u   l::::l ");
            //sb.AppendLine("  C:::::CCCCCCCC::::Co:::::ooooo:::::o  n::::n    n::::ns:::::ssss::::::so:::::ooooo:::::ou:::::::::::::::uul::::::l");
            //sb.AppendLine("   CC:::::::::::::::Co:::::::::::::::o  n::::n    n::::ns::::::::::::::s o:::::::::::::::o u:::::::::::::::ul::::::l");
            //sb.AppendLine("     CCC::::::::::::C oo:::::::::::oo   n::::n    n::::n s:::::::::::ss   oo:::::::::::oo   uu::::::::uu:::ul::::::l");
            //sb.AppendLine("        CCCCCCCCCCCCC   ooooooooooo     nnnnnn    nnnnnn  sssssssssss       ooooooooooo       uuuuuuuu  uuuullllllll");

            //Consoul.Write(sb.ToString(), title);

            //sb.Clear();

            //sb.AppendLine($" .-.");
            //sb.AppendLine($"/_/ \\");
            //sb.AppendLine($"\\ \\  \\");
            //sb.AppendLine($" \\ \\  \\");
            //sb.AppendLine($"  \\ \\  \\");
            //sb.AppendLine($"   >->  `");
            //sb.AppendLine($"  / /  /");
            //sb.AppendLine($" / /  /");
            //Consoul.Write(sb.ToString(), carrot, false);
            //Consoul.Write($"/_/  /     ", carrot, false);
            //Consoul.Write($"________________", underscore);
            //Consoul.Write($"\\ \\ /     ", carrot, false);
            //Consoul.Write($"|________________|", underscore);
            //Consoul.Write($" `-`", carrot);


            //Console.ReadLine();
            // **************************************************************************
            // **********************************Credit**********************************
            //
            // Notes:
            //     
            //     https://codereview.stackexchange.com/questions/36768/tiny-text-adventure 
            //
            // **************************************************************************
            Routines.MonitorInputs = true;
            RenderOptions.WaitOnError = true;
            Routines.InitializeRoutine(args);
            //Routines.UseDelays = true; // Showcases the usecase of reusing input delays to simulate user response
            try
            {
                var person = new { Name = "Trais" };
                Consoul.Write("Hello {Name:Green}", ConsoleColor.Cyan, writeLine: true, args: person);
                throw new Exception("Testing this shit");
            }
            catch (Exception ex)
            {
                Consoul.Write(ex, includeStackTrace: false);
            }
            Consoul.Wait();

            Consoul.Render<Test.Views.TableView>()
                .SaveInput("Test.xml");
        }

    }
}
