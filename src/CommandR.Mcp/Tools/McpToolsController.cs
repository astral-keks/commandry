using CommandR.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.Collections;

namespace CommandR.Mcp.Tools
{
    public class McpToolsController
    {
        private readonly CommandHost _commandHost;
        private readonly McpPrimitiveMonitor<McpServerTool> _toolMonitor = new();

        public McpToolsController(CommandHost commandHost)
        {
            _commandHost = commandHost;
            _commandHost.WatchCommands((sender, args) => _toolMonitor.NotifyChanged());
        }

        public McpPrimitiveMonitor<McpServerTool> ToolMonitor => _toolMonitor;

        public async ValueTask<ListToolsResult> ListToolsAsync(CancellationToken cancellation)
        {
            ListToolsResult result = new();

            foreach (var command in _commandHost.GetCommands())
            {
                CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                if (commandMetadata.HasProperty("Role", "MCP tool"))
                {
                    Tool tool = new()
                    {
                        Name = commandMetadata.Name,
                        Description = commandMetadata.Description,
                        InputSchema = commandMetadata.Schema.ToJsonSchema(),
                        Annotations = new()
                        {
                            Title = commandMetadata.Title ?? commandMetadata.Name,
                            IdempotentHint = commandMetadata.HasProperty(nameof(ToolAnnotations.IdempotentHint), bool.TrueString),
                            DestructiveHint = commandMetadata.HasProperty(nameof(ToolAnnotations.DestructiveHint), bool.TrueString),
                            OpenWorldHint = commandMetadata.HasProperty(nameof(ToolAnnotations.OpenWorldHint), bool.TrueString),
                            ReadOnlyHint = commandMetadata.HasProperty(nameof(ToolAnnotations.ReadOnlyHint), bool.TrueString)
                        }
                    };

                    result.Tools.Add(tool);
                }
            }

            return result;
        }

        public async ValueTask<CallToolResponse> CallToolAsync(CallToolRequestParams? request, ILogger logger, CancellationToken cancellation)
        {
            CallToolResponse result;

            try
            {
                if (string.IsNullOrWhiteSpace(request?.Name))
                    throw new ArgumentException("Tool name is missing");

                string commandName = request.Name;
                Command? command = _commandHost.GetCommand(commandName);
                if (command is null)
                    throw new ArgumentException($"Tool {commandName} was not found");

                CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                command.Parameters = request?.Arguments?.ToParameters(commandMetadata.Schema) ?? [];
                command.Logger = logger;

                await command.ExecuteAsync(cancellation);

                CommandResult? commandResult = command.Result;
                if (commandResult?.Error is not null)
                    throw commandResult.Error;

                IEnumerable<Content>? results = commandResult?.Records.Select(record =>
                {
                    Content content = new();

                    if (record is IDictionary dictionary)
                        content = dictionary.ToContent();
                    else
                    {
                        content.Text = record?.ToString();
                        content.Type = "text";
                    }

                    return content;
                });

                result = new()
                {
                    Content = results is not null ? [.. results] : [],
                    IsError = commandResult?.Error is not null
                };
            }
            catch (Exception e)
            {
                result = new()
                {
                    Content = [new() { Text = $"Error: {e.Message}", Type = "text" }]
                };
            }

            return result;
        }
    }
}
