using Json.Schema;
using Json.Schema.Generation;
using System.Text.Json;

namespace Commandry.Mcp.Tools;

internal static class McpToolsMapper
{
    public static JsonElement ToJsonSchema(this CommandSchema commandSchema)
    {
        JsonSchemaBuilder comandJsonSchemaBuilder = new();
        comandJsonSchemaBuilder.Type(SchemaValueType.Object);

        List<string> requiredParameterNames = [];
        Dictionary<string, JsonSchema> parameterJsonSchemas = [];
        foreach (var parameterSchema in commandSchema.Parameters)
        {
            parameterJsonSchemas[parameterSchema.Name] = new JsonSchemaBuilder()
                .FromType(parameterSchema.Type)
                .Description(parameterSchema.Description)
                .Build();

            if (!parameterSchema.IsOptional)
                requiredParameterNames.Add(parameterSchema.Name);
        }

        JsonSchema commandJsonSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(parameterJsonSchemas)
            .Required(requiredParameterNames)
            .Build();

        return JsonSerializer.SerializeToElement(commandJsonSchema);
    }
}
