using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace CommandR
{
    public class PowerShellCommandException : Exception
    {
        public required List<ErrorRecord> Errors { get; init; }

        public override string Message => string.Join("\n\n", Errors);
    }
}
