using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style status and list builder compatibility.
    /// </summary>
    public class StatusListBuilderCompatibilityTests
    {
        /// <summary>
        /// Badge aliases render through the existing badge renderer.
        /// </summary>
        [Fact]
        public void Badge_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var badge = new Badge("ready")
                    .Success()
                    .MaxWidth(12)
                    .Padding(1)
                    .Color(ConsoleColor.White, ConsoleColor.DarkGreen);

                AnsiConsole.Write(badge);

                VisualAssert.ContainsText(scope.Driver, "[ READY ]");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[ READY ]") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkGreen);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Callout aliases render through the existing callout renderer.
        /// </summary>
        [Fact]
        public void Callout_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var callout = new Callout("Deployment is waiting for approval.")
                    .Warning()
                    .Title("Deploy")
                    .Width(36)
                    .Padding(1)
                    .TitleColor(ConsoleColor.Yellow)
                    .ContentColor(ConsoleColor.Cyan);

                AnsiConsole.Write(callout);

                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Deployment is waiting");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployment") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Callout line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Badge and callout color helpers accept Spectre-style style strings.
        /// </summary>
        [Fact]
        public void BadgeAndCallout_FluentBuilders_AcceptStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var badge = new Badge("ready")
                    .MaxWidth(12)
                    .Color("white on navy");
                var callout = new Callout("Approval complete.")
                    .Title("Deploy")
                    .Width(34)
                    .BorderColor("grey")
                    .TitleColor("yellow")
                    .ContentColor("#22c55e");

                AnsiConsole.Write(badge);
                AnsiConsole.Write(callout);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("READY") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval complete.") && write.Color == ConsoleColor.DarkCyan);
                Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.Gray && !string.IsNullOrWhiteSpace(write.Text));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumb aliases render through the existing breadcrumb renderer.
        /// </summary>
        [Fact]
        public void Breadcrumb_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var breadcrumb = new Breadcrumb()
                    .Width(32)
                    .Separator(" / ")
                    .ItemColor("darkgray")
                    .CurrentColor("aqua")
                    .AddItem("Home")
                    .AddItem("Services")
                    .AddItem("Deployments");

                AnsiConsole.Write(breadcrumb);

                VisualAssert.ContainsText(scope.Driver, "Home");
                VisualAssert.ContainsText(scope.Driver, "Deployments");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Home") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumb builders can keep per-segment style text in model-backed navigation state.
        /// </summary>
        [Fact]
        public void Breadcrumb_FluentBuilder_AddItemsProjectsStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var trail = new[]
                {
                    new BreadcrumbSegment("Services", "grey"),
                    new BreadcrumbSegment("Deployments", "green")
                };

                var breadcrumb = new Breadcrumb()
                    .Width(40)
                    .Separator(" / ")
                    .CurrentColor("aqua")
                    .AddItem("Home", "white on navy")
                    .AddItems(trail, segment => segment.Label, segment => segment.StyleText);

                AnsiConsole.Write(breadcrumb);

                VisualAssert.ContainsText(scope.Driver, "Home");
                VisualAssert.ContainsText(scope.Driver, "Services");
                VisualAssert.ContainsText(scope.Driver, "Deployments");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Home") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Services") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumb builders can project Spectre-style color values from model-backed navigation state.
        /// </summary>
        [Fact]
        public void Breadcrumb_FluentBuilder_AddItemsProjectsColorValues()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                var trail = new[]
                {
                    new StatusPaletteStep("Services", ChecklistItemState.Complete, "Ready", SpectreColor.Blue, RgbColor.Aqua),
                    new StatusPaletteStep("Deployments", ChecklistItemState.Warning, "Waiting", SpectreColor.Red, RgbColor.Yellow)
                };

                var breadcrumb = new Breadcrumb()
                    .Width(96)
                    .Separator(" / ")
                    .AddItem("Home")
                    .AddItems(trail, segment => "Color " + segment.Label, segment => segment.Color)
                    .AddItems(trail, segment => "Rgb " + segment.Label, segment => segment.RgbColor);

                AnsiConsole.Write(breadcrumb);

                VisualAssert.ContainsText(scope.Driver, "Color Services");
                VisualAssert.ContainsText(scope.Driver, "Rgb Deployments");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Color Services") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Color Deployments") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb Services") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb Deployments") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet list aliases support nested item snippets through the existing bullet list renderer.
        /// </summary>
        [Fact]
        public void BulletList_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var list = new BulletList()
                    .Width(36)
                    .Markers("-", "*")
                    .MarkerColor("yellow")
                    .TextColor("white");

                BulletListItem checks = list.AddItem("Checks");
                checks.AddItem("Build", ConsoleColor.Cyan);
                checks.AddItem("Tests");
                list.AddItem("Deploy");

                AnsiConsole.Write(list);

                VisualAssert.ContainsText(scope.Driver, "Checks");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Bullet list line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet list aliases accept params labels and reusable item collections for quick MVC status notes.
        /// </summary>
        [Fact]
        public void BulletList_FluentBuilder_AcceptsParamsAndReusableItems()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var notes = new[]
                {
                    BulletListItem.Create("Validate", ConsoleColor.Cyan, ConsoleColor.Cyan),
                    BulletListItem.Create("Publish", ConsoleColor.Green, ConsoleColor.Green)
                };

                var list = new BulletList()
                    .Width(40)
                    .Markers("-", "*")
                    .AddItems("Restore", "Build")
                    .AddItems(notes);

                BulletListItem package = list.AddItem("Package");
                package
                    .AddItems("Lint", "Sign")
                    .AddItems(new[]
                    {
                        BulletListItem.Create("Audit", ConsoleColor.Magenta, ConsoleColor.Magenta)
                    });

                list.AddItems(
                    BulletListItem.Create("Notify", ConsoleColor.Yellow, ConsoleColor.Yellow),
                    BulletListItem.Create("Archive"));

                AnsiConsole.Write(list);

                VisualAssert.ContainsText(scope.Driver, "Restore");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Validate");
                VisualAssert.ContainsText(scope.Driver, "Publish");
                VisualAssert.ContainsText(scope.Driver, "Package");
                VisualAssert.ContainsText(scope.Driver, "Lint");
                VisualAssert.ContainsText(scope.Driver, "Sign");
                VisualAssert.ContainsText(scope.Driver, "Audit");
                VisualAssert.ContainsText(scope.Driver, "Notify");
                VisualAssert.ContainsText(scope.Driver, "Archive");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Validate") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Publish") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Audit") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Notify") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Bullet list line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet lists can project model collections for top-level and nested MVC status notes.
        /// </summary>
        [Fact]
        public void BulletList_FluentBuilder_AddItemsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var steps = new[]
                {
                    new StatusStep("Restore", ChecklistItemState.Complete, "Packages restored", ConsoleColor.Green, "green", "green"),
                    new StatusStep("Review", ChecklistItemState.Warning, "Waiting for owner approval", ConsoleColor.Yellow, "white on navy", "yellow")
                };

                var list = new BulletList()
                    .Width(44)
                    .Markers("-", "*");

                list.AddItem("Direct styled", "aqua", "grey");
                list.AddItems(
                    steps,
                    step => step.Label,
                    step => (ConsoleColor?)step.Color,
                    step => (ConsoleColor?)step.Color);
                list.AddItems(
                    steps,
                    step => "Styled " + step.Label,
                    step => step.StyleText,
                    step => step.MarkerStyleText);

                BulletListItem details = list.AddItem("Details", ConsoleColor.White);
                details.AddItem("Nested direct styled", "white on navy", "yellow");
                details.AddItems(
                    steps,
                    step => step.Description,
                    step => new Style(step.Color));
                details.AddItems(
                    steps,
                    step => "Nested " + step.Label,
                    step => step.StyleText,
                    step => step.MarkerStyleText);

                AnsiConsole.Write(list);

                VisualAssert.ContainsText(scope.Driver, "Direct styled");
                VisualAssert.ContainsText(scope.Driver, "Restore");
                VisualAssert.ContainsText(scope.Driver, "Review");
                VisualAssert.ContainsText(scope.Driver, "Styled Restore");
                VisualAssert.ContainsText(scope.Driver, "Styled Review");
                VisualAssert.ContainsText(scope.Driver, "Details");
                VisualAssert.ContainsText(scope.Driver, "Nested direct styled");
                VisualAssert.ContainsText(scope.Driver, "Packages restored");
                VisualAssert.ContainsText(scope.Driver, "Waiting for owner");
                VisualAssert.ContainsText(scope.Driver, "Nested Restore");
                VisualAssert.ContainsText(scope.Driver, "Nested Review");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct styled") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Restore") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Review") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Restore") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Review") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Nested direct styled") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Packages") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Nested Restore") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Nested Review") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected bullet list line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist aliases render state helpers through the existing checklist renderer.
        /// </summary>
        [Fact]
        public void Checklist_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var checklist = new Checklist()
                    .Width(40)
                    .LabelColor("white")
                    .DescriptionColor("darkgray")
                    .AddSuccess("Build", "Artifacts are ready")
                    .AddWarning("Approval", "Waiting for owner")
                    .AddError("Smoke tests", "One check failed");

                AnsiConsole.Write(checklist);

                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Smoke tests");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[x]") && write.Color == RenderOptions.OptionColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[!]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting for owner") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Checklist line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist builders can project model collections without hand-written checklist item loops.
        /// </summary>
        [Fact]
        public void Checklist_FluentBuilder_AddItemsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var steps = new[]
                {
                    new StatusStep("Build", ChecklistItemState.Complete, "Artifacts ready", ConsoleColor.Green, "green", "green", "grey"),
                    new StatusStep("Approval", ChecklistItemState.Warning, "Waiting for owner", ConsoleColor.Yellow, "white on navy", "yellow", "grey")
                };

                var checklist = new Checklist()
                    .Width(44)
                    .ShowDescriptions()
                    .AddItem(
                        "Direct styled",
                        "aqua",
                        "green",
                        "grey",
                        ChecklistItemState.Complete,
                        "Direct description")
                    .AddItems(
                        steps,
                        step => step.Label,
                        step => step.State,
                        step => step.Description)
                    .AddItems(
                        steps,
                        step => "Styled " + step.Label,
                        step => step.StyleText,
                        step => step.MarkerStyleText,
                        step => step.DescriptionStyleText)
                    .AddItems(
                        steps,
                        step => "Styled state " + step.Label,
                        step => step.State,
                        step => "Styled " + step.Description,
                        step => step.StyleText,
                        step => step.MarkerStyleText,
                        step => step.DescriptionStyleText)
                    .AddItems(
                        steps,
                        step => ChecklistItem.Create(
                            step.Label + " owner",
                            step.State,
                            step.Description,
                            labelColor: step.Color,
                            markerColor: step.Color,
                            descriptionColor: ConsoleColor.DarkGray));

                AnsiConsole.Write(checklist);

                VisualAssert.ContainsText(scope.Driver, "Direct styled");
                VisualAssert.ContainsText(scope.Driver, "Direct description");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Artifacts ready");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Styled Build");
                VisualAssert.ContainsText(scope.Driver, "Styled Approval");
                VisualAssert.ContainsText(scope.Driver, "Styled state Build");
                VisualAssert.ContainsText(scope.Driver, "Styled Artifacts ready");
                VisualAssert.ContainsText(scope.Driver, "Build owner");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct styled") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct description") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled state Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled state Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Artifacts ready") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build owner") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval owner") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting for owner") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected checklist line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist builders can project Spectre-style color values from model-backed status rows.
        /// </summary>
        [Fact]
        public void Checklist_FluentBuilder_AddItemsProjectsColorValues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var steps = new[]
                {
                    new StatusPaletteStep("Build", ChecklistItemState.Complete, "Artifacts ready", SpectreColor.Blue, RgbColor.Aqua),
                    new StatusPaletteStep("Approval", ChecklistItemState.Warning, "Waiting for owner", SpectreColor.Red, RgbColor.Yellow)
                };

                var checklist = new Checklist()
                    .Width(52)
                    .ShowDescriptions()
                    .AddItems(
                        steps,
                        step => "Color " + step.Label,
                        step => step.Color,
                        step => step.Color,
                        step => step.Color)
                    .AddItems(
                        steps,
                        step => "Rgb " + step.Label,
                        step => step.State,
                        step => "Rgb " + step.Description,
                        step => step.RgbColor,
                        step => step.RgbColor,
                        step => step.RgbColor);

                AnsiConsole.Write(checklist);

                VisualAssert.ContainsText(scope.Driver, "Color Build");
                VisualAssert.ContainsText(scope.Driver, "Rgb Approval");
                VisualAssert.ContainsText(scope.Driver, "Rgb Waiting for owner");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Color Build") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Color Approval") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb Build") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb Approval") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb Artifacts ready") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb Waiting for owner") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 52, "Color-projected checklist line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class StatusStep
        {
            public StatusStep(string label, ChecklistItemState state, string description, ConsoleColor color, string styleText = "", string markerStyleText = "", string descriptionStyleText = "")
            {
                Label = label;
                State = state;
                Description = description;
                Color = color;
                StyleText = styleText;
                MarkerStyleText = markerStyleText;
                DescriptionStyleText = descriptionStyleText;
            }

            public string Label { get; }

            public ChecklistItemState State { get; }

            public string Description { get; }

            public ConsoleColor Color { get; }

            public string StyleText { get; }

            public string MarkerStyleText { get; }

            public string DescriptionStyleText { get; }
        }

        private sealed class StatusPaletteStep
        {
            public StatusPaletteStep(string label, ChecklistItemState state, string description, SpectreColor color, RgbColor rgbColor)
            {
                Label = label;
                State = state;
                Description = description;
                Color = color;
                RgbColor = rgbColor;
            }

            public string Label { get; }

            public ChecklistItemState State { get; }

            public string Description { get; }

            public SpectreColor Color { get; }

            public RgbColor RgbColor { get; }
        }

        private sealed class BreadcrumbSegment
        {
            public BreadcrumbSegment(string label, string styleText)
            {
                Label = label;
                StyleText = styleText;
            }

            public string Label { get; }

            public string StyleText { get; }
        }

        /// <summary>
        /// Checklist aliases accept reusable item collections for model-backed status views.
        /// </summary>
        [Fact]
        public void Checklist_FluentBuilder_AcceptsReusableItemsAndLabelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var items = new[]
                {
                    ChecklistItem.CreateStyled(
                        "Build",
                        ChecklistItemState.Complete,
                        "Artifacts ready",
                        labelStyle: "aqua",
                        markerStyle: "green",
                        descriptionStyle: "grey"),
                    ChecklistItem.Create("Approval", ChecklistItemState.Warning, "Waiting for owner")
                };

                var checklist = new Checklist(items)
                    .Width(44)
                    .ShowDescriptions()
                    .AddItems(new[] { "Docs", "Notify" }, ChecklistItemState.Skipped)
                    .AddItem(ChecklistItem.Create(
                        "Smoke tests",
                        ChecklistItemState.Error,
                        "One check failed",
                        labelColor: ConsoleColor.Red,
                        markerColor: ConsoleColor.Red,
                        descriptionColor: ConsoleColor.DarkGray));

                AnsiConsole.Write(checklist);

                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Artifacts ready");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Docs");
                VisualAssert.ContainsText(scope.Driver, "Notify");
                VisualAssert.ContainsText(scope.Driver, "Smoke tests");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Artifacts ready") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Smoke tests") && write.Color == ConsoleColor.Red);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Checklist line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
