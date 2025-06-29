using ModelContextProtocol.Protocol;

namespace Commandry.Mcp.Resources
{
    public static class McpResourcesMapper
    {
        public static Resource ToResource(this object? source) => source switch
        {
            Resource resource => resource,
            string text => new() { Uri = text, Name = text },
            _ => throw new NotSupportedException($"Unsupported resource: {source}")
        };

        public static ResourceTemplate ToResourceTemplate(this object? source) => source switch
        {
            ResourceTemplate resourceTemplate => resourceTemplate,
            string text => new() { UriTemplate = text, Name = text },
            _ => throw new NotSupportedException($"Unsupported resource template: {source}")
        };

        public static ResourceContents ToResourceContents(this object? source, string uri)
        {
            ResourceContents resourceContents = source.ToResourceContents();
            resourceContents.Uri = uri;
            return resourceContents;
        }

        public static ResourceContents ToResourceContents(this object? source) => source switch
        {
            ResourceContents resourceContents => resourceContents,
            string text => new TextResourceContents { Text = text },
            _ => throw new NotSupportedException($"Unsupported resource contents: {source}")
        };
    }
}
