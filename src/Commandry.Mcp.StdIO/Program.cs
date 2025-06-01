using Commandry.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.IO;

RootCommand rootCommand = new()
{
    Description = "Starts MCP server with STDIO transport.",
};

Option<DirectoryInfo[]> scanDirectoryOption = new("--scan-directory")
{
    Arity = ArgumentArity.OneOrMore,
    Description = "Directory to scan for PowerShell scripts with non-empty .DESCRIPTION and .ROLE set to 'MCP tool'.",
};
rootCommand.AddOption(scanDirectoryOption);

Option<string[]> moduleNameOption = new("--scan-module")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "PowerShell module to scan for PowerShell functions with non-empty .DESCRIPTION and .ROLE set to 'MCP tool'.",
};
rootCommand.AddOption(moduleNameOption);


rootCommand.SetHandler(async (DirectoryInfo[] scanDirectories, string[] scanModules) =>
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace); // Configure all logs to go to stderr
    builder.Services
        .AddCommandHostMcpServer(scanDirectories, scanModules)
        .WithStdioServerTransport();
    IHost host = builder.Build();

    await host.RunAsync();
}, scanDirectoryOption, moduleNameOption);

await rootCommand.InvokeAsync(args);
