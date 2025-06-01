using System;
using System.Collections.Generic;

namespace Commandry
{
    public class CommandMetadata
    {
        public required string Name { get; init; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public CommandSchema Schema { get; set; } = CommandSchema.Empty;


        private readonly Dictionary<string, string> _properties = new(StringComparer.OrdinalIgnoreCase);

        public bool HasProperty(string name, string value) => 
            string.Equals(GetProperty(name), value, StringComparison.OrdinalIgnoreCase);

        public string? GetProperty(string name) => 
            _properties.TryGetValue(name, out string? value) ? value : default;
        
        public void SetProperty(string name, string value) =>
            _properties[name] = value;
    }
}
