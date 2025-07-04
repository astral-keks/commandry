using Commandry.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Commandry
{
    public class CommandMetadata
    {
        public required string Name { get; init; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public CommandSchema Schema { get; set; } = CommandSchema.Empty;


        private readonly Dictionary<string, List<string>> _properties = new(StringComparer.OrdinalIgnoreCase);

        public bool HasProperty(string name) => 
            _properties.ContainsKey(name);
        
        public bool HasProperty(string name, string value) =>
            GetProperties(name).Any(v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase));

        public string? GetProperty(string name) =>
            GetProperties(name).FirstOrDefault();
        
        public IEnumerable<string> GetProperties(string name) => 
            _properties.TryGetValue(name, out List<string>? values) ? values : [];
        
        public void SetProperty(string name, string value) =>
            _properties[name] = [value];

        public void AddProperty(string name, string value)
        {
            if (!_properties.ContainsKey(name))
                _properties.Add(name, []);
            _properties[name].Add(value);
        }
    }
}
