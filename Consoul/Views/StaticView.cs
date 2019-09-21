using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Consoul.Entry;
using Consoul.Attributes;

namespace Consoul.Views
{
    public abstract class StaticView : IView
    {
        private bool _goBackRequested = false;

        public string Title { get; set; }
        public List<Option> Options { get; set; } = new List<Option>();
        public bool GoBackRequested => _goBackRequested;


        public StaticView()
        {
            Type thisType = this.GetType();

            Type viewType = typeof(ViewAttribute);
            ViewAttribute viewAttr = thisType.GetCustomAttribute(viewType) as ViewAttribute;
            if (viewAttr != null)
            {
                Title = viewAttr.Title;
            }

            // Build the options from local methods decorated with ViewOption
            Type viewOptionType = typeof(ViewOptionAttribute);
            IEnumerable<MethodInfo> allOptionMethods = thisType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<MethodInfo> viewOptionMethods = allOptionMethods.Where(o => o.GetCustomAttribute(viewOptionType) != null);
            foreach (MethodInfo method in viewOptionMethods)
            {
                ViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as ViewOptionAttribute;
                if (attr != null)
                {
                    Options.Add(new Option(
                        attr.Message, 
                        () => thisType.InvokeMember(
                            method.Name,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null,
                                this,
                                null
                            ),
                        attr.Color)
                    );
                }
            }
        }

        public void GoBack()
        {
            _goBackRequested = true;
        }

        public void Run(ChoiceCallback callback = null)
        {
            int idx = -1;
            do
            {
                Prompt prompt = new Prompt(Title, true);
                foreach (var option in Options)
                {
                    prompt.Add(option.BuildMessage(), option.Color);
                }
                prompt.Add($"<==\tGo Back", ConsoleColor.Gray);

                try
                {
                    idx = prompt.Run();
                    if (idx >= 0 && idx < Options.Count)
                    {
                        Options[idx].Action.Compile().Invoke();
                        if (callback != null)
                        {
                            callback(idx);
                        }
                        idx = -1;
                    }
                    else if (idx == Options.Count)
                    {
                        idx = int.MaxValue;
                    }
                }
                catch (Exception ex)
                {
                    Consoul.Write($"{Title}[{idx}]\t{ex.Message}\r\n\tStack Trace: {ex.StackTrace}", ConsoleColor.Red);
                }
            } while (idx < 0 && !GoBackRequested);
        }
    }
}
