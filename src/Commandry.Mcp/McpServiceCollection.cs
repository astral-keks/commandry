using Commandry.Functions;
using Commandry.Hosting;
using Commandry.Mcp.Tools;
using Commandry.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace Commandry.Mcp
{
    public static class McpServiceCollection
    {
        public static IMcpServerBuilder AddCommandHostMcpServer(this IServiceCollection serviceCollection, 
            IEnumerable<DirectoryInfo> scanDirectories, IEnumerable<string> scanModules) =>
            serviceCollection.AddMcpServer(options =>
            {
                PwshRunspace pwshRunspace = new(Thread.CurrentThread.GetApartmentState());
                PwshScriptCommandSource pwshScriptCommandSource = new(pwshRunspace);
                PwshFunctionCommandSource pwshFunctionCommandSource = new(pwshRunspace);

                foreach (var scanDirectory in scanDirectories)
                {
                    pwshScriptCommandSource.IncludeDirectory(scanDirectory);
                    pwshFunctionCommandSource.IncludeModules(scanDirectory);
                }
                foreach (var scanModule in scanModules)
                    pwshFunctionCommandSource.IncludeModule(scanModule);

                CommandHost commandHost = new([pwshScriptCommandSource, pwshFunctionCommandSource]);

                McpToolsController mcpToolsController = new(commandHost);

                options.Capabilities = new()
                {
                    Tools = mcpToolsController.ToToolsCapability()
                };
            });
    }
}
