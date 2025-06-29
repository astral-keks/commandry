using Commandry.Hosting;
using System.Text;

namespace Commandry.Mcp.Instructions;

internal class McpInstructionsController
{
    private CommandHost _commandHost;

    public McpInstructionsController(CommandHost commandHost)
    {
        _commandHost = commandHost;
    }

    public string? GetInstructions()
    {
        StringBuilder result = new();

        foreach (var command in _commandHost.GetCommands())
        {
            CommandMetadata commandMetadata = command.Describe();
            if (commandMetadata.IsInstructions())
            {
                command.Execute();

                if (command.Result is not null)
                {
                    foreach (var instruction in command.Result.Records.OfType<string>())
                    {
                        result.AppendLine(instruction);
                    }
                }
            }
        }

        return result.Length > 0 ? result.ToString() : default;
    }
}
