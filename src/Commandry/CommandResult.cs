using System;
using System.Collections.Generic;

namespace Commandry
{
    public class CommandResult
    {
        public List<object?> Records { get; init; } = [];

        public Exception? Error { get; init; }
    }
}
