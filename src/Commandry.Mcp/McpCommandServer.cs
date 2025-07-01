using Commandry.Functions;
using Commandry.Hosting;
using Commandry.Mcp.Instructions;
using Commandry.Mcp.Logging;
using Commandry.Mcp.Prompts;
using Commandry.Mcp.Resources;
using Commandry.Mcp.Tools;
using Commandry.Scripts;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp
{
    public static class McpCommandServer
    {
        public static IMcpServerBuilder AddCommandHostMcpServer(this IServiceCollection serviceCollection,
            IEnumerable<DirectoryInfo> scanDirectories, IEnumerable<string> scanModules, LoggingLevel logVerbosity) => serviceCollection
            .AddMcpServer(options =>
            {
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
                McpParameterSerializer parameterSerializer = new(pwshRunspace);

                McpInstructionsController instructionsController = new(commandHost);
                options.ServerInstructions = instructionsController.GetInstructions();

                options.Capabilities = new()
                {
                    Tools = commandHost.ToToolsCapability(mcpServer => 
                        new McpToolsController(commandHost, parameterSerializer, mcpServer, mcpServer.AsClientLoggingProvider(logVerbosity))),
                    Prompts = commandHost.ToPromptsCapability(mcpServer => 
                        new McpPromptsController(commandHost, parameterSerializer, mcpServer.AsClientLoggingProvider(logVerbosity))),
                    Resources = commandHost.ToResourcesCapability(mcpServer => 
                        new McpResourcesController(commandHost, mcpServer, mcpServer.AsClientLoggingProvider(logVerbosity))),
                };
            });
    }
}
