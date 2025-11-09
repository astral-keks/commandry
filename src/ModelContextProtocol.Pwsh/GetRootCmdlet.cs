using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsCommon.Get, "Root")]
    [OutputType(typeof(Root))]
    public class GetRootCmdlet : ModelContextProtocolCmdlet
    {
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
