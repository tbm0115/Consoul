using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Views;

namespace ConsoulLibrary
{
    /// <summary>
    /// An abstract view that relies on an underlying model to dynamically change the labels and colors of choices whenever the view re-renders.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DynamicView<T> : IView, INavigationAwareView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;
        private ViewNavigationContext _navigationContext = new ViewNavigationContext();

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

        ViewNavigationContext INavigationAwareView.NavigationContext
        {
            get => _navigationContext;
            set => _navigationContext = value ?? new ViewNavigationContext();
        }

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
            _navigationContext.RequestNavigation(NavigationCommand.Pop());
        }

        /// <summary>
        /// Requests navigation to a new view instance of type <typeparamref name="TView"/>.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="replace">
        /// When <see langword="true"/> (default), the current view is replaced with the new view. When <see langword="false"/>,
        /// the new view is pushed on top of the navigation stack.
        /// </param>
        protected void NavigateTo<TView>(bool replace = true) where TView : IView
        {
            if (replace)
            {
                _navigationContext.RequestNavigation(NavigationCommand.Replace(typeof(TView)));
            }
            else
            {
                _navigationContext.RequestNavigation(NavigationCommand.Push(typeof(TView)));
            }
        }

        /// <summary>
        /// Requests navigation to a new view created via the provided factory.
        /// </summary>
        /// <typeparam name="TView">The view type to navigate to.</typeparam>
        /// <param name="viewFactory">Factory used to create the view instance.</param>
        /// <param name="replace">
        /// When <see langword="true"/> (default), the current view is replaced with the new view. When <see langword="false"/>,
        /// the new view is pushed on top of the navigation stack.
        /// </param>
        protected void NavigateTo<TView>(Func<TView> viewFactory, bool replace = true) where TView : IView
        {
            if (viewFactory == null)
            {
                throw new ArgumentNullException(nameof(viewFactory));
            }

            if (replace)
            {
                _navigationContext.RequestNavigation(NavigationCommand.Replace(() => (IView)viewFactory()));
            }
            else
            {
                _navigationContext.RequestNavigation(NavigationCommand.Push(() => (IView)viewFactory()));
            }
        }

        /// <summary>
        /// Renders the current dynamic view
        /// </summary>
        /// <returns>awaitable task</returns>
        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            int idx = -1;
            _goBackRequested = false;
            _navigationContext.Reset();
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
                        _navigationContext.Reset();
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
                        if (_navigationContext.HasPendingCommand)
                        {
                            if (_navigationContext.PendingCommand.CommandType == NavigationCommandType.Pop)
                            {
                                _goBackRequested = true;
                            }
                            break;
                        }

                        if (_goBackRequested)
                        {
                            break;
                        }

                        idx = -1;
                    }
                    else if (idx == _options.Count)
                    {
                        GoBack();
                        break;
                    }

                }
                catch (Exception ex)
                {
                    Consoul.Write(ex, $"Failed to render '{Title}[{idx}]' view", true, RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError)
                        Consoul.Wait();
                }
            } while (idx < 0 && !_navigationContext.HasPendingCommand && !GoBackRequested);
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
