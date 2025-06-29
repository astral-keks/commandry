using Commandry.Hosting;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Tools
{
    internal static class McpToolsCapability
    {
        public static ToolsCapability ToToolsCapability(this CommandHost commandHost, Func<IMcpServer, McpToolsController> toolsController)
        {
            McpPrimitiveMonitor<McpServerTool> toolMonitor = new();
            commandHost.WatchCommands((sender, args) => toolMonitor.NotifyChanged());

            return new()
            {
                ListChanged = true,
                ToolCollection = toolMonitor,
                ListToolsHandler = (request, cancellation) =>
                {
                    using McpToolsController controller = toolsController(request.Server);
                    return controller.ListToolsAsync(cancellation);
                },
                CallToolHandler = (request, cancellation) =>
                {
                    using McpToolsController controller = toolsController(request.Server);
                    return controller.CallToolAsync(request.Params, cancellation);
                },
            };
        }
    }
}
