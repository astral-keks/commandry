using System;
using System.Collections.Generic;

namespace CommandR.Scripting
{
    public class ScriptLoadException : Exception
    {
        public List<ScriptDiagnostic> Diagnostics { get; }

        public ScriptLoadException(List<ScriptDiagnostic> diagnostics)
        {
            Diagnostics = diagnostics;
        }
    }
}
