using System;
using System.Collections.Generic;

namespace CommandR
{
    public class CommandResult
    {
        public List<object?> Records { get; init; } = [];

        public Exception? Error { get; init; }
    }
}
