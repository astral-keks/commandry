using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;
using System.Management.Automation.DependencyInjection;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Elicitation")]
    [OutputType(typeof(ElicitResult))]
    public class InvokeElicitationCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public ElicitRequestParams? Request { get; set; }

        private IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();

        protected override void BeginProcessing()
        {
            if (Request is not null)
            {
                ElicitResult result = McpServer.ElicitAsync(Request).GetAwaiter().GetResult();
                
                WriteObject(result);
            }
        }
    }
}
