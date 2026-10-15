using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    internal static class CommandCompletionProvider
    {
        public static IReadOnlyList<CommandCompletionItem> GetCompletions(
            CommandAppConfigurator configurator,
            CommandRegistration defaultCommand,
            string[] args)
        {
            configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            args = args ?? new string[0];

            string current = args.Length == 0 ? string.Empty : args[args.Length - 1] ?? string.Empty;
            string[] completed = args.Length == 0 ? new string[0] : args.Take(args.Length - 1).ToArray();
            CompletionState state = WalkCompletedTokens(configurator, defaultCommand, completed);
            if (state.Invalid || state.AfterTerminator)
            {
                return Array.Empty<CommandCompletionItem>();
            }

            if (state.PendingOption != null)
            {
                return CompleteOptionValues(state.PendingOption, current, null);
            }

            if (TrySplitInlineOptionValue(current, out string optionToken, out string valuePrefix))
            {
                CommandOptionDescriptor option = FindOption(GetActiveOptions(state, defaultCommand), optionToken, configurator.Settings.CaseSensitive);
                return option == null
                    ? Array.Empty<CommandCompletionItem>()
                    : CompleteOptionValues(option, valuePrefix, optionToken + "=");
            }

            if (LooksLikeOption(current))
            {
                return CompleteOptions(GetActiveOptions(state, defaultCommand), current);
            }

            if (state.Command != null)
            {
                return Array.Empty<CommandCompletionItem>();
            }

            return CompleteCommands(state, configurator, defaultCommand, current);
        }

        private static CompletionState WalkCompletedTokens(
            CommandAppConfigurator configurator,
            CommandRegistration defaultCommand,
            string[] completed)
        {
            var state = new CompletionState();
            foreach (string rawToken in completed ?? new string[0])
            {
                string token = rawToken ?? string.Empty;
                if (token == "--")
                {
                    state.AfterTerminator = true;
                    return state;
                }

                if (state.PendingOption != null)
                {
                    state.PendingOption = null;
                    continue;
                }

                if (state.Command != null)
                {
                    ConsumeOptionToken(state, token, CommandSettingsBinder.Describe(state.Command.SettingsType).Options, configurator.Settings.CaseSensitive);
                    continue;
                }

                if (state.Branch != null)
                {
                    IReadOnlyList<CommandOptionDescriptor> branchOptions = CommandSettingsBinder.Describe(state.Branch.SettingsType).Options;
                    if (TryConsumeBranchOption(state, token, branchOptions, configurator.Settings.CaseSensitive))
                    {
                        continue;
                    }

                    CommandBranchRegistration childBranch = state.Branch.FindBranch(token, configurator.Settings.CaseSensitive);
                    if (childBranch != null)
                    {
                        state.Branch = childBranch;
                        continue;
                    }

                    CommandRegistration childCommand = state.Branch.FindCommand(token, configurator.Settings.CaseSensitive);
                    if (childCommand != null)
                    {
                        state.Command = childCommand;
                        continue;
                    }

                    state.Invalid = true;
                    return state;
                }

                CommandRegistration command = configurator.Find(token);
                if (command != null)
                {
                    state.Command = command;
                    continue;
                }

                CommandBranchRegistration branch = configurator.FindBranch(token);
                if (branch != null)
                {
                    state.Branch = branch;
                    continue;
                }

                if (IsBuiltInToken(token, configurator, defaultCommand))
                {
                    state.Invalid = true;
                    return state;
                }

                if (defaultCommand != null)
                {
                    state.Command = defaultCommand;
                    ConsumeOptionToken(state, token, CommandSettingsBinder.Describe(defaultCommand.SettingsType).Options, configurator.Settings.CaseSensitive);
                    continue;
                }

                state.Invalid = true;
                return state;
            }

            return state;
        }

        private static bool TryConsumeBranchOption(
            CompletionState state,
            string token,
            IReadOnlyList<CommandOptionDescriptor> branchOptions,
            bool caseSensitive)
        {
            if (!LooksLikeOption(token))
            {
                return false;
            }

            return ConsumeOptionToken(state, token, branchOptions, caseSensitive);
        }

        private static bool ConsumeOptionToken(
            CompletionState state,
            string token,
            IReadOnlyList<CommandOptionDescriptor> options,
            bool caseSensitive)
        {
            CommandOptionDescriptor option = FindOption(options, GetOptionName(token), caseSensitive);
            if (option == null)
            {
                return false;
            }

            if (!OptionConsumesSeparateValue(option, token))
            {
                return true;
            }

            state.PendingOption = option;
            return true;
        }

        private static IEnumerable<CommandOptionDescriptor> GetActiveOptions(CompletionState state, CommandRegistration defaultCommand)
        {
            if (state.Command != null)
            {
                return CommandSettingsBinder.Describe(state.Command.SettingsType).Options.Where(option => !option.IsHidden);
            }

            if (state.Branch != null)
            {
                return CommandSettingsBinder.Describe(state.Branch.SettingsType).Options.Where(option => !option.IsHidden);
            }

            if (defaultCommand != null)
            {
                return CommandSettingsBinder.Describe(defaultCommand.SettingsType).Options.Where(option => !option.IsHidden);
            }

            return Enumerable.Empty<CommandOptionDescriptor>();
        }

        private static IReadOnlyList<CommandCompletionItem> CompleteCommands(
            CompletionState state,
            CommandAppConfigurator configurator,
            CommandRegistration defaultCommand,
            string prefix)
        {
            var items = new List<CommandCompletionItem>();
            if (state.Branch != null)
            {
                AddBranchItems(items, state.Branch.Branches, prefix);
                AddCommandItems(items, state.Branch.Commands, prefix);
                AddOptionItems(items, CommandSettingsBinder.Describe(state.Branch.SettingsType).Options, prefix);
            }
            else
            {
                AddBuiltIns(items, configurator, defaultCommand, prefix);
                AddBranchItems(items, configurator.Branches, prefix);
                AddCommandItems(items, configurator.Commands, prefix);
            }

            return Distinct(items);
        }

        private static IReadOnlyList<CommandCompletionItem> CompleteOptions(IEnumerable<CommandOptionDescriptor> options, string prefix)
        {
            var items = new List<CommandCompletionItem>();
            AddOptionItems(items, options, prefix);
            AddIfMatches(items, "--help", prefix, CommandCompletionKind.BuiltIn, "Show help.");
            AddIfMatches(items, "-h", prefix, CommandCompletionKind.BuiltIn, "Show help.");
            return Distinct(items);
        }

        private static IReadOnlyList<CommandCompletionItem> CompleteOptionValues(
            CommandOptionDescriptor option,
            string prefix,
            string inlinePrefix)
        {
            var items = new List<CommandCompletionItem>();
            Type valueType = GetOptionValueType(option);
            if (valueType == typeof(bool))
            {
                AddValue(items, "true", prefix, inlinePrefix);
                AddValue(items, "false", prefix, inlinePrefix);
                return Distinct(items);
            }

            if (valueType == null || !valueType.IsEnum)
            {
                return Array.Empty<CommandCompletionItem>();
            }

            foreach (string value in CommandSettingsBinder.GetEnumValueHelpNames(valueType))
            {
                AddValue(items, value, prefix, inlinePrefix);
            }

            return Distinct(items);
        }

        private static void AddBuiltIns(
            IList<CommandCompletionItem> items,
            CommandAppConfigurator configurator,
            CommandRegistration defaultCommand,
            string prefix)
        {
            AddIfMatches(items, "help", prefix, CommandCompletionKind.BuiltIn, "Show help.");
            AddIfMatches(items, "--help", prefix, CommandCompletionKind.BuiltIn, "Show help.");
            AddIfMatches(items, "-h", prefix, CommandCompletionKind.BuiltIn, "Show help.");

            bool versionCommandRegistered = configurator.Commands.Any(command => command.Matches("version", configurator.Settings.CaseSensitive));
            if (!string.IsNullOrWhiteSpace(configurator.Settings.ApplicationVersion))
            {
                AddIfMatches(items, "--version", prefix, CommandCompletionKind.BuiltIn, "Show version.");
                AddIfMatches(items, "-v", prefix, CommandCompletionKind.BuiltIn, "Show version.");
                if (!versionCommandRegistered)
                {
                    AddIfMatches(items, "version", prefix, CommandCompletionKind.BuiltIn, "Show version.");
                }
            }

            if (defaultCommand != null && LooksLikeOption(prefix))
            {
                AddOptionItems(items, CommandSettingsBinder.Describe(defaultCommand.SettingsType).Options, prefix);
            }
        }

        private static void AddBranchItems(IList<CommandCompletionItem> items, IEnumerable<CommandBranchRegistration> branches, string prefix)
        {
            foreach (CommandBranchRegistration branch in (branches ?? Enumerable.Empty<CommandBranchRegistration>()).Where(branch => !branch.Hidden))
            {
                AddIfMatches(items, branch.Name, prefix, CommandCompletionKind.Branch, branch.Description);
                foreach (string alias in branch.Aliases)
                {
                    AddIfMatches(items, alias, prefix, CommandCompletionKind.Branch, branch.Description);
                }
            }
        }

        private static void AddCommandItems(IList<CommandCompletionItem> items, IEnumerable<CommandRegistration> commands, string prefix)
        {
            foreach (CommandRegistration command in (commands ?? Enumerable.Empty<CommandRegistration>()).Where(command => !command.Hidden))
            {
                AddIfMatches(items, command.Name, prefix, CommandCompletionKind.Command, command.Description);
                foreach (string alias in command.Aliases)
                {
                    AddIfMatches(items, alias, prefix, CommandCompletionKind.Command, command.Description);
                }
            }
        }

        private static void AddOptionItems(IList<CommandCompletionItem> items, IEnumerable<CommandOptionDescriptor> options, string prefix)
        {
            foreach (CommandOptionDescriptor option in (options ?? Enumerable.Empty<CommandOptionDescriptor>()).Where(option => !option.IsHidden))
            {
                foreach (string alias in option.Aliases)
                {
                    AddIfMatches(items, alias, prefix, CommandCompletionKind.Option, option.Description);
                }
            }
        }

        private static void AddValue(IList<CommandCompletionItem> items, string value, string prefix, string inlinePrefix)
        {
            if (!MatchesPrefix(value, prefix))
            {
                return;
            }

            items.Add(new CommandCompletionItem((inlinePrefix ?? string.Empty) + value, CommandCompletionKind.Value));
        }

        private static void AddIfMatches(
            IList<CommandCompletionItem> items,
            string value,
            string prefix,
            CommandCompletionKind kind,
            string description)
        {
            if (!MatchesPrefix(value, prefix))
            {
                return;
            }

            items.Add(new CommandCompletionItem(value, kind, description));
        }

        private static IReadOnlyList<CommandCompletionItem> Distinct(IEnumerable<CommandCompletionItem> items)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return (items ?? Enumerable.Empty<CommandCompletionItem>())
                .Where(item => item != null && seen.Add(item.Value))
                .OrderBy(item => item.Value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static CommandOptionDescriptor FindOption(IEnumerable<CommandOptionDescriptor> options, string optionName, bool caseSensitive)
        {
            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return (options ?? Enumerable.Empty<CommandOptionDescriptor>())
                .FirstOrDefault(option => option.Aliases.Any(alias => string.Equals(alias, optionName, comparison)));
        }

        private static Type GetOptionValueType(CommandOptionDescriptor option)
        {
            if (option == null)
            {
                return null;
            }

            Type type = option.IsArray
                ? option.Property.PropertyType.GetElementType()
                : option.CollectionElementType ?? option.FlagValueType ?? option.Property.PropertyType;
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static bool OptionConsumesSeparateValue(CommandOptionDescriptor option, string token)
        {
            return option != null &&
                !option.IsBoolean &&
                !option.ValueIsOptional &&
                token.IndexOf('=') < 0;
        }

        private static bool IsBuiltInToken(string token, CommandAppConfigurator configurator, CommandRegistration defaultCommand)
        {
            if (string.Equals(token, "help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "-h", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "--version", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "-v", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(token, "version", StringComparison.OrdinalIgnoreCase) &&
                !configurator.Commands.Any(command => command.Matches("version", configurator.Settings.CaseSensitive));
        }

        private static bool TrySplitInlineOptionValue(string token, out string optionToken, out string valuePrefix)
        {
            optionToken = string.Empty;
            valuePrefix = string.Empty;
            if (!LooksLikeOption(token))
            {
                return false;
            }

            int equalsIndex = token.IndexOf('=');
            if (equalsIndex < 0)
            {
                return false;
            }

            optionToken = token.Substring(0, equalsIndex);
            valuePrefix = token.Substring(equalsIndex + 1);
            return true;
        }

        private static string GetOptionName(string token)
        {
            string optionName = token ?? string.Empty;
            int equalsIndex = optionName.IndexOf('=');
            return equalsIndex >= 0 ? optionName.Substring(0, equalsIndex) : optionName;
        }

        private static bool LooksLikeOption(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '-';
        }

        private static bool MatchesPrefix(string value, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ||
                (value ?? string.Empty).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class CompletionState
        {
            public CommandBranchRegistration Branch { get; set; }

            public CommandRegistration Command { get; set; }

            public CommandOptionDescriptor PendingOption { get; set; }

            public bool AfterTerminator { get; set; }

            public bool Invalid { get; set; }
        }
    }
}
