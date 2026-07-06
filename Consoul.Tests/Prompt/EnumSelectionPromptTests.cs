using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests enum selection prompt helpers.
    /// </summary>
    public class EnumSelectionPromptTests
    {
        /// <summary>
        /// Enum selection returns the selected enum value and uses metadata-backed labels.
        /// </summary>
        [Fact]
        public void SelectEnum_ReturnsSelectedEnumValue()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    SelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.SelectEnum<DeploymentEnvironment>("Pick environment");

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal(DeploymentEnvironment.StagingSlot, result.SelectedItem);
                    VisualAssert.WroteText(scope.Driver, "Local dev");
                    VisualAssert.WroteText(scope.Driver, "Staging slot");
                    VisualAssert.WroteText(scope.Driver, "Production");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum selection accepts the configured default when the user presses Enter.
        /// </summary>
        [Fact]
        public void SelectEnum_EmptyInputUsesDefaultValue()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    SelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.SelectEnum<DeploymentEnvironment>(
                            "Pick environment",
                            defaultValue: DeploymentEnvironment.Production);

                    Assert.True(result.HasSelection);
                    Assert.Equal(DeploymentEnvironment.Production, result.SelectedItem);
                    VisualAssert.WroteText(scope.Driver, "(default)");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum selection can present a filtered set with custom labels.
        /// </summary>
        [Fact]
        public void SelectEnum_FilteredValuesAndCustomLabels_RenderExpectedOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var values = new[]
                    {
                        DeploymentEnvironment.Development,
                        DeploymentEnvironment.Production
                    };

                    SelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.SelectEnum(
                            "Pick environment",
                            values,
                            value => "ENV: " + value);

                    Assert.True(result.HasSelection);
                    Assert.Equal(DeploymentEnvironment.Production, result.SelectedItem);
                    VisualAssert.WroteText(scope.Driver, "ENV: Development");
                    VisualAssert.WroteText(scope.Driver, "ENV: Production");
                    Assert.DoesNotContain("Staging slot", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum selection returns cancellation instead of a default enum value for cancellation phrases.
        /// </summary>
        [Fact]
        public void SelectEnum_CancellationPhraseReturnsCanceled()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "exit" }));

                try
                {
                    SelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.SelectEnum<DeploymentEnvironment>("Pick environment");

                    Assert.True(result.IsCanceled);
                    Assert.False(result.HasSelection);
                    Assert.Equal(default(DeploymentEnvironment), result.SelectedItem);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum multi-selection returns selected enum values and uses metadata-backed labels.
        /// </summary>
        [Fact]
        public void MultiSelectEnum_ReturnsSelectedEnumValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,3" }));

                try
                {
                    MultiSelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.MultiSelectEnum<DeploymentEnvironment>("Pick environments");

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(
                        new[] { DeploymentEnvironment.Development, DeploymentEnvironment.Production },
                        result.SelectedItems.ToArray());
                    VisualAssert.WroteText(scope.Driver, "Local dev");
                    VisualAssert.WroteText(scope.Driver, "Staging slot");
                    VisualAssert.WroteText(scope.Driver, "Production");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum multi-selection accepts configured defaults when the user presses Enter.
        /// </summary>
        [Fact]
        public void MultiSelectEnum_EmptyInputUsesDefaultValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    MultiSelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.MultiSelectEnum(
                            "Pick environments",
                            Enum.GetValues(typeof(DeploymentEnvironment)).Cast<DeploymentEnvironment>(),
                            defaultValues: new[] { DeploymentEnvironment.Development, DeploymentEnvironment.Production });

                    Assert.True(result.HasSelection);
                    Assert.Equal(
                        new[] { DeploymentEnvironment.Development, DeploymentEnvironment.Production },
                        result.SelectedItems.ToArray());
                    VisualAssert.WroteText(scope.Driver, "[x]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum multi-selection can present filtered values with custom labels.
        /// </summary>
        [Fact]
        public void MultiSelectEnum_FilteredValuesAndCustomLabels_RenderExpectedOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "all" }));

                try
                {
                    var values = new[]
                    {
                        DeploymentEnvironment.Development,
                        DeploymentEnvironment.Production
                    };

                    MultiSelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.MultiSelectEnum(
                            "Pick environments",
                            values,
                            value => "ENV: " + value);

                    Assert.True(result.HasSelection);
                    Assert.Equal(values, result.SelectedItems.ToArray());
                    VisualAssert.WroteText(scope.Driver, "ENV: Development");
                    VisualAssert.WroteText(scope.Driver, "ENV: Production");
                    Assert.DoesNotContain("Staging slot", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Enum multi-selection preserves cancellation state.
        /// </summary>
        [Fact]
        public void MultiSelectEnum_CancellationPhraseReturnsCanceled()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "exit" }));

                try
                {
                    MultiSelectionResult<DeploymentEnvironment> result =
                        ConsoulLibrary.Consoul.MultiSelectEnum<DeploymentEnvironment>("Pick environments");

                    Assert.True(result.IsCanceled);
                    Assert.False(result.HasSelection);
                    Assert.Empty(result.SelectedItems);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        private static void ClearRoutine()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
        }

        private enum DeploymentEnvironment
        {
            [Description("Local dev")]
            Development,

            [Description("Staging slot")]
            StagingSlot,

            Production
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(IEnumerable<string> inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }
    }
}
