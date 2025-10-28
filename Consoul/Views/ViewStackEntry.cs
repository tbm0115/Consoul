using System;

namespace ConsoulLibrary.Views
{
    internal readonly struct ViewStackEntry
    {
        internal ViewStackEntry(Type viewType, Func<IView> factory, Action<IView> configure)
        {
            ViewType = viewType;
            Factory = factory;
            Configure = configure;
        }

        internal Type ViewType { get; }

        internal Func<IView> Factory { get; }

        internal Action<IView> Configure { get; }
    }
}
