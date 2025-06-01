using System.Collections.Generic;

namespace Commandry
{
    public record CommandIntent
    {
        public string Command { get; set; } = string.Empty;

        public Dictionary<object, object?> Parameters { get; set; } = [];
    }
}
