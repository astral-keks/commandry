using CommandR.Hosting;
using ModelContextProtocol.Protocol.Types;

namespace CommandR.Mcp.Tools
{
    public static class McpToolsCapability
    {
        public static ToolsCapability ToToolsCapability(this McpToolsController controller) => new()
        {
            ListChanged = true,
            ToolCollection = controller.ToolMonitor,
            ListToolsHandler = (request, cancellation) => controller.ListToolsAsync(cancellation),
            CallToolHandler = (request, cancellation) => controller.CallToolAsync(request.Params, cancellation),
        };
    }
}
