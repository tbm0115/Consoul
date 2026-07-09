using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one task row in a <see cref="ProgressContext"/>.
    /// </summary>
    public class ProgressTask : IDisposable
    {
        private static int _nextId;

        private readonly ProgressContext _context;
        private readonly FixedMessage _message;
        private bool _disposed;
        private int _renderCount;
        private bool? _hideWhenCompleted;

        internal ProgressTask(ProgressContext context, FixedMessage message, string description, double maxValue, bool autoStart = true)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _description = description ?? string.Empty;
            _maxValue = maxValue <= 0 ? 1 : maxValue;
            State = autoStart ? ProgressTaskState.Active : ProgressTaskState.Pending;
            StartedAt = DateTimeOffset.UtcNow;
            LastUpdatedAt = StartedAt;
            Id = Interlocked.Increment(ref _nextId);
            StartTime = autoStart ? DateTime.UtcNow : (DateTime?)null;
        }

        /// <summary>
        /// Gets the task identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets or sets optional user data associated with the task.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets whether this task should be hidden after it completes, overriding the context default.
        /// </summary>
        public bool? HideWhenCompleted
        {
            get => _hideWhenCompleted;
            set
            {
                _hideWhenCompleted = value;
                RenderIfActive();
            }
        }

        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value ?? string.Empty;
                Touch();
                RenderIfActive();
            }
        }

        /// <summary>
        /// Gets or sets the current task value.
        /// </summary>
        public double Value
        {
            get => _value;
            set => SetValue(value, updateState: true);
        }

        /// <summary>
        /// Gets or sets the value that represents completion.
        /// </summary>
        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value <= 0 ? 1 : value;
                _value = Math.Max(0, Math.Min(_maxValue, _value));
                if (_value >= _maxValue && !IsFailed)
                {
                    State = ProgressTaskState.Completed;
                    StopTime = DateTime.UtcNow;
                }

                Touch();
                RenderIfActive();
            }
        }

        private string _description;
        private double _value;
        private double _maxValue;
        private bool _isIndeterminate;

        /// <summary>
        /// Gets a value between 0.0 and 1.0 representing progress.
        /// </summary>
        public double Progress => Math.Max(0.0, Math.Min(1.0, Value / MaxValue));

        /// <summary>
        /// Gets the percentage complete for Spectre-style progress snippets.
        /// </summary>
        public double Percentage => Math.Max(0.0, Math.Min(100.0, Progress * 100.0));

        /// <summary>
        /// Gets the approximate task speed in units per second.
        /// </summary>
        public double? Speed
        {
            get
            {
                TimeSpan? elapsed = ElapsedTime;
                if (!elapsed.HasValue || elapsed.Value.TotalSeconds <= 0 || Value <= 0)
                {
                    return null;
                }

                return Value / elapsed.Value.TotalSeconds;
            }
        }

        /// <summary>
        /// Gets the current lifecycle state of the task.
        /// </summary>
        public ProgressTaskState State { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this task has started.
        /// </summary>
        public bool IsStarted => State != ProgressTaskState.Pending;

        /// <summary>
        /// Gets a value indicating whether this task has completed.
        /// </summary>
        public bool IsCompleted => State == ProgressTaskState.Completed;

        /// <summary>
        /// Gets a value indicating whether this task has failed.
        /// </summary>
        public bool IsFailed => State == ProgressTaskState.Failed;

        /// <summary>
        /// Gets a value indicating whether this task has completed or failed.
        /// </summary>
        public bool IsFinished => IsCompleted || IsFailed;

        /// <summary>
        /// Gets a value indicating whether the task is currently indeterminate.
        /// </summary>
        public bool IsIndeterminateValue => _isIndeterminate;

        /// <summary>
        /// Gets the time at which this task was created.
        /// </summary>
        public DateTimeOffset StartedAt { get; }

        /// <summary>
        /// Gets the most recent time at which this task was updated.
        /// </summary>
        public DateTimeOffset LastUpdatedAt { get; private set; }

        /// <summary>
        /// Gets the time at which this task was started.
        /// </summary>
        public DateTime? StartTime { get; private set; }

        /// <summary>
        /// Gets the time at which this task was completed or failed.
        /// </summary>
        public DateTime? StopTime { get; private set; }

        /// <summary>
        /// Gets the elapsed time for the task.
        /// </summary>
        public TimeSpan Elapsed => GetElapsed(DateTimeOffset.UtcNow);

        /// <summary>
        /// Gets the elapsed time from task start to now, completion, or failure.
        /// </summary>
        public TimeSpan? ElapsedTime
        {
            get
            {
                if (!StartTime.HasValue)
                {
                    return null;
                }

                DateTime end = StopTime ?? DateTime.UtcNow;
                TimeSpan elapsed = end - StartTime.Value;
                return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
            }
        }

        /// <summary>
        /// Gets the estimated remaining time for the task when a speed can be inferred.
        /// </summary>
        public TimeSpan? RemainingTime
        {
            get
            {
                if (IsFinished)
                {
                    return TimeSpan.Zero;
                }

                double? speed = Speed;
                if (!speed.HasValue || speed.Value <= 0)
                {
                    return null;
                }

                double remainingUnits = Math.Max(0, MaxValue - Value);
                double remainingSeconds = remainingUnits / speed.Value;
                if (remainingSeconds > TimeSpan.MaxValue.TotalSeconds)
                {
                    return TimeSpan.MaxValue;
                }

                return TimeSpan.FromSeconds(remainingSeconds);
            }
        }

        /// <summary>
        /// Gets or sets the maximum time a cached speed estimate may be reused.
        /// </summary>
        public TimeSpan MaxTimeForSpeedCache { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the maximum age of samples kept for estimating speed.
        /// </summary>
        public TimeSpan MaxSamplingAge { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the maximum number of samples kept for estimating speed.
        /// </summary>
        public int MaxSamplesKept { get; set; } = 1000;

        /// <summary>
        /// Sets whether the task should render as indeterminate and returns the current task.
        /// </summary>
        /// <param name="enabled"><c>true</c> to render as indeterminate; otherwise, <c>false</c>.</param>
        /// <returns>The current progress task.</returns>
        public ProgressTask IsIndeterminate(bool enabled = true)
        {
            ThrowIfDisposed();
            _isIndeterminate = enabled;
            Touch();
            RenderIfActive();
            return this;
        }

        /// <summary>
        /// Marks the task as active and optionally replaces the task description.
        /// </summary>
        /// <param name="description">Optional replacement description.</param>
        public void Start(string description = null)
        {
            ThrowIfDisposed();
            if (description != null)
            {
                _description = description;
            }

            State = ProgressTaskState.Active;
            StartTime = DateTime.UtcNow;
            StopTime = null;
            Touch();
            RenderIfActive();
        }

        /// <summary>
        /// Starts the task using the Spectre-style lifecycle method name.
        /// </summary>
        public void StartTask()
        {
            Start();
        }

        /// <summary>
        /// Updates the task value and optionally replaces the task description.
        /// </summary>
        /// <param name="value">The new task value.</param>
        /// <param name="description">Optional replacement description.</param>
        public void Update(double value, string description = null)
        {
            ThrowIfDisposed();
            SetValue(value, updateState: true, render: false);
            if (description != null)
            {
                _description = description;
            }

            if (!StartTime.HasValue)
            {
                StartTime = DateTime.UtcNow;
            }

            Touch();
            RenderIfActive();
        }

        /// <summary>
        /// Increments the task value and optionally replaces the task description.
        /// </summary>
        /// <param name="amount">The amount to add to the current value.</param>
        /// <param name="description">Optional replacement description.</param>
        public void Increment(double amount, string description = null)
        {
            Update(Value + amount, description);
        }

        /// <summary>
        /// Marks the task as complete and optionally replaces the task description.
        /// </summary>
        /// <param name="description">Optional replacement description.</param>
        public void Complete(string description = null)
        {
            Update(MaxValue, description);
            State = ProgressTaskState.Completed;
            if (!StartTime.HasValue)
            {
                StartTime = DateTime.UtcNow;
            }

            StopTime = DateTime.UtcNow;
            Touch();
            RenderIfActive();
        }

        /// <summary>
        /// Stops the task and marks it complete using the Spectre-style lifecycle method name.
        /// </summary>
        public void StopTask()
        {
            Complete();
        }

        /// <summary>
        /// Marks the task as failed and optionally replaces the task description.
        /// </summary>
        /// <param name="description">Optional replacement description.</param>
        public void Fail(string description = null)
        {
            ThrowIfDisposed();
            if (description != null)
            {
                _description = description;
            }

            State = ProgressTaskState.Failed;
            if (!StartTime.HasValue)
            {
                StartTime = DateTime.UtcNow;
            }

            StopTime = DateTime.UtcNow;
            Touch();
            RenderIfActive();
        }

        /// <summary>
        /// Releases fixed message resources used by this task.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _message.Dispose();
        }

        internal void Render()
        {
            if (_context.ShouldHide(this))
            {
                Clear(preservePosition: true);
                return;
            }

            _renderCount++;
            ConsoleColor color = _context.ResolveColor(this);
            if (_context.UsesSegmentRendering(this))
            {
                IReadOnlyList<FixedMessageSegment> row = _context.FormatRowSegments(this, color);
                _message.RenderSegments(row, color);
                return;
            }

            _message.Render(_context.FormatRow(this), color);
        }

        internal void SetRefreshRate(int milliseconds)
        {
            _message.RefreshRate = RefreshRateHelpers.Normalize(milliseconds);
        }

        internal void Clear()
        {
            Clear(preservePosition: false);
        }

        internal void Clear(bool preservePosition)
        {
            int left = _message.FixedLeft;
            int top = _message.FixedTop;
            bool hadPosition = _message.HasFixedPosition;
            _message.Reset();
            if (preservePosition && hadPosition)
            {
                _message.SetFixedPosition(left, top);
            }
        }

        internal int RenderCount => _renderCount;

        internal bool HasFixedPosition => _message.HasFixedPosition;

        internal int FixedLeft => _message.FixedLeft;

        internal int FixedTop => _message.FixedTop;

        internal void SetFixedPosition(int left, int top)
        {
            _message.SetFixedPosition(left, top);
        }

        internal void SetIndeterminateSilently(bool enabled)
        {
            _isIndeterminate = enabled;
            Touch();
        }

        internal void SetHideWhenCompletedSilently(bool? hideWhenCompleted)
        {
            _hideWhenCompleted = hideWhenCompleted;
        }

        internal TimeSpan GetElapsed(DateTimeOffset now)
        {
            DateTimeOffset end = IsFinished ? LastUpdatedAt : now;
            TimeSpan elapsed = end - StartedAt;
            return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ProgressTask));
            }
        }

        private void SetValue(double value, bool updateState, bool render = true)
        {
            ThrowIfDisposed();
            _value = Math.Max(0, Math.Min(MaxValue, value));
            if (updateState && !IsFailed)
            {
                State = _value >= MaxValue ? ProgressTaskState.Completed : ProgressTaskState.Active;
                if (!StartTime.HasValue)
                {
                    StartTime = DateTime.UtcNow;
                }

                if (State == ProgressTaskState.Completed)
                {
                    _isIndeterminate = false;
                    StopTime = DateTime.UtcNow;
                }
                else
                {
                    StopTime = null;
                }
            }

            if (render)
            {
                Touch();
                RenderIfActive();
            }
        }

        private void Touch()
        {
            LastUpdatedAt = DateTimeOffset.UtcNow;
        }

        private void RenderIfActive()
        {
            if (_disposed || !_context.Options.AutoRefresh)
            {
                return;
            }

            Render();
        }
    }

    /// <summary>
    /// Provides Spectre-style chainable helpers for <see cref="ProgressTask"/>.
    /// </summary>
    public static class ProgressTaskExtensions
    {
        /// <summary>
        /// Sets the task description.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <param name="description">The task description.</param>
        /// <returns>The current progress task.</returns>
        public static ProgressTask Description(this ProgressTask task, string description)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.Description = description;
            return task;
        }

        /// <summary>
        /// Sets the value that represents task completion.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <param name="value">The maximum value.</param>
        /// <returns>The current progress task.</returns>
        public static ProgressTask MaxValue(this ProgressTask task, double value)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.MaxValue = value;
            return task;
        }

        /// <summary>
        /// Sets the current task value.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <param name="value">The task value.</param>
        /// <returns>The current progress task.</returns>
        public static ProgressTask Value(this ProgressTask task, double value)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.Value = value;
            return task;
        }

        /// <summary>
        /// Sets whether the task should be hidden after it completes.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <param name="hideWhenCompleted"><c>true</c> to hide the task when complete; otherwise, <c>false</c>.</param>
        /// <returns>The current progress task.</returns>
        public static ProgressTask HideWhenCompleted(this ProgressTask task, bool hideWhenCompleted = true)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.HideWhenCompleted = hideWhenCompleted;
            return task;
        }

        /// <summary>
        /// Sets optional user data associated with the task.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <param name="tag">The user data to attach.</param>
        /// <returns>The current progress task.</returns>
        public static ProgressTask Tag(this ProgressTask task, object tag)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.Tag = tag;
            return task;
        }
    }
}
