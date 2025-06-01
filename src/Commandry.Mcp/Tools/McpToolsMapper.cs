using AutoMapper;
using ModelContextProtocol.Protocol.Types;
using System.Collections;
using System.Text.Json;
using Commandry.Mcp.Tools;

namespace Commandry.Mcp.Tools;

internal static class McpToolsMapper
{
    private static readonly IMapper _contentMapper = new MapperConfiguration(cfg => 
    {
        cfg.CreateMap<IDictionary, ResourceContents?>().ConstructUsing(src => src.ToResource());
    }).CreateMapper();
    private static readonly IMapper _resourceMapper = new MapperConfiguration(cfg => {}).CreateMapper();

    public static Dictionary<object, object?> ToParameters(this IReadOnlyDictionary<string, JsonElement> arguments, CommandSchema commandSchema)
    {
        return arguments
            .Join(commandSchema.Parameters, arg => arg.Key, par => par.Name, (arg, par) => (Argument: arg, Schema: par))
            .ToDictionary(
                x => x.Schema.Name as object,
                x => x.Argument.Value.Deserialize(x.Schema.Type));
    }

    public static Content ToContent(this IDictionary source)
    {
        Content content = new();

        source = source.ToDictionary();
        _contentMapper.Map(source, content);

        return content;
    }

    public static Dictionary<string, object?> ToDictionary(this IDictionary source) =>
        source.Cast<DictionaryEntry>().ToDictionary(
            entry => $"{entry.Key}",
            entry => entry.Value is IDictionary value ? value.ToDictionary() : entry.Value);

    public static JsonElement ToJsonSchema(this CommandSchema commandSchema)
    {
        // Helper to map .NET types to JSON Schema types
        static string GetJsonSchemaType(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(object)) return "object";
            if (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return "array";
            return "string"; // fallback
        }

        Dictionary<string, object?> schema = new()
        {
            ["type"] = "object",
            ["properties"] = commandSchema.Parameters.ToDictionary(
                parameter => parameter.Name,
                parameter => new Dictionary<string, object?>
                {
                    ["type"] = GetJsonSchemaType(parameter.Type),
                    ["description"] = parameter.Description
                }
            ),
            ["required"] = commandSchema.Parameters
                .Where(parameter => !parameter.IsOptional)
                .Select(parameter => parameter.Name).ToList()
        };

        string json = JsonSerializer.Serialize(schema);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static ResourceContents? ToResource(this IDictionary src)
    {
        if (src.Contains(nameof(BlobResourceContents.Blob)))
            return _resourceMapper.Map<BlobResourceContents>(src);
        else if (src.Contains(nameof(TextResourceContents.Text)))
            return _resourceMapper.Map<TextResourceContents>(src);
        return null;
    }

}
