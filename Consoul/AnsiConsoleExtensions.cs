using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides additive Spectre-style helpers for <see cref="IAnsiConsole"/> without expanding the interface contract.
    /// </summary>
    public static class AnsiConsoleExtensions
    {
        /// <summary>
        /// Clears the console and accepts Spectre-style cursor-home intent.
        /// </summary>
        /// <param name="console">The console facade to clear.</param>
        /// <param name="home">Whether the cursor should be moved to the home position after clearing.</param>
        public static void Clear(this IAnsiConsole console, bool home)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console is ConsoulAnsiConsole consoulConsole)
            {
                consoulConsole.Clear(home);
                return;
            }

            console.Clear();
        }

        /// <summary>
        /// Enters the terminal alternate screen when the active console supports ANSI sequences.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <returns>A disposable scope that restores the main screen on disposal.</returns>
        public static AlternateScreenScope AlternateScreen(this IAnsiConsole console)
        {
            IAnsiConsole target = EnsureConsole(console);
            return new AlternateScreenScope(target, target.Profile.Capabilities.Ansi);
        }

        /// <summary>
        /// Renders a horizontal rule using Spectre-style alignment vocabulary without expanding the console facade contract.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="title">Optional rule title.</param>
        /// <param name="alignment">Title alignment when a title is supplied.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for the rule.</param>
        public static void Rule(this IAnsiConsole console, string title, Justify alignment, int? width = null, ConsoleColor? color = null)
        {
            EnsureConsole(console).Rule(title, CreateRuleOptions(alignment, width, color));
        }

        /// <summary>
        /// Renders a horizontal rule using a foreground color and Spectre-style alignment vocabulary without expanding the console facade contract.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="title">Optional rule title.</param>
        /// <param name="color">Foreground color for the rule.</param>
        /// <param name="alignment">Title alignment when a title is supplied.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        public static void Rule(this IAnsiConsole console, string title, ConsoleColor color, Justify alignment, int? width = null)
        {
            Rule(console, title, alignment, width, color);
        }

        /// <summary>
        /// Renders content inside a responsive bordered panel using the quick Consoul panel options.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="content">The panel content to render.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        public static void Panel(this IAnsiConsole console, string content, string title = null, int? width = null, ConsoleColor? color = null, ConsoleColor? borderColor = null)
        {
            EnsureConsole(console).Panel(content, CreatePanelOptions(title, width, color, borderColor));
        }

        /// <summary>
        /// Captures and renders an action inside a responsive bordered panel using the quick Consoul panel options.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="render">The render action used to produce panel content.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for captured content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Panel(
            this IAnsiConsole console,
            Action render,
            string title = null,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? borderColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            EnsureConsole(console).Panel(render, CreatePanelOptions(title, width, color, borderColor), captureWidth, captureHeight);
        }

        /// <summary>
        /// Captures and renders a renderable inside a responsive bordered panel using the quick Consoul panel options.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="renderable">The renderable component used to produce panel content.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for captured content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Panel(
            this IAnsiConsole console,
            IConsoulRenderable renderable,
            string title = null,
            int? width = null,
            ConsoleColor? color = null,
            ConsoleColor? borderColor = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            EnsureConsole(console).Panel(renderable, CreatePanelOptions(title, width, color, borderColor), captureWidth, captureHeight);
        }

        /// <summary>
        /// Renders a file path prompt through the console facade and returns the explicit prompt result.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="mustExist">Whether the entered path must reference an existing file.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The file path prompt result.</returns>
        public static TextPromptResult<string> FilePath(this IAnsiConsole console, string message, bool clear = false, bool mustExist = false, CancellationToken cancellationToken = default)
        {
            return EnsureConsole(console).Prompt(Consoul.FilePath(message, clear, mustExist), cancellationToken);
        }

        /// <summary>
        /// Renders a directory path prompt through the console facade and returns the explicit prompt result.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="mustExist">Whether the entered path must reference an existing directory.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The directory path prompt result.</returns>
        public static TextPromptResult<string> DirectoryPath(this IAnsiConsole console, string message, bool clear = false, bool mustExist = false, CancellationToken cancellationToken = default)
        {
            return EnsureConsole(console).Prompt(Consoul.DirectoryPath(message, clear, mustExist), cancellationToken);
        }

        /// <summary>
        /// Prompts with a simple list of choices and returns the full prompt result.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(this IAnsiConsole console, string message, bool clear = false, params string[] options)
        {
            return Select(console, message, clear, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts with a simple list of choices and returns the full prompt result.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(this IAnsiConsole console, string message, bool clear = false, CancellationToken cancellationToken = default, params string[] options)
        {
            return EnsureConsole(console).Prompt(new SelectionPrompt(message, clear, options ?? Array.Empty<string>()), cancellationToken);
        }

        /// <summary>
        /// Prompts with a complex list of choices and returns the full prompt result.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Complex options to present.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(this IAnsiConsole console, string message, SelectOption[] options, bool clear = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return EnsureConsole(console).Prompt(new SelectionPrompt(message, clear, options), cancellationToken);
        }

        /// <summary>
        /// Prompts with a list of choices, marks a default option, and returns the full prompt result.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Simple list of options.</param>
        /// <param name="defaultOption">Optional option label selected when the user presses Enter.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The prompt result describing the selected option or cancellation state.</returns>
        public static PromptResult Select(this IAnsiConsole console, string message, IEnumerable<string> options, string defaultOption = null, bool clear = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var prompt = new SelectionPrompt(message, clear);
            foreach (string option in options)
            {
                bool isDefault = defaultOption != null && string.Equals(option, defaultOption, StringComparison.Ordinal);
                prompt.Add(option, isDefault: isDefault);
            }

            return EnsureConsole(console).Prompt(prompt, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(this IAnsiConsole console, string message, Func<T, string> labelSelector, bool clear = false, params T[] options)
        {
            return Select(console, message, labelSelector, clear, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts with typed choices and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(this IAnsiConsole console, string message, Func<T, string> labelSelector, bool clear = false, CancellationToken cancellationToken = default, params T[] options)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return Select(console, message, options ?? Array.Empty<T>(), labelSelector, clear, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(this IAnsiConsole console, string message, IEnumerable<T> options, Func<T, string> labelSelector, bool clear = false, CancellationToken cancellationToken = default)
        {
            return Select(console, message, options, labelSelector, (Func<T, bool>)null, clear, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices, marks a default item, and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="defaultItem">Optional item selected when the user presses Enter.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(this IAnsiConsole console, string message, IEnumerable<T> options, Func<T, string> labelSelector, T defaultItem, bool clear = false, CancellationToken cancellationToken = default)
        {
            return Select(console, message, options, labelSelector, item => EqualityComparer<T>.Default.Equals(item, defaultItem), clear, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices, marks items matching a default predicate, and returns both the prompt result and selected item.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="isDefault">Optional predicate used to mark a default option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed selection result.</returns>
        public static SelectionResult<T> Select<T>(this IAnsiConsole console, string message, IEnumerable<T> options, Func<T, string> labelSelector, Func<T, bool> isDefault, bool clear = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            var prompt = new SelectionPrompt<T>(message, clear, labelSelector);
            foreach (T option in options)
            {
                bool defaultOption = isDefault != null && isDefault(option);
                prompt.Add(option, isDefault: defaultOption);
            }

            return EnsureConsole(console).Prompt(prompt, cancellationToken);
        }

        /// <summary>
        /// Prompts with a simple list of choices and returns all selected indexes.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(this IAnsiConsole console, string message, bool clear = false, params string[] options)
        {
            return MultiSelect(console, message, clear, false, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts with a simple list of choices and returns all selected indexes.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(this IAnsiConsole console, string message, bool clear = false, bool allowEmpty = false, params string[] options)
        {
            return MultiSelect(console, message, clear, allowEmpty, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts with a simple list of choices and returns all selected indexes.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Simple list of options.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(this IAnsiConsole console, string message, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default, params string[] options)
        {
            return EnsureConsole(console).Prompt(new MultiSelectionPrompt(message, clear, allowEmpty, options ?? Array.Empty<string>()), cancellationToken);
        }

        /// <summary>
        /// Prompts with a list of choices, marks default options, and returns all selected indexes.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Simple list of options.</param>
        /// <param name="defaultOptions">Optional option labels selected when the user presses Enter.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The multi-selection result describing selected indexes or cancellation state.</returns>
        public static MultiSelectionResult MultiSelect(this IAnsiConsole console, string message, IEnumerable<string> options, IEnumerable<string> defaultOptions = null, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var defaults = new HashSet<string>(defaultOptions ?? Array.Empty<string>(), StringComparer.Ordinal);
            var prompt = new MultiSelectionPrompt(message, clear, allowEmpty);
            foreach (string option in options)
            {
                prompt.Add(option, isDefault: defaults.Contains(option));
            }

            return EnsureConsole(console).Prompt(prompt, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(this IAnsiConsole console, string message, Func<T, string> labelSelector, bool clear = false, params T[] options)
        {
            return MultiSelect(console, message, labelSelector, clear, false, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(this IAnsiConsole console, string message, Func<T, string> labelSelector, bool clear = false, bool allowEmpty = false, params T[] options)
        {
            return MultiSelect(console, message, labelSelector, clear, allowEmpty, CancellationToken.None, options);
        }

        /// <summary>
        /// Prompts with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="options">Typed options to present.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(this IAnsiConsole console, string message, Func<T, string> labelSelector, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default, params T[] options)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            return MultiSelect(console, message, options ?? Array.Empty<T>(), labelSelector, (Func<T, bool>)null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(this IAnsiConsole console, string message, IEnumerable<T> options, Func<T, string> labelSelector, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            return MultiSelect(console, message, options, labelSelector, (Func<T, bool>)null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices, marks default items, and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="defaultItems">Optional items selected when the user presses Enter.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(this IAnsiConsole console, string message, IEnumerable<T> options, Func<T, string> labelSelector, IEnumerable<T> defaultItems, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            var defaults = new HashSet<T>(defaultItems ?? Array.Empty<T>());
            return MultiSelect(console, message, options, labelSelector, item => defaults.Contains(item), clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts with typed choices, marks items matching a default predicate, and returns both selected indexes and selected items.
        /// </summary>
        /// <typeparam name="T">The type of object presented as an option.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="options">Typed options to present.</param>
        /// <param name="labelSelector">A function that creates a display label for each option.</param>
        /// <param name="isDefault">Optional predicate used to mark default options.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed multi-selection result.</returns>
        public static MultiSelectionResult<T> MultiSelect<T>(this IAnsiConsole console, string message, IEnumerable<T> options, Func<T, string> labelSelector, Func<T, bool> isDefault, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            var prompt = new MultiSelectionPrompt<T>(message, clear, allowEmpty, labelSelector);
            foreach (T option in options)
            {
                bool defaultOption = isDefault != null && isDefault(option);
                prompt.Add(option, isDefault: defaultOption);
            }

            return EnsureConsole(console).Prompt(prompt, cancellationToken);
        }

        /// <summary>
        /// Prompts with enum values and returns both the prompt result and selected enum value.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="defaultValue">Optional default value selected when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum selection result.</returns>
        public static SelectionResult<TEnum> SelectEnum<TEnum>(this IAnsiConsole console, string message, bool clear = false, TEnum? defaultValue = null, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return SelectEnum(console, message, EnumValues<TEnum>(), null, clear, defaultValue, cancellationToken);
        }

        /// <summary>
        /// Prompts with enum values and custom labels.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each enum value.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="defaultValue">Optional default value selected when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum selection result.</returns>
        public static SelectionResult<TEnum> SelectEnum<TEnum>(this IAnsiConsole console, string message, Func<TEnum, string> labelSelector, bool clear = false, TEnum? defaultValue = null, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return SelectEnum(console, message, EnumValues<TEnum>(), labelSelector, clear, defaultValue, cancellationToken);
        }

        /// <summary>
        /// Prompts with a filtered set of enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="values">Enum values to present.</param>
        /// <param name="labelSelector">Optional function that creates a display label for each enum value.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="defaultValue">Optional default value selected when the user presses Enter.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum selection result.</returns>
        public static SelectionResult<TEnum> SelectEnum<TEnum>(this IAnsiConsole console, string message, IEnumerable<TEnum> values, Func<TEnum, string> labelSelector = null, bool clear = false, TEnum? defaultValue = null, CancellationToken cancellationToken = default) where TEnum : struct
        {
            EnsureEnumType<TEnum>();
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return Select(
                console,
                message,
                values,
                labelSelector ?? FormatEnumValue,
                value => defaultValue.HasValue && EqualityComparer<TEnum>.Default.Equals(value, defaultValue.Value),
                clear,
                cancellationToken);
        }

        /// <summary>
        /// Prompts with enum values and returns selected enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum multi-selection result.</returns>
        public static MultiSelectionResult<TEnum> MultiSelectEnum<TEnum>(this IAnsiConsole console, string message, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return MultiSelectEnum(console, message, EnumValues<TEnum>(), null, null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts with enum values and custom labels.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="labelSelector">A function that creates a display label for each enum value.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum multi-selection result.</returns>
        public static MultiSelectionResult<TEnum> MultiSelectEnum<TEnum>(this IAnsiConsole console, string message, Func<TEnum, string> labelSelector, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default) where TEnum : struct
        {
            return MultiSelectEnum(console, message, EnumValues<TEnum>(), labelSelector, null, clear, allowEmpty, cancellationToken);
        }

        /// <summary>
        /// Prompts with a filtered set of enum values and returns selected enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type presented as options.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">Prompt message to display.</param>
        /// <param name="values">Enum values to present.</param>
        /// <param name="labelSelector">Optional function that creates a display label for each enum value.</param>
        /// <param name="defaultValues">Optional enum values selected when the user presses Enter.</param>
        /// <param name="clear">Whether to clear the console before rendering the prompt.</param>
        /// <param name="allowEmpty">Whether no selected options is a valid result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed enum multi-selection result.</returns>
        public static MultiSelectionResult<TEnum> MultiSelectEnum<TEnum>(this IAnsiConsole console, string message, IEnumerable<TEnum> values, Func<TEnum, string> labelSelector = null, IEnumerable<TEnum> defaultValues = null, bool clear = false, bool allowEmpty = false, CancellationToken cancellationToken = default) where TEnum : struct
        {
            EnsureEnumType<TEnum>();
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var defaults = new HashSet<TEnum>(defaultValues ?? Array.Empty<TEnum>());
            return MultiSelect(
                console,
                message,
                values,
                labelSelector ?? FormatEnumValue,
                value => defaults.Contains(value),
                clear,
                allowEmpty,
                cancellationToken);
        }

        /// <summary>
        /// Runs an action inside the terminal alternate screen when ANSI sequences are supported.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The action to run inside the alternate screen.</param>
        public static void AlternateScreen(this IAnsiConsole console, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (AlternateScreen(console))
            {
                action();
            }
        }

        /// <summary>
        /// Runs a function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The function to run inside the alternate screen.</param>
        /// <returns>The function result.</returns>
        public static T AlternateScreen<T>(this IAnsiConsole console, Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (AlternateScreen(console))
            {
                return func();
            }
        }

        /// <summary>
        /// Runs an asynchronous action inside the terminal alternate screen without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The asynchronous action to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the action completes.</returns>
        public static Task AlternateScreenAsync(this IAnsiConsole console, Func<Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return AlternateScreenAsync(console, token => action(), cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside the terminal alternate screen.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The asynchronous action to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the action completes.</returns>
        public static async Task AlternateScreenAsync(this IAnsiConsole console, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (AlternateScreen(console))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The asynchronous function to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the function result.</returns>
        public static Task<T> AlternateScreenAsync<T>(this IAnsiConsole console, Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return AlternateScreenAsync<T>(console, token => func(), cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous function inside the terminal alternate screen and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The asynchronous function to run inside the alternate screen.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the function result.</returns>
        public static async Task<T> AlternateScreenAsync<T>(this IAnsiConsole console, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (AlternateScreen(console))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await func(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, int value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, int value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, uint value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, uint value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, long value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, long value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, ulong value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, ulong value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, float value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, float value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, double value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, double value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a decimal value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, decimal value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, decimal value)
        {
            EnsureConsole(console).Write(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a Boolean value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, bool value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, bool value)
        {
            EnsureConsole(console).Write(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes a Unicode character without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, char value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes a Unicode character without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, char value)
        {
            EnsureConsole(console).Write(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes an array of Unicode characters without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The characters to write.</param>
        public static void Write(this IAnsiConsole console, char[] value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes an array of Unicode characters without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The characters to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            EnsureConsole(console).Write(new string(value));
        }

        /// <summary>
        /// Writes the text representation of an object without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The object to write.</param>
        public static void Write(this IAnsiConsole console, object value)
        {
            console.Write(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of an object without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The object to write.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, object value)
        {
            IAnsiConsole target = EnsureConsole(console);
            if (value is IConsoulRenderable renderable)
            {
                target.Write(renderable);
                return;
            }

            if (value is IRenderable spectreRenderable)
            {
                target.Write(Consoul.Renderable(spectreRenderable));
                return;
            }

            target.Write(Convert.ToString(value, provider) ?? string.Empty);
        }

        /// <summary>
        /// Renders a Spectre-style renderable component without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Write(this IAnsiConsole console, IRenderable renderable)
        {
            EnsureConsole(console).Write(Consoul.Renderable(renderable));
        }

        /// <summary>
        /// Writes a formatted string without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void Write(this IAnsiConsole console, string format, params object[] args)
        {
            console.Write(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes a formatted string without appending a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void Write(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).Write(string.Format(provider, format ?? string.Empty, args ?? Array.Empty<object>()));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, int value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, int value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, uint value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 32-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, uint value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, long value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit signed integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, long value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, ulong value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a 64-bit unsigned integer followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, ulong value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, float value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a single-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, float value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, double value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a double-precision floating-point value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, double value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, decimal value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, decimal value)
        {
            EnsureConsole(console).WriteLine(value.ToString(provider));
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, bool value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, bool value)
        {
            EnsureConsole(console).WriteLine(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes a Unicode character followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, char value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes a Unicode character followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, char value)
        {
            EnsureConsole(console).WriteLine(Convert.ToString(value, provider));
        }

        /// <summary>
        /// Writes an array of Unicode characters followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The characters to write.</param>
        public static void WriteLine(this IAnsiConsole console, char[] value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes an array of Unicode characters followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The characters to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, char[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            EnsureConsole(console).WriteLine(new string(value));
        }

        /// <summary>
        /// Writes the text representation of an object followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="value">The object to write.</param>
        public static void WriteLine(this IAnsiConsole console, object value)
        {
            console.WriteLine(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Writes the text representation of an object followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="value">The object to write.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, object value)
        {
            IAnsiConsole target = EnsureConsole(console);
            if (value is IConsoulRenderable renderable)
            {
                target.Write(renderable);
                target.WriteLine();
                return;
            }

            if (value is IRenderable spectreRenderable)
            {
                target.Write(Consoul.Renderable(spectreRenderable));
                target.WriteLine();
                return;
            }

            target.WriteLine(Convert.ToString(value, provider) ?? string.Empty);
        }

        /// <summary>
        /// Renders a Consoul renderable component followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void WriteLine(this IAnsiConsole console, IConsoulRenderable renderable)
        {
            IAnsiConsole target = EnsureConsole(console);
            target.Write(renderable);
            target.WriteLine();
        }

        /// <summary>
        /// Renders a Spectre-style renderable component followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void WriteLine(this IAnsiConsole console, IRenderable renderable)
        {
            IAnsiConsole target = EnsureConsole(console);
            target.Write(Consoul.Renderable(renderable));
            target.WriteLine();
        }

        /// <summary>
        /// Writes a formatted string followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void WriteLine(this IAnsiConsole console, string format, params object[] args)
        {
            console.WriteLine(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes a formatted string followed by a line break.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Objects to format.</param>
        public static void WriteLine(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).WriteLine(string.Format(provider, format ?? string.Empty, args ?? Array.Empty<object>()));
        }

        /// <summary>
        /// Writes formatted lightweight markup without appending a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(this IAnsiConsole console, string format, params object[] args)
        {
            console.Markup(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes formatted lightweight markup without appending a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void Markup(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).Markup(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Writes formatted lightweight markup followed by a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(this IAnsiConsole console, string format, params object[] args)
        {
            console.MarkupLine(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Writes formatted lightweight markup followed by a line break, escaping formatted values before markup parsing.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite markup format string.</param>
        /// <param name="args">Objects to format and escape into the markup template.</param>
        public static void MarkupLine(this IAnsiConsole console, IFormatProvider provider, string format, params object[] args)
        {
            EnsureConsole(console).MarkupLine(MarkupEntry.FormatMarkup(provider, format, args));
        }

        /// <summary>
        /// Renders a Consoul renderable component using the legacy Spectre-style method name.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(this IAnsiConsole console, IConsoulRenderable renderable)
        {
            EnsureConsole(console).Write(renderable);
        }

        /// <summary>
        /// Renders a Spectre-style renderable component using the legacy Spectre-style method name.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="renderable">The renderable component to render.</param>
        public static void Render(this IAnsiConsole console, IRenderable renderable)
        {
            EnsureConsole(console).Write(Consoul.Renderable(renderable));
        }

        /// <summary>
        /// Captures a Spectre-style renderable component through an injectable console facade.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="renderable">The renderable component to capture.</param>
        /// <param name="width">The virtual capture width.</param>
        /// <param name="height">The virtual capture height.</param>
        /// <returns>The captured plain text output.</returns>
        public static string Capture(this IAnsiConsole console, IRenderable renderable, int width = 80, int height = 200)
        {
            return EnsureConsole(console).Capture(Consoul.Renderable(renderable), width, height);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial status message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task StatusAsync(this IAnsiConsole console, string message, Func<StatusScope, Task> action, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).StatusAsync(message, (status, token) => action(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> StatusAsync<T>(this IAnsiConsole console, string message, Func<StatusScope, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StatusAsync<T>(console, message, (status, token) => func(status), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place status scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial status message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional status rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> StatusAsync<T>(this IAnsiConsole console, string message, Func<StatusScope, CancellationToken, Task<T>> func, StatusOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).StatusAsync(message, async (status, token) =>
            {
                result = await func(status, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="action">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task SpinnerAsync(this IAnsiConsole console, string message, Func<SpinnerScope, Task> action, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).SpinnerAsync(message, (spinner, token) => action(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> SpinnerAsync<T>(this IAnsiConsole console, string message, Func<SpinnerScope, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return SpinnerAsync<T>(console, message, (spinner, token) => func(spinner), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside an in-place spinner scope and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="func">The scoped asynchronous action.</param>
        /// <param name="options">Optional spinner rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> SpinnerAsync<T>(this IAnsiConsole console, string message, Func<SpinnerScope, CancellationToken, Task<T>> func, SpinnerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).SpinnerAsync(message, async (spinner, token) =>
            {
                result = await func(spinner, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task ProgressAsync(this IAnsiConsole console, Func<ProgressContext, Task> action, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).ProgressAsync((progress, token) => action(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> ProgressAsync<T>(this IAnsiConsole console, Func<ProgressContext, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return ProgressAsync<T>(console, (progress, token) => func(progress), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a multi-task progress context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous progress action.</param>
        /// <param name="options">Optional progress rendering options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> ProgressAsync<T>(this IAnsiConsole console, Func<ProgressContext, CancellationToken, Task<T>> func, ProgressContextOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).ProgressAsync(async (progress, token) =>
            {
                result = await func(progress, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(this IAnsiConsole console, Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).LiveAsync((live, token) => action(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(this IAnsiConsole console, Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>(console, (live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> LiveAsync<T>(this IAnsiConsole console, Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).LiveAsync(async (live, token) =>
            {
                result = await func(live, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target without requiring a cancellation-token parameter.
        /// </summary>
        /// <param name="console">The console facade to use.</param>
        /// <param name="target">The initial render target.</param>
        /// <param name="action">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the scoped action completes.</returns>
        public static Task LiveAsync(this IAnsiConsole console, IConsoulRenderable target, Func<LiveDisplayContext, Task> action, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return EnsureConsole(console).LiveAsync(target, (live, token) => action(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target and returns its result without requiring a cancellation-token parameter.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static Task<T> LiveAsync<T>(this IAnsiConsole console, IConsoulRenderable target, Func<LiveDisplayContext, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return LiveAsync<T>(console, target, (live, token) => func(live), options, cancellationToken);
        }

        /// <summary>
        /// Runs an asynchronous action inside a live display context with an initial render target and returns its result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the scoped action.</typeparam>
        /// <param name="console">The console facade to use.</param>
        /// <param name="target">The initial render target.</param>
        /// <param name="func">The scoped asynchronous live display action.</param>
        /// <param name="options">Optional live display options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the scoped action result.</returns>
        public static async Task<T> LiveAsync<T>(this IAnsiConsole console, IConsoulRenderable target, Func<LiveDisplayContext, CancellationToken, Task<T>> func, LiveDisplayOptions options = null, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            await EnsureConsole(console).LiveAsync(target, async (live, token) =>
            {
                result = await func(live, token).ConfigureAwait(false);
            }, options, cancellationToken).ConfigureAwait(false);

            return result;
        }

        private static IAnsiConsole EnsureConsole(IAnsiConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return console;
        }

        private static IEnumerable<TEnum> EnumValues<TEnum>() where TEnum : struct
        {
            EnsureEnumType<TEnum>();
            foreach (object value in Enum.GetValues(typeof(TEnum)))
            {
                yield return (TEnum)value;
            }
        }

        private static void EnsureEnumType<TEnum>() where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum type.");
            }
        }

        private static string FormatEnumValue<TEnum>(TEnum value) where TEnum : struct
        {
            string name = Enum.GetName(typeof(TEnum), value);
            if (string.IsNullOrEmpty(name))
            {
                return value.ToString();
            }

            FieldInfo field = typeof(TEnum).GetField(name);
            string label = ResolveEnumDisplayName(field);
            return !string.IsNullOrWhiteSpace(label) ? label : SplitPascalCase(name);
        }

        private static string ResolveEnumDisplayName(FieldInfo field)
        {
            if (field == null)
            {
                return null;
            }

            object displayAttribute = null;
            foreach (object attribute in field.GetCustomAttributes(true))
            {
                if (string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DataAnnotations.DisplayAttribute",
                    StringComparison.Ordinal))
                {
                    displayAttribute = attribute;
                    break;
                }
            }

            if (displayAttribute != null)
            {
                string displayName = InvokeStringMethod(displayAttribute, "GetName");
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    return displayName;
                }
            }

            var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>(true);
            if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>(true);
            if (descriptionAttribute != null && !string.IsNullOrWhiteSpace(descriptionAttribute.Description))
            {
                return descriptionAttribute.Description;
            }

            return null;
        }

        private static string InvokeStringMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            return method?.Invoke(target, null) as string;
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");
        }

        private static PanelOptions CreatePanelOptions(string title, int? width, ConsoleColor? color, ConsoleColor? borderColor)
        {
            var options = new PanelOptions
            {
                Title = title,
                Width = width
            };

            if (color.HasValue)
            {
                options.ContentScheme = CreateColorScheme(color.Value);
            }

            if (borderColor.HasValue)
            {
                options.BorderScheme = CreateColorScheme(borderColor.Value);
            }

            return options;
        }

        private static RuleOptions CreateRuleOptions(Justify alignment, int? width, ConsoleColor? color)
        {
            var options = new RuleOptions
            {
                Width = width,
                Alignment = JustifyMap.ToRuleTitleAlignment(alignment)
            };

            if (color.HasValue)
            {
                options.RuleScheme = CreateColorScheme(color.Value);
            }

            return options;
        }

        private static ColorScheme CreateColorScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
