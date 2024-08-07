﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoulLibrary.Views
{
    /// <summary>
    /// An abstract view that relies on an underlying model to dynamically change the labels and colors of choices whenever the view re-renders.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DynamicView<T> : IView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;
        protected string GoBackMessage
        {
            get
            {
                return _goBackMessage;
            }
            set
            {
                _goBackMessage = value;
            }
        }

        /// <summary>
        /// Text displayed at the top of the console
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Collection of view options
        /// </summary>
        public List<DynamicOption<T>> Options { get; set; } = new List<DynamicOption<T>>();

        /// <summary>
        /// Flag indicating whether or not an underlying process has requested this view to go back.
        /// </summary>
        public bool GoBackRequested => _goBackRequested;

        /// <summary>
        /// Reference to the source model of the dynamic view.
        /// </summary>
        public T Source { get; set; }

        public DynamicView()
        {
            Type thisType = this.GetType();

            Type viewType = typeof(ViewAttribute);
            ViewAttribute viewAttr = thisType.GetCustomAttribute(viewType) as ViewAttribute;
            if (viewAttr != null)
            {
                Title = BannerEntry.Render(viewAttr.Title);
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
                        colorBuilder = allMethods.FirstOrDefault(o => o.Name == attr.ColorMethod);

                    if (messageBuilder != null) {
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

                        Options.Add(new DynamicOption<T>(
                            () =>
                            {
                                return thisType.InvokeMember(
                                    attr.MessageMethod,
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                    null,
                                    this,
                                    null
                                ).ToString();
                            },
                            () => thisType.InvokeMember(
                                method.Name,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null,
                                this,
                                useParameters ? implementedMethodParameters.ToArray() : null
                            ),
                            () =>
                            {
                                return colorBuilder != null
                                ? (ConsoleColor)thisType.InvokeMember(
                                        attr.ColorMethod,
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                        null,
                                        this,
                                        null
                                    )
                                : RenderOptions.DefaultColor;
                            }
                        ));
                    }
                }
            }
        }

        /// <summary>
        /// Triggers the choice to go back to the previous view (or exit to the main <see cref="Program.Main"/> if this is the top view)
        /// </summary>
        public void GoBack()
        {
            _goBackRequested = true;
        }

        /// <summary>
        /// Renders the current dynamic view
        /// </summary>
        /// <param name="callback">Callback function whenever a choice for this dynamic view is made.</param>
        /// <returns>awaitable task</returns>
        public async Task RunAsync(ChoiceCallback callback = null)
        {
            int idx = -1;
            do
            {
                Prompt prompt = new Prompt(Title, true);
                foreach (DynamicOption<T> option in Options)
                    prompt.Add(option.Entry.MessageExpression(), option.Entry.ColorExpression());// option.BuildMessage(Source), option.BuildColor(Source));
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
    
        /// <summary>
        /// Renders the current dynamic view
        /// </summary>
        /// <param name="callback">Callback function whenever a choice for this dynamic view is made.</param>
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
