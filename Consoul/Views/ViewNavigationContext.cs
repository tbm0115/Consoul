namespace ConsoulLibrary.Views
{
    internal class ViewNavigationContext
    {
        private NavigationCommand _pendingCommand = NavigationCommand.None;

        internal NavigationCommand PendingCommand => _pendingCommand;

        internal bool HasPendingCommand => _pendingCommand.HasValue;

        internal void RequestNavigation(NavigationCommand command)
        {
            _pendingCommand = command;
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
