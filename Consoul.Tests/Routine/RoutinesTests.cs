using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.RoutineSetup
{
    /// <summary>
    /// Tests routine setup behavior.
    /// </summary>
    public class RoutinesTests
    {
        /// <summary>
        /// Routine initialization updates the title through the active console driver.
        /// </summary>
        [Fact]
        public void InitializeRoutine_WithNameUpdatesDriverTitle()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.Title = "Original";

                try
                {
                    Routines.InitializeRoutine(new EmptyRoutine(), "Demo");

                    Assert.Equal("Routine: Demo - Original", scope.Driver.Title);
                }
                finally
                {
                    Routines.InitializeRoutine(new EmptyRoutine());
                }
            }
        }

        /// <summary>
        /// Option-text routine inputs select the matching current prompt option instead of depending on stable menu order.
        /// </summary>
        [Fact]
        public void SelectionPrompt_RoutineInputOptionText_SelectsMatchingOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var input = new RoutineInput
                {
                    Value = "Deploy",
                    Method = RoutineInput.InputMethod.OptionText
                };
                Routines.InitializeRoutine(new InlineRoutine(new[] { input }));

                try
                {
                    var prompt = new SelectionPrompt("Pick action");
                    prompt.Add("Preview");
                    prompt.Add("Deploy");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal("Deploy", input.Value);
                    Assert.NotNull(input.OptionReference);
                    Assert.Equal("Deploy", input.OptionReference.Text);
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutineState();
                }
            }
        }

        /// <summary>
        /// Option-text routine inputs can also use registered hidden search terms for resilient scripted demos.
        /// </summary>
        [Fact]
        public void SelectionPrompt_RoutineInputOptionText_MatchesRegisteredSearchTerms()
        {
            using (new ConsoleDriverScope(80))
            {
                var input = new RoutineInput
                {
                    Value = "ship",
                    Method = RoutineInput.InputMethod.OptionText
                };
                Routines.InitializeRoutine(new InlineRoutine(new[] { input }));

                try
                {
                    var prompt = new SelectionPrompt("Pick action");
                    prompt.Add("Preview");
                    int deploy = prompt.Add("Deploy");
                    prompt.AddSearchTerms(deploy, "ship", "release");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.NotNull(input.OptionReference);
                    Assert.Equal("Deploy", input.OptionReference.Text);
                    Assert.Contains("ship", input.OptionReference.SearchTerms);
                }
                finally
                {
                    ClearRoutineState();
                }
            }
        }

        /// <summary>
        /// Routine monitoring captures shortcut-selected option metadata for later XML playback.
        /// </summary>
        [Fact]
        public void SelectionPrompt_MonitorInputs_CapturesShortcutOptionReference()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.UserInputs.Clear();
                Routines.MonitorInputs = true;
                Routines.InitializeRoutine(new InlineRoutine(new[] { new RoutineInput { Value = "d" } }));

                try
                {
                    var prompt = new SelectionPrompt("Pick action");
                    prompt.AddShortcut("Preview", "p");
                    prompt.AddShortcut("Deploy", "d");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    RoutineInput recorded = Assert.Single(Routines.UserInputs);
                    Assert.Equal("d", recorded.Value);
                    Assert.NotNull(recorded.OptionReference);
                    Assert.Equal("Deploy", recorded.OptionReference.Text);
                    Assert.Equal("d", recorded.OptionReference.Shortcut);
                }
                finally
                {
                    ClearRoutineState();
                }
            }
        }

        /// <summary>
        /// Registered option XML preserves the compatibility Choice node and newer option metadata.
        /// </summary>
        [Fact]
        public void RegisteredOption_ToXmlNode_RoundTripsIndexChoiceAndMetadata()
        {
            var document = new XmlDocument();
            var option = new RegisteredOption
            {
                Prompt = "Pick action",
                Text = "Deploy",
                Index = 2,
                Description = "Publishes the release.",
                Shortcut = "d",
                IsDisabled = true,
                DisabledReason = "approval required"
            };
            option.SearchTerms.Add("ship");

            XmlNode node = option.ToXmlNode(document);
            var roundTrip = new RegisteredOption(node);

            Assert.Equal("Pick action", roundTrip.Prompt);
            Assert.Equal("Deploy", roundTrip.Text);
            Assert.Equal(2, roundTrip.Index);
            Assert.Equal("Publishes the release.", roundTrip.Description);
            Assert.Equal("d", roundTrip.Shortcut);
            Assert.True(roundTrip.IsDisabled);
            Assert.Equal("approval required", roundTrip.DisabledReason);
            Assert.Contains("ship", roundTrip.SearchTerms);
            XmlNode choiceNode = Assert.IsType<XmlElement>(node["Choice"]);
            XmlNode indexNode = Assert.IsType<XmlElement>(node["Index"]);
            Assert.Equal("2", choiceNode.InnerText);
            Assert.Equal("2", indexNode.InnerText);
        }

        /// <summary>
        /// Routine input XML can be written before a response is available and reads option references back.
        /// </summary>
        [Fact]
        public void RoutineInput_ToXmlNode_RoundTripsOptionalTimingAndOptionReference()
        {
            var document = new XmlDocument();
            var input = new RoutineInput
            {
                Description = "Select deploy",
                Method = RoutineInput.InputMethod.OptionText,
                OptionReference = new RegisteredOption
                {
                    Prompt = "Pick action",
                    Text = "Deploy",
                    Index = 1
                }
            };

            XmlNode node = input.ToXmlNode(document);
            var roundTrip = new RoutineInput(node);

            Assert.Equal(string.Empty, roundTrip.Value);
            Assert.Equal("Select deploy", roundTrip.Description);
            Assert.Equal(RoutineInput.InputMethod.OptionText, roundTrip.Method);
            RegisteredOption optionReference = Assert.IsType<RegisteredOption>(roundTrip.OptionReference);
            Assert.Equal("Deploy", optionReference.Text);
            XmlNode responseTimeNode = Assert.IsType<XmlElement>(node["ResponseTime"]);
            XmlNode delayNode = Assert.IsType<XmlElement>(node["Delay"]);
            Assert.Equal(string.Empty, responseTimeNode.InnerText);
            Assert.Equal(string.Empty, delayNode.InnerText);
        }

        /// <summary>
        /// Routine input transforms tolerate missing raw values so sparse XML inputs fail softly.
        /// </summary>
        [Fact]
        public void RoutineInput_Value_WithTransformAndNoRawValueReturnsEmptyString()
        {
            var input = new RoutineInput
            {
                Transforms = new[]
                {
                    new InputTransform
                    {
                        Key = "Target",
                        Value = "production"
                    }
                }
            };

            Assert.Equal(string.Empty, input.Value);
        }

        /// <summary>
        /// XML routines can declare document and routine variables for reusable scripted POC demos.
        /// </summary>
        [Fact]
        public void XmlRoutine_Load_ReplacesVariablesFromDocumentAndRoutineMetadata()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n") + ".xml");
            File.WriteAllText(path, @"<?xml version=""1.0"" encoding=""utf-8""?>
<XmlRoutineDoc>
  <Meta>
    <Name>Demo</Name>
    <DateCreated>2026-07-05T00:00:00Z</DateCreated>
    <UseDelays>false</UseDelays>
    <Variables>
      <Variable key=""Environment"" value=""production"" />
      <Variable>
        <Key>Region</Key>
        <Value>east</Value>
      </Variable>
    </Variables>
  </Meta>
  <Routines>
    <Routine name=""Deploy"">
      <Meta>
        <Description><![CDATA[Deploy demo]]></Description>
        <Variables>
          <Service>api</Service>
          <Variable name=""Environment"">staging</Variable>
        </Variables>
      </Meta>
      <Inputs>
        <Input>
          <Value>{{Environment}}/{{Service}}/{{Region}}</Value>
          <RequestTime></RequestTime>
          <ResponseTime></ResponseTime>
          <Method>Value</Method>
          <Description></Description>
        </Input>
      </Inputs>
    </Routine>
  </Routines>
</XmlRoutineDoc>");

            try
            {
                var routine = new XmlRoutine(path, "Deploy");

                Assert.Equal("production", routine.Variables["Environment"]);
                Assert.Equal("east", routine.Variables["Region"]);
                RoutineInput input = Assert.Single(routine);
                Assert.Equal("staging/api/east", input.Value);
                Assert.Contains(input.Transforms, transform => transform.Key == "Environment" && transform.Value == "staging");
                Assert.Contains(input.Transforms, transform => transform.Key == "Service" && transform.Value == "api");
                Assert.Contains(input.Transforms, transform => transform.Key == "Region" && transform.Value == "east");
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                ClearRoutineState();
            }
        }

        /// <summary>
        /// Saved XML routines include reusable variables and named routines so recorded demos can be reloaded directly.
        /// </summary>
        [Fact]
        public void XmlRoutine_SaveInputs_WritesVariablesAndNamedRoutine()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n") + ".xml");
            ClearRoutineState();
            Routines.UserInputs.Push(new RoutineInput { Value = "{{Target}}" });

            try
            {
                var routine = new XmlRoutine
                {
                    Name = "Demo"
                };
                routine.XmlRoutines.Clear();
                routine.XmlRoutines.Add("Deploy", new List<RoutineInput>());
                routine.Variables["Target"] = "production";

                routine.SaveInputs(path);

                var document = new XmlDocument();
                document.Load(path);
                XmlNode xRoutine = Assert.IsType<XmlElement>(document.SelectSingleNode("//Routine"));
                XmlAttribute nameAttribute = Assert.IsType<XmlAttribute>(xRoutine.Attributes?["name"]);
                Assert.Equal("Deploy", nameAttribute.Value);
                Assert.Equal(
                    "production",
                    document.SelectSingleNode("//XmlRoutineDoc/Meta/Variables/Variable[Key='Target']/Value")?.InnerText);

                var reloaded = new XmlRoutine(path, "Deploy");
                RoutineInput input = Assert.Single(reloaded);
                Assert.Equal("production", input.Value);
            }
            finally
            {
                ClearRoutineState();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Missing command-line routine names produce actionable errors instead of placeholder exceptions.
        /// </summary>
        [Fact]
        public void InitializeRoutine_MissingRoutineName_ThrowsActionableError()
        {
            KeyNotFoundException exception = Assert.Throws<KeyNotFoundException>(() =>
                Routines.InitializeRoutine(new[] { "-Routine", "DoesNotExist" }));

            Assert.Contains("DoesNotExist", exception.Message);
            Assert.Contains("Available routines", exception.Message);
        }

        private static void ClearRoutineState()
        {
            Routines.InitializeRoutine(new EmptyRoutine());
            Routines.ClearRegisteredOptions();
            Routines.MonitorInputs = false;
            Routines.UserInputs.Clear();
        }

        private sealed class EmptyRoutine : ConsoulLibrary.Routine
        {
        }

        private sealed class InlineRoutine : ConsoulLibrary.Routine
        {
            public InlineRoutine(IEnumerable<RoutineInput> inputs)
            {
                foreach (RoutineInput input in inputs)
                {
                    Enqueue(input);
                }
            }
        }
    }
}
