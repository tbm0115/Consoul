# Consoul.Cli

`Consoul.Cli` is the optional command-line entry package for Consoul. It adds a small `CommandApp`-style runner for proof-of-concept tools while keeping the core `Consoul` package focused on dependency-light MVC-style console views and renderables.

For **3.0.0-preview.1**, start with the [documentation index](https://github.com/tbm0115/Consoul/blob/develop/docs/index.md), [migration guide](https://github.com/tbm0115/Consoul/blob/v3.0.0-preview.1/docs/migration-3.0.md), and [release notes](https://github.com/tbm0115/Consoul/blob/v3.0.0-preview.1/docs/releases/3.0.0-preview.1.md). Versioned documentation links become available when the release tag is published.

## Installation

After the preview is published to your configured source:

```bash
dotnet add package Consoul.Cli --version 3.0.0-preview.1
```

See the [NuGet guide](https://github.com/tbm0115/Consoul/blob/v3.0.0-preview.1/docs/nuget.md) for local builds and package-source setup. These instructions do not imply that the preview is already published.

Install the core package when you only need views, prompts, renderables, tables, and progress helpers:

```bash
dotnet add package Consoul --version 3.0.0-preview.1
```

## Quick Start

```csharp
using ConsoulLibrary;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.SetApplicationVersion("1.0.0");
    config.SetApplicationDescription("Deployment operations console");

    config.AddCommand("status", context =>
    {
        context.Console.MarkupLine("[green]Ready[/]");
    })
    .WithDescription("Show deployment status")
    .WithAlias("s")
    .WithExample("status");
});

return await app.RunAsync(args);
```

## Typed Commands

Use Spectre-style base classes when porting existing command examples. They still run through Consoul's dependency-light runner and can launch MVC-style views or renderables from `context.Console`:

```csharp
public sealed class StatusCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        context.Console.Write(new Panel("Ready").Header("Status"));
        return 0;
    }
}

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<StatusCommand>("status")
        .WithDescription("Show deployment status")
        .WithAlias("s");
});

return app.Run(args);
```

Typed commands bind `CommandSettings` before `Execute(...)` is called:

```csharp
public sealed class CopySettings : CommandSettings
{
    [CommandArgument(0, "<source>")]
    public string Source { get; set; } = string.Empty;

    [CommandOption("-f|--force")]
    public bool Force { get; set; }
}

public sealed class CopyCommand : Command<CopySettings>
{
    protected override int Execute(CommandContext context, CopySettings settings, CancellationToken cancellationToken)
    {
        context.Console.MarkupLineInterpolated($"Copying [cyan]{settings.Source}[/]");
        return 0;
    }
}
```

Use `AsyncCommand` or `AsyncCommand<TSettings>` for command handlers that await work or need cancellation:

```csharp
public sealed class BuildSettings : CommandSettings
{
    [CommandArgument(0, "<project>")]
    public string Project { get; set; } = string.Empty;
}

public sealed class BuildCommand : AsyncCommand<BuildSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        BuildSettings settings,
        CancellationToken cancellationToken)
    {
        await Task.Delay(250, cancellationToken);
        context.Console.MarkupLineInterpolated($"[green]Built[/] {settings.Project}");
        return 0;
    }
}
```

`ICommand`, `ICommand<TSettings>`, `IAsyncCommand`, and `IAsyncCommand<TSettings>` remain available when an interface fits your codebase better than inheritance. `CommandContext.CancellationToken` is also available to interface commands, delegate commands, and interceptors that need to pass cancellation into helper services:

```csharp
public sealed class StatusCommand : ICommand
{
    public int Execute(CommandContext context)
    {
        statusService.Refresh(context.CancellationToken);
        return 0;
    }
}
```

`CommandContext.Name` and `CommandContext.Remaining` are Spectre-style aliases for `CommandName` and `Arguments`, and `CommandContext.Data` is a per-command bag shared with interceptors. That keeps tiny POC commands and cross-cutting services source-friendly without giving up Consoul's MVC-style rendering model:

```csharp
public sealed class CorrelationInterceptor : CommandInterceptor
{
    public override void Intercept(CommandContext context, CommandSettings settings)
    {
        context.Data["CorrelationId"] = Guid.NewGuid().ToString("n");
    }
}

config.AddCommand("forward", context =>
{
    context.Console.MarkupLineInterpolated($"Running [cyan]{context.Name}[/]");
    context.Console.WriteLine(string.Join(" ", context.Remaining));
    context.Console.WriteLine((string)context.Data["CorrelationId"]);
    return 0;
});
```

For tiny POC commands, bind typed settings directly to a delegate and graduate to a command class when the workflow becomes reusable:

```csharp
app.Configure(config =>
{
    config.AddCommand<RestartSettings>("restart", (context, settings) =>
    {
        context.Console.MarkupLineInterpolated($"Restarting [cyan]{settings.Service}[/]");
        return 0;
    });
});
```

Spectre-style `AddDelegate(...)` and `AddAsyncDelegate(...)` aliases are also available for snippets you want to paste in with minimal edits:

```csharp
app.Configure(config =>
{
    config.AddDelegate<RestartSettings>("restart", (context, settings, cancellationToken) =>
    {
        context.Console.MarkupLineInterpolated($"Restarting [cyan]{settings.Service}[/]");
        return 0;
    });

    config.AddAsyncDelegate<BuildSettings>("build", async (context, settings, cancellationToken) =>
    {
        await buildService.RunAsync(settings.Project, cancellationToken);
        return 0;
    });
});
```

## Compatibility Interfaces

`CommandApp`, `CommandAppConfigurator`, `CommandBranchRegistration`, `CommandRegistration`, and `CommandAppSettings` implement small Spectre-style contracts so configuration helpers can depend on interfaces while still using Consoul's MVC-style command/view runner:

```csharp
static void ConfigureCommands(IConfigurator config)
{
    ICommandAppSettings settings = config.Settings;
    settings.CaseSensitivity = CaseSensitivity.None;

    ICommandConfigurator status = config
        .AddCommand<StatusCommand>("status")
        .WithDescription("Show service status");

    IBranchConfigurator tools = config.AddBranch("tools", branch =>
    {
        branch.AddCommand<InspectCommand>("inspect");
    });

    tools.WithDescription("Operational tools");
}

ICommandApp app = new CommandApp();
app.Configure(ConfigureCommands);
return await app.RunAsync(args);
```

Spectre-style configurator extension aliases are available for common setup snippets:

```csharp
app.Configure(config =>
{
    config
        .And(current => current.SetApplicationName("deploy"))
        .SetApplicationCulture(CultureInfo.InvariantCulture)
        .CaseSensitivity(CaseSensitivity.None)
        .UseStrictParsing()
        .UseAssemblyInformationalVersion();
});
```

`Run(...)` and `RunAsync(...)` accept both `string[]` and `IEnumerable<string>` arguments. `EmptyCommandSettings` is available when a command needs an explicit settings type but no parameters.

Use `ICommandLimiter<TSettings>` when a shared registration helper should only accept commands compatible with a settings type:

```csharp
static CommandRegistration AddOperationalCommand<TSettings, TCommand>(
    IBranchConfigurator branch,
    string name)
    where TSettings : CommandSettings
    where TCommand : class, ICommandLimiter<TSettings>
{
    return branch.AddCommand<TCommand>(name);
}
```

When commands are discovered at runtime, import `ConsoulLibrary.Unsafe` and register command or branch settings types from `Type` objects. Keep this path for plugin/catalog scenarios; normal POC commands should prefer the typed API above:

```csharp
using ConsoulLibrary.Unsafe;

app.Configure(config =>
{
    config.AsUnsafe()
        .SetDefaultCommand(typeof(StatusCommand))
        .WithDescription("Run default status");

    config.AddCommand("list", typeof(ListRemotesCommand))
        .WithDescription("List remotes");

    config.AddBranch("remote", typeof(RemoteSettings), remote =>
    {
        remote.AddCommand("add", typeof(AddRemoteCommand))
            .WithDescription("Add a remote");
    });
});
```

Use a Spectre-style `ITypeRegistrar` bridge when command or MVC-style view classes need constructor dependencies. `Consoul.Cli` does not take a dependency on `Microsoft.Extensions.DependencyInjection`; your app can opt into the container it already uses:

```csharp
using ConsoulLibrary;
using Microsoft.Extensions.DependencyInjection;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar(IServiceCollection services)
    {
        _services = services;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_services.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _services.AddSingleton(service, _ => factory());
    }
}

public sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public object Resolve(Type type)
    {
        return type == null ? null : _provider.GetService(type);
    }
}

var services = new ServiceCollection();
services.AddSingleton<DeploymentService>();

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(config =>
{
    config.AddCommand<DeployCommand>("deploy");
    config.AddView<DashboardView>("dashboard");
});
```

`CommandApp<TDefaultCommand>(registrar)` works for default-command apps too. Consoul registers command/view types, `IAnsiConsole`, and parsed `CommandSettings` instances with the registrar before resolving command instances, which keeps command constructors testable without making DI packages transitive dependencies of the library.

`AddView<TView>(...)` resolves the view through the same activator/registrar path and launches it through `ViewRenderer`, so MVC-style view commands keep breadcrumb chrome, `NavigateTo<TView>()`, `GoBack()`, cancellation, and iterative stack navigation.

When the same container setup is reused across several command apps, put the app-specific registrations in an `ITypeRegistrarFrontend` and pass it alongside the registrar:

```csharp
public sealed class DeploymentRegistrarFrontend : ITypeRegistrarFrontend
{
    public void Configure(ITypeRegistrar registrar)
    {
        registrar.RegisterInstance(typeof(DeploymentService), new DeploymentService());
    }
}

var app = new CommandApp(new TypeRegistrar(services), new DeploymentRegistrarFrontend());
```

The same shape is available from shared configuration helpers through `config.SetTypeRegistrar(registrar, frontend)`.

For very small POCs, `SetCommandActivator(...)` or an explicit command factory can still be the shortest route:

```csharp
var service = new DeploymentService();

app.Configure(config =>
{
    config.SetCommandActivator(type =>
    {
        if (type == typeof(DeployCommand))
        {
            return new DeployCommand(service);
        }

        return Activator.CreateInstance(type)!;
    });

    config.AddCommand<DeployCommand>("deploy");
    config.AddView<DashboardView>("dashboard");
});
```

For one-off commands inside a branch, a local factory is often enough:

```csharp
config.AddBranch("maintenance", maintenance =>
{
    maintenance.AddCommand("prune-cache", () => new PruneCacheCommand(service));
});
```

## Arguments And Options

For command apps that need typed parameters, create a `CommandSettings` class and decorate properties with `CommandArgument` and `CommandOption`:

```csharp
public sealed class CopySettings : CommandSettings
{
    [CommandArgument(0, "<source>")]
    [Description("The source file to copy")]
    public string Source { get; set; } = string.Empty;

    [CommandArgument(1, "[destination]")]
    [Description("The destination path")]
    [DefaultValue(".")]
    public string Destination { get; set; } = ".";

    [CommandOption("-f|--force")]
    [Description("Overwrite existing files")]
    public bool Force { get; set; }

    [CommandOption("-b|--buffer-size <INT>")]
    [Description("Buffer size in KB")]
    [DefaultValue(64)]
    public int BufferSize { get; set; } = 64;
}

public sealed class CopyCommand : ICommand<CopySettings>
{
    public int Execute(CommandContext context, CopySettings settings)
    {
        context.Console.MarkupLineInterpolated($"Copying [cyan]{settings.Source}[/]");
        return 0;
    }
}

app.Configure(config =>
{
    config.AddCommand<CopyCommand>("copy")
        .WithDescription("Copy a file")
        .WithExample("copy", "source.txt", "dest.txt", "--force");
});
```

The built-in binder supports positional arguments, named options, boolean flags, clustered short boolean flags such as `-vf`, enums, repeated array and generic collection options, catch-all array or generic collection arguments, option environment-variable fallbacks, `[Description]`, `[DefaultValue]`, and required options:

```csharp
[CommandOption("--environment <ENVIRONMENT>", isRequired: true, EnvironmentVariable = "DEPLOY_ENVIRONMENT")]
public string Environment { get; set; } = string.Empty;

[CommandOption("--tag <TAG>")]
public List<string> Tags { get; set; } = new List<string>();

[CommandArgument(1, "[arguments]")]
public IReadOnlyList<string> ForwardedArguments { get; set; } = Array.Empty<string>();
```

Collection-shaped properties can use `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, or writable concrete collections with a parameterless constructor. Repeated option values are appended, while collection arguments capture the remaining positional values just like arrays.

Set `EnvironmentVariable` on `CommandOptionAttribute` when a POC command should read a dependency-free ambient default before parsing command-line arguments. Environment values satisfy required options, use the same converters and validation as CLI values, and are cleared when the same repeated option is explicitly supplied on the command line, so `--tag api` does not silently append stale `DEPLOY_TAG` values.

Boolean options with a long alias also accept a generated `--no-*` form after exact aliases are checked. This keeps default-enabled POC settings terse without making you add both aliases yourself:

```csharp
[CommandOption("--cache")]
[DefaultValue(true)]
public bool Cache { get; set; } = true;
```

`tool build --no-cache` binds `Cache` to `false`, while an explicitly declared `[CommandOption("--no-cache")]` property still wins as an exact alias.

Single-character boolean aliases can be clustered for terse local tools. For example, `[CommandOption("-v|--verbose")]` and `[CommandOption("-f|--force")]` can be supplied as `-vf`. Cluster expansion is intentionally conservative: every character must map to a boolean short option, so valued options such as `-b 128` stay explicit and readable.

Enum options parse exact names first, then accept CLI-friendly multi-word spellings that ignore hyphens, underscores, and spaces. For example, `--mode release-candidate` binds to a `ReleaseCandidate` enum member without requiring a custom converter. Common enum metadata labels are accepted too, so `[Description("nightly build")]`, `[Display(Name = "release train")]`, and `[EnumMember(Value = "stable-channel")]` can be entered as natural CLI values.

When several required arguments or options are missing, Consoul reports them together in one parser error so early POC command runs do not turn into one-fix-per-run loops.

Missing option values also include the expected value token from the option template, such as `Missing value for option '--buffer-size'. Expected <INT>.`, so command settings classes remain self-documenting while you iterate on MVC-style console flows.

Invalid value conversion errors use the same command-facing labels, for example `Could not bind value 'large' to option '--buffer-size'. Expected <INT>.`, and custom `TypeConverter` failures keep the converter's domain hint in the parser error. Enum errors also list the accepted CLI spellings, which helps MVC-style command settings remain discoverable while you iterate.

Argument and option templates are validated when settings are described or bound. Use `<name>` for required arguments and option values, `[name]` for optional arguments and option values, and option aliases that start with `-` or `--`. Duplicate positional indexes, duplicate option aliases, required arguments after optional arguments, and catch-all arguments before trailing arguments fail with `CommandTemplateException`, which keeps MVC-style command and view entry points honest without waiting for a malformed POC path to execute.

Use `FlagValue<T>` when an option needs three states: omitted, present without a value, or present with an explicit value. Put the value name in square brackets to mark it optional:

```csharp
public sealed class ListenSettings : CommandSettings
{
    [CommandOption("--port [PORT]")]
    [DefaultValue(3000)]
    public FlagValue<int> Port { get; set; }

    [CommandOption("--timeout [SECONDS]")]
    public FlagValue<int?> Timeout { get; set; }
}

public sealed class ListenCommand : Command<ListenSettings>
{
    protected override int Execute(CommandContext context, ListenSettings settings)
    {
        if (settings.Port.IsSet)
        {
            context.Console.WriteLine("Port: " + settings.Port.Value);
        }

        return 0;
    }
}
```

`tool listen` leaves `Port.IsSet` false, `tool listen --port` sets it to `3000`, and `tool listen --port 8080` sets it to `8080`.

Repeated `KEY=VALUE` options can bind to dictionaries and lookups. Dictionary values use the last value for a repeated key; lookups preserve all values:

```csharp
public sealed class DeploySettings : CommandSettings
{
    [CommandOption("--env <KEY=VALUE>")]
    public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

    [CommandOption("--tag <KEY=VALUE>")]
    public ILookup<string, string> Tags { get; set; } = Enumerable.Empty<KeyValuePair<string, string>>()
        .ToLookup(pair => pair.Key, pair => pair.Value);
}
```

For example, `--env region=us --env region=eu` leaves `Environment["region"]` as `eu`, while `--tag tier=api --tag tier=worker` leaves both values under `Tags["tier"]`.

Use `PairDeconstructor<TKey, TValue>` when a POC tool needs a compact domain-specific pair syntax instead of `KEY=VALUE`. Apply the deconstructor with `TypeConverterAttribute` on the dictionary or lookup property:

```csharp
public sealed class WeightPairDeconstructor : PairDeconstructor<string, int>
{
    public override bool TryDeconstruct(string input, out string key, out int value)
    {
        key = string.Empty;
        value = 0;

        string[] parts = (input ?? string.Empty).Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        key = parts[0];
        return int.TryParse(parts[1], out value);
    }
}

public sealed class DeploySettings : CommandSettings
{
    [CommandOption("--weight <NAME/WEIGHT>")]
    [TypeConverter(typeof(WeightPairDeconstructor))]
    public IDictionary<string, int> Weights { get; set; } = new Dictionary<string, int>();
}
```

If your command settings already look like MVC view models, you can use common DataAnnotations without adding a dependency to `Consoul.Cli`; the binder detects validation attributes by reflection and fails before the command handler runs:

```csharp
public sealed class DeploySettings : CommandSettings
{
    [CommandOption("--environment <ENVIRONMENT>")]
    [Required(ErrorMessage = "Environment is required.")]
    public string Environment { get; set; } = string.Empty;

    [CommandOption("--replicas <COUNT>")]
    [Range(1, 10, ErrorMessage = "Replicas must be between 1 and 10.")]
    public int Replicas { get; set; } = 1;
}
```

Settings that implement `IValidatableObject` can also return model-level failures. Add imperative validation by overriding `Validate()`:

```csharp
public override CommandValidationResult Validate()
{
    return BufferSize <= 0
        ? CommandValidationResult.Error("Buffer size must be positive.")
        : CommandValidationResult.Success();
}
```

When porting Spectre-shaped snippets, import `ConsoulLibrary.Cli` and return the scoped `ValidationResult` alias. It intentionally lives in a sub-namespace so model validation code that uses `System.ComponentModel.DataAnnotations.ValidationResult` stays unambiguous:

```csharp
using ConsoulLibrary.Cli;

public override ValidationResult Validate()
{
    return BufferSize <= 0
        ? ValidationResult.Error("Buffer size must be positive.")
        : ValidationResult.Success();
}
```

Use `ParameterValidationAttribute` when the same rule should apply to many arguments or options:

```csharp
public sealed class PortRangeAttribute : ParameterValidationAttribute
{
    public override ValidationResult Validate(CommandParameterContext context)
    {
        int port = context.Value is int value ? value : 0;
        return port >= 1 && port <= 65535
            ? ValidationResult.Success()
            : ValidationResult.Error($"{context.Parameter.Name} must be between 1 and 65535.");
    }
}

public sealed class ListenSettings : CommandSettings
{
    [CommandOption("--port <PORT>", isRequired: true)]
    [PortRange]
    public int Port { get; set; }
}
```

`CommandParameterContext.Parameter` exposes Spectre-style metadata such as `Name`, `Template`, `Description`, `IsRequired`, `IsOption`, `Property`, and `ValueType`.

Custom value objects can bind through the standard `TypeConverter` pattern:

```csharp
[TypeConverter(typeof(EndpointConverter))]
public sealed class Endpoint
{
    public Endpoint(string host, int port)
    {
        Host = host;
        Port = port;
    }

    public string Host { get; }

    public int Port { get; }
}

public sealed class ConnectSettings : CommandSettings
{
    [CommandOption("--endpoint <ENDPOINT>", isRequired: true)]
    public Endpoint Endpoint { get; set; } = new Endpoint("localhost", 80);
}
```

When you do not own the target type, put `TypeConverterAttribute` on the settings property instead:

```csharp
public sealed class ConnectSettings : CommandSettings
{
    [CommandOption("--endpoint <ENDPOINT>", isRequired: true)]
    [TypeConverter(typeof(EndpointConverter))]
    public Endpoint Endpoint { get; set; } = new Endpoint("localhost", 80);
}
```

Parser behavior is strict by default: unknown option-shaped tokens produce a clean command error with a nearby visible option suggestion when one is available. Use `IgnoreUnknownOptions()` when building a wrapper command that should forward extra flags into a catch-all positional array:

```csharp
app.Configure(config =>
{
    config.IgnoreUnknownOptions();
    config.AddCommand<ForwardCommand>("forward");
});

public sealed class ForwardSettings : CommandSettings
{
    [CommandArgument(0, "<target>")]
    public string Target { get; set; } = string.Empty;

    [CommandArgument(1, "[arguments]")]
    public IReadOnlyList<string> Arguments { get; set; } = Array.Empty<string>();
}
```

Use `IRemainingArguments` when the wrapper also needs to inspect option-shaped tokens while preserving the exact raw arguments for forwarding:

```csharp
public sealed class ForwardSettings : CommandSettings
{
    [CommandArgument(0, "<target>")]
    public string Target { get; set; } = string.Empty;

    [CommandArgument(1, "[arguments]")]
    public IRemainingArguments Arguments { get; set; } = RemainingArguments.Empty;
}

public sealed class ForwardCommand : Command<ForwardSettings>
{
    protected override int Execute(CommandContext context, ForwardSettings settings)
    {
        string[] raw = settings.Arguments.Raw.ToArray();
        string logLevel = settings.Arguments.Parsed["--log-level"].LastOrDefault();
        return processRunner.Run(settings.Target, raw, logLevel);
    }
}
```

`Raw` preserves the captured tokens in order. `Parsed` groups option-shaped tokens such as `--name value`, `--name=value`, and bare flags by option name.

Use `SetStrictParsing(false)` for the same behavior when a boolean setting reads better in shared configuration code. Command and branch lookup is case-insensitive by default; call `SetCaseSensitive()` when a tool intentionally distinguishes command casing.

Spectre-shaped settings aliases are also available for pasted configuration snippets:

```csharp
app.Configure(config =>
{
    config.Settings.CaseSensitivity = CaseSensitivity.None;
    config.Settings.StrictParsing = false;
});
```

## Command Branches

Use branches for git-style command groups:

```csharp
app.Configure(config =>
{
    config.SetApplicationName("deploy");

    config.AddBranch("service", service =>
    {
        service.SetDescription("Manage services");
        service.WithAliases("svc", "services");

        service.AddCommand<ListServicesCommand>("list")
            .WithDescription("List services")
            .WithAliases("ls", "show");

        service.AddCommand<RestartServiceCommand>("restart")
            .WithDescription("Restart a service");
    });
});
```

This creates commands such as:

```bash
deploy service list
deploy svc ls
deploy service restart worker-api
deploy service --help
```

Branches can also have shared settings. Subcommand settings should inherit from the branch settings so shared options bind naturally whether they appear before or after the subcommand:

```csharp
public class ServiceSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Show detailed output")]
    public bool Verbose { get; set; }
}

public sealed class RestartSettings : ServiceSettings
{
    [CommandArgument(0, "<name>")]
    public string Name { get; set; } = string.Empty;
}

app.Configure(config =>
{
    config.AddBranch<ServiceSettings>("service", service =>
    {
        service.AddCommand<RestartServiceCommand>("restart");
    });
});
```

Branch helpers can depend on the typed `IConfigurator<TSettings>` interface when porting shared Spectre-style setup code:

```csharp
static void ConfigureServiceBranch(IConfigurator<ServiceSettings> service)
{
    service.SetDescription("Manage services");
    service.AddCommand<RestartServiceCommand>("restart");
}

app.Configure(config =>
{
    config.AddBranch<ServiceSettings>("service", ConfigureServiceBranch);
});
```

Both invocations bind `Verbose`:

```bash
deploy service --verbose restart worker-api
deploy service restart worker-api --verbose
```

Command help under a branch also shows shared branch options. For untyped handlers, those options appear as `INHERITED OPTIONS`; for typed handlers that inherit the branch settings type, they are folded into the normal `OPTIONS` section without duplication.

## MVC-Style View Commands

Consoul views remain the preferred shape for exploratory screens. Register a view command when a CLI command should open a menu or dashboard:

```csharp
public sealed class DashboardView : StaticView
{
    public DashboardView()
    {
        Title = "Dashboard";
        Options.Add(new ViewOption("Refresh", () => Consoul.MarkupLine("[green]Refreshed[/]")));
    }
}

var app = new CommandApp();
app.Configure(config =>
{
    config.AddView<DashboardView>("dashboard")
        .WithDescription("Open the interactive dashboard")
        .WithAlias("dash");
});

return await app.RunAsync(args);
```

## Help Output

`CommandApp` handles `help`, `--help`, `-h`, command aliases, descriptions, and examples:

```csharp
config.AddCommand<StatusCommand>("status")
    .WithDescription("Show deployment status")
    .WithAliases("s", "stat")
    .WithExamples(
        new[] { "status" },
        new[] { "s" });
```

For quick MVC-style POC tools, class-level `[Description]` attributes seed help text automatically for command classes, command settings used by delegate registrations, branch settings, default commands, and registered views. Fluent calls still win when a registration needs different wording:

```csharp
[Description("Show deployment status")]
public sealed class StatusCommand : Command
{
    protected override int Execute(CommandContext context) => 0;
}

[Description("Open the operations dashboard")]
public sealed class DashboardView : DynamicView
{
}

config.AddCommand<StatusCommand>("status");
config.AddView<DashboardView>("dashboard")
    .WithDescription("Open dashboard");
```

Use `AddExample(...)` or `AddExamples(...)` on the app configurator for application-level examples that should appear in root help:

```csharp
app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.AddExamples(
        new[] { "production" },
        new[] { "staging", "--force" });
});
```

Examples:

```bash
deploy --help
deploy help status
deploy status --help
```

Invalid help paths return a non-zero exit code with the same nearest visible command or branch suggestions as normal command routing. When the path already reached a command but includes extra segments, Consoul reports the full invalid path and renders that command's help for context.

For MVC-style command apps that render branded help as part of the console view layer, install a custom `IHelpProvider`. The default help output remains available when no provider is configured:

```csharp
app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.SetHelpProvider(new HelpProvider(new HelpProviderStyle
    {
        Command = new CommandStyle { Name = "yellow" },
        Option = new OptionStyle { Name = "cyan" },
        Argument = new ArgumentStyle { Template = "magenta" }
    }));
    config.AddCommand<StatusCommand>("status");
});
```

For fully custom help views, implement `IHelpProvider` and render from the read-only command model:

```csharp
app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.SetHelpProvider(new PocHelpProvider());
    config.AddCommand<StatusCommand>("status");
});

