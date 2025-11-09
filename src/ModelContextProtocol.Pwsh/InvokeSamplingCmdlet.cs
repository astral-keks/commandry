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
    [Cmdlet(VerbsLifecycle.Invoke, "Sampling")]
    [OutputType(typeof(ChatResponse))]
    public class InvokeSamplingCmdlet : PSCmdlet
    {
        [Parameter(ParameterSetName = nameof(Text))]
        [ValidateNotNullOrWhiteSpace]
        public string? Text { get; set; }
        [Parameter(ParameterSetName = nameof(Text))]
        [ValidateSet([nameof(Role.Assistant), nameof(Role.User)])]
        public Role Role { get; set; } = Role.Assistant;

        [Parameter(ParameterSetName = nameof(Messages), ValueFromPipeline = true)]
        public IEnumerable<SamplingMessage> Messages { get; set; } = [];

        [Parameter]
        public ContextInclusion IncludeContext { get; set; }

        [Parameter]
        public string[]? ModelNameHints { get; set; }
        [Parameter]
        public float? ModelCostPriority { get; set; }
        [Parameter]
        public float? ModelIntelligencePriority { get; set; }
        [Parameter]
        public float? ModelSpeedPriority { get; set; }

        [Parameter]
        public int? MaxTokens { get; set; }
        [Parameter]
        public float? Temperature { get; set; }
        [Parameter]
        public string[]? StopSequences { get; set; }
        [Parameter]
        public string? SystemPrompt { get; set; }

        private IMcpServer McpServer => this.GetServiceProvider().GetRequiredService<IMcpServer>();

        protected override void BeginProcessing()
        {
            CreateMessageRequestParams request = new()
            {
                Messages = GetMessages(),
                ModelPreferences = 
                    ModelNameHints is not null || 
                    ModelCostPriority is not null || 
                    ModelIntelligencePriority is not null || 
                    ModelSpeedPriority is not null 
                    ? new()
                    {
                        Hints = ModelNameHints?.Select(nameHint => new ModelHint { Name = nameHint }).ToList(),
                        CostPriority = ModelCostPriority,
                        IntelligencePriority = ModelIntelligencePriority,
                        SpeedPriority = ModelSpeedPriority,
                    }
                    : default,
                Temperature = Temperature,
                MaxTokens = MaxTokens,
                StopSequences = StopSequences,
                SystemPrompt = SystemPrompt,
                IncludeContext = IncludeContext,
            };

            if (request.Messages.Any())
            {
                CreateMessageResult result = McpServer.SampleAsync(request).GetAwaiter().GetResult();

                WriteObject(result);
            }
        }

        private List<SamplingMessage> GetMessages()
        {
            IEnumerable<SamplingMessage> messages = Messages;
            if (!string.IsNullOrWhiteSpace(Text))
            {
                messages = messages.Append(new()
                {
                    Content = new TextContentBlock { Text = Text },
                    Role = Role
                });
            }
            return [.. messages];
        }
    }
}
