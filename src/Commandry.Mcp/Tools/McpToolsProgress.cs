using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Tools
{
    internal class McpToolsProgress(IMcpServer mcpServer, ProgressToken? progressToken, CancellationToken cancellationToken) : CommandProgress
    {
        public override void Report(float status, string message)
        {
            if (progressToken.HasValue)
            {
                ProgressNotificationValue progress = new()
                {
                    Progress = status,
                    Message = message
                };
                mcpServer.NotifyProgressAsync(progressToken.Value, progress, cancellationToken);
            }
        }
    }
}
