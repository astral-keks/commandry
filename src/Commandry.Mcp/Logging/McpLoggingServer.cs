using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Logging
{
    internal class McpLoggingServer(IMcpServer inner, LoggingLevel? verbosity) : IMcpServer
    {
        public LoggingLevel? LoggingLevel => verbosity;

        #region Trivial
        public ClientCapabilities? ClientCapabilities => inner.ClientCapabilities;

        public Implementation? ClientInfo => inner.ClientInfo;

        public McpServerOptions ServerOptions => inner.ServerOptions;

        public IServiceProvider? Services => inner.Services;

        public string? SessionId => inner.SessionId;

        public ValueTask DisposeAsync()
        {
            return inner.DisposeAsync();
        }

        public IAsyncDisposable RegisterNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
        {
            return inner.RegisterNotificationHandler(method, handler);
        }

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            return inner.RunAsync(cancellationToken);
        }

        public Task SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
        {
            return inner.SendMessageAsync(message, cancellationToken);
        }

        public Task<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default)
        {
            return inner.SendRequestAsync(request, cancellationToken);
        }
        #endregion
    }
}
