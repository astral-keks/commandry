namespace Commandry.Mcp
{
    internal static class McpCommandRoles
    {
        public static bool IsTool(this CommandMetadata commandMetadata) =>
            commandMetadata.HasProperty("Role", "MCP tool");

        public static bool IsPrompt(this CommandMetadata commandMetadata) =>
            commandMetadata.HasProperty("Role", "MCP prompt");

        public static bool IsInstructions(this CommandMetadata commandMetadata) =>
            commandMetadata.HasProperty("Role", "MCP instructions");

        public static bool IsResourceList(this CommandMetadata commandMetadata) =>
            commandMetadata.HasProperty("Role", "MCP resource list");

        public static bool IsResourceTemplateList(this CommandMetadata commandMetadata) =>
            commandMetadata.HasProperty("Role", "MCP resource template list");

        public static bool IsResourceContent(this CommandMetadata commandMetadata, string resourceUri) =>
            commandMetadata.HasProperty("Role", $"MCP resource content") && commandMetadata.MatchesLink(resourceUri);

        public static bool IsResourceSubscription(this CommandMetadata commandMetadata, string resourceUri) =>
            commandMetadata.HasProperty("Role", $"MCP resource subscription") && commandMetadata.MatchesLink(resourceUri);

        private static bool MatchesLink(this CommandMetadata commandMetadata, string resourceUri) =>
            commandMetadata.HasProperty("Link") && resourceUri.StartsWith(commandMetadata.GetProperty("Link") ?? string.Empty);
    }
}
