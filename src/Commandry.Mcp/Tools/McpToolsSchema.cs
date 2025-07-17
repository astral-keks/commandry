using Commandry.Schemas;
using Json.Schema;
using Json.Schema.Generation;
using System.Text.Json;

namespace Commandry.Mcp.Tools;

internal static class McpToolsSchema
{
    public static JsonElement ToJsonSchema(this IEnumerable<CommandParameterSchema> parameterSchemas)
    {
        List<string> requiredParameterNames = [];
        Dictionary<string, JsonSchema> parameterJsonSchemas = [];
        foreach (var parameterSchema in parameterSchemas)
        {
            parameterJsonSchemas[parameterSchema.Name] = new JsonSchemaBuilder()
                .FromType(parameterSchema.Type)
                .Description(parameterSchema.Description)
                .Build();

            if (!parameterSchema.IsOptional)
                requiredParameterNames.Add(parameterSchema.Name);
        }

        JsonSchema inputJsonSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(parameterJsonSchemas)
            .Required(requiredParameterNames)
            .Build();

        return JsonSerializer.SerializeToElement(inputJsonSchema);
    }

    public static JsonElement ToJsonSchema(this ICollection<CommandResultSchema> resultSchemas)
    {
        if (resultSchemas.Count == 1)
            return resultSchemas.Single().ToJsonSchema();

        List<JsonSchema> resultJsonSchemas = [];
        foreach (var resultSchema in resultSchemas)
        {
            JsonSchema resultJsonSchema = new JsonSchemaBuilder()
                .FromType(resultSchema.Type)
                .Description(resultSchema.Description)
                .Build();
        }

        JsonSchema outputJsonSchema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .OneOf(resultJsonSchemas)
            .Build();

        return JsonSerializer.SerializeToElement(outputJsonSchema);
    }

    private static JsonElement ToJsonSchema(this CommandResultSchema resultSchema)
    {
        JsonSchema outputJsonSchema = new JsonSchemaBuilder()
            .FromType(resultSchema.Type)
            .Description(resultSchema.Description)
            .Build();

        return JsonSerializer.SerializeToElement(outputJsonSchema);
    }

}
