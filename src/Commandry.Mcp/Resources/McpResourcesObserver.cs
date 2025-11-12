using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Resources
{
    internal static class McpResourcesObserverExtensions
    {
        public static McpResourcesObserver AsResourceObserver(this IMcpServer server, ILogger logger, CancellationToken cancellation) => 
            new(server, logger, cancellation);
    }

    internal class McpResourcesObserver(IMcpServer server, ILogger logger, CancellationToken cancellation) : IObserver<string>
    {
        public void OnNext(string resourceUri)
        {
            server.SendNotificationAsync(
                "notifications/resources/updated", 
                new { Uri = resourceUri },
                cancellationToken: cancellation);
            logger.LogInformation($"Resource '{resourceUri}' has been updated");
        }

        public void OnCompleted()
        {
            logger.LogInformation($"Completed observing resource");
        }

        public void OnError(Exception error)
        {
            logger.LogError(error, "Error received while observing resource");
        }
    }
}
