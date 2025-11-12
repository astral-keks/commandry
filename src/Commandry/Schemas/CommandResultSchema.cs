using System;

namespace Commandry.Schemas
{
    public class CommandResultSchema
    {
        public required string Description { get; set; }
        public required Type Type { get; set; }
    }
}
