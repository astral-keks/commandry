using ModelContextProtocol.Protocol;

namespace Commandry.Mcp;

internal static class McpContentMapper
{
    public static ContentBlock ToContentBlock(this object? source) => source switch
    {
        ContentBlock contentBlock => contentBlock,
        string text => new TextContentBlock { Text = text },
        _ => throw new NotSupportedException($"Unsupported content: {source}"),
    };
}
