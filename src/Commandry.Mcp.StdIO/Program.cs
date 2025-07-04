using Commandry;
using Commandry.Functions;
using Commandry.Hosting;
using Commandry.Mcp;
using Commandry.Scripts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using System.CommandLine;
using System.IO;
using System.Threading;

RootCommand rootCommand = new()
{
    Description = "Starts MCP server with STDIO transport.",
};

Option<DirectoryInfo[]> scanDirectoryOption = new("--scan-directory")
{
    Arity = ArgumentArity.OneOrMore,
    Description = "Directory to scan for PowerShell scripts.",
};
rootCommand.AddOption(scanDirectoryOption);

Option<string[]> moduleNameOption = new("--scan-module")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "PowerShell module to scan for PowerShell functions.",
};
rootCommand.AddOption(moduleNameOption);

Option<LoggingLevel> logVerbosityOption = new("--log-verbosity", () => LoggingLevel.Info)
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "Log verbosity.",
};
rootCommand.AddOption(logVerbosityOption);


rootCommand.SetHandler(async (scanDirectories, scanModules, logVerbosity) =>
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    PwshRunspace pwshRunspace = new(Thread.CurrentThread.GetApartmentState());
    PwshScriptCommandSource pwshScriptCommandSource = new(pwshRunspace);
    PwshFunctionCommandSource pwshFunctionCommandSource = new(pwshRunspace);

    foreach (var scanDirectory in scanDirectories)
    {
        pwshScriptCommandSource.IncludeDirectory(scanDirectory);
        pwshFunctionCommandSource.IncludeModules(scanDirectory);
    }
    foreach (var scanModule in scanModules)
        pwshFunctionCommandSource.IncludeModule(scanModule);

    CommandHost commandHost = new([pwshScriptCommandSource, pwshFunctionCommandSource]);

    builder.Services
        .AddCommandHostMcpServer(commandHost, logVerbosity)
        .WithStdioServerTransport();
    IHost host = builder.Build();

    await host.RunAsync();
}, scanDirectoryOption, moduleNameOption, logVerbosityOption);

await rootCommand.InvokeAsync(args);
