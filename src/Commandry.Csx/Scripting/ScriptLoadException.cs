using System;
using System.Collections.Generic;

namespace Commandry.Scripting
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
