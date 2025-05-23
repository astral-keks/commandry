using System;
using System.Collections.Generic;

namespace CommandR
{
    public class CommandSchema
    {
        public static readonly CommandSchema Empty = new() { Parameters = [] };

        public required List<ParameterSchema> Parameters { get; init; }
        public class ParameterSchema
        {
            public required string Name { get; set; }
            public required string Description { get; set; }
            public required bool IsOptional { get; set; }
            public required Type Type { get; set; }
        }
    }
}
