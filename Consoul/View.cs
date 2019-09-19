using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Consoul
{
    public delegate void OptionAction();

    public abstract class View
    {
        public string Title { get; set; }
        public List<ViewOption> Options { get; set; } = new List<ViewOption>();

        public View()
        {
            Type thisType = this.GetType();

            // Build the options from local methods decorated with ViewOption
            Type viewOptionType = typeof(ViewOptionAttribute);
            IEnumerable<MethodInfo> viewOptionMethods = thisType.GetMethods().Where(o => o.GetCustomAttribute(viewOptionType) != null);
            foreach (MethodInfo method in viewOptionMethods)
            {
                ViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as ViewOptionAttribute;
                if (attr != null)
                {
                    Options.Add(new ViewOption(attr.Message, () => method.Invoke(this, null), attr.Color));
                }
            }
        }

        public View(string title) : this()
        {
            Title = title;
        }

        public virtual void Run()
        {
            int idx = -1;
            do
            {
                Prompt prompt = new Prompt(Title, true);
                foreach (var option in Options)
                {
                    prompt.Add(option.Message, option.Color);
                }
                prompt.Add($"<==\tGo Back", ConsoleColor.Gray);

                try
                {
                    idx = prompt.Run();
                    if (idx >= 0 && idx < Options.Count)
                    {
                        Options[idx].Action.Compile().Invoke();
                        idx = -1;
                    }
                    else if (idx == Options.Count)
                    {
                        idx = int.MaxValue;
                        //PreviousView.Run(); // Displays the previous View
                    }
                }
                catch (Exception ex)
                {
                    Consoul.Write($"{Title}[{idx}]\t{ex.Message}\r\n\tStack Trace: {ex.StackTrace}", ConsoleColor.Red);
                }
            } while (idx < 0);
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ViewOptionAttribute : Attribute
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public ViewOptionAttribute(string message)
        {
            Message = message;
        }
    }
}
