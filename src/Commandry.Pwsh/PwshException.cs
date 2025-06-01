using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Commandry
{
    public class PwshException : Exception
    {
        public required List<ErrorRecord> Errors { get; init; }

        public override string Message => string.Join("\n\n", Errors);
    }
}
