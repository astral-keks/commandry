using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.DependencyInjection;

namespace ModelContextProtocol.Pwsh
{
    [Cmdlet(VerbsLifecycle.Invoke, "Sampling")]
    [OutputType(typeof(ChatResponse))]
    public class InvokeSamplingCmdlet : PSCmdlet
    {
        [Parameter]
        [ValidateNotNullOrWhiteSpace]
        public string? Text { get; set; }

        [Parameter]
        [ValidateSet(["Assistant", "User"])]
        public ChatRole Role { get; set; } = ChatRole.Assistant;

        [Parameter(ValueFromPipeline = true)]
        public IEnumerable<ChatMessage> Messages { get; set; } = [];

        [Parameter]
        public ChatOptions? Options { get; set; }

        private IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();

        protected override void BeginProcessing()
        {
            List<ChatMessage> messages = GetMessages();

            using IChatClient chat = McpServer.AsSamplingChatClient();
            ChatResponse response = chat.GetResponseAsync(messages, Options).GetAwaiter().GetResult();

            WriteObject(response);
        }

        private List<ChatMessage> GetMessages()
        {
            IEnumerable<ChatMessage> messages = Messages;
            if (!string.IsNullOrWhiteSpace(Text))
                messages = messages.Append(new(Role, Text));

            return [.. messages];
        }
    }
}
