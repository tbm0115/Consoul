using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConsoulLibrary.Entry;
using ConsoulLibrary.Attributes;

namespace ConsoulLibrary.Views
{
    public abstract class DynamicView<T> : IView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;

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
                _goBackMessage = viewAttr.GoBackMessage;
            }

            // Build the options from local methods decorated with ViewOption
            Type viewOptionType = typeof(DynamicViewOptionAttribute);
            IEnumerable<MethodInfo> allMethods = thisType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<MethodInfo> viewOptionMethods = allMethods.Where(o => o.GetCustomAttribute(viewOptionType) != null);
            foreach (MethodInfo method in viewOptionMethods)
            {
                DynamicViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as DynamicViewOptionAttribute;
                if (attr != null)
                {
                    MethodInfo messageBuilder = allMethods.FirstOrDefault(o => o.Name == attr.MessageMethod);
                    MethodInfo colorBuilder = null;
                    if (!string.IsNullOrEmpty(attr.ColorMethod))
                    {
                        colorBuilder = allMethods.FirstOrDefault(o => o.Name == attr.ColorMethod);
                    }
                    if (messageBuilder != null)
                    {
                        Options.Add(new DynamicOption<T>(
                            o => thisType.InvokeMember(
                                attr.MessageMethod, 
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, 
                                null,
                                this,
                                null
                            ).ToString(),
                            () => thisType.InvokeMember(
                                method.Name,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null,
                                this,
                                null
                            ),
                            o => colorBuilder != null 
                                ? (ConsoleColor)thisType.InvokeMember(
                                        attr.ColorMethod,
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                        null,
                                        this,
                                        null
                                    )
                                : RenderOptions.DefaultColor
                        ));
                    }
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
                    prompt.Add(option.BuildMessage(Source), option.BuildColor(Source));
                }
                prompt.Add(_goBackMessage, RenderOptions.SubnoteColor);

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
                    Consoul.Write($"{Title}[{idx}]\t{ex.Message}\r\n\tStack Trace: {ex.StackTrace}", RenderOptions.InvalidColor);
                }
            } while (idx < 0 && !GoBackRequested);
        }
    }
}
