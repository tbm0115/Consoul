using System;
using System.ComponentModel.DataAnnotations;
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
    }
}
