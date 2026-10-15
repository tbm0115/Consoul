using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary.Views
{
    internal class ViewNavigationContext
    {
        private NavigationCommand _pendingCommand = NavigationCommand.None;
        private IReadOnlyList<string> _breadcrumbTrail = new List<string>();

        internal NavigationCommand PendingCommand => _pendingCommand;

        internal bool HasPendingCommand => _pendingCommand.HasValue;

        internal IReadOnlyList<string> BreadcrumbTrail => _breadcrumbTrail;

        internal void RequestNavigation(NavigationCommand command)
        {
            _pendingCommand = command;
        }

        internal void SetBreadcrumbTrail(IEnumerable<string> breadcrumbTrail)
        {
            _breadcrumbTrail = breadcrumbTrail == null
                ? (IReadOnlyList<string>)new List<string>()
                : breadcrumbTrail.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
        }

        internal void Reset()
        {
            _pendingCommand = NavigationCommand.None;
        }

        internal NavigationCommand Consume()
        {
            NavigationCommand command = _pendingCommand;
            _pendingCommand = NavigationCommand.None;
            return command;
        }
    }
}
