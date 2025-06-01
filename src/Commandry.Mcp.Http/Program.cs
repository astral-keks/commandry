using Commandry.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.IO;

RootCommand rootCommand = new()
{
    Description = "Starts MCP server on `http://localhost:<port>/sse`.",
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

Option<int> portOption = new("--port", () => 3001)
{
    IsRequired = false,
    Description = $"Port to listen on.",
};
rootCommand.AddOption(portOption);


rootCommand.SetHandler(async (DirectoryInfo[] scanDirectories, string[] scanModules, int port) =>
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services
        .AddCommandHostMcpServer(scanDirectories, scanModules)
        .WithHttpTransport();

    var app = builder.Build();

    app.MapMcp();

    await app.RunAsync($"http://localhost:{port}");
}, scanDirectoryOption, moduleNameOption, portOption);

await rootCommand.InvokeAsync(args);


