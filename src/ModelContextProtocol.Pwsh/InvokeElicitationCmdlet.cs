using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;
using static ModelContextProtocol.Protocol.ElicitRequestParams;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Elicitation")]
    [OutputType(typeof(ElicitResult))]
    public class InvokeElicitationCmdlet : ModelContextProtocolCmdlet
    {
        [Parameter(ParameterSetName = nameof(Message), Mandatory = true)]
        [ValidateNotNull]
        public string? Message { get; set; }
        [Parameter(ParameterSetName = nameof(Message), Mandatory = true)]
        [ValidateNotNull]
        public RequestSchema? RequestedSchema { get; set; }

        [Parameter(ParameterSetName = nameof(Request), Mandatory = true, ValueFromPipeline = true)]
        public ElicitRequestParams? Request { get; set; }

        protected override void BeginProcessing()
        {
            ElicitRequestParams request;
            if (Request is not null)
                request = Request;
            else if (!string.IsNullOrWhiteSpace(Message) && RequestedSchema is not null)
                request = new()
                {
                    Message = Message,
                    RequestedSchema = RequestedSchema
                };
            else
                throw new ArgumentException("Required parameters were not provided.");

            if (!string.IsNullOrEmpty(request.Message))
            {
                ElicitResult result = McpServer.ElicitAsync(request).GetAwaiter().GetResult();
                WriteObject(result);
            }
        }
    }
}
