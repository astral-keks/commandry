using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsCommon.Get, "Root")]
    [OutputType(typeof(Root))]
    public class GetRootCmdlet : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (this.TryGetMcpServer(out IMcpServer? mcp))
            {
                ListRootsResult result = mcp.RequestRootsAsync(new()).GetAwaiter().GetResult();

                foreach (var root in result.Roots)
                {
                    WriteObject(root);
                }
            }
        }
    }
}
