using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a static console view containing a set of predefined options.
    /// This view can display options to the user and respond to their selection, including supporting asynchronous rendering.
    /// </summary>
    public abstract class StaticView : IView
    {
        private bool _goBackRequested = false;
        private string _goBackMessage = RenderOptions.DefaultGoBackMessage;

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the list of view options available in this view.
        /// </summary>
        public List<ViewOption> Options { get; set; } = new List<ViewOption>();

        /// <summary>
        /// Gets a value indicating whether a "go back" request has been made.
        /// </summary>
        public bool GoBackRequested => _goBackRequested;

        public ChoiceCallback OnOptionSelected { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticView"/> class.
        /// The constructor initializes the view based on attributes and methods that define the options for the view.
        /// </summary>
        public StaticView(ChoiceCallback callback = null)
        {
            OnOptionSelected = callback;

            Type thisType = this.GetType();

            // Set the view's title and "go back" message if the ViewAttribute is present.
            Type viewType = typeof(ViewAttribute);
            ViewAttribute viewAttr = thisType.GetCustomAttribute(viewType) as ViewAttribute;
            if (viewAttr != null)
            {
                Title = viewAttr.Title;
                _goBackMessage = viewAttr.GoBackMessage;
            }

            // Build the options from local methods decorated with the ViewOption attribute.
            Type viewOptionType = typeof(ViewOptionAttribute);
            IEnumerable<MethodInfo> allOptionMethods = thisType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<MethodInfo> viewOptionMethods = allOptionMethods.Where(o => o.GetCustomAttribute(viewOptionType) != null);

            foreach (MethodInfo method in viewOptionMethods)
            {
                ViewOptionAttribute attr = method.GetCustomAttribute(viewOptionType) as ViewOptionAttribute;
                if (attr == null)
                    continue;

                ParameterInfo[] methodParameters = method.GetParameters();
                List<object> implementedMethodParameters = new List<object>();

                // Set default values for parameters where applicable.
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

                // Create a callback for the option's action.
                ViewOptionCallback p = delegate ()
                {
                    thisType.InvokeMember(
                        method.Name,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                        null,
                        this,
                        useParameters ? implementedMethodParameters.ToArray() : null
                    );
                };

                // Add the option to the list of available options in the view.
                Options.Add(new ViewOption(
                    attr.Message,
                    p,
                    attr.Color)
                );
            }
        }

        /// <summary>
        /// Requests to navigate back from the current view.
        /// </summary>
        public void GoBack()
        {
            _goBackRequested = true;
        }

        /// <summary>
        /// Asynchronously renders the view and handles user selection of options.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            int idx = -1;
            do
            {
                Console.Clear();

                BannerEntry.Render(Title, RenderOptions.PromptColor);

                // Create and configure a SelectionPrompt to display the view options.
                SelectionPrompt prompt = new SelectionPrompt(string.Empty, false);
                foreach (ViewOption option in Options)
                {
                    prompt.Add(option.Render(), option.Color);
                }

                // Add the "go back" option to the prompt.
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
                    if (idx >= 0 && idx < Options.Count)
                    {
                        // Execute the selected option's action asynchronously.
                        await Task.Run(() =>
                        {
                            try
                            {
                                Options[idx].Action.Invoke();
                            }
                            catch (Exception ex2)
                            {
                                Consoul.Write(ex2, $"Failed to render '{Title}[{idx}]' view", true, RenderOptions.InvalidColor);
                                if (RenderOptions.WaitOnError)
                                    Consoul.Wait();
                            }
                        });

                        // Invoke the callback if provided.
                        if (OnOptionSelected != null)
                        {
                            await OnOptionSelected(idx);
                        }

                        idx = -1;
                    }
                    else if (idx == Options.Count)
                    {
                        idx = int.MaxValue; // "Go back" selected.
                    }
                    
                }
                catch (Exception ex)
                {
                    Consoul.Write(ex, $"Failed to render '{Title}' view", true, RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError)
                        Consoul.Wait();
                }
            } while (idx < 0 && !GoBackRequested);
        }

        /// <summary>
        /// Renders the view synchronously and waits for user input.
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
                if (RenderOptions.WaitOnError) Consoul.Wait();
            }
        }
    }
}