public sealed class PocHelpProvider : IHelpProvider
{
    public void WriteApplicationHelp(IAnsiConsole console, CommandAppHelpContext context)
    {
        ICommandModel model = context.Model;
        console.MarkupLineInterpolated($"[bold]{model.ApplicationName}[/]");
        if (context.DefaultCommandInfo != null)
        {
            console.MarkupLineInterpolated($"  [green](default)[/] {context.DefaultCommandInfo.Description}");
        }

        foreach (ICommandInfo command in model.Commands.Where(command => !command.Hidden))
        {
            console.MarkupLineInterpolated($"  [green]{command.Name}[/] {command.Description}");
        }
    }

    public void WriteBranchHelp(IAnsiConsole console, CommandBranchHelpContext context)
    {
        console.MarkupLineInterpolated($"[bold]{context.BranchPath}[/]");
        foreach (ICommandInfo command in context.BranchInfo.Commands.Where(command => !command.Hidden))
        {
            console.MarkupLineInterpolated($"  [green]{command.Name}[/] {command.Description}");
        }
    }

    public void WriteCommandHelp(IAnsiConsole console, CommandHelpContext context)
    {
        console.MarkupLineInterpolated($"[bold]{context.CommandPath}[/]");
        console.WriteLine(context.CommandInfo.Description);
        foreach (ICommandOption option in context.Options.Where(option => !option.IsHidden))
        {
            console.MarkupLineInterpolated($"  [green]{option.PrimaryName}[/] {option.Description}");
        }
    }
}
```

`CommandAppHelpContext.Model`, `CommandAppHelpContext.DefaultCommandInfo`, `CommandBranchHelpContext.BranchInfo`, and `CommandHelpContext.CommandInfo` expose read-only command model interfaces for help renderers that should not depend on mutable registrations. Use `DefaultCommandInfo` when a branded MVC-style root help view should show the default action's arguments, options, examples, or description.

When `SetApplicationVersion(...)` is configured, root-level `--version`, `-v`, and `version` print the application name and version. A registered command named `version` takes precedence over the built-in word form.

## Completion Metadata

`CommandApp.GetCompletions(...)` returns dependency-free completion metadata from the configured command model. Use it to build shell adapters, REPL hints, or editor integrations without executing commands or taking a dependency on a specific shell:

```csharp
var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("deploy");
    config.SetApplicationVersion("1.0.0");
    config.AddBranch<ServiceSettings>("service", service =>
    {
        service.WithAlias("svc");
        service.AddCommand<RestartServiceCommand>("restart")
            .WithAlias("rs");
    });
});

