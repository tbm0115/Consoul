using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    internal sealed class DelegateCommand : IAsyncCommand
    {
        private readonly Func<CommandContext, CancellationToken, Task<int>> _execute;

        private DelegateCommand(Func<CommandContext, CancellationToken, Task<int>> execute)
        {
            _execute = execute;
        }

        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            return _execute(context, cancellationToken);
        }

        public static DelegateCommand FromAction(Action<CommandContext> execute)
        {
            return new DelegateCommand((context, cancellationToken) =>
            {
                execute(context);
                return Task.FromResult(0);
            });
        }

        public static DelegateCommand FromFunc(Func<CommandContext, int> execute)
        {
            return new DelegateCommand((context, cancellationToken) => Task.FromResult(execute(context)));
        }

        public static DelegateCommand FromAsyncFunc(Func<CommandContext, CancellationToken, Task<int>> execute)
        {
            return new DelegateCommand(execute);
        }
    }

    internal sealed class DelegateCommand<TSettings> : IAsyncCommand<TSettings> where TSettings : CommandSettings
    {
        private readonly Func<CommandContext, TSettings, CancellationToken, Task<int>> _execute;

        private DelegateCommand(Func<CommandContext, TSettings, CancellationToken, Task<int>> execute)
        {
            _execute = execute;
        }

        public Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken = default)
        {
            return _execute(context, settings, cancellationToken);
        }

        public static DelegateCommand<TSettings> FromAction(Action<CommandContext, TSettings> execute)
        {
            return new DelegateCommand<TSettings>((context, settings, cancellationToken) =>
            {
                execute(context, settings);
                return Task.FromResult(0);
            });
        }

        public static DelegateCommand<TSettings> FromFunc(Func<CommandContext, TSettings, int> execute)
        {
            return new DelegateCommand<TSettings>((context, settings, cancellationToken) => Task.FromResult(execute(context, settings)));
        }

        public static DelegateCommand<TSettings> FromAsyncFunc(Func<CommandContext, TSettings, CancellationToken, Task<int>> execute)
        {
            return new DelegateCommand<TSettings>(execute);
        }
    }
}
