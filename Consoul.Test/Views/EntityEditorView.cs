using System;
using System.Collections.Generic;
using ConsoulLibrary.Views.Editing;

namespace ConsoulLibrary.Test.Views
{
    /// <summary>
    /// Demonstrates the <see cref="EditObjectView"/> using nested dictionaries and custom metadata layers.
    /// </summary>
    public class EntityEditorView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityEditorView"/> class.
        /// </summary>
        public EntityEditorView()
        {
            Title = new BannerEntry("Edit Object View").Message;
        }

        /// <summary>
        /// Opens the editor for a simple person model.
        /// </summary>
        [ViewOption("Edit Person")]
        internal void EditPerson()
        {
            var person = new Person();
            var view = new EditObjectView(person);
            view.Render();

            var lastName = person.LastName ?? string.Empty;
            var firstName = person.FirstName ?? string.Empty;
            var birthDate = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("MM/dd/yyyy") : "<N/A>";
            Consoul.Write(lastName + ", " + firstName + " created on " + birthDate + "!", ConsoleColor.Green);

            if (person.Spouse != null)
            {
                var spouseName = person.Spouse.Spouse != null ? person.Spouse.Spouse.FirstName : string.Empty;
                Consoul.Write("\tMarried to " + spouseName + " on " + person.Spouse.Anniversary.ToString("MM/dd/yyyy"));
            }

            if (person.Children != null)
            {
                Consoul.Write("\tHas Children");
                foreach (var child in person.Children)
                {
                    var childBirth = child.Value.DateOfBirth.HasValue ? child.Value.DateOfBirth.Value.ToString("MM/dd/yyyy") : "<N/A>";
                    Consoul.Write("\t\t" + child.Value.FirstName + ", created on " + childBirth);
                }
            }

            Consoul.Wait();
        }

        /// <summary>
        /// Opens the editor for a mixed object that resolves remote constructor metadata.
        /// </summary>
        [ViewOption("Edit Mixed Object")]
        public void EditMixedObject()
        {
            var adapterType = typeof(SampleRemoteAdapter);
            var mixedObject = new MixedObject
            {
                AdapterSourceAssembly = adapterType.Assembly.Location,
                AdapterSourceType = adapterType.FullName,
                Options = new Dictionary<string, object>()
            };

            var view = new EditObjectView(mixedObject);
            view.Render();

            if (mixedObject.Options != null && mixedObject.Options.Count > 0)
            {
                foreach (var kvp in mixedObject.Options)
                {
                    Consoul.Write("\t{Key:White}: {Value:Green}", args: new object[] { kvp.Key, kvp.Value ?? string.Empty });
                }
            }
            else
            {
                Consoul.Write("No constructor options found!", ConsoleColor.Red);
            }

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Sample model demonstrating person editing.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Legal first name of the person.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Legal last name of the person.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Legal date of birth of the person.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Legal married spouse of the person.
        /// </summary>
        public Marriage Spouse { get; set; }

        /// <summary>
        /// Legal dependent children of the person.
        /// </summary>
        public Dictionary<string, Person> Children { get; set; }
    }

    /// <summary>
    /// Represents a marriage relationship for the sample.
    /// </summary>
    public class Marriage
    {
        /// <summary>
        /// Legal marriage date.
        /// </summary>
        public DateTime Anniversary { get; set; }

        /// <summary>
        /// Reference to the spouse.
        /// </summary>
        public Person Spouse { get; set; }
    }

    /// <summary>
    /// Sample model demonstrating remote constructor metadata.
    /// </summary>
    public class MixedObject
    {
        /// <summary>
        /// Location of the remote type.
        /// </summary>
        public string AdapterSourceAssembly { get; set; }

        /// <summary>
        /// Fully qualified remote type name.
        /// </summary>
        public string AdapterSourceType { get; set; }

        /// <summary>
        /// Constructor options for the referenced type.
        /// </summary>
        [PropertyMetadataResolver(typeof(RemoteConstructorMetadataResolver))]
        [RemoteConstructorOptions(nameof(AdapterSourceAssembly), nameof(AdapterSourceType), DisplayName = "Options", Description = "Constructor options for the referenced type.", Instructions = "Each key should match a constructor parameter on the remote type.")]
        public Dictionary<string, object> Options { get; set; }
    }

    /// <summary>
    /// Demonstrates a remote adapter with nested configuration.
    /// </summary>
    public class SampleRemoteAdapter
    {
        /// <summary>
        /// Constructs the sample adapter.
        /// </summary>
        /// <param name="configPath">Filepath to a JSON configuration file for the adapter.</param>
        /// <param name="options">Nested options for the adapter.</param>
        /// <param name="logger">Optional logger implementation.</param>
        public SampleRemoteAdapter(string configPath, SampleRemoteAdapterOptions options, IAdapterLogger logger = null)
        {
        }

        /// <summary>
        /// Nested configuration object to demonstrate recursive editing.
        /// </summary>
        public class SampleRemoteAdapterOptions
        {
            /// <summary>
            /// Operating mode for the adapter.
            /// </summary>
            public string Mode { get; set; }

            /// <summary>
            /// Retry count before failing operations.
            /// </summary>
            public int RetryCount { get; set; }
        }

        /// <summary>
        /// Represents an adapter logger for demonstration purposes.
        /// </summary>
        public interface IAdapterLogger
        {
        }
    }
}