IReadOnlyList<CommandCompletionItem> items =
    app.GetCompletions(new[] { "svc", string.Empty });

foreach (CommandCompletionItem item in items)
{
    Console.WriteLine($"{item.Kind}: {item.Value} {item.Description}");
}
```

Pass the current token as the final array element. Use a trailing empty string when completing the next token after a command or branch, such as `new[] { "service", "" }`. Suggestions include visible commands, branches, aliases, branch options, command options, built-in help/version tokens, and enum or boolean option values. Hidden commands and hidden options are omitted from suggestions but remain invokable by exact name.

Use `IsHidden()` or `WithHidden()` for internal, experimental, or compatibility commands that should remain invokable but stay out of command listings:

```csharp
config.AddCommand<DiagnosticsCommand>("diagnostics")
    .WithDescription("Run internal diagnostics")
    .IsHidden();

config.AddBranch("internal", branch =>
{
    branch.AddCommand<CachePruneCommand>("prune-cache");
})
.WithDescription("Internal maintenance commands")
.IsHidden();
```

Use `HideOptionDefaultValues()` when defaults make help too noisy, and `TrimTrailingPeriods()` when copying `[Description]` text from model classes that already use sentence punctuation:

```csharp
app.Configure(config =>
{
    config.HideOptionDefaultValues();
    config.TrimTrailingPeriods();
});
```

Use `ValidateExamples()` when examples should be parser-checked before commands run. This validates application examples, command examples, default-command examples, branch paths, inherited branch options, typed setting binding, parameter validation attributes, and `CommandSettings.Validate()` without executing the command handler:

```csharp
app.Configure(config =>
{
    config.ValidateExamples();

    config.AddCommand<CopyCommand>("copy")
        .WithExample("copy", "source.txt", "dest.txt", "--force");
});
```

## Error Handling

By default, `CommandApp` catches parser, validation, activation, and handler exceptions, writes `ERROR: {message}`, and returns exit code `1`. Use `SetExceptionHandler(...)` when a proof-of-concept app needs branded output, MVC-style error views, telemetry, or command-specific exit codes:

```csharp
app.Configure(config =>
{
    config.SetExceptionHandler(context =>
    {
        context.Console.MarkupLineInterpolated($"[red]ERROR:[/] {context.Exception.Message}");
        return 2;
    });

    config.AddCommand<DeployCommand>("deploy");
});
```

Unknown command errors include nearest visible command or branch suggestions, including alias matches, before rendering the relevant help output. Hidden commands and branches stay hidden from suggestions.

Handlers receive the original argument list, active `IAnsiConsole`, exception, and command app settings. Exceptions thrown by typed commands are unwrapped from reflection calls before they reach the handler, so POC hosts see the command exception they actually threw. `PropagateExceptions()` still wins when you want tests or host applications to receive the original exception directly.

Canceled command execution returns exit code `130` by default. Use `CancellationExitCode(...)` when your shell, CI host, or MVC-style command launcher expects a different code:

```csharp
app.Configure(config =>
{
    config.CancellationExitCode(99);
});
```

Command app failures use Spectre-style exception categories so host applications can catch the layer they care about:

```csharp
try
{
    return await app.RunAsync(args);
}
catch (CommandConfigurationException ex)
{
    // Duplicate commands, invalid examples, and other setup mistakes.
    throw;
}
catch (CommandParseException ex)
{
    // Argument parsing and settings binding failures.
    return 64;
}
catch (CommandRuntimeException ex)
{
    // Command activation or command-shape failures.
    return 70;
}
```

`CommandParameterException` remains the concrete settings-binding exception and now derives from `CommandParseException`. `CommandAppException` is the shared base for command-app infrastructure errors.

## Command Interceptors

Use `ICommandInterceptor` for cross-cutting command behavior such as timing, authorization, logging scopes, or exit-code normalization. Interceptors run only for executed commands, after settings bind and validate but before the handler runs:

```csharp
public sealed class TimingInterceptor : ICommandInterceptor
{
    private Stopwatch? _stopwatch;

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        _stopwatch = Stopwatch.StartNew();
        context.Console.MarkupLineInterpolated($"Starting [cyan]{context.CommandName}[/]");
    }

    public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
    {
        _stopwatch?.Stop();
        context.Console.MarkupLineInterpolated($"Finished in [green]{_stopwatch?.ElapsedMilliseconds}ms[/]");
    }
}

