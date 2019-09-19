using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Consoul.Entry;
using Consoul.Attributes;

namespace Consoul.Views
{
    public abstract class DynamicView<T> : IView
    {
        private bool _goBackRequested = false;

        public string Title { get; set; }
        public List<DynamicOption<T>> Options { get; set; } = new List<DynamicOption<T>>();
        public bool GoBackRequested => _goBackRequested;
        public T Source { get; set; }

        public DynamicView()
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
            IEnumerable<MethodInfo> viewOptionMethods = thisType.GetMethods().Where(o => o.GetCustomAttribute(viewOptionType) != null);
            foreach (MethodInfo method in viewOptionMethods)
            {
                DynamicViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as DynamicViewOptionAttribute;
                if (attr != null)
                {
                    MethodInfo messageBuilder = thisType.GetMethod(attr.BuildMethod);
                    Options.Add(new DynamicOption<T>(o => messageBuilder.Invoke(Source, null).ToString(), () => method.Invoke(this, null), attr.Color));
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
                    prompt.Add(option.BuildMessage(Source), option.Color);
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
