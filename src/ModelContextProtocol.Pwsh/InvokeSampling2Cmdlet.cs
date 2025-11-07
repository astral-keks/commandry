using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.DependencyInjection;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Sampling2")]
    [OutputType(typeof(ChatResponse))]
    public class InvokeSampling2Cmdlet : PSCmdlet
    {
        [Parameter]
        [ValidateNotNullOrWhiteSpace]
        public string? Text { get; set; }

        [Parameter]
        [ValidateSet(["Assistant", "User"])]
        public Role Role { get; set; } = Role.Assistant;

        [Parameter(ValueFromPipeline = true)]
        public IEnumerable<SamplingMessage> Messages { get; set; } = [];

        private IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();

        protected override void BeginProcessing()
        {
            CreateMessageRequestParams request = new()
            {
                Messages = GetMessages(),
                IncludeContext = ContextInclusion.AllServers,
            };
            CreateMessageResult result = McpServer.SampleAsync(request).GetAwaiter().GetResult();

            WriteObject(result);
        }

        private List<SamplingMessage> GetMessages()
        {
            IEnumerable<SamplingMessage> messages = Messages;
            if (!string.IsNullOrWhiteSpace(Text))
                messages = messages.Append(new()
                {
                    Content = new TextContentBlock { Text = Text },
                    Role = Role
                });

            return [.. messages];
        }
    }
}
