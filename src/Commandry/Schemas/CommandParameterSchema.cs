using System;
using System.Text.Json;

namespace Commandry.Schemas
{
    public class CommandParameterSchema
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsOptional { get; set; }
        public required Type Type { get; set; }

        public virtual object? Deserialize(JsonElement source)
        {
            return source.Deserialize(Type);
        }
    }
}
