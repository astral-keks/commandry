using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Tools
{
    public static class McpToolsCapability
    {
        public static ToolsCapability ToToolsCapability(this McpToolsController controller) => new()
        {
            ListChanged = true,
            ToolCollection = controller.ToolMonitor,
            ListToolsHandler = (request, cancellation) =>
            {
                return controller.ListToolsAsync(cancellation);
            },
            CallToolHandler = (request, cancellation) =>
            {
                using ILoggerProvider loggerProvider = request.Server.AsClientLoggerProvider();
                return controller.CallToolAsync(request.Params, loggerProvider.CreateLogger(nameof(McpToolsController)), cancellation);
            },
        };
    }
}
