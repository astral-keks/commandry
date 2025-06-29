using ModelContextProtocol.Protocol;

namespace Commandry.Mcp.Prompts;

internal static class McpPromptsMapper
{
    public static List<PromptArgument> ToPromptArguments(this CommandSchema schema) => [..
        schema.Parameters.Select(parameter => new PromptArgument
        {
            Name = parameter.Name,
            Description = parameter.Description,
            Required = !parameter.IsOptional,
        })
    ];

    public static PromptMessage ToMessage(this object? source) => source switch
    {
        PromptMessage promptMessage => promptMessage,
        _ => new() { Role = Role.Assistant, Content = source.ToContentBlock() }
    };
}
