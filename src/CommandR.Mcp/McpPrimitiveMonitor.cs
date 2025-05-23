using ModelContextProtocol.Server;

namespace CommandR.Mcp
{
    public class McpPrimitiveMonitor<T> : McpServerPrimitiveCollection<T>
        where T : IMcpServerPrimitive
    {
        public void NotifyChanged() => RaiseChanged();
    }
}
