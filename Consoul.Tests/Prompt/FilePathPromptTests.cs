using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Prompt
{
    /// <summary>
    /// Tests file path prompt validation and normalization.
    /// </summary>
    public class FilePathPromptTests
    {
        /// <summary>
        /// Existing-file prompts reject missing paths, accept a later valid path, and trim pasted quotes before validating.
        /// </summary>
        [Fact]
        public void PromptForFilepath_CheckExistsRejectsMissingPathAndTrimsQuotedValidPath()
        {
            string directory = Path.Combine(Path.GetTempPath(), "Consoul.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string existingPath = Path.Combine(directory, "script with spaces.ps1");
            File.WriteAllText(existingPath, "Write-Host 'ok'");
            string missingPath = Path.Combine(directory, "missing.ps1");

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    missingPath,
                    "\"" + existingPath + "\""
                }));

                try
                {
                    string result = Consoul.PromptForFilepath("Script path", checkExists: true);

                    Assert.Equal(existingPath, result);
                    Assert.True(scope.Driver.Writes.Count(write => write.Text.Contains("Script path")) >= 2);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        /// <summary>
        /// Permissive file path prompts still accept non-existing paths, while returning the normalized text.
        /// </summary>
        [Fact]
        public void PromptForFilepath_CheckExistsFalseAcceptsMissingQuotedPath()
        {
            string path = Path.Combine(Path.GetTempPath(), "Consoul.Tests", Guid.NewGuid().ToString("N"), "new file.txt");

            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    "  \"" + path + "\"  "
                }));

                try
                {
                    string result = Consoul.PromptForFilepath("New file", checkExists: false);

                    Assert.Equal(path, result);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Default paths are normalized and can be accepted when existence checks are disabled.
        /// </summary>
        [Fact]
        public void PromptForFilepath_DefaultPathAcceptsMissingPathWhenExistsCheckDisabled()
        {
            string path = Path.Combine(Path.GetTempPath(), "Consoul.Tests", Guid.NewGuid().ToString("N"), "new file.txt");

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "y" }));

                try
                {
                    string result = Consoul.PromptForFilepath("\"" + path + "\"", "Replacement path", checkExists: false);

                    Assert.Equal(path, result);
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Replacement path"));
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Result-first file path prompts reject missing files, retry, and return the normalized existing path.
        /// </summary>
        [Fact]
        public void FilePath_ResultFirstPromptRejectsMissingThenReturnsExistingFile()
        {
            string directory = Path.Combine(Path.GetTempPath(), "Consoul.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string existingPath = Path.Combine(directory, "deploy script.ps1");
            File.WriteAllText(existingPath, "Write-Host 'deploy'");
            string missingPath = Path.Combine(directory, "missing.ps1");

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    missingPath,
                    "\"" + existingPath + "\""
                }));

                try
                {
                    TextPromptResult<string> result = Consoul.FilePath("Script path", mustExist: true).Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(existingPath, result.Value);
                    VisualAssert.WroteText(scope.Driver, "File does not exist.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        /// <summary>
        /// Result-first file path prompts can accept a normalized missing path when existence is not required.
        /// </summary>
        [Fact]
        public void FilePath_ResultFirstPromptAcceptsMissingPathWhenNotRequired()
        {
            string path = Path.Combine(Path.GetTempPath(), "Consoul.Tests", Guid.NewGuid().ToString("N"), "new file.txt");

            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "  \"" + path + "\"  " }));

                try
                {
                    TextPromptResult<string> result = Consoul.FilePath("New file").Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(path, result.Value);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Directory path prompts validate against directories instead of accepting existing files.
        /// </summary>
        [Fact]
        public void DirectoryPath_ResultFirstPromptRejectsFileThenReturnsDirectory()
        {
            string directory = Path.Combine(Path.GetTempPath(), "Consoul.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, "settings.json");
            File.WriteAllText(filePath, "{}");

            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    filePath,
                    "\"" + directory + "\""
                }));

                try
                {
                    TextPromptResult<string> result = Consoul.DirectoryPath("Artifact directory", mustExist: true).Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(directory, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Directory does not exist.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        /// <summary>
        /// Path prompts keep the result-first cancellation behavior from typed text prompts.
        /// </summary>
        [Fact]
        public void DirectoryPath_ResultFirstPromptCanBeCanceled()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "back" }));

                try
                {
                    TextPromptResult<string> result = Consoul.DirectoryPath("Artifact directory", mustExist: true).Render();

                    Assert.True(result.IsCanceled);
                    Assert.False(result.HasValue);
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
