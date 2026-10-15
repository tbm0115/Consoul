using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using ConsoulLibrary;

// Keep the recipes identical to the gallery: --check verifies the source blocks.
string docs = Path.GetFullPath(args.FirstOrDefault(arg => arg != "--check") ?? "docs");
bool check = args.Contains("--check");
string gallery = File.ReadAllText(Path.Combine(docs, "examples.md"));
string source = File.ReadAllText(Path.Combine(docs, "..", "scripts", "DocsExamples", "Program.cs"));
foreach (string recipe in new[] { "Table", "Panel", "Progress", "Menu", "Interpolation" })
{
    string block = Regex.Match(source, $@"// BEGIN {recipe}\r?\n(.*?)\s*// END {recipe}", RegexOptions.Singleline).Groups[1].Value;
    string normalized = Regex.Replace(block, @"\s+", " ").Trim();
    if (normalized.Length == 0 || !Regex.Replace(gallery, @"\s+", " ").Contains(normalized))
        throw new InvalidOperationException($"The {recipe} recipe differs from docs/examples.md.");
}

Directory.CreateDirectory(Path.Combine(docs, "assets", "screenshots"));
Save("table", "Service table", Consoul.Capture(TableExample, width: 72));
Save("panel", "Build summary panel", Consoul.Capture(PanelExample, width: 72));
Save("progress", "Completed build and check tasks", Consoul.Capture(ProgressExample, width: 72));
Save("overview", "Consoul service dashboard", Consoul.Capture(() =>
{
    Consoul.MarkupLine("[cyan]Service dashboard[/]");
    TableExample();
    Consoul.Write("");
    PanelExample();
}, width: 72));
Console.WriteLine(check ? "Documentation recipes and output previews verified." : "Generated four output previews from compiled documentation recipes.");

void Save(string name, string title, string output)
{
    string[] lines = output.Replace("\r", "").TrimEnd('\n').Split('\n');
    int height = 82 + lines.Length * 24;
    var svg = new StringBuilder();
    svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"760\" height=\"{height}\" viewBox=\"0 0 760 {height}\" role=\"img\" aria-labelledby=\"title desc\">");
    svg.AppendLine($"<title id=\"title\">{Escape(title)}</title><desc id=\"desc\">Plain-text output captured from Consoul. {Escape(string.Join(" ", lines).Trim())}</desc>");
    svg.AppendLine($"<rect width=\"760\" height=\"{height}\" rx=\"10\" fill=\"#102238\"/>");
    svg.AppendLine("<circle cx=\"24\" cy=\"24\" r=\"5\" fill=\"#f48b62\"/><circle cx=\"42\" cy=\"24\" r=\"5\" fill=\"#e8c56a\"/><circle cx=\"60\" cy=\"24\" r=\"5\" fill=\"#86cba6\"/>");
    svg.AppendLine($"<text x=\"88\" y=\"29\" font-family=\"Consolas,monospace\" font-size=\"12\" fill=\"#a9bed1\">{Escape(title)} · Consoul</text>");
    svg.AppendLine("<g font-family=\"Consolas,'Liberation Mono',monospace\" font-size=\"16\" fill=\"#e1eaf3\">");
    for (int i = 0; i < lines.Length; i++)
        svg.AppendLine($"<text x=\"24\" y=\"{70 + i * 24}\" style=\"white-space:pre\">{Escape(lines[i])}</text>");
    svg.AppendLine("</g></svg>");
    string path = Path.Combine(docs, "assets", "screenshots", name + ".svg");
    string content = svg.ToString().Replace("\r\n", "\n");
    if (check)
    {
        if (!File.Exists(path) || File.ReadAllText(path).Replace("\r\n", "\n") != content)
            throw new InvalidOperationException($"Output preview is stale: {path}");
    }
    else File.WriteAllText(path, content, new UTF8Encoding(false));
}

static string Escape(string value) => SecurityElement.Escape(value) ?? "";

static void TableExample()
{
    // BEGIN Table
    var table = new Table()
        .RoundedBorder()
        .Width(48)
        .HideFooter()
        .AddColumn("Service")
        .AddColumn("State")
        .AddRow("API", "[green]Healthy[/]")
        .AddRow("Worker", "[yellow]Starting[/]");
    AnsiConsole.Write(table);
    // END Table
}

static void PanelExample()
{
    // BEGIN Panel
    Consoul.Panel(
        "[green]Build complete[/]\n3 projects compiled. Ready to deploy.",
        title: "Build summary",
        width: 48);
    // END Panel
}

static void ProgressExample()
{
    // BEGIN Progress
    Consoul.Progress(progress =>
    {
        var build = progress.AddTask("Build", 3);
        var checks = progress.AddTask("Checks", 2);

        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(150);
            build.Increment(1);
        }
        build.Complete();
        checks.Increment(1);
        Thread.Sleep(150);
        checks.Complete();
    });
    // END Progress
}

// Compile interactive recipes, but do not block the output generator on input.
#pragma warning disable CS8321
static void MenuExample()
{
    // BEGIN Menu
    Consoul.View("Developer tools")
        .Option("Say hello", () =>
        {
            Consoul.Write("Hello from Consoul!");
            Consoul.Wait();
        })
        .DescribeLastOption("Write a message and return to the menu.")
        .ShortcutLastOption("h")
        .Render();
    // END Menu
}

static void InterpolationExample()
{
    // BEGIN Interpolation
    string project = "demo[1]";
    Consoul.MarkupLineInterpolated($"[cyan]Building {project}[/]");
    // END Interpolation
}
