using Commandry.Hosting;
using Commandry.Mcp.Instructions;
using Commandry.Mcp.Logging;
using Commandry.Mcp.Prompts;
using Commandry.Mcp.Resources;
using Commandry.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace Commandry.Mcp
{
    public static class McpServiceCollection
    {
        public static IMcpServerBuilder AddCommandHostMcpServer(this IServiceCollection serviceCollection,
            CommandHost commandHost, LoggingLevel logVerbosity) => serviceCollection
            .AddMcpServer(options =>
            {
                McpInstructionsController instructionsController = new(commandHost);
                options.ServerInstructions = instructionsController.GetInstructions();

                options.Capabilities = new()
                {
                    Tools = commandHost.ToToolsCapability(mcpServer =>
                        new McpToolsController(commandHost, mcpServer, mcpServer.AsClientLoggingProvider(logVerbosity))),
                    Prompts = commandHost.ToPromptsCapability(mcpServer =>
                        new McpPromptsController(commandHost, mcpServer.AsClientLoggingProvider(logVerbosity))),
                    Resources = commandHost.ToResourcesCapability(mcpServer =>
                        new McpResourcesController(commandHost, mcpServer, mcpServer.AsClientLoggingProvider(logVerbosity))),
                };
            });
    }
}
