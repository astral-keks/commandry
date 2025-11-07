using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Commandry.Schemas
{
    public class CommandSchema
    {
        public static readonly CommandSchema Empty = new() { Parameters = [], Results = [] };

        public required List<CommandParameterSchema> Parameters { get; init; }

        public required List<CommandResultSchema> Results { get; init; }

        public virtual CommandParameters DeserializeParameters(IReadOnlyDictionary<string, JsonElement>? parametersJson)
        {
            CommandParameters result = [];

            if (parametersJson is not null)
            {
                foreach (var (parameterSource, parameterSchema) in parametersJson
                    .Join(Parameters, src => src.Key, par => par.Name, (src, par) => (Source: src.Value, Schema: par)))
                {
                    object? parameterValue = parameterSchema.DeserializeValue(parameterSource);
                    result[parameterSchema.Name] = parameterValue;
                }
            }

            return result;
        }

        public virtual bool CanSerializeResult(object? result)
        {
            return result is not null && Results.Any(resultSchema => resultSchema.Type.IsAssignableFrom(result.GetType()));
        }

        public virtual JsonNode? SerializeResult(object? result)
        {
            return CanSerializeResult(result) ? JsonSerializer.SerializeToNode(result) : default;
        }
    }
}
