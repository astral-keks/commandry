using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;
using System.Management.Automation.DependencyInjection;
using System.Text.Json;
using static ModelContextProtocol.Protocol.ElicitRequestParams;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Elicitation")]
    [OutputType(typeof(ElicitResult))]
    public class InvokeElicitationCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public string? Message { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public string? RequestedSchema { get; set; }
        private RequestSchema? GetRequestedSchema() => RequestedSchema is not null ? JsonSerializer.Deserialize<RequestSchema>(RequestedSchema) : default;

        private IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();

        protected override void BeginProcessing()
        {
            ElicitRequestParams request = new()
            {
                Message = Message ?? "",
                RequestedSchema = GetRequestedSchema() ?? new()
            };
            if (!string.IsNullOrEmpty(request.Message))
            {
                ElicitResult result = McpServer.ElicitAsync(request).GetAwaiter().GetResult();
                
                WriteObject(result);
            }
        }
    }
}
