using System;
using ConsoulLibrary;

namespace ConsoulLibrary.Views
{
    internal readonly struct NavigationCommand
    {
        private NavigationCommand(
            NavigationCommandType type,
            Type targetType,
            Func<IView> factory,
            Action<IView> configureAction)
        {
            CommandType = type;
            TargetViewType = targetType;
            TargetFactory = factory;
            ConfigureAction = configureAction;
        }

        internal NavigationCommandType CommandType { get; }

        internal Type TargetViewType { get; }

        internal Func<IView> TargetFactory { get; }

        internal Action<IView> ConfigureAction { get; }

        internal static NavigationCommand None => new NavigationCommand(NavigationCommandType.None, null, null, null);

        internal static NavigationCommand Push(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }

            return new NavigationCommand(NavigationCommandType.Push, viewType, null, null);
        }

        internal static NavigationCommand Push(Type viewType, Action<IView> configure)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }

            return new NavigationCommand(NavigationCommandType.Push, viewType, null, configure);
        }

        internal static NavigationCommand Push(Func<IView> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new NavigationCommand(NavigationCommandType.Push, null, factory, null);
        }

        internal static NavigationCommand Push(Func<IView> factory, Action<IView> configure)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new NavigationCommand(NavigationCommandType.Push, null, factory, configure);
        }

        internal static NavigationCommand Replace(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }

            return new NavigationCommand(NavigationCommandType.Replace, viewType, null, null);
        }

        internal static NavigationCommand Replace(Type viewType, Action<IView> configure)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }

            return new NavigationCommand(NavigationCommandType.Replace, viewType, null, configure);
        }

        internal static NavigationCommand Replace(Func<IView> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new NavigationCommand(NavigationCommandType.Replace, null, factory, null);
        }

        internal static NavigationCommand Replace(Func<IView> factory, Action<IView> configure)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new NavigationCommand(NavigationCommandType.Replace, null, factory, configure);
        }

        internal static NavigationCommand Pop() => new NavigationCommand(NavigationCommandType.Pop, null, null, null);

        internal bool HasTarget => TargetViewType != null || TargetFactory != null;

        internal bool HasFactory => TargetFactory != null;

        internal bool HasConfigureAction => ConfigureAction != null;

        internal bool HasValue => CommandType != NavigationCommandType.None;
    }
}
