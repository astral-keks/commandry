using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;
using System.Management.Automation.DependencyInjection;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsCommon.Get, "Root")]
    [OutputType(typeof(Root))]
    public class GetRootCmdlet : PSCmdlet
    {
        private IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();

        protected override void BeginProcessing()
        {
            ListRootsResult result = McpServer.RequestRootsAsync(new()).GetAwaiter().GetResult();

            foreach (var root in result.Roots)
            {
                WriteObject(root);
            }
        }
    }
}
