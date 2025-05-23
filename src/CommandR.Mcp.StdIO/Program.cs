using CommandR.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.IO;

RootCommand rootCommand = new()
{
    Description = "Starts MCP server with STDIO transport.",
};

Option<DirectoryInfo> scriptDirectoryOption = new("--script-directory")
{
    IsRequired = true,
    Description = "Directory containing PowerShell scripts to be included.",
};
rootCommand.AddOption(scriptDirectoryOption);


rootCommand.SetHandler(async (DirectoryInfo scriptDirectory) =>
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace); // Configure all logs to go to stderr
    builder.Services
        .AddCommandHostMcpServer(scriptDirectory)
        .WithStdioServerTransport();
    IHost host = builder.Build();

    await host.RunAsync();
}, scriptDirectoryOption);

await rootCommand.InvokeAsync(args);
