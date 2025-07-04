using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Elicitation")]
    [OutputType(typeof(ElicitResult))]
    public class InvokeElicitationCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public ElicitRequestParams? Request { get; set; }

        protected override void BeginProcessing()
        {
            if (this.TryGetMcpServer(out IMcpServer? mcp) && Request is not null)
            {
                ElicitResult result = mcp.ElicitAsync(Request).GetAwaiter().GetResult();
                
                WriteObject(result);
            }
        }
    }
}
