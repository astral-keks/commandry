using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
    }
}
