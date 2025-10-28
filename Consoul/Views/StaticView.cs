using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConsoulLibrary.Entry;

namespace ConsoulLibrary.Views
{
    public abstract class StaticView : IView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;

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
                _goBackMessage = viewAttr.GoBackMessage;
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
                    ParameterInfo[] methodParameters = method.GetParameters();
                    List<object> implementedMethodParameters = new List<object>();
                    foreach (ParameterInfo methodParameter in methodParameters) {
                        if (methodParameter.HasDefaultValue) {
                            implementedMethodParameters.Add(methodParameter.DefaultValue);
                        } else if (Nullable.GetUnderlyingType(methodParameter.ParameterType) != null) {
                            implementedMethodParameters.Add(null);
                        }
                    }
                    bool useParameters = methodParameters.Length > 0 && implementedMethodParameters.Count == methodParameters.Length;
                    OptionAction p = delegate ()
                           {
                               thisType.InvokeMember(
                                   method.Name,
                                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                   null,
                                   this,
                                   useParameters ? implementedMethodParameters.ToArray() : null
                               );
                           };
                    Options.Add(new Option(
                        attr.Message,
                        p,
                        attr.Color)
                    );
                }
            }
        }

        public void GoBack()
        {
            _goBackRequested = true;
        }

        public async Task RunAsync(ChoiceCallback callback = null)
        {
            int idx = -1;
            do
            {
                Prompt prompt = new Prompt(Title, true);
                foreach (Option option in Options)
                    prompt.Add(option.BuildMessage(), option.Color);

                prompt.Add(_goBackMessage, RenderOptions.SubnoteColor);

                try
                {
                    idx = prompt.Run();
                    if (idx >= 0 && idx < Options.Count)
                    {
                        await Task.Run(() => {
                            try
                            {
                                Options[idx].Action.Invoke();
                            }
                            catch (Exception ex2)
                            {
                                Consoul.Write($"{Title}[{idx}]\t{ex2.Message}\r\n\tStack Trace: {ex2.StackTrace}", RenderOptions.InvalidColor);
                                if (RenderOptions.WaitOnError) Consoul.Wait();
                            }
                        });
                        if (callback != null)
                             await callback(idx);
                        idx = -1;
                    }
                    else if (idx == Options.Count)
                    {
                        idx = int.MaxValue;
                    }
                    else if (idx == Consoul.EscapeIndex)
                    {
                        GoBack();
                    }
                }
                catch (Exception ex)
                {
                    Consoul.Write($"{Title}[{idx}]\t{ex.Message}\r\n\tStack Trace: {ex.StackTrace}", RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError) Consoul.Wait();
                }
            } while (idx < 0 && !GoBackRequested);
        }

        public void Run(ChoiceCallback callback = null)
        {
            try
            {
                RunAsync(callback).Wait();
            }
            catch (Exception ex)
            {
                Consoul.Write($"{ex.Message}\r\nStack Trace: {ex.StackTrace}", ConsoleColor.Red);
                if (RenderOptions.WaitOnError) Consoul.Wait();
            }
        }
    }
}
