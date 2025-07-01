using Commandry.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using System.CommandLine;
using System.IO;

RootCommand rootCommand = new()
{
    Description = "Starts MCP server on `http://localhost:<port>/sse`.",
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

Option<int> portOption = new("--port", () => 3001)
{
    IsRequired = false,
    Description = $"Port to listen on.",
};
rootCommand.AddOption(portOption);

Option<LoggingLevel> logVerbosityOption = new("--log-verbosity", () => LoggingLevel.Info)
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "Log verbosity.",
};
rootCommand.AddOption(logVerbosityOption);


rootCommand.SetHandler(async (scanDirectories, scanModules, port, logVerbosity) =>
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services
        .AddCommandHostMcpServer(scanDirectories, scanModules, logVerbosity)
        .WithHttpTransport();
    var app = builder.Build();

    app.MapMcp();

    await app.RunAsync($"http://localhost:{port}");
}, scanDirectoryOption, moduleNameOption, portOption, logVerbosityOption);

return await rootCommand.InvokeAsync(args);