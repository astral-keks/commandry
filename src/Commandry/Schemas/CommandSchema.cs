using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Commandry.Schemas
{
    public class CommandSchema
    {
        public static readonly CommandSchema Empty = new() { Parameters = [] };

        public required List<CommandParameterSchema> Parameters { get; init; }

        public virtual CommandParameters Deserialize(IReadOnlyDictionary<string, JsonElement>? source)
        {
            CommandParameters result = [];

            if (source is not null)
            {
                foreach (var (parameterSource, parameterSchema) in source
                    .Join(Parameters, src => src.Key, par => par.Name, (src, par) => (Source: src.Value, Schema: par)))
                {
                    object? parameterValue = parameterSchema.Deserialize(parameterSource);
                    result[parameterSchema.Name] = parameterValue;
                }
            }

            return result;
        }
    }
}
