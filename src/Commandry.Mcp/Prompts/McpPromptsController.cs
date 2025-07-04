using Commandry.Hosting;
using Commandry.Mcp.Resources;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace Commandry.Mcp.Prompts;

internal class McpPromptsController : IDisposable
{
    private readonly CommandHost _commandHost;
    private readonly ILoggerProvider _loggerProvider;
    private readonly ILogger _logger;

    public McpPromptsController(CommandHost commandHost, ILoggerProvider loggerProvider)
    {
        _commandHost = commandHost;
        _loggerProvider = loggerProvider;
        _logger = _loggerProvider.CreateLogger(nameof(McpResourcesController));
    }

    public void Dispose()
    {
        _loggerProvider.Dispose();
    }

    public async ValueTask<ListPromptsResult> ListPromptsAsync(CancellationToken cancellation)
    {
        ListPromptsResult result = new();

        foreach (var command in _commandHost.GetCommands())
        {
            try
            {
                CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                if (commandMetadata.IsPrompt())
                {
                    Prompt prompt = new()
                    {
                        Name = commandMetadata.GetProperty(nameof(Prompt.Name)) ?? commandMetadata.Name,
                        Description = commandMetadata.Description,
                        Arguments = commandMetadata.Schema.ToPromptArguments(),
                    };

                    result.Prompts.Add(prompt);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while getting prompts");
            }
        }

        return result;
    }

    public async ValueTask<GetPromptResult> GetPromptAsync(GetPromptRequestParams? request, CancellationToken cancellation)
    {
        GetPromptResult result;

        try
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                throw new ArgumentException("Prompt name is missing");

            string commandName = request.Name;
            Command? command = _commandHost.GetCommand(commandName);
            if (command is null)
                throw new ArgumentException($"Prompt {commandName} was not found");

            CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
            command.Parameters = commandMetadata.Schema.Deserialize(request?.Arguments);
            command.Logger = _logger;

            await command.ExecuteAsync(cancellation);

            result = new()
            {
                Messages = [.. command.Inspect().Records
                    .Where(record => record is not null)
                    .Select(record => record.ToMessage())],
                Description = commandMetadata.Description
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while getting prompt");
            throw;
        }

        return result;
    }
}
