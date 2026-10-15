using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the result of validating a model for a validation summary.
    /// </summary>
    public sealed class ValidationSummaryResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationSummaryResult"/> class.
        /// </summary>
        /// <param name="issues">The validation issues discovered for the model.</param>
        public ValidationSummaryResult(IEnumerable<ValidationIssue> issues)
        {
            Issues = (issues ?? Enumerable.Empty<ValidationIssue>()).ToArray();
        }

        /// <summary>
        /// Gets a value indicating whether the validated model has no issues.
        /// </summary>
        public bool IsValid => Issues.Count == 0;

        /// <summary>
        /// Gets the validation issues discovered for the model.
        /// </summary>
        public IReadOnlyList<ValidationIssue> Issues { get; }

        /// <summary>
        /// Gets the number of validation issues discovered for the model.
        /// </summary>
        public int Count => Issues.Count;
    }
}
