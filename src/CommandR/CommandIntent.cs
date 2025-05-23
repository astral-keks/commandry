using System.Collections.Generic;

namespace CommandR
{
    public record CommandIntent
    {
        public string Command { get; set; } = string.Empty;

        public Dictionary<object, object?> Parameters { get; set; } = [];
    }
}
