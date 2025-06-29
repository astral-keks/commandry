using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Commandry
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

        private List<ChatMessage> GetMessages()
        {
            IEnumerable<ChatMessage> messages = Messages;
            if (!string.IsNullOrWhiteSpace(Text))
                messages = messages.Append(new(Role, Text));

            return [.. messages];
        }

        protected override void BeginProcessing()
        {
            if (this.TryGetMcpServer(out IMcpServer? mcp))
            {
                List<ChatMessage> messages = GetMessages();

                using IChatClient chat = mcp.AsSamplingChatClient();
                ChatResponse response = chat.GetResponseAsync(messages, Options).GetAwaiter().GetResult();

                WriteObject(response);
            }
        }
    }
}
