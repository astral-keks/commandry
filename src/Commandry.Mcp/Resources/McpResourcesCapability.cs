using Commandry.Hosting;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
namespace Commandry.Mcp.Resources
{
    internal static class McpResourcesCapability
    {
        public static ResourcesCapability ToResourcesCapability(this CommandHost commandHost, Func<IMcpServer, McpResourcesController> resourcesController)
        {
            McpPrimitiveMonitor<McpServerResource> resourcesMonitor = new();
            commandHost.WatchCommands((sender, args) => resourcesMonitor.NotifyChanged());

            return new()
            {
                Subscribe = true,
                ListChanged = true,
                ResourceCollection = resourcesMonitor,
                ListResourcesHandler = (request, cancellation) =>
                {
                    using McpResourcesController controller = resourcesController(request.Server);
                    return controller.ListResourcesAsync(cancellation);
                },
                ListResourceTemplatesHandler = (request, cancellation) =>
                {
                    using McpResourcesController controller = resourcesController(request.Server);
                    return controller.ListResourceTemplatesAsync(cancellation);
                },
                ReadResourceHandler = (request, cancellation) =>
                {
                    using McpResourcesController controller = resourcesController(request.Server);
                    return controller.ReadResourceAsync(request.Params, cancellation);
                },
                SubscribeToResourcesHandler = async (request, cancellation) =>
                {
                    using McpResourcesController controller = resourcesController(request.Server);
                    await controller.SubscribeToResourceAsync(request.Params, cancellation);
                    return new EmptyResult();
                }
            };
        }
    }
}
