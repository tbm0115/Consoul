using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary.Test.Views
{
    /// <summary>
    /// Demonstrates building a menu view without subclassing <see cref="StaticView"/>.
    /// </summary>
    public class PocMenuView : IView
    {
        private readonly FluentView _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocMenuView"/> class.
        /// </summary>
        public PocMenuView()
        {
            _view = Consoul.View("5-Minute POC View")
                .Option("Say hello", () =>
                {
                    Consoul.Write("Hello from a fluent proof-of-concept view.", ConsoleColor.Green);
                    Consoul.Wait();
                })
                .Navigate<PocTableView>("Show a table")
                .Navigate<PocSettingsView>("Edit settings");
        }

        /// <summary>
        /// Gets or sets the title of the view.
        /// </summary>
        public string Title
        {
            get => _view.Title;
            set => _view.Title = value;
        }

        /// <summary>
        /// Gets a value indicating whether a request to navigate back has been made.
        /// </summary>
        public bool GoBackRequested => _view.GoBackRequested;

        /// <summary>
        /// Signals a request to navigate back from the current view.
        /// </summary>
        public void GoBack()
        {
            _view.GoBack();
        }

        /// <summary>
        /// Renders the view synchronously.
        /// </summary>
        public void Render()
        {
            _view.Render();
        }

        /// <summary>
        /// Renders the view asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during rendering.</param>
        /// <returns>A task representing the asynchronous render operation.</returns>
        public Task RenderAsync(CancellationToken cancellationToken = default)
        {
            return _view.RenderAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Demonstrates a minimal table-focused proof-of-concept view.
    /// </summary>
    public class PocTableView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocTableView"/> class.
        /// </summary>
        public PocTableView()
        {
            Title = "POC Table View";
        }

        /// <summary>
        /// Renders a small table of work items.
        /// </summary>
        [ViewOption("Render work-item table")]
        public void RenderWorkItems()
        {
            var items = new List<PocWorkItem>
            {
                new PocWorkItem { Name = "Prototype menu", Status = "Done", Owner = "Alex" },
                new PocWorkItem { Name = "Wire table", Status = "In review", Owner = "Sam" },
                new PocWorkItem { Name = "Tune settings", Status = "Next", Owner = "Jordan" }
            };

            var table = DynamicTableView<PocWorkItem>.Create(items, item => item.Name, item => item.Status, item => item.Owner);
            table.Render("Current proof-of-concept work", ConsoleColor.Cyan);
            Consoul.Wait();
        }

        private sealed class PocWorkItem
        {
            public string Name { get; set; }

            public string Status { get; set; }

            public string Owner { get; set; }
        }
    }

    /// <summary>
    /// Demonstrates editing a settings object from a small proof-of-concept view.
    /// </summary>
    public class PocSettingsView : StaticView
    {
        private readonly PocSettings _settings = new PocSettings
        {
            Environment = "Development",
            EnablePreviewMode = true,
            RetryCount = 3
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PocSettingsView"/> class.
        /// </summary>
        public PocSettingsView()
        {
            Title = "POC Settings View";
        }

        /// <summary>
        /// Opens the settings object editor.
        /// </summary>
        [ViewOption("Edit settings object")]
        public void EditSettings()
        {
            var editor = new EditObjectView(_settings);
            editor.Render();

            Consoul.Write("Settings saved for " + _settings.Environment + ".", ConsoleColor.Green);
            Consoul.Wait();
        }

        private sealed class PocSettings
        {
            public string Environment { get; set; }

            public bool EnablePreviewMode { get; set; }

            public int RetryCount { get; set; }
        }
    }
}
