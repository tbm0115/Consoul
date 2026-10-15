using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders command app help from the read-only command model.
    /// </summary>
    public sealed class HelpProvider : IHelpProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpProvider"/> class.
        /// </summary>
        public HelpProvider() : this(new HelpProviderStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpProvider"/> class.
        /// </summary>
        /// <param name="style">The style settings used while rendering help.</param>
        public HelpProvider(HelpProviderStyle style)
        {
            Style = style ?? new HelpProviderStyle();
        }

        /// <summary>
        /// Gets the style settings used while rendering help.
        /// </summary>
        public HelpProviderStyle Style { get; }

        /// <inheritdoc />
        public void WriteApplicationHelp(IAnsiConsole console, CommandAppHelpContext context)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ICommandModel model = context.Model;
            string title = string.IsNullOrWhiteSpace(model.ApplicationVersion)
                ? model.ApplicationName
                : model.ApplicationName + " " + model.ApplicationVersion;
            WriteStyledLine(console, Style.Command.Name, title);
            WriteDescriptionLine(console, model.ApplicationDescription, context.Settings);

            bool hasVisibleCommands = model.Commands.Any(command => !command.Hidden);
            bool hasVisibleBranches = model.Branches.Any(branch => !branch.Hidden);
            bool hasDefaultCommand = context.DefaultCommandInfo != null;

            WriteBlank(console);
            WriteHeading(console, Style.Usage, "USAGE:");
            if (hasVisibleCommands || hasVisibleBranches)
            {
                WriteStyledLine(console, Style.Usage.Syntax, "    " + model.ApplicationName + " [COMMAND] [ARGUMENTS]");
            }

            if (hasDefaultCommand)
            {
                WriteStyledLine(console, Style.Usage.Syntax, "    " + model.ApplicationName + BuildUsageSuffix(context.DefaultCommandInfo, null));
            }

            if (!hasVisibleCommands && !hasVisibleBranches)
            {
                if (!hasDefaultCommand)
                {
                    WriteStyledLine(console, Style.Usage.Syntax, "    " + model.ApplicationName + " [COMMAND] [ARGUMENTS]");
                    WriteBlank(console);
                    WriteHeading(console, Style.Command, "COMMANDS:");
                    WriteStyledLine(console, Style.Description.Text, "    No commands registered.");
                }

                WriteDefaultCommandHelp(console, context.DefaultCommandInfo, model.ApplicationName, context.Settings);
                WriteExamples(console, model.ApplicationName, model.Examples);
                return;
            }

            WriteDefaultCommandHelp(console, context.DefaultCommandInfo, model.ApplicationName, context.Settings);

            WriteBlank(console);
            WriteHeading(console, Style.Command, "COMMANDS:");
            foreach (ICommandContainer branch in model.Branches.Where(branch => !branch.Hidden))
            {
                WriteCommandRow(console, branch.Name, branch.Aliases, branch.Description, context.Settings);
            }

            foreach (ICommandInfo command in model.Commands.Where(command => !command.Hidden))
            {
                WriteCommandRow(console, command.Name, command.Aliases, command.Description, context.Settings);
            }

            WriteExamples(console, model.ApplicationName, model.Examples);
        }

        /// <inheritdoc />
        public void WriteBranchHelp(IAnsiConsole console, CommandBranchHelpContext context)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ICommandContainer branch = context.BranchInfo;
            WriteStyledLine(console, Style.Command.Name, context.BranchPath);
            WriteDescriptionLine(console, branch.Description, context.Settings);

            WriteBlank(console);
            WriteHeading(console, Style.Usage, "USAGE:");
            WriteStyledLine(console, Style.Usage.Syntax, "    " + context.ApplicationName + " " + context.BranchPath + " [COMMAND] [ARGUMENTS]");

            if (branch.Aliases.Count > 0)
            {
                WriteBlank(console);
                WriteHeading(console, Style.Command, "ALIASES:");
                WriteStyledLine(console, Style.Command.Alias, "    " + string.Join(", ", branch.Aliases));
            }

            WriteOptions(console, branch.Options.Where(option => !option.IsHidden), "OPTIONS:", context.Settings);

            WriteBlank(console);
            WriteHeading(console, Style.Command, "COMMANDS:");
            bool hasVisibleBranches = branch.Branches.Any(childBranch => !childBranch.Hidden);
            bool hasVisibleCommands = branch.Commands.Any(command => !command.Hidden);
            if (!hasVisibleBranches && !hasVisibleCommands)
            {
                WriteStyledLine(console, Style.Description.Text, "    No commands registered.");
                return;
            }

            foreach (ICommandContainer childBranch in branch.Branches.Where(childBranch => !childBranch.Hidden))
            {
                WriteCommandRow(console, childBranch.Name, childBranch.Aliases, childBranch.Description, context.Settings);
            }

            foreach (ICommandInfo command in branch.Commands.Where(command => !command.Hidden))
            {
                WriteCommandRow(console, command.Name, command.Aliases, command.Description, context.Settings);
            }
        }

        /// <inheritdoc />
        public void WriteCommandHelp(IAnsiConsole console, CommandHelpContext context)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ICommandInfo command = context.CommandInfo;
            WriteStyledLine(console, Style.Command.Name, context.CommandPath);
            WriteDescriptionLine(console, command.Description, context.Settings);

            WriteBlank(console);
            WriteHeading(console, Style.Usage, "USAGE:");
            WriteStyledLine(console, Style.Usage.Syntax, "    " + context.ApplicationName + " " + context.CommandPath + BuildUsageSuffix(command, context.InheritedOptions));

            if (command.Aliases.Count > 0)
            {
                WriteBlank(console);
                WriteHeading(console, Style.Command, "ALIASES:");
                WriteStyledLine(console, Style.Command.Alias, "    " + string.Join(", ", command.Aliases));
            }

            WriteExamples(console, context.ApplicationName, command.Examples);
            WriteArguments(console, command.Arguments, context.Settings);
            WriteOptions(console, command.Options.Where(option => !option.IsHidden), "OPTIONS:", context.Settings);
            WriteOptions(console, context.InheritedOptions, "INHERITED OPTIONS:", context.Settings);
        }

        private void WriteDefaultCommandHelp(IAnsiConsole console, ICommandInfo defaultCommand, string applicationName, CommandAppSettings settings)
        {
            if (defaultCommand == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(defaultCommand.Description))
            {
                WriteBlank(console);
                WriteHeading(console, Style.Command, "DEFAULT COMMAND:");
                WriteStyledLine(console, Style.Description.Text, "    " + FormatDescription(defaultCommand.Description, settings));
            }

            if (defaultCommand.Examples.Count > 0)
            {
                WriteBlank(console);
                WriteHeading(console, Style.Example, "DEFAULT EXAMPLES:");
                foreach (IReadOnlyList<string> example in defaultCommand.Examples)
                {
                    string suffix = example == null || example.Count == 0
                        ? string.Empty
                        : " " + string.Join(" ", example);
                    WriteStyledLine(console, Style.Example.Command, "    " + applicationName + suffix);
                }
            }

            WriteArguments(console, defaultCommand.Arguments, settings);
            WriteOptions(console, defaultCommand.Options.Where(option => !option.IsHidden), "OPTIONS:", settings);
        }

        private void WriteArguments(IAnsiConsole console, IEnumerable<ICommandArgument> arguments, CommandAppSettings settings)
        {
            ICommandArgument[] visibleArguments = arguments == null ? Array.Empty<ICommandArgument>() : arguments.ToArray();
            if (visibleArguments.Length == 0)
            {
                return;
            }

            WriteBlank(console);
            WriteHeading(console, Style.Argument, "ARGUMENTS:");
            foreach (ICommandArgument argument in visibleArguments)
            {
                string description = BuildDescription(argument.Description, argument.DefaultValue, false, true, settings);
                WriteRow(console, Style.Argument.Template, "    " + argument.Template, 28, description);
            }
        }

        private void WriteOptions(IAnsiConsole console, IEnumerable<ICommandOption> options, string heading, CommandAppSettings settings)
        {
            ICommandOption[] visibleOptions = options == null ? Array.Empty<ICommandOption>() : options.ToArray();
            if (visibleOptions.Length == 0)
            {
                return;
            }

            WriteBlank(console);
            WriteHeading(console, Style.Option, heading);
            foreach (ICommandOption option in visibleOptions)
            {
                string valueName = IsBooleanOption(option) && !IsFlagValueType(option.ValueType)
                    ? string.Empty
                    : option.ValueIsOptional
                        ? " [" + option.ValueName + "]"
                        : " <" + option.ValueName + ">";
                string aliases = string.Join(", ", option.Aliases) + valueName;
                string description = BuildDescription(option.Description, option.DefaultValue, option.IsRequired, !settings.HideOptionDefaultValues, settings);
                WriteRow(console, Style.Option.Name, "    " + aliases, 32, description);
            }
        }

        private void WriteExamples(IAnsiConsole console, string applicationName, IReadOnlyList<IReadOnlyList<string>> examples)
        {
            if (examples == null || examples.Count == 0)
            {
                return;
            }

            WriteBlank(console);
            WriteHeading(console, Style.Example, "EXAMPLES:");
            foreach (IReadOnlyList<string> example in examples)
            {
                string suffix = example == null || example.Count == 0
                    ? string.Empty
                    : " " + string.Join(" ", example);
                WriteStyledLine(console, Style.Example.Command, "    " + applicationName + suffix);
            }
        }

        private void WriteCommandRow(IAnsiConsole console, string name, IReadOnlyList<string> aliases, string description, CommandAppSettings settings)
        {
            string aliasText = aliases == null || aliases.Count == 0
                ? string.Empty
                : " (" + string.Join(", ", aliases) + ")";
            string label = "    " + name + aliasText;
            WriteRow(console, Style.Command.Name, label, 28, FormatDescription(description, settings));
        }

        private void WriteDescriptionLine(IAnsiConsole console, string description, CommandAppSettings settings)
        {
            string text = FormatDescription(description, settings);
            if (!string.IsNullOrWhiteSpace(text))
            {
                WriteStyledLine(console, Style.Description.Text, text);
            }
        }

        private void WriteHeading(IAnsiConsole console, HelpTextStyle style, string text)
        {
            WriteStyledLine(console, style == null ? null : style.Heading, text);
        }

        private void WriteRow(IAnsiConsole console, string labelStyle, string label, int labelWidth, string description)
        {
            string paddedLabel = PadRight(label, labelWidth);
            string markup = StyleText(console, labelStyle, paddedLabel) + StyleText(console, Style.Description.Text, description);
            console.MarkupLine(markup);
        }

        private void WriteStyledLine(IAnsiConsole console, string style, string text)
        {
            if (text == null)
            {
                console.WriteLine();
                return;
            }

            console.MarkupLine(StyleText(console, style, text));
        }

        private static void WriteBlank(IAnsiConsole console)
        {
            console.WriteLine();
        }

        private static string StyleText(IAnsiConsole console, string style, string text)
        {
            text = console.EscapeMarkup(text ?? string.Empty);
            if (string.IsNullOrWhiteSpace(style))
            {
                return text;
            }

            return "[" + style + "]" + text + "[/]";
        }

        private static string BuildDescription(string description, object defaultValue, bool required, bool includeDefaultValue, CommandAppSettings settings)
        {
            string result = FormatDescription(description, settings);
            if (required)
            {
                result = AppendNote(result, "required");
            }

            if (includeDefaultValue && defaultValue != null)
            {
                result = AppendNote(result, "default: " + Convert.ToString(defaultValue, CultureInfo.InvariantCulture));
            }

            return result;
        }

        private static string FormatDescription(string description, CommandAppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            string result = description;
            if (settings != null && settings.TrimTrailingPeriods)
            {
                result = result.TrimEnd();
                while (result.EndsWith(".", StringComparison.Ordinal))
                {
                    result = result.Substring(0, result.Length - 1).TrimEnd();
                }
            }

            return result;
        }

        private static string AppendNote(string value, string note)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "[" + note + "]";
            }

            return value + " [" + note + "]";
        }

        private static string BuildUsageSuffix(ICommandInfo command, IReadOnlyList<ICommandOption> inheritedOptions)
        {
            string arguments = string.Join(" ", command.Arguments.Select(argument => argument.Template));
            bool hasOptions = command.Options.Count > 0 || (inheritedOptions != null && inheritedOptions.Count > 0);
            string options = hasOptions ? "[OPTIONS]" : string.Empty;
            string suffix = string.Join(" ", new[] { arguments, options }.Where(value => !string.IsNullOrWhiteSpace(value)));
            return string.IsNullOrWhiteSpace(suffix) ? " [ARGUMENTS]" : " " + suffix;
        }

        private static bool IsBooleanOption(ICommandOption option)
        {
            Type type = option.ValueType;
            if (type != null && type.IsArray)
            {
                return false;
            }

            if (CommandSettingsBinder.GetCollectionElementType(type) != null)
            {
                return false;
            }

            type = GetFlagValueType(type) ?? type;
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(bool);
        }

        private static bool IsFlagValueType(Type type)
        {
            return GetFlagValueType(type) != null;
        }

        private static Type GetFlagValueType(Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FlagValue<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        private static string PadRight(string value, int totalWidth)
        {
            value = value ?? string.Empty;
            return value.Length >= totalWidth ? value + "  " : value.PadRight(totalWidth);
        }
    }

    /// <summary>
    /// Configures styles used by <see cref="HelpProvider"/>.
    /// </summary>
    public sealed class HelpProviderStyle
    {
        private CommandStyle _command = new CommandStyle();
        private UsageStyle _usage = new UsageStyle();
        private ArgumentStyle _argument = new ArgumentStyle();
        private OptionStyle _option = new OptionStyle();
        private DescriptionStyle _description = new DescriptionStyle();
        private ExampleStyle _example = new ExampleStyle();

        /// <summary>
        /// Gets or sets command and branch styles.
        /// </summary>
        public CommandStyle Command
        {
            get => _command;
            set => _command = value ?? new CommandStyle();
        }

        /// <summary>
        /// Gets or sets usage styles.
        /// </summary>
        public UsageStyle Usage
        {
            get => _usage;
            set => _usage = value ?? new UsageStyle();
        }

        /// <summary>
        /// Gets or sets argument styles.
        /// </summary>
        public ArgumentStyle Argument
        {
            get => _argument;
            set => _argument = value ?? new ArgumentStyle();
        }

        /// <summary>
        /// Gets or sets option styles.
        /// </summary>
        public OptionStyle Option
        {
            get => _option;
            set => _option = value ?? new OptionStyle();
        }

        /// <summary>
        /// Gets or sets description styles.
        /// </summary>
        public DescriptionStyle Description
        {
            get => _description;
            set => _description = value ?? new DescriptionStyle();
        }

        /// <summary>
        /// Gets or sets example styles.
        /// </summary>
        public ExampleStyle Example
        {
            get => _example;
            set => _example = value ?? new ExampleStyle();
        }
    }

    /// <summary>
    /// Configures markup styles for one help section.
    /// </summary>
    public class HelpTextStyle
    {
        /// <summary>
        /// Gets or sets the markup style used for the section heading.
        /// </summary>
        public string Heading { get; set; } = "bold";

        /// <summary>
        /// Gets or sets the markup style used for names, templates, or primary values.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the markup style used for descriptive text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the markup style used for secondary labels and notes.
        /// </summary>
        public string Note { get; set; } = "dim";
    }

    /// <summary>
    /// Configures styles for positional arguments in help output.
    /// </summary>
    public sealed class ArgumentStyle : HelpTextStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentStyle"/> class.
        /// </summary>
        public ArgumentStyle()
        {
            Name = "cyan";
        }

        /// <summary>
        /// Gets or sets the markup style used for argument templates.
        /// </summary>
        public string Template
        {
            get => Name;
            set => Name = value;
        }
    }

    /// <summary>
    /// Configures styles for commands and branches in help output.
    /// </summary>
    public sealed class CommandStyle : HelpTextStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandStyle"/> class.
        /// </summary>
        public CommandStyle()
        {
            Name = "green";
        }

        /// <summary>
        /// Gets or sets the markup style used for command aliases.
        /// </summary>
        public string Alias { get; set; } = "dim";
    }

    /// <summary>
    /// Configures styles for descriptions in help output.
    /// </summary>
    public sealed class DescriptionStyle : HelpTextStyle
    {
    }

    /// <summary>
    /// Configures styles for examples in help output.
    /// </summary>
    public sealed class ExampleStyle : HelpTextStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleStyle"/> class.
        /// </summary>
        public ExampleStyle()
        {
            Name = "dim";
        }

        /// <summary>
        /// Gets or sets the markup style used for example command lines.
        /// </summary>
        public string Command
        {
            get => Name;
            set => Name = value;
        }
    }

    /// <summary>
    /// Configures styles for options in help output.
    /// </summary>
    public sealed class OptionStyle : HelpTextStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionStyle"/> class.
        /// </summary>
        public OptionStyle()
        {
            Name = "cyan";
        }
    }

    /// <summary>
    /// Configures styles for usage lines in help output.
    /// </summary>
    public sealed class UsageStyle : HelpTextStyle
    {
        /// <summary>
        /// Gets or sets the markup style used for usage syntax.
        /// </summary>
        public string Syntax
        {
            get => Name;
            set => Name = value;
        }
    }
}
