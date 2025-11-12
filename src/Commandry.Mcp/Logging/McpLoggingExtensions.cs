using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Logging
{
    internal static class McpLoggingExtensions
    {
        public static ILoggerProvider AsClientLoggingProvider(this IMcpServer server, LoggingLevel verbosity) =>
            McpServerExtensions.AsClientLoggerProvider(new McpLoggingServer(server, verbosity));
    }
}
