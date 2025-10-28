using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
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

        internal string _title;
        /// <summary>
        /// Text displayed at the top of the console
        /// </summary>
        public string Title { get => _title; set { _title = value; } }

        internal List<DynamicOption<T>> _options = new List<DynamicOption<T>>();
        /// <summary>
        /// Collection of view options
        /// </summary>
        public IEnumerable<DynamicOption<T>> Options => _options;

        /// <summary>
        /// Flag indicating whether or not an underlying process has requested this view to go back.
        /// </summary>
        public bool GoBackRequested => _goBackRequested;

        /// <summary>
        /// Reference to the source model of the dynamic view.
        /// </summary>
        public T Model { get; set; }

        public ChoiceCallback OnOptionSelected { get; set; }

        public DynamicView(ChoiceCallback callback = null)
        {
            OnOptionSelected = callback;

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
                if (attr == null)
                    continue;

                MethodInfo messageBuilder = allMethods.FirstOrDefault(o => o.Name == attr.MessageMethod);
                MethodInfo colorBuilder = null;
                if (!string.IsNullOrEmpty(attr.ColorMethod))
                    colorBuilder = allMethods.FirstOrDefault(o => o.Name == attr.ColorMethod);

                if (messageBuilder == null)
                    continue;

                ParameterInfo[] methodParameters = method.GetParameters();
                List<object> implementedMethodParameters = new List<object>();
                foreach (ParameterInfo methodParameter in methodParameters)
                {
                    if (methodParameter.HasDefaultValue)
                    {
                        implementedMethodParameters.Add(methodParameter.DefaultValue);
                    }
                    else if (Nullable.GetUnderlyingType(methodParameter.ParameterType) != null)
                    {
                        implementedMethodParameters.Add(null);
                    }
                }
                bool useParameters = methodParameters.Length > 0 && implementedMethodParameters.Count == methodParameters.Length;

                _options.Add(new DynamicOption<T>(
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
        /// <returns>awaitable task</returns>
        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            int idx = -1;
            do
            {
                SelectionPrompt prompt = new SelectionPrompt(Title, true);
                foreach (DynamicOption<T> option in Options)
                    prompt.Add(option.Entry.SetMessage(), option.Entry.SetColor());
                prompt.Add(_goBackMessage, RenderOptions.SubnoteColor);

                try
                {
                    var result = prompt.Render();
                    if (result.IsCanceled)
                    {
                        GoBack();
                        continue;
                    }

                    if (!result.HasSelection)
                    {
                        idx = -1;
                        continue;
                    }

                    idx = result.Index;
                    if (idx >= 0 && idx < _options.Count)
                    {
                        await Task.Run(() => {
                            try
                            {
                                _options[idx].Action.Invoke();
                            }
                            catch (Exception ex2)
                            {
                                Consoul.Write(ex2, $"Failed to render '{Title}[{idx}]' view", true, RenderOptions.InvalidColor);
                                if (RenderOptions.WaitOnError)
                                    Consoul.Wait();
                            }
                        });
                        if (OnOptionSelected != null)
                            await OnOptionSelected(idx);
                        idx = -1;
                    }
                    else if (idx == _options.Count)
                    {
                        idx = int.MaxValue;
                    }
                    
                }
                catch (Exception ex)
                {
                    Consoul.Write(ex, $"Failed to render '{Title}[{idx}]' view", true, RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError)
                        Consoul.Wait();
                }
            } while (idx < 0 && !GoBackRequested);
        }
    
        /// <summary>
        /// Renders the current dynamic view
        /// </summary>
        public void Render()
        {
            try
            {
                RenderAsync().Wait();
            }
            catch (Exception ex)
            {
                Consoul.Write(ex, $"Failed to render '{Title}' view", true, RenderOptions.InvalidColor);
                if (RenderOptions.WaitOnError)
                    Consoul.Wait();
            }
        }
    }
}
