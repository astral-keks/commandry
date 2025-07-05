using Commandry.Hosting;
using Commandry.Mcp.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Tools
{
    internal class McpToolsController : IDisposable
    {
        private readonly CommandHost _commandHost;
        private readonly IMcpServer _mcpServer;
        private readonly ILoggerProvider _loggerProvider;
        private readonly ILogger _logger;

        public McpToolsController(CommandHost commandHost, IMcpServer mcpServer, ILoggerProvider loggerProvider)
        {
            _commandHost = commandHost;
            _mcpServer = mcpServer;
            _loggerProvider = loggerProvider;
            _logger = _loggerProvider.CreateLogger(nameof(McpResourcesController));
        }

        public void Dispose()
        {
            _loggerProvider.Dispose();
        }

        public async ValueTask<ListToolsResult> ListToolsAsync(CancellationToken cancellation)
        {
            ListToolsResult result = new();

            foreach (var command in _commandHost.GetCommands())
            {
                try
                {
                    CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                    if (commandMetadata.IsTool())
                    {
                        Tool tool = new()
                        {
                            Name = commandMetadata.GetProperty(nameof(Tool.Name)) ?? commandMetadata.Name,
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
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error while getting tools");
                }
            }

            return result;
        }

        public async ValueTask<CallToolResult> CallToolAsync(CallToolRequestParams? request, CancellationToken cancellation)
        {
            CallToolResult result;

            try
            {
                if (string.IsNullOrWhiteSpace(request?.Name))
                    throw new ArgumentException("Tool name is missing");

                string commandName = request.Name;
                Command? command = _commandHost.GetCommand(commandName);
                if (command is null)
                    throw new ArgumentException($"Tool {commandName} was not found");

                CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);

                command.Parameters = commandMetadata.Schema.Deserialize(request.Arguments);

                ServiceCollection services = [];
                services.AddSingleton(_mcpServer);
                command.Services = services.BuildServiceProvider();

                command.Progress = new McpToolsProgress(_mcpServer, request.ProgressToken, cancellation);
                command.Logger = _logger;


                await command.ExecuteAsync(cancellation);

                CommandResult commandResult = command.Inspect();
                result = new()
                {
                    Content = [.. commandResult.Records
                        .Where(record => record is not null)
                        .Select(record => record.ToContentBlock())],
                    IsError = command.Result?.Error is not null
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while calling tool");
                result = new()
                {
                    Content = [new TextContentBlock { Text = $"Error: {e.Message}", Type = "text" }]
                };
            }

            return result;
        }
    }
}
