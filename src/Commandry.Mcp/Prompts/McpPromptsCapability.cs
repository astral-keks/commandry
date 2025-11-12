using Commandry.Hosting;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Prompts;

internal static class McpPromptsCapability
{
    public static PromptsCapability ToPromptsCapability(this CommandHost commandHost, Func<IMcpServer, McpPromptsController> promptsController)
    {
        McpPrimitiveMonitor<McpServerPrompt> promptsMonitor = new();
        commandHost.WatchCommands((sender, args) => promptsMonitor.NotifyChanged());

        return new()
        {
            ListChanged = true,
            PromptCollection = promptsMonitor,
            ListPromptsHandler = (request, cancellation) =>
            {
                using McpPromptsController controller = promptsController(request.Server);
                return controller.ListPromptsAsync(cancellation);
            },
            GetPromptHandler = (request, cancellation) =>
            {
                using McpPromptsController controller = promptsController(request.Server);
                return controller.GetPromptAsync(request.Params, cancellation);
            },
        };
    }
}
