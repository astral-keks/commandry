using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Management.Automation;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Sampling")]
    [OutputType(typeof(CreateMessageResult))]
    public class InvokeSamplingCmdlet : ModelContextProtocolCmdlet
    {
        [Parameter(ParameterSetName = nameof(Text), Mandatory = true)]
        [ValidateNotNullOrWhiteSpace]
        public string? Text { get; set; }
        [Parameter(ParameterSetName = nameof(Text))]
        [ValidateSet([nameof(Role.Assistant), nameof(Role.User)])]
        public Role Role { get; set; } = Role.Assistant;

        [Parameter(ParameterSetName = nameof(Messages), Mandatory = true, ValueFromPipeline = true)]
        public IEnumerable<SamplingMessage> Messages { get; set; } = [];

        [Parameter(ParameterSetName = nameof(Request), Mandatory = true, ValueFromPipeline = true)]
        public CreateMessageRequestParams? Request { get; set; }

        protected override void BeginProcessing()
        {
            CreateMessageRequestParams request;
            if (Request is not null)
                request = Request;
            else if (Messages.Any())
                request = new() { Messages = [.. Messages] };
            else if (Text is not null)
                request = new() { Messages = [new() { Content = new TextContentBlock { Text = Text }, Role = Role }] };
            else
                throw new ArgumentException("Required parameters were not provided.");

            if (request.Messages.Any())
            {
                CreateMessageResult result = McpServer.SampleAsync(request).GetAwaiter().GetResult();
                WriteObject(result);
            }
        }
    }
}
