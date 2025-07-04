using Commandry;
using Commandry.Functions;
using Commandry.Hosting;
using Commandry.Mcp;
using Commandry.Scripts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using System.CommandLine;
using System.IO;
using System.Threading;

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
        .WithHttpTransport();

    var app = builder.Build();

    app.MapMcp();

    await app.RunAsync($"http://localhost:{port}");
}, scanDirectoryOption, moduleNameOption, portOption, logVerbosityOption);

return await rootCommand.InvokeAsync(args);