using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains multiple in-place progress task rows for a workflow.
    /// </summary>
    public class ProgressContext : IDisposable
    {
        private readonly List<ProgressTask> _tasks = new List<ProgressTask>();
        private readonly ProgressContextOptions _options;
        private readonly IConsoleDriver _driver;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressContext"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for progress rows.</param>
        public ProgressContext(ProgressContextOptions options = null)
        {
            _driver = Consoul.ConsoleDriver;
            _options = options != null ? options.Clone() : new ProgressContextOptions();
        }

        /// <summary>
        /// Gets the progress tasks owned by the context.
        /// </summary>
        public IReadOnlyList<ProgressTask> Tasks => _tasks;

        /// <summary>
        /// Gets the options used to render progress rows.
        /// </summary>
        public ProgressContextOptions Options => _options;

        /// <summary>
        /// Gets the number of tasks that completed successfully.
        /// </summary>
        public int CompletedTaskCount => _tasks.Count(task => task.IsCompleted);

        /// <summary>
        /// Gets the number of tasks that have failed.
        /// </summary>
        public int FailedTaskCount => _tasks.Count(task => task.IsFailed);

        /// <summary>
        /// Gets a value indicating whether any task in the context has failed.
        /// </summary>
        public bool HasFailures => FailedTaskCount > 0;

        /// <summary>
        /// Gets a value indicating whether every task in the context has reached a finished state.
        /// </summary>
        public bool IsFinished => _tasks.Count > 0 && _tasks.All(task => task.IsFinished || ShouldHide(task));

        /// <summary>
        /// Gets a value indicating whether every task in the context completed successfully.
        /// </summary>
        public bool IsCompleted => _tasks.Count > 0 && _tasks.All(task => task.IsCompleted || ShouldHide(task));

        /// <summary>
        /// Adds a progress task row.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="maxValue">The value that represents completion.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTask(string description, double maxValue = 100.0)
        {
            return AddTaskAtInternal(
                description,
                new ProgressTaskSettings { AutoStart = true, MaxValue = maxValue },
                _tasks.Count);
        }

        /// <summary>
        /// Adds a progress task row using Spectre-style auto-start and max-value parameters.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="autoStart"><c>true</c> to start the task immediately; otherwise, <c>false</c>.</param>
        /// <param name="maxValue">The value that represents completion.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTask(string description, bool autoStart, double maxValue = 100.0)
        {
            return AddTaskAtInternal(
                description,
                new ProgressTaskSettings { AutoStart = autoStart, MaxValue = maxValue },
                _tasks.Count);
        }

        /// <summary>
        /// Adds a progress task row using Spectre-style task settings.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="settings">The task creation settings.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTask(string description, ProgressTaskSettings settings)
        {
            return AddTaskAtInternal(description, settings ?? new ProgressTaskSettings(), _tasks.Count);
        }

        /// <summary>
        /// Adds progress task rows for the supplied descriptions.
        /// </summary>
        /// <param name="descriptions">The task descriptions to add.</param>
        /// <returns>The added progress tasks in creation order.</returns>
        public IReadOnlyList<ProgressTask> AddTasks(params string[] descriptions)
        {
            return AddTasks((IEnumerable<string>)descriptions);
        }

        /// <summary>
        /// Adds progress task rows for the supplied descriptions.
        /// </summary>
        /// <param name="descriptions">The task descriptions to add.</param>
        /// <param name="autoStart"><c>true</c> to start each task immediately; otherwise, <c>false</c>.</param>
        /// <param name="maxValue">The value that represents completion for each task.</param>
        /// <returns>The added progress tasks in creation order.</returns>
        public IReadOnlyList<ProgressTask> AddTasks(IEnumerable<string> descriptions, bool autoStart = true, double maxValue = 100.0)
        {
            ThrowIfDisposed();
            if (descriptions == null)
            {
                throw new ArgumentNullException(nameof(descriptions));
            }

            var tasks = new List<ProgressTask>();
            foreach (string description in descriptions)
            {
                tasks.Add(AddTask(description, autoStart, maxValue));
            }

            return tasks;
        }

        /// <summary>
        /// Adds progress task rows from prepared task settings.
        /// </summary>
        /// <param name="settings">The task settings to add.</param>
        /// <returns>The added progress tasks in creation order.</returns>
        public IReadOnlyList<ProgressTask> AddTasks(IEnumerable<ProgressTaskSettings> settings)
        {
            ThrowIfDisposed();
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            ProgressTaskSettings[] normalized = settings.ToArray();
            if (normalized.Any(taskSettings => taskSettings == null))
            {
                throw new ArgumentException("Task settings collections cannot contain null items.", nameof(settings));
            }

            var tasks = new List<ProgressTask>();
            foreach (ProgressTaskSettings taskSettings in normalized)
            {
                tasks.Add(AddTaskAtInternal(taskSettings.Description, taskSettings, _tasks.Count));
            }

            return tasks;
        }

        /// <summary>
        /// Adds a progress task row at the specified index.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="index">The index at which the task should be inserted.</param>
        /// <param name="autoStart"><c>true</c> to start the task immediately; otherwise, <c>false</c>.</param>
        /// <param name="maxValue">The value that represents completion.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTaskAt(string description, int index, bool autoStart = true, double maxValue = 100.0)
        {
            return AddTaskAtInternal(
                description,
                new ProgressTaskSettings { AutoStart = autoStart, MaxValue = maxValue },
                index);
        }

        /// <summary>
        /// Adds a progress task row at the specified index using task settings.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="settings">The task creation settings.</param>
        /// <param name="index">The index at which the task should be inserted.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTaskAt(string description, ProgressTaskSettings settings, int index)
        {
            return AddTaskAtInternal(description, settings ?? new ProgressTaskSettings(), index);
        }

        /// <summary>
        /// Adds a progress task row before an existing reference task.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="referenceProgressTask">The task before which the new task should be inserted.</param>
        /// <param name="autoStart"><c>true</c> to start the task immediately; otherwise, <c>false</c>.</param>
        /// <param name="maxValue">The value that represents completion.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTaskBefore(string description, ProgressTask referenceProgressTask, bool autoStart = true, double maxValue = 100.0)
        {
            return AddTaskAt(
                description,
                GetReferenceTaskIndex(referenceProgressTask),
                autoStart,
                maxValue);
        }

        /// <summary>
        /// Adds a progress task row before an existing reference task using task settings.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="settings">The task creation settings.</param>
        /// <param name="referenceProgressTask">The task before which the new task should be inserted.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTaskBefore(string description, ProgressTaskSettings settings, ProgressTask referenceProgressTask)
        {
            return AddTaskAt(description, settings, GetReferenceTaskIndex(referenceProgressTask));
        }

        /// <summary>
        /// Adds a progress task row after an existing reference task.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="referenceProgressTask">The task after which the new task should be inserted.</param>
        /// <param name="autoStart"><c>true</c> to start the task immediately; otherwise, <c>false</c>.</param>
        /// <param name="maxValue">The value that represents completion.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTaskAfter(string description, ProgressTask referenceProgressTask, bool autoStart = true, double maxValue = 100.0)
        {
            return AddTaskAt(
                description,
                GetReferenceTaskIndex(referenceProgressTask) + 1,
                autoStart,
                maxValue);
        }

        /// <summary>
        /// Adds a progress task row after an existing reference task using task settings.
        /// </summary>
        /// <param name="description">The task description.</param>
        /// <param name="settings">The task creation settings.</param>
        /// <param name="referenceProgressTask">The task after which the new task should be inserted.</param>
        /// <returns>The added progress task.</returns>
        public ProgressTask AddTaskAfter(string description, ProgressTaskSettings settings, ProgressTask referenceProgressTask)
        {
            return AddTaskAt(description, settings, GetReferenceTaskIndex(referenceProgressTask) + 1);
        }

        /// <summary>
        /// Removes a progress task row from the context.
        /// </summary>
        /// <param name="task">The task to remove.</param>
        /// <returns><c>true</c> when the task was removed; otherwise, <c>false</c>.</returns>
        public bool RemoveTask(ProgressTask task)
        {
            ThrowIfDisposed();
            if (task == null)
            {
                return false;
            }

            int index = _tasks.IndexOf(task);
            if (index < 0)
            {
                return false;
            }

            task.Clear();
            _tasks.RemoveAt(index);
            task.Dispose();
            RenderTaskRows();
            return true;
        }

        private ProgressTask AddTaskAtInternal(string description, ProgressTaskSettings settings, int index)
        {
            ThrowIfDisposed();
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (index < 0 || index > _tasks.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "The task index must be within the current task collection bounds.");
            }

            var task = new ProgressTask(this, CreateTaskMessage(), description, settings.MaxValue, settings.AutoStart);
            task.Tag = settings.Tag;
            task.SetHideWhenCompletedSilently(settings.HideWhenCompleted);
            if (settings.IsIndeterminate)
            {
                task.SetIndeterminateSilently(true);
            }

            _tasks.Insert(index, task);
            RenderTaskRows();
            return task;
        }

        private FixedMessage CreateTaskMessage()
        {
            return new FixedMessage
            {
                MaxWidth = _options.Width,
                Height = 1,
                RefreshRate = Math.Max(0, _options.RefreshRate)
            };
        }

        private int GetReferenceTaskIndex(ProgressTask referenceProgressTask)
        {
            ThrowIfDisposed();
            int index = _tasks.IndexOf(referenceProgressTask);
            if (index < 0)
            {
                throw new ArgumentException("The reference task does not belong to this progress context.", nameof(referenceProgressTask));
            }

            return index;
        }

        private void RenderTaskRows()
        {
            using (UseCapturedDriver())
            {
                if (_tasks.Count == 0)
                {
                    return;
                }

                int baseLeft = ResolveTaskBaseLeft();
                int baseTop = ResolveTaskBaseTop();
                foreach (ProgressTask task in _tasks)
                {
                    if (task.HasFixedPosition)
                    {
                        task.Clear(preservePosition: true);
                    }
                }

                for (int i = 0; i < _tasks.Count; i++)
                {
                    ProgressTask task = _tasks[i];
                    task.SetFixedPosition(baseLeft, baseTop + i);
                    task.Render();
                }

                MoveCursorBelowTaskRows(baseTop);
            }
        }

        private int ResolveTaskBaseLeft()
        {
            ProgressTask positionedTask = _tasks.FirstOrDefault(task => task.HasFixedPosition);
            int left = positionedTask != null ? positionedTask.FixedLeft : Consoul.ConsoleDriver.CursorLeft;
            return ClampCursorLeft(left);
        }

        private int ResolveTaskBaseTop()
        {
            List<ProgressTask> positionedTasks = _tasks.Where(task => task.HasFixedPosition).ToList();
            int top = positionedTasks.Count > 0 ? positionedTasks.Min(task => task.FixedTop) : Consoul.ConsoleDriver.CursorTop;
            return ClampCursorTop(top);
        }

        private void MoveCursorBelowTaskRows(int baseTop)
        {
            Consoul.ConsoleDriver.SetCursorPosition(0, ClampCursorTop(baseTop + _tasks.Count));
        }

        private static int ClampCursorLeft(int left)
        {
            int width = Consoul.ConsoleBufferWidth;
            return Math.Max(0, Math.Min(left, Math.Max(0, width - 1)));
        }

        private static int ClampCursorTop(int top)
        {
            int height = Consoul.ConsoleBufferHeight;
            return Math.Max(0, Math.Min(top, Math.Max(0, height - 1)));
        }

        /// <summary>
        /// Enables immediate rendering for task updates.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext AutoRefresh()
        {
            return AutoRefresh(true);
        }

        /// <summary>
        /// Sets whether task updates render immediately or wait for an explicit refresh.
        /// </summary>
        /// <param name="enabled"><c>true</c> to render task updates immediately; otherwise, <c>false</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext AutoRefresh(bool enabled)
        {
            ThrowIfDisposed();
            _options.AutoRefresh = enabled;
            return this;
        }

        /// <summary>
        /// Disables immediate rendering so updates wait for an explicit <see cref="Refresh"/>.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext ManualRefresh()
        {
            return AutoRefresh(false);
        }

        /// <summary>
        /// Sets the minimum time between rendered progress row updates.
        /// </summary>
        /// <param name="milliseconds">The refresh rate in milliseconds. Negative values are treated as zero.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext RefreshRate(int milliseconds)
        {
            ThrowIfDisposed();
            int normalized = RefreshRateHelpers.Normalize(milliseconds);
            _options.RefreshRate = normalized;
            foreach (ProgressTask task in _tasks)
            {
                task.SetRefreshRate(normalized);
            }

            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered progress row updates.
        /// </summary>
        /// <param name="interval">The refresh interval. Negative values are treated as zero.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext RefreshRate(TimeSpan interval)
        {
            return RefreshRate(RefreshRateHelpers.Normalize(interval));
        }

        /// <summary>
        /// Enables clearing completed task rows when the context is disposed.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext AutoClear()
        {
            return AutoClear(true);
        }

        /// <summary>
        /// Sets whether completed task rows are cleared when the context is disposed.
        /// </summary>
        /// <param name="enabled"><c>true</c> to clear rows on dispose; otherwise, <c>false</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext AutoClear(bool enabled)
        {
            ThrowIfDisposed();
            _options.AutoClear = enabled;
            return this;
        }

        /// <summary>
        /// Hides completed task rows.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext HideCompleted()
        {
            return HideCompleted(true);
        }

        /// <summary>
        /// Sets whether completed task rows should be hidden.
        /// </summary>
        /// <param name="enabled"><c>true</c> to hide completed rows; otherwise, <c>false</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext HideCompleted(bool enabled)
        {
            ThrowIfDisposed();
            _options.HideCompleted = enabled;
            Refresh();
            return this;
        }

        /// <summary>
        /// Shows completed task rows.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext ShowCompleted()
        {
            return HideCompleted(false);
        }

        /// <summary>
        /// Excludes vertical padding in migration-friendly progress builder code.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext ExcludeVerticalPadding()
        {
            return ExcludeVerticalPadding(true);
        }

        /// <summary>
        /// Sets whether vertical padding should be excluded in migration-friendly progress builder code.
        /// </summary>
        /// <param name="enabled"><c>true</c> to exclude vertical padding; otherwise, <c>false</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext ExcludeVerticalPadding(bool enabled)
        {
            ThrowIfDisposed();
            _options.ExcludeVerticalPadding = enabled;
            return this;
        }

        /// <summary>
        /// Includes vertical padding in migration-friendly progress builder code.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext IncludeVerticalPadding()
        {
            return ExcludeVerticalPadding(false);
        }

        /// <summary>
        /// Sets the foreground color used for active progress rows.
        /// </summary>
        /// <param name="color">The active row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext ActiveColor(ConsoleColor color)
        {
            ThrowIfDisposed();
            _options.ActiveColor = color;
            _options.ActiveStyle = null;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active progress rows from a Spectre-style color.
        /// </summary>
        /// <param name="color">The active row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext ActiveColor(SpectreColor color)
        {
            return ActiveStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active progress rows from an RGB color.
        /// </summary>
        /// <param name="color">The active row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext ActiveColor(RgbColor color)
        {
            return ActiveStyle(new Style(color));
        }

        /// <summary>
        /// Sets the style used for active progress rows from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>#22c55e</c>, or <c>bold white on blue</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext ActiveStyle(string style)
        {
            return ActiveStyle(Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for active progress rows.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext ActiveStyle(Style style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            ThrowIfDisposed();
            _options.ActiveStyle = style;
            _options.ActiveColor = style.ToForegroundColor(_options.ActiveColor);
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for completed progress rows.
        /// </summary>
        /// <param name="color">The completed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompleteColor(ConsoleColor color)
        {
            ThrowIfDisposed();
            _options.CompleteColor = color;
            _options.CompleteStyle = null;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for completed progress rows from a Spectre-style color.
        /// </summary>
        /// <param name="color">The completed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompleteColor(SpectreColor color)
        {
            return CompleteStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for completed progress rows from an RGB color.
        /// </summary>
        /// <param name="color">The completed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompleteColor(RgbColor color)
        {
            return CompleteStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for completed progress rows.
        /// </summary>
        /// <param name="color">The completed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompletedColor(ConsoleColor color)
        {
            return CompleteColor(color);
        }

        /// <summary>
        /// Sets the foreground color used for completed progress rows from a Spectre-style color.
        /// </summary>
        /// <param name="color">The completed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompletedColor(SpectreColor color)
        {
            return CompleteColor(color);
        }

        /// <summary>
        /// Sets the foreground color used for completed progress rows from an RGB color.
        /// </summary>
        /// <param name="color">The completed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompletedColor(RgbColor color)
        {
            return CompleteColor(color);
        }

        /// <summary>
        /// Sets the style used for completed progress rows from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>#22c55e</c>, or <c>bold white on blue</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompleteStyle(string style)
        {
            return CompleteStyle(Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for completed progress rows.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompleteStyle(Style style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            ThrowIfDisposed();
            _options.CompleteStyle = style;
            _options.CompleteColor = style.ToForegroundColor(_options.CompleteColor);
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the style used for completed progress rows from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>#22c55e</c>, or <c>bold white on blue</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompletedStyle(string style)
        {
            return CompleteStyle(style);
        }

        /// <summary>
        /// Sets the style used for completed progress rows.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext CompletedStyle(Style style)
        {
            return CompleteStyle(style);
        }

        /// <summary>
        /// Sets the foreground color used for failed progress rows.
        /// </summary>
        /// <param name="color">The failed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailedColor(ConsoleColor color)
        {
            ThrowIfDisposed();
            _options.FailedColor = color;
            _options.FailedStyle = null;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for failed progress rows from a Spectre-style color.
        /// </summary>
        /// <param name="color">The failed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailedColor(SpectreColor color)
        {
            return FailedStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for failed progress rows from an RGB color.
        /// </summary>
        /// <param name="color">The failed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailedColor(RgbColor color)
        {
            return FailedStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for failed progress rows.
        /// </summary>
        /// <param name="color">The failed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailureColor(ConsoleColor color)
        {
            return FailedColor(color);
        }

        /// <summary>
        /// Sets the foreground color used for failed progress rows from a Spectre-style color.
        /// </summary>
        /// <param name="color">The failed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailureColor(SpectreColor color)
        {
            return FailedColor(color);
        }

        /// <summary>
        /// Sets the foreground color used for failed progress rows from an RGB color.
        /// </summary>
        /// <param name="color">The failed row foreground color.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailureColor(RgbColor color)
        {
            return FailedColor(color);
        }

        /// <summary>
        /// Sets the style used for failed progress rows from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>#ef4444</c>, or <c>bold white on red</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailedStyle(string style)
        {
            return FailedStyle(Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used for failed progress rows.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailedStyle(Style style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            ThrowIfDisposed();
            _options.FailedStyle = style;
            _options.FailedColor = style.ToForegroundColor(_options.FailedColor);
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the style used for failed progress rows from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>#ef4444</c>, or <c>bold white on red</c>.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailureStyle(string style)
        {
            return FailedStyle(style);
        }

        /// <summary>
        /// Sets the style used for failed progress rows.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext FailureStyle(Style style)
        {
            return FailedStyle(style);
        }

        /// <summary>
        /// Stores the requested progress columns and maps common columns to the current Consoul row renderer.
        /// </summary>
        /// <param name="columns">The requested progress columns.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext Columns(params ProgressColumn[] columns)
        {
            return SetColumns(columns);
        }

        /// <summary>
        /// Stores the requested progress columns and maps common columns to the current Consoul row renderer.
        /// </summary>
        /// <param name="columns">The requested progress columns.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext Columns(IEnumerable<ProgressColumn> columns)
        {
            return SetColumns(columns);
        }

        /// <summary>
        /// Appends a progress column without replacing columns that were already configured.
        /// </summary>
        /// <param name="column">The progress column to append. A <see langword="null"/> column is ignored.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext AddColumn(ProgressColumn column)
        {
            return AddColumns(column);
        }

        /// <summary>
        /// Appends progress columns without replacing columns that were already configured.
        /// </summary>
        /// <param name="columns">The progress columns to append. <see langword="null"/> columns are ignored.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext AddColumns(params ProgressColumn[] columns)
        {
            return AddColumns((IEnumerable<ProgressColumn>)columns);
        }

        /// <summary>
        /// Appends progress columns without replacing columns that were already configured.
        /// </summary>
        /// <param name="columns">The progress columns to append. <see langword="null"/> columns are ignored.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext AddColumns(IEnumerable<ProgressColumn> columns)
        {
            ThrowIfDisposed();
            if (columns == null)
            {
                return this;
            }

            ProgressColumn[] additions = columns.Where(column => column != null).ToArray();
            if (additions.Length == 0)
            {
                return this;
            }

            return SetColumns(_options.GetColumns().Concat(additions));
        }

        private ProgressContext SetColumns(IEnumerable<ProgressColumn> columns)
        {
            ThrowIfDisposed();
            _options.Columns = columns == null ? null : columns.ToArray();
            IReadOnlyList<ProgressColumn> configured = _options.GetColumns();
            if (configured.Count > 0)
            {
                _options.ShowPercentage = configured.Any(column => column is PercentageColumn);
            }

            Refresh();
            return this;
        }

        /// <summary>
        /// Sets an optional progress render hook that can replace each generated row.
        /// </summary>
        /// <param name="renderHook">The render hook to use, or <see langword="null"/> to render generated rows directly.</param>
        /// <returns>The current progress context.</returns>
        public ProgressContext UseRenderHook(ProgressRenderHook renderHook)
        {
            ThrowIfDisposed();
            _options.RenderHook = renderHook;
            Refresh();
            return this;
        }

        /// <summary>
        /// Renders every task in the current context immediately.
        /// </summary>
        /// <returns>The current progress context.</returns>
        public ProgressContext Refresh()
        {
            ThrowIfDisposed();
            foreach (ProgressTask task in _tasks)
            {
                task.Render();
            }

            return this;
        }

        /// <summary>
        /// Runs an operation inside this progress context and disposes the context when the operation completes.
        /// </summary>
        /// <param name="action">The operation to run with the active progress context.</param>
        public void Start(Action<ProgressContext> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (this)
            {
                action(this);
            }
        }

        /// <summary>
        /// Runs an operation inside this progress context, disposes the context, and returns the operation result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active progress context.</param>
        /// <returns>The operation result.</returns>
        public T Start<T>(Func<ProgressContext, T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (this)
            {
                return func(this);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside this progress context and disposes the context when the operation completes.
        /// </summary>
        /// <param name="action">The operation to run with the active progress context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task StartAsync(Func<ProgressContext, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return StartAsync((progress, cancellationToken) => action(progress));
        }

        /// <summary>
        /// Runs an asynchronous operation inside this progress context and disposes the context when the operation completes.
        /// </summary>
        /// <param name="action">The operation to run with the active progress context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync(Func<ProgressContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (this)
            {
                await action(this, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs an asynchronous operation inside this progress context, disposes the context, and returns the operation result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active progress context.</param>
        /// <returns>A task that yields the operation result.</returns>
        public Task<T> StartAsync<T>(Func<ProgressContext, Task<T>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return StartAsync((progress, cancellationToken) => func(progress));
        }

        /// <summary>
        /// Runs an asynchronous operation inside this progress context, disposes the context, and returns the operation result.
        /// </summary>
        /// <typeparam name="T">The result type returned by the operation.</typeparam>
        /// <param name="func">The operation to run with the active progress context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that yields the operation result.</returns>
        public async Task<T> StartAsync<T>(Func<ProgressContext, CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            using (this)
            {
                return await func(this, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Releases fixed message resources used by all progress task rows.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_options.AutoClear)
            {
                foreach (ProgressTask task in _tasks)
                {
                    task.Clear();
                }
            }

            foreach (ProgressTask task in _tasks)
            {
                task.Dispose();
            }
        }

        internal string FormatRow(ProgressTask task)
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            if (width <= 0)
            {
                return string.Empty;
            }

            IReadOnlyList<ProgressColumn> columns = _options.GetColumns();
            if (columns.Count > 0)
            {
                return ApplyRenderHook(FormatColumnRow(task, width, columns), width);
            }

            return ApplyRenderHook(FormatDefaultRow(task, width), width);
        }

        internal IReadOnlyList<FixedMessageSegment> FormatRowSegments(ProgressTask task, ConsoleColor fallbackColor)
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            if (width <= 0)
            {
                return new[] { new FixedMessageSegment(string.Empty) };
            }

            Style fallbackStyle = ResolveStyle(task);
            ColorScheme fallbackScheme = CreateFallbackScheme(fallbackColor, fallbackStyle);
            IReadOnlyList<ProgressColumn> columns = _options.GetColumns();
            if (columns.Count > 0)
            {
                return ApplyRenderHook(
                    FormatColumnRowSegments(task, width, columns, fallbackScheme, fallbackStyle),
                    width,
                    fallbackScheme,
                    fallbackStyle);
            }

            return ApplyRenderHook(FormatDefaultRowSegments(task, width, fallbackScheme, fallbackStyle), width, fallbackScheme, fallbackStyle);
        }

        internal bool UsesSegmentRendering(ProgressTask task)
        {
            return ResolveStyle(task) != null ||
                HasRenderableMarkup(task?.Description) || _options.GetColumns().Any(column =>
                column is ProgressBarColumn progressBarColumn &&
                (progressBarColumn.CompletedStyle != null ||
                    progressBarColumn.FinishedStyle != null ||
                    progressBarColumn.RemainingStyle != null ||
                    progressBarColumn.IndeterminateStyle != null) ||
                column is SpinnerColumn spinnerColumn && spinnerColumn.Style != null);
        }

        private string ApplyRenderHook(string row, int width)
        {
            if (_options.RenderHook == null)
            {
                return row;
            }

            IConsoulRenderable generated = new RenderActionEntry(() => Consoul.Write(row ?? string.Empty, writeLine: false));
            IConsoulRenderable replacement = _options.RenderHook(generated, Tasks) ?? generated;
            int captureWidth = width < int.MaxValue ? width + 1 : width;
            string captured = RenderActionContent.Capture(replacement, captureWidth, 20);
            return PadOrTrim(FirstLine(captured), width);
        }

        private IReadOnlyList<FixedMessageSegment> ApplyRenderHook(
            IReadOnlyList<FixedMessageSegment> row,
            int width,
            ColorScheme fallbackScheme,
            Style fallbackStyle)
        {
            if (_options.RenderHook == null)
            {
                return row;
            }

            string text = string.Concat((row ?? new[] { new FixedMessageSegment(string.Empty) }).Select(segment => segment.Text));
            string hooked = ApplyRenderHook(text, width);
            return PadOrTrimSegments(new[] { CreateSegment(hooked, fallbackScheme, fallbackStyle) }, width, fallbackScheme, fallbackStyle);
        }

        private string FormatDefaultRow(ProgressTask task, int width)
        {
            ProgressLayout layout = ResolveLayout(width);
            string row = string.Empty;
            if (layout.LabelWidth > 0)
            {
                row += PadOrTrimMarkupText(task.Description, layout.LabelWidth) + " ";
            }

            int filled = 0;
            if (!task.IsIndeterminateValue && task.Progress > 0)
            {
                filled = (int)Math.Round(task.Progress * layout.BarWidth, MidpointRounding.AwayFromZero);
                filled = Math.Max(1, Math.Min(layout.BarWidth, filled));
            }

            row += "[";
            row += new string(_options.CompletedCharacter, filled);
            row += new string(_options.RemainingCharacter, layout.BarWidth - filled);
            row += "]";

            if (layout.ShowPercentage && !task.IsIndeterminateValue)
            {
                row += " " + PadOrTrim(Math.Round(task.Progress * 100).ToString("0") + "%", layout.PercentageWidth);
            }

            return PadOrTrim(row, width);
        }

        private IReadOnlyList<FixedMessageSegment> FormatDefaultRowSegments(
            ProgressTask task,
            int width,
            ColorScheme fallbackScheme,
            Style fallbackStyle)
        {
            ProgressLayout layout = ResolveLayout(width);
            var row = new List<FixedMessageSegment>();
            if (layout.LabelWidth > 0)
            {
                row.AddRange(PadOrTrimMarkupSegments(task.Description, layout.LabelWidth, fallbackScheme, fallbackStyle));
                row.Add(CreateSegment(" ", fallbackScheme, fallbackStyle));
            }

            int filled = 0;
            if (!task.IsIndeterminateValue && task.Progress > 0)
            {
                filled = (int)Math.Round(task.Progress * layout.BarWidth, MidpointRounding.AwayFromZero);
                filled = Math.Max(1, Math.Min(layout.BarWidth, filled));
            }

            row.Add(CreateSegment("[", fallbackScheme, fallbackStyle));
            row.Add(CreateSegment(new string(_options.CompletedCharacter, filled), fallbackScheme, fallbackStyle));
            row.Add(CreateSegment(new string(_options.RemainingCharacter, layout.BarWidth - filled), fallbackScheme, fallbackStyle));
            row.Add(CreateSegment("]", fallbackScheme, fallbackStyle));

            if (layout.ShowPercentage && !task.IsIndeterminateValue)
            {
                row.Add(CreateSegment(" ", fallbackScheme, fallbackStyle));
                row.Add(CreateSegment(PadOrTrim(Math.Round(task.Progress * 100).ToString("0") + "%", layout.PercentageWidth), fallbackScheme, fallbackStyle));
            }

            return PadOrTrimSegments(row, width, fallbackScheme, fallbackStyle);
        }

        private string FormatColumnRow(ProgressTask task, int width, IReadOnlyList<ProgressColumn> columns)
        {
            var segments = new List<ProgressColumnSegment>();
            foreach (ProgressColumn column in columns.Where(column => column != null))
            {
                ProgressColumnSegment segment = CreateColumnSegment(task, column, width);
                if (segment != null)
                {
                    segments.Add(segment);
                }
            }

            if (segments.Count == 0)
            {
                return PadOrTrim(string.Empty, width);
            }

            FitSegments(segments, width);
            string row = string.Join(" ", segments.Select(segment => segment.Render()).Where(text => text.Length > 0));
            return PadOrTrim(row, width);
        }

        private IReadOnlyList<FixedMessageSegment> FormatColumnRowSegments(
            ProgressTask task,
            int width,
            IReadOnlyList<ProgressColumn> columns,
            ColorScheme fallbackScheme,
            Style fallbackStyle)
        {
            var segments = new List<ProgressColumnSegment>();
            foreach (ProgressColumn column in columns.Where(column => column != null))
            {
                ProgressColumnSegment segment = CreateColumnSegment(task, column, width, fallbackScheme, fallbackStyle);
                if (segment != null)
                {
                    segments.Add(segment);
                }
            }

            if (segments.Count == 0)
            {
                return new[] { new FixedMessageSegment(PadOrTrim(string.Empty, width)) };
            }

            FitSegments(segments, width);
            var output = new List<FixedMessageSegment>();
            foreach (ProgressColumnSegment segment in segments.Where(segment => segment.Width > 0))
            {
                if (output.Count > 0)
                {
                    output.Add(CreateSegment(" ", fallbackScheme, fallbackStyle));
                }

                output.AddRange(segment.RenderSegments());
            }

            return output;
        }

        private ProgressColumnSegment CreateColumnSegment(ProgressTask task, ProgressColumn column, int width)
        {
            return CreateColumnSegment(task, column, width, RenderOptions.DefaultScheme);
        }

        private ProgressColumnSegment CreateColumnSegment(ProgressTask task, ProgressColumn column, int width, ColorScheme fallbackScheme)
        {
            return CreateColumnSegment(task, column, width, fallbackScheme, null);
        }

        private ProgressColumnSegment CreateColumnSegment(
            ProgressTask task,
            ProgressColumn column,
            int width,
            ColorScheme fallbackScheme,
            Style fallbackStyle)
        {
            if (column is TaskDescriptionColumn)
            {
                int desiredWidth = Math.Min(_options.ResolveLabelWidth(), Math.Max(0, width));
                return ProgressColumnSegment.Text(task.Description, desiredWidth, true, fallbackScheme, fallbackStyle);
            }

            if (column is ProgressBarColumn progressBarColumn)
            {
                int minimumBarWidth = _options.ResolveMinimumBarWidth();
                int preferredBarWidth = progressBarColumn.Width.HasValue
                    ? Math.Max(1, progressBarColumn.Width.Value)
                    : 20;
                int desiredBarWidth = Math.Max(1, Math.Min(preferredBarWidth, Math.Max(1, width - 2)));
                int minimumSegmentWidth = Math.Min(minimumBarWidth + 2, desiredBarWidth + 2);
                return ProgressColumnSegment.Bar(BuildBarSegments(task, desiredBarWidth, progressBarColumn, fallbackScheme, fallbackStyle), minimumSegmentWidth);
            }

            if (column is PercentageColumn)
            {
                return ProgressColumnSegment.Fixed(FormatPercentage(task), fallbackScheme, fallbackStyle);
            }

            if (column is RemainingTimeColumn)
            {
                return ProgressColumnSegment.Fixed(FormatRemaining(task), fallbackScheme, fallbackStyle);
            }

            if (column is ElapsedTimeColumn)
            {
                return ProgressColumnSegment.Fixed(FormatDuration(task.Elapsed), fallbackScheme, fallbackStyle);
            }

            if (column is SpinnerColumn spinnerColumn)
            {
                Style effectiveStyle = spinnerColumn.Style ?? fallbackStyle;
                return ProgressColumnSegment.Fixed(FormatSpinner(task, spinnerColumn), ResolveScheme(effectiveStyle, fallbackScheme), effectiveStyle);
            }

            if (column is TextColumn textColumn)
            {
                string text = FormatTextColumn(task, textColumn);
                int desiredWidth = textColumn.Width.HasValue
                    ? Math.Max(0, textColumn.Width.Value)
                    : Math.Min(MarkupRenderText.GetPlainText(text).Length, Math.Max(0, width));
                Style effectiveStyle = textColumn.Style ?? fallbackStyle;
                ColorScheme textFallbackScheme = ResolveScheme(effectiveStyle, fallbackScheme);
                return ProgressColumnSegment.Text(text, desiredWidth, true, textFallbackScheme, effectiveStyle);
            }

            if (column is DownloadedColumn)
            {
                return ProgressColumnSegment.Fixed(FormatQuantity(task.Value), fallbackScheme, fallbackStyle);
            }

            if (column is TransferSpeedColumn)
            {
                return ProgressColumnSegment.Fixed(FormatTransferSpeed(task), fallbackScheme, fallbackStyle);
            }

            return null;
        }

        private static string FormatTextColumn(ProgressTask task, TextColumn column)
        {
            string text = column?.Text ?? string.Empty;
            if (task == null || text.IndexOf('{') < 0)
            {
                return text;
            }

            text = ReplaceToken(text, "{task.Id}", task.Id.ToString());
            text = ReplaceToken(text, "{task.Description}", task.Description);
            text = ReplaceToken(text, "{task.Value}", FormatNumber(task.Value));
            text = ReplaceToken(text, "{task.MaxValue}", FormatNumber(task.MaxValue));
            text = ReplaceToken(text, "{task.Percentage}", FormatPercentage(task));
            text = ReplaceToken(text, "{task.Progress}", FormatNumber(task.Progress));
            text = ReplaceToken(text, "{task.State}", task.State.ToString());
            text = ReplaceToken(text, "{task.Tag}", Convert.ToString(task.Tag));
            text = ReplaceToken(text, "{task.Elapsed}", FormatDuration(task.Elapsed));
            text = ReplaceToken(text, "{task.ElapsedTime}", FormatOptionalDuration(task.ElapsedTime));
            text = ReplaceToken(text, "{task.RemainingTime}", FormatOptionalDuration(task.RemainingTime));
            text = ReplaceToken(text, "{task.Speed}", FormatTransferSpeed(task));
            text = ReplaceToken(text, "{task.IsStarted}", task.IsStarted.ToString());
            text = ReplaceToken(text, "{task.IsCompleted}", task.IsCompleted.ToString());
            text = ReplaceToken(text, "{task.IsFailed}", task.IsFailed.ToString());
            text = ReplaceToken(text, "{task.IsFinished}", task.IsFinished.ToString());
            text = ReplaceToken(text, "{task.IsIndeterminate}", task.IsIndeterminateValue.ToString());
            return text;
        }

        private static string ReplaceToken(string text, string token, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token))
            {
                return text ?? string.Empty;
            }

            int startIndex = 0;
            int index = text.IndexOf(token, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return text;
            }

            var builder = new StringBuilder(text.Length);
            while (index >= 0)
            {
                builder.Append(text, startIndex, index - startIndex);
                builder.Append(value ?? string.Empty);
                startIndex = index + token.Length;
                index = text.IndexOf(token, startIndex, StringComparison.OrdinalIgnoreCase);
            }

            builder.Append(text, startIndex, text.Length - startIndex);
            return builder.ToString();
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##");
        }

        private static string FormatOptionalDuration(TimeSpan? duration)
        {
            return duration.HasValue ? FormatDuration(duration.Value) : "--:--:--";
        }

        private void FitSegments(IList<ProgressColumnSegment> segments, int width)
        {
            while (GetSegmentWidth(segments) > width && TryShrink(segments, segment => segment.IsFlexibleText))
            {
            }

            while (GetSegmentWidth(segments) > width && TryShrink(segments, segment => segment.IsBar))
            {
            }

            while (GetSegmentWidth(segments) > width && TryShrink(segments, segment => segment.Width > segment.MinimumWidth))
            {
            }
        }

        private static bool TryShrink(IList<ProgressColumnSegment> segments, Func<ProgressColumnSegment, bool> predicate)
        {
            ProgressColumnSegment segment = segments
                .Where(item => item.Width > item.MinimumWidth && predicate(item))
                .OrderByDescending(item => item.Width)
                .FirstOrDefault();
            if (segment == null)
            {
                return false;
            }

            segment.Width--;
            return true;
        }

        private static int GetSegmentWidth(IList<ProgressColumnSegment> segments)
        {
            int visibleCount = segments.Count(segment => segment.Width > 0);
            int gaps = Math.Max(0, visibleCount - 1);
            return segments.Sum(segment => segment.Width) + gaps;
        }

        private string BuildBar(ProgressTask task, int barWidth)
        {
            barWidth = Math.Max(1, barWidth);
            int filled = 0;
            if (!task.IsIndeterminateValue && task.Progress > 0)
            {
                filled = (int)Math.Round(task.Progress * barWidth, MidpointRounding.AwayFromZero);
                filled = Math.Max(1, Math.Min(barWidth, filled));
            }

            if (task.IsIndeterminateValue && barWidth > 0)
            {
                int position = task.RenderCount % barWidth;
                char[] cells = Enumerable.Repeat(_options.RemainingCharacter, barWidth).ToArray();
                cells[position] = _options.CompletedCharacter;
                return "[" + new string(cells) + "]";
            }

            return "[" +
                new string(_options.CompletedCharacter, filled) +
                new string(_options.RemainingCharacter, barWidth - filled) +
                "]";
        }

        private IReadOnlyList<FixedMessageSegment> BuildBarSegments(
            ProgressTask task,
            int barWidth,
            ProgressBarColumn column,
            ColorScheme fallbackScheme,
            Style fallbackStyle)
        {
            barWidth = Math.Max(1, barWidth);
            var segments = new List<FixedMessageSegment>
            {
                CreateSegment("[", fallbackScheme, fallbackStyle)
            };

            Style remainingStyle = column.RemainingStyle ?? fallbackStyle;
            ColorScheme remainingScheme = ResolveScheme(remainingStyle, fallbackScheme);
            if (task.IsIndeterminateValue)
            {
                int position = task.RenderCount % barWidth;
                Style indeterminateStyle = column.IndeterminateStyle ?? column.CompletedStyle ?? fallbackStyle;
                AddSegment(segments, new string(_options.RemainingCharacter, position), remainingScheme, remainingStyle);
                AddSegment(segments, _options.CompletedCharacter.ToString(), ResolveScheme(indeterminateStyle, fallbackScheme), indeterminateStyle);
                AddSegment(segments, new string(_options.RemainingCharacter, barWidth - position - 1), remainingScheme, remainingStyle);
                segments.Add(CreateSegment("]", fallbackScheme, fallbackStyle));
                return segments;
            }

            int filled = 0;
            if (task.Progress > 0)
            {
                filled = (int)Math.Round(task.Progress * barWidth, MidpointRounding.AwayFromZero);
                filled = Math.Max(1, Math.Min(barWidth, filled));
            }

            Style completedStyle = task.IsCompleted && column.FinishedStyle != null
                ? column.FinishedStyle
                : column.CompletedStyle ?? fallbackStyle;
            AddSegment(segments, new string(_options.CompletedCharacter, filled), ResolveScheme(completedStyle, fallbackScheme), completedStyle);
            AddSegment(segments, new string(_options.RemainingCharacter, barWidth - filled), remainingScheme, remainingStyle);
            segments.Add(CreateSegment("]", fallbackScheme, fallbackStyle));
            return segments;
        }

        private static void AddSegment(ICollection<FixedMessageSegment> segments, string text, ColorScheme scheme, Style style)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            segments.Add(CreateSegment(text, scheme, style));
        }

        private static ColorScheme ResolveScheme(Style style, ColorScheme fallbackScheme)
        {
            return style == null ? fallbackScheme : style.ToColorScheme(fallbackScheme);
        }

        private static string FormatPercentage(ProgressTask task)
        {
            if (task.IsIndeterminateValue)
            {
                return "--%";
            }

            return Math.Round(task.Progress * 100).ToString("0") + "%";
        }

        private static string FormatRemaining(ProgressTask task)
        {
            if (task.IsIndeterminateValue || task.Value <= 0 || task.Progress <= 0)
            {
                return "--:--:--";
            }

            TimeSpan elapsed = task.GetElapsed(DateTimeOffset.UtcNow);
            if (elapsed.TotalSeconds <= 0)
            {
                return "--:--:--";
            }

            double unitsPerSecond = task.Value / elapsed.TotalSeconds;
            if (unitsPerSecond <= 0)
            {
                return "--:--:--";
            }

            double remainingUnits = Math.Max(0, task.MaxValue - task.Value);
            return FormatDuration(TimeSpan.FromSeconds(remainingUnits / unitsPerSecond));
        }

        private static string FormatTransferSpeed(ProgressTask task)
        {
            TimeSpan elapsed = task.GetElapsed(DateTimeOffset.UtcNow);
            if (elapsed.TotalSeconds <= 0 || task.Value <= 0)
            {
                return "--/s";
            }

            return FormatQuantity(task.Value / elapsed.TotalSeconds) + "/s";
        }

        private static string FormatSpinner(ProgressTask task, SpinnerColumn column)
        {
            Spinner spinner = column.Spinner ?? Spinner.Known.Default;
            IReadOnlyList<string> frames = spinner.Frames;
            if (frames.Count == 0)
            {
                return string.Empty;
            }

            return task.IsFinished ? frames[frames.Count - 1] : frames[task.RenderCount % frames.Count];
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                duration = TimeSpan.Zero;
            }

            if (duration.TotalHours >= 100)
            {
                return ((int)duration.TotalHours).ToString("0") + ":" + duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");
            }

            return ((int)duration.TotalHours).ToString("00") + ":" + duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");
        }

        private static string FormatQuantity(double value)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double scaled = Math.Max(0, value);
            int unitIndex = 0;
            while (scaled >= 1024 && unitIndex < units.Length - 1)
            {
                scaled /= 1024;
                unitIndex++;
            }

            string number = unitIndex == 0 ? scaled.ToString("0") : scaled.ToString("0.#");
            return number + " " + units[unitIndex];
        }

        internal ConsoleColor ResolveColor(ProgressTask task)
        {
            if (task.IsFailed)
            {
                return _options.FailedColor;
            }

            return task.IsCompleted ? _options.CompleteColor : _options.ActiveColor;
        }

        internal bool ShouldHide(ProgressTask task)
        {
            if (task == null || !task.IsCompleted)
            {
                return false;
            }

            return task.HideWhenCompleted ?? _options.HideCompleted;
        }

        private ProgressLayout ResolveLayout(int width)
        {
            int labelWidth = Math.Min(_options.ResolveLabelWidth(), Math.Max(0, width - 1));
            int minimumBarWidth = _options.ResolveMinimumBarWidth();
            bool showPercentage = _options.ShowPercentage;
            int percentageWidth = showPercentage ? 4 : 0;

            while (showPercentage && labelWidth + (labelWidth > 0 ? 1 : 0) + 2 + minimumBarWidth + 1 + percentageWidth > width)
            {
                showPercentage = false;
                percentageWidth = 0;
            }

            while (labelWidth > 0 && labelWidth + 1 + 2 + minimumBarWidth + (showPercentage ? 1 + percentageWidth : 0) > width)
            {
                labelWidth--;
            }

            int barWidth = width - (labelWidth > 0 ? labelWidth + 1 : 0) - 2 - (showPercentage ? 1 + percentageWidth : 0);
            if (barWidth < 1)
            {
                showPercentage = false;
                percentageWidth = 0;
                labelWidth = Math.Max(0, width - 3);
                barWidth = Math.Max(1, width - (labelWidth > 0 ? labelWidth + 1 : 0) - 2);
            }

            return new ProgressLayout(labelWidth, barWidth, showPercentage, percentageWidth);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ProgressContext));
            }
        }

        internal IDisposable UseCapturedDriver()
        {
            return Consoul.UseConsoleDriver(_driver);
        }

        private static string PadOrTrim(string text, int width)
        {
            text = text ?? string.Empty;
            width = Math.Max(0, width);
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            return text.PadRight(width);
        }

        private static string FirstLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            int lineBreak = text.IndexOfAny(new[] { '\r', '\n' });
            return lineBreak < 0 ? text : text.Substring(0, lineBreak);
        }

        private static string PadOrTrimMarkupText(string text, int width)
        {
            return PadOrTrim(MarkupRenderText.GetPlainText(text), width);
        }

        private static List<FixedMessageSegment> PadOrTrimMarkupSegments(
            string text,
            int width,
            ColorScheme fallbackScheme,
            Style fallbackStyle)
        {
            return PadOrTrimSegments(
                MarkupRenderText.ParseSegments(
                    text,
                    fallbackScheme,
                    fallbackStyle?.Decoration ?? Decoration.None,
                    fallbackStyle?.ForegroundRgb,
                    fallbackStyle?.BackgroundRgb),
                width,
                fallbackScheme,
                fallbackStyle);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(
            IList<FixedMessageSegment> segments,
            int width,
            ColorScheme paddingScheme,
            Style paddingStyle)
        {
            var result = MarkupRenderText.TruncateSegments(segments, width);
            MarkupRenderText.AddPadding(
                result,
                Math.Max(0, width) - MarkupRenderText.GetLength(result),
                paddingScheme,
                paddingStyle?.Decoration ?? Decoration.None,
                paddingStyle?.ForegroundRgb,
                paddingStyle?.BackgroundRgb);
            return result;
        }

        private static bool HasRenderableMarkup(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return !string.Equals(MarkupRenderText.GetPlainText(text), text, StringComparison.Ordinal);
        }

        private Style ResolveStyle(ProgressTask task)
        {
            if (task?.IsFailed == true)
            {
                return _options.FailedStyle;
            }

            return task?.IsCompleted == true ? _options.CompleteStyle : _options.ActiveStyle;
        }

        private static FixedMessageSegment CreateSegment(string text, ColorScheme scheme, Style style)
        {
            return new FixedMessageSegment(
                text,
                scheme,
                style?.Decoration ?? Decoration.None,
                null,
                style?.ForegroundRgb,
                style?.BackgroundRgb);
        }

        private static ColorScheme CreateFallbackScheme(ConsoleColor color, Style style)
        {
            ColorScheme scheme = RenderOptions.DefaultScheme;
            scheme.Color = color;
            return style == null ? scheme : style.ToColorScheme(scheme);
        }

        private sealed class ProgressColumnSegment
        {
            private readonly string _text;
            private readonly IReadOnlyList<FixedMessageSegment> _segments;
            private readonly bool _isBar;

            private ProgressColumnSegment(IReadOnlyList<FixedMessageSegment> segments, int width, int minimumWidth, bool isFlexibleText, bool isBar)
            {
                _segments = segments ?? new[] { new FixedMessageSegment(string.Empty) };
                _text = string.Concat(_segments.Select(segment => segment.Text));
                Width = Math.Max(0, width);
                MinimumWidth = Math.Max(0, Math.Min(minimumWidth, Width));
                IsFlexibleText = isFlexibleText;
                _isBar = isBar;
            }

            public int Width { get; set; }

            public int MinimumWidth { get; }

            public bool IsFlexibleText { get; }

            public bool IsBar => _isBar;

            public static ProgressColumnSegment Text(string text, int width, bool flexible)
            {
                return Text(text, width, flexible, RenderOptions.DefaultScheme, null);
            }

            public static ProgressColumnSegment Text(string text, int width, bool flexible, ColorScheme fallbackScheme)
            {
                return Text(text, width, flexible, fallbackScheme, null);
            }

            public static ProgressColumnSegment Text(string text, int width, bool flexible, ColorScheme fallbackScheme, Style fallbackStyle)
            {
                return new ProgressColumnSegment(
                    MarkupRenderText.ParseSegments(
                        text,
                        fallbackScheme,
                        fallbackStyle?.Decoration ?? Decoration.None,
                        fallbackStyle?.ForegroundRgb,
                        fallbackStyle?.BackgroundRgb),
                    width,
                    flexible ? 0 : width,
                    flexible,
                    isBar: false);
            }

            public static ProgressColumnSegment Fixed(string text, ColorScheme? scheme = null)
            {
                text = text ?? string.Empty;
                return new ProgressColumnSegment(new[] { new FixedMessageSegment(text, scheme) }, text.Length, text.Length, isFlexibleText: false, isBar: false);
            }

            public static ProgressColumnSegment Fixed(string text, ColorScheme scheme, Style style)
            {
                text = text ?? string.Empty;
                return new ProgressColumnSegment(
                    new[] { CreateSegment(text, scheme, style) },
                    text.Length,
                    text.Length,
                    isFlexibleText: false,
                    isBar: false);
            }

            public static ProgressColumnSegment Bar(IReadOnlyList<FixedMessageSegment> segments, int minimumWidth)
            {
                segments = segments ?? new[] { new FixedMessageSegment(string.Empty) };
                int length = segments.Sum(segment => segment.Text.Length);
                return new ProgressColumnSegment(segments, length, Math.Max(3, minimumWidth), isFlexibleText: false, isBar: true);
            }

            public string Render()
            {
                return PadOrTrim(_text, Width);
            }

            public IReadOnlyList<FixedMessageSegment> RenderSegments()
            {
                var result = new List<FixedMessageSegment>();
                int remaining = Width;
                foreach (FixedMessageSegment segment in _segments)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    string text = segment.Text ?? string.Empty;
                    if (text.Length > remaining)
                    {
                        text = text.Substring(0, remaining);
                    }

                    if (text.Length > 0)
                    {
                        result.Add(new FixedMessageSegment(
                            text,
                            segment.Scheme,
                            segment.Decoration,
                            segment.Link,
                            segment.ForegroundRgb,
                            segment.BackgroundRgb));
                    }

                    remaining -= text.Length;
                }

                if (remaining > 0)
                {
                    FixedMessageSegment paddingSource = _segments.LastOrDefault();
                    result.Add(new FixedMessageSegment(
                        new string(' ', remaining),
                        paddingSource.Scheme,
                        paddingSource.Decoration,
                        null,
                        paddingSource.ForegroundRgb,
                        paddingSource.BackgroundRgb));
                }

                return result;
            }
        }

        private readonly struct ProgressLayout
        {
            public ProgressLayout(int labelWidth, int barWidth, bool showPercentage, int percentageWidth)
            {
                LabelWidth = labelWidth;
                BarWidth = barWidth;
                ShowPercentage = showPercentage;
                PercentageWidth = percentageWidth;
            }

            public int LabelWidth { get; }

            public int BarWidth { get; }

            public bool ShowPercentage { get; }

            public int PercentageWidth { get; }
        }
    }
}
