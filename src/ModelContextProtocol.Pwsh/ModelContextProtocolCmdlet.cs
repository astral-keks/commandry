using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using System.Management.Automation;
using System.Management.Automation.DependencyInjection;

namespace ModelContextProtocol.Pwsh
{
    public abstract class ModelContextProtocolCmdlet : PSCmdlet
    {
        protected IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();
    }
}
