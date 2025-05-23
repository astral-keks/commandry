using CommandR.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.IO;

RootCommand rootCommand = new()
{
    Description = "Starts MCP server on `http://localhost:<port>/sse`.",
};

Option<DirectoryInfo> scriptDirectoryOption = new("--script-directory")
{
    IsRequired = true,
    Description = "Directory containing PowerShell scripts to be included.",
};
rootCommand.AddOption(scriptDirectoryOption);

Option<int> portOption = new("--port", () => 3001)
{
    IsRequired = false,
    Description = $"Port to listen on.",
};
rootCommand.AddOption(portOption);


rootCommand.SetHandler(async (DirectoryInfo scriptDirectory, int port) =>
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services
        .AddCommandHostMcpServer(scriptDirectory)
        .WithHttpTransport();

    var app = builder.Build();

    app.MapMcp();

    await app.RunAsync($"http://localhost:{port}");
}, scriptDirectoryOption, portOption);

await rootCommand.InvokeAsync(args);