app.Configure(config =>
{
    config.SetInterceptor(new TimingInterceptor());
    config.AddCommand<DeployCommand>("deploy");
});
```

For quick MVC-style POC apps, inherit `CommandInterceptor` when you only need one hook, or `CommandInterceptor<TSettings>` when the cross-cutting behavior belongs to a specific command model:

```csharp
public sealed class DeployGuard : CommandInterceptor<DeploySettings>
{
    public override void Intercept(CommandContext context, DeploySettings settings)
    {
        if (settings.Environment == "production" && !settings.Force)
        {
            throw new InvalidOperationException("Production deploys require --force.");
        }
    }

    public override void InterceptResult(CommandContext context, DeploySettings settings, ref int result)
    {
        context.Console.MarkupLineInterpolated($"Deploy finished with [yellow]{result}[/]");
    }
}

app.Configure(config =>
{
    config.SetInterceptor(new DeployGuard());
    config.AddCommand<DeployCommand>("deploy");
});
```

## Default Command

Use `SetDefaultCommand<TCommand>()` when a POC tool should behave like a single command unless the user chooses a named command. Registered commands and branches still take precedence; otherwise the default command receives the full argument list and binds settings normally:

```csharp
var app = new CommandApp();
app.Configure(config =>
{
    config.SetDefaultCommand<CopyCommand>()
        .WithExample("source.txt", "dest.txt", "--force");
    config.AddCommand<DeployCommand>("deploy");
});

return await app.RunAsync(args);
```

Factories work here too:

```csharp
config.SetDefaultCommand(() => new StatusCommand(service));
```

`CommandApp<TDefaultCommand>` remains available for tiny apps where the default handler is the whole command surface:

```csharp
var app = new CommandApp<CopyCommand>();
return await app.RunAsync(args);
```

Root `--help` includes the default command's usage, arguments, options, description, and default examples, so single-command POC tools can still document their actual parameter model without inventing a command name. The built-in styled `HelpProvider` renders that same default-action detail, and custom providers receive it through `CommandAppHelpContext.DefaultCommandInfo`.

## Package Scope

This package depends on `Consoul` and adds no external command framework dependency. It focuses on POC-friendly command apps that can launch MVC-style console views, with settings binding for arguments, options, branch hierarchies, reusable validation, and custom converters.
