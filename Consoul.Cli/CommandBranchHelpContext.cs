using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides data used to render help for a command branch.
    /// </summary>
    public sealed class CommandBranchHelpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBranchHelpContext"/> class.
        /// </summary>
        /// <param name="settings">The command app settings.</param>
        /// <param name="path">The command tokens that identify the branch.</param>
        /// <param name="branch">The branch registration.</param>
        public CommandBranchHelpContext(CommandAppSettings settings, IReadOnlyList<string> path, CommandBranchRegistration branch)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Path = path == null ? Array.Empty<string>() : path.ToArray();
            Branch = branch ?? throw new ArgumentNullException(nameof(branch));
            BranchInfo = CommandHelpModelFactory.CreateContainer(Branch);
        }

        /// <summary>
        /// Gets the command app settings.
        /// </summary>
        public CommandAppSettings Settings { get; }

        /// <summary>
        /// Gets the Spectre-shaped command container for custom help renderers.
        /// </summary>
        public ICommandContainer BranchInfo { get; }

        /// <summary>
        /// Gets the application name shown in help output.
        /// </summary>
        public string ApplicationName => Settings.ApplicationName;

        /// <summary>
        /// Gets the command tokens that identify the branch.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        /// Gets the branch path as a display string.
        /// </summary>
        public string BranchPath => string.Join(" ", Path);

        /// <summary>
        /// Gets the branch registration.
        /// </summary>
        public CommandBranchRegistration Branch { get; }

        /// <summary>
        /// Gets shared options declared by the branch settings type.
        /// </summary>
        public IReadOnlyList<ICommandOption> Options => BranchInfo.Options;
    }
}
