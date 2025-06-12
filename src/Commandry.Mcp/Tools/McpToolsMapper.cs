using AutoMapper;
using Json.Schema;
using Json.Schema.Generation;
using ModelContextProtocol.Protocol.Types;
using System.Collections;
using System.Text.Json;

namespace Commandry.Mcp.Tools;

public class McpToolsMapper
{
    private readonly IMapper _contentMapper;
    private readonly IMapper _resourceMapper = new MapperConfiguration(cfg => { }).CreateMapper();
    private readonly PwshRunspace _runspace;

    public McpToolsMapper(PwshRunspace runspace)
    {
        _contentMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<IDictionary, ResourceContents?>().ConstructUsing(src => ToResource(src));
        }).CreateMapper();
        _resourceMapper = new MapperConfiguration(cfg => { }).CreateMapper();

        _runspace = runspace;
    }

    public Dictionary<object, object?>? ToParameters(IReadOnlyDictionary<string, JsonElement>? arguments, CommandSchema commandSchema)
    {
        using Pwsh pwsh = _runspace.CreatePwsh();
        return pwsh.WithRunspace(() => 
            arguments
                ?.Join(commandSchema.Parameters, arg => arg.Key, par => par.Name, (arg, par) => (Argument: arg, Schema: par))
                .ToDictionary(
                    x => x.Schema.Name as object,
                    x => x.Argument.Value.Deserialize(x.Schema.Type))
        );
    }

    public Content ToContent(IDictionary source)
    {
        Content content = new();

        source = ToDictionary(source);
        _contentMapper.Map(source, content);

        return content;
    }

    public Dictionary<string, object?> ToDictionary(IDictionary source) =>
        source.Cast<DictionaryEntry>().ToDictionary(
            entry => $"{entry.Key}",
            entry => entry.Value is IDictionary value ? ToDictionary(value) : entry.Value);

    public JsonElement ToJsonSchema(CommandSchema commandSchema)
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

    private ResourceContents? ToResource(IDictionary src)
    {
        if (src.Contains(nameof(BlobResourceContents.Blob)))
            return _resourceMapper.Map<BlobResourceContents>(src);
        else if (src.Contains(nameof(TextResourceContents.Text)))
            return _resourceMapper.Map<TextResourceContents>(src);
        return null;
    }

}
