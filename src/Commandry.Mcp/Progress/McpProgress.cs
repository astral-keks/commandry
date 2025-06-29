using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Progress
{
    internal class McpProgress(IMcpServer mcpServer, ProgressToken? progressToken, CancellationToken cancellationToken) : CommandProgress
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
