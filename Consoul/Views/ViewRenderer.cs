using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary.Views
{
    /// <summary>
    /// Contains methods for fluently chaining renderings and managing multiple <see cref="IView"/>s.
    /// </summary>
    public class ViewRenderer
    {
        /// <summary>
        /// Renders the specified implementation of <see cref="IView"/>.
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="IView"/>.</typeparam>
        /// <param name="factory">Optional factory function to create the view instance.</param>
        /// <param name="configure">Optional action to configure the view instance before rendering.</param>
        /// <returns>Reference to the current <see cref="ViewRenderer"/> to fluently chain commands.</returns>
        public ViewRenderer Render<T>(Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            return Render<T>(CancellationToken.None, factory, configure);
        }

        /// <summary>
        /// Renders the specified implementation of <see cref="IView"/>.
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="IView"/>.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <param name="factory">Optional factory function to create the view instance.</param>
        /// <param name="configure">Optional action to configure the view instance before rendering.</param>
        /// <returns>Reference to the current <see cref="ViewRenderer"/> to fluently chain commands.</returns>
        public ViewRenderer Render<T>(CancellationToken cancellationToken, Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            try
            {
                RunNavigationLoopAsync<T>(factory, configure, cancellationToken).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Consoul.Write(ex, "Failed to render Consoul view");
                if (RenderOptions.WaitOnError)
                {
                    Consoul.Wait();
                }

                if (RenderOptions.ViewErrorMode == RenderOptions.ViewErrorModes.Throw)
                {
                    throw;
                }
            }

            return this;
        }

        /// <summary>
        /// Renders the specified implementation of <see cref="IView"/>.
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="IView"/>.</typeparam>
        /// <param name="factory">Optional factory function to create the view instance.</param>
        /// <param name="configure">Optional action to configure the view instance before rendering.</param>
        /// <returns>Reference to the current <see cref="ViewRenderer"/> to fluently chain commands.</returns>
        public async Task<ViewRenderer> RenderAsync<T>(Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            return await RenderAsync<T>(CancellationToken.None, factory, configure);
        }

        /// <summary>
        /// Renders the specified implementation of <see cref="IView"/>.
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="IView"/>.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <param name="factory">Optional factory function to create the view instance.</param>
        /// <param name="configure">Optional action to configure the view instance before rendering.</param>
        /// <returns>Reference to the current <see cref="ViewRenderer"/> to fluently chain commands.</returns>
        public async Task<ViewRenderer> RenderAsync<T>(CancellationToken cancellationToken, Func<IView> factory = null, Action<IView> configure = null) where T : IView
        {
            try
            {
                await RunNavigationLoopAsync<T>(factory, configure, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Consoul.Write(ex, "Failed to render Consoul view");
                if (RenderOptions.WaitOnError)
                {
                    Consoul.Wait();
                }

                if (RenderOptions.ViewErrorMode == RenderOptions.ViewErrorModes.Throw)
                {
                    throw;
                }
            }

            return this;
        }

        /// <summary>
        /// Saves all inputs to the specified filepath.
        /// </summary>
        /// <param name="filepath">Filepath to save the XML containing the list of Consoul inputs.</param>
        public void SaveInput(string filepath)
        {
            var xRoutine = new XmlRoutine();
            xRoutine.SaveInputs(filepath);
        }

        private async Task RunNavigationLoopAsync<T>(Func<IView> factory = null, Action<IView> configure = null, CancellationToken cancellationToken = default) where T : IView
        {
            Type initialViewType = typeof(T);
            Stack<ViewStackEntry> navigationStack = new Stack<ViewStackEntry>();
            navigationStack.Push(new ViewStackEntry(initialViewType, factory, configure, ResolveViewTypeTitle(initialViewType)));

            while (navigationStack.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ViewStackEntry currentEntry = navigationStack.Peek();
                Type currentViewType = currentEntry.ViewType;
                IView viewInstance;

                try
                {
                    if (currentEntry.Factory != null)
                    {
                        viewInstance = currentEntry.Factory();
                        currentViewType = viewInstance?.GetType();
                    }
                    else
                    {
                        viewInstance = currentViewType != null ? Activator.CreateInstance(currentViewType) as IView : null;
                    }
                }
                catch (Exception ex)
                {
                    string failureName = currentViewType != null ? currentViewType.FullName : "<unknown view>";
                    Consoul.Write(ex, $"Failed to create view '{failureName}'", true, RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError)
                    {
                        Consoul.Wait();
                    }

                    if (RenderOptions.ViewErrorMode == RenderOptions.ViewErrorModes.Throw)
                    {
                        throw;
                    }

                    navigationStack.Pop();
                    continue;
                }

                if (viewInstance == null)
                {
                    string failureName = currentViewType != null ? currentViewType.FullName : "<unknown view>";
                    Consoul.Write($"View type '{failureName}' does not implement IView.", ConsoleColor.Red);
                    if (RenderOptions.WaitOnError)
                    {
                        Consoul.Wait();
                    }

                    navigationStack.Pop();
                    continue;
                }

                if (currentEntry.Configure != null)
                {
                    try
                    {
                        currentEntry.Configure(viewInstance);
                    }
                    catch (Exception ex)
                    {
                        string configureName = currentViewType != null ? currentViewType.FullName : viewInstance.GetType().FullName;
                        Consoul.Write(ex, $"Failed to configure view '{configureName}'", true, RenderOptions.InvalidColor);
                        if (RenderOptions.WaitOnError)
                        {
                            Consoul.Wait();
                        }

                        if (RenderOptions.ViewErrorMode == RenderOptions.ViewErrorModes.Throw)
                        {
                            throw;
                        }

                        navigationStack.Pop();
                        continue;
                    }
                }

                string currentDisplayName = ResolveViewTitle(viewInstance, currentViewType);
                currentEntry = currentEntry.WithDisplayName(currentDisplayName);
                navigationStack.Pop();
                navigationStack.Push(currentEntry);

                ViewNavigationContext navigationContext = new ViewNavigationContext();
                navigationContext.SetBreadcrumbTrail(BuildBreadcrumbTrail(navigationStack, currentDisplayName));
                if (viewInstance is INavigationAwareView navigationAwareView)
                {
                    navigationAwareView.NavigationContext = navigationContext;
                }

                try
                {
                    await viewInstance.RenderAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Consoul.Write(ex, $"Failed to render '{currentViewType.FullName}' view", true, RenderOptions.InvalidColor);
                    if (RenderOptions.WaitOnError)
                    {
                        Consoul.Wait();
                    }

                    if (RenderOptions.ViewErrorMode == RenderOptions.ViewErrorModes.Throw)
                    {
                        throw;
                    }
                }

                NavigationCommand command = navigationContext.Consume();
                if (!command.HasValue)
                {
                    navigationStack.Pop();
                    continue;
                }

                switch (command.CommandType)
                {
                    case NavigationCommandType.Pop:
                        navigationStack.Pop();
                        break;
                    case NavigationCommandType.Push:
                        if (command.HasTarget)
                        {
                            navigationStack.Push(CreateStackEntry(command));
                        }
                        break;
                    case NavigationCommandType.Replace:
                        navigationStack.Pop();
                        if (command.HasTarget)
                        {
                            navigationStack.Push(CreateStackEntry(command));
                        }
                        break;
                }
            }
        }

        private static ViewStackEntry CreateStackEntry(NavigationCommand command)
        {
            return new ViewStackEntry(command.TargetViewType, command.TargetFactory, command.ConfigureAction, ResolveViewTypeTitle(command.TargetViewType));
        }

        private static IEnumerable<string> BuildBreadcrumbTrail(Stack<ViewStackEntry> navigationStack, string currentDisplayName)
        {
            var trail = navigationStack
                .Reverse()
                .Select(entry => entry.DisplayName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (trail.Count > 0 && !string.IsNullOrWhiteSpace(currentDisplayName))
            {
                trail[trail.Count - 1] = currentDisplayName;
            }

            return trail;
        }

        private static string ResolveViewTitle(IView view, Type fallbackType)
        {
            string title = view != null ? NormalizeTitle(view.Title) : null;
            if (!string.IsNullOrWhiteSpace(title) && !HasLineBreak(view.Title))
            {
                return title;
            }

            return ResolveViewTypeTitle(fallbackType);
        }

        private static string ResolveViewTypeTitle(Type viewType)
        {
            if (viewType == null)
            {
                return "View";
            }

            var attribute = viewType.GetCustomAttribute<ViewAttribute>(true);
            string title = NormalizeTitle(attribute?.Title);
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title;
            }

            return viewType.Name;
        }

        private static bool HasLineBreak(string value)
        {
            return value != null && value.IndexOfAny(new[] { '\r', '\n' }) >= 0;
        }

        private static string NormalizeTitle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();

            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }

            return normalized;
        }
    }
}
