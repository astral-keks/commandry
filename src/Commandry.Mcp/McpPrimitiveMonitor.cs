using ModelContextProtocol.Server;

namespace Commandry.Mcp
{
    public class McpPrimitiveMonitor<T> : McpServerPrimitiveCollection<T>
        where T : IMcpServerPrimitive
    {
        public void NotifyChanged() => RaiseChanged();
    }
}
