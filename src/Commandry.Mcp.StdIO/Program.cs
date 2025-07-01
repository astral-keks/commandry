using Commandry.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using System.CommandLine;
using System.IO;

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
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace); // Configure all logs to go to stderr
    builder.Services
        .AddCommandHostMcpServer(scanDirectories, scanModules, logVerbosity)
        .WithStdioServerTransport();
    IHost host = builder.Build();

    await host.RunAsync();
}, scanDirectoryOption, moduleNameOption, logVerbosityOption);

await rootCommand.InvokeAsync(args);
