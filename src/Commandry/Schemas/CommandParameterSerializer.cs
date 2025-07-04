using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Commandry.Schemas
{
    public class CommandParameterSerializer
    {
        public virtual void Deserialize(IReadOnlyDictionary<string, JsonElement>? source, CommandParameters result, CommandSchema schema)
        {
            if (source is not null)
            {
                foreach (var (parameterSource, parameterSchema) in source
                    .Join(schema.Parameters, src => src.Key, par => par.Name, (src, par) => (Source: src.Value, Schema: par)))
                {
                    object? parameterValue = Deserialize(parameterSource, parameterSchema);
                    result[parameterSchema.Name] = parameterValue;
                }
            }
        }

        public virtual object? Deserialize(JsonElement source, CommandParameterSchema schema)
        {
            return source.Deserialize(schema.Type);
        }
    }
}
