using System;

namespace ConsoulLibrary.Views
{
    internal readonly struct ViewStackEntry
    {
        internal ViewStackEntry(Type viewType, Func<IView> factory, Action<IView> configure, string displayName = null)
        {
            ViewType = viewType;
            Factory = factory;
            Configure = configure;
            DisplayName = displayName;
        }

        internal Type ViewType { get; }

        internal Func<IView> Factory { get; }

        internal Action<IView> Configure { get; }

        internal string DisplayName { get; }

        internal ViewStackEntry WithDisplayName(string displayName)
        {
            return new ViewStackEntry(ViewType, Factory, Configure, displayName);
        }
    }
}
