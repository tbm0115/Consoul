using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ConsoulLibrary;
using ConsoulLibrary.Views.Editing;
using Xunit;

namespace ConsoulLibrary.Tests.Views
{
    /// <summary>
    /// Tests for the property metadata helpers leveraged by <see cref="ConsoulLibrary.EditObjectView"/>.
    /// </summary>
    public class EditObjectViewMetadataTests
    {
        /// <summary>
        /// Validates that display attributes and XML documentation are surfaced through <see cref="PropertyDocumentation"/>.
        /// </summary>
        [Fact]
        public void PropertyDocumentation_UsesDisplayMetadataAndXmlSummary()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Adapter));
            Assert.NotNull(property);

            if (property == null)
            {
                throw new InvalidOperationException("Expected Adapter property to exist.");
            }

            var documentation = new PropertyDocumentation(property);
            Assert.Equal("Adapter name", documentation.DisplayName);
            Assert.Equal("Adapter path description", documentation.DisplayDescription);
            Assert.Contains("SampleDependency", documentation.XmlSummary);
        }

        /// <summary>
        /// Validates that metadata resolvers can override documentation and editor behaviour.
        /// </summary>
        [Fact]
        public void MetadataResolver_OverridesDocumentationAndEditor()
        {
            var model = new ResolverModel();
            var view = new EditObjectView(model);

            var field = typeof(EditObjectView).GetField("_descriptors", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);

            if (field == null)
            {
                throw new InvalidOperationException("Expected descriptor field to exist.");
            }

            var descriptors = field.GetValue(view) as List<EditablePropertyDescriptor>;
            Assert.NotNull(descriptors);

            if (descriptors == null)
            {
                throw new InvalidOperationException("Expected descriptor list to be initialised.");
            }

            Assert.Single(descriptors);

            var descriptor = descriptors[0];
            Assert.Equal("Resolved name", descriptor.Documentation.DisplayName);
            Assert.Equal("Resolved description", descriptor.Documentation.DisplayDescription);
            Assert.Equal("Resolved summary", descriptor.Documentation.XmlSummary);
            Assert.IsType<StubEditor>(descriptor.EditorOverride);
            Assert.IsType<StubFormatter>(descriptor.FormatterOverride);
        }

        private sealed class SampleDependency
        {
        }

        private sealed class SampleModel
        {
            /// <summary>
            /// Filename for the <see cref="SampleDependency"/> implementation.
            /// </summary>
            [Display(Name = "Adapter name", Description = "Adapter path description")]
            public string Adapter { get; set; } = string.Empty;
        }

        private sealed class ResolverModel
        {
            [PropertyMetadataResolver(typeof(SampleResolver))]
            public string Value { get; set; } = string.Empty;
        }

        private sealed class SampleResolver : IPropertyMetadataResolver
        {
            public ResolvedPropertyMetadata Resolve(PropertyInfo property)
            {
                var documentation = new PropertyDocumentation(
                    property,
                    "Resolved name",
                    "Resolved description",
                    new Func<string>(() => "Resolved summary"));

                return new ResolvedPropertyMetadata(
                    documentation,
                    new StubEditor(),
                    new StubFormatter());
            }
        }

        private sealed class StubEditor : IPropertyEditor
        {
            public bool TryEdit(PropertyEditContext context, out object value)
            {
                value = string.Empty;
                return false;
            }
        }

        private sealed class StubFormatter : IPropertyValueFormatter
        {
            public object Format(PropertyEditContext context, object value)
            {
                return value;
            }
        }
    }
}
