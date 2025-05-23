using CommandR.Hosting;
using CommandR.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace CommandR.Mcp
{
    public static class McpServiceCollection
    {
        public static IMcpServerBuilder AddCommandHostMcpServer(this IServiceCollection serviceCollection, DirectoryInfo scriptDirectory) =>
            serviceCollection.AddMcpServer(options =>
            {
                PowerShellCommandSource powerShellCommandSource = new(apartmentState: Thread.CurrentThread.GetApartmentState());
                powerShellCommandSource.IncludeDirectory(scriptDirectory);

                CommandHost commandHost = new([powerShellCommandSource]);

                McpToolsController mcpToolsController = new(commandHost);

                options.Capabilities = new()
                {
                    Tools = mcpToolsController.ToToolsCapability()
                };
            });
    }
}
