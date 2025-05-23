using Microsoft.CodeAnalysis;
using System.Globalization;

namespace CommandR.Scripting
{
    public class ScriptDiagnostic(Diagnostic diagnostic)
    {
        private readonly Diagnostic _diagnostic = diagnostic;

        public override string ToString() => _diagnostic.GetMessage(CultureInfo.InvariantCulture);
    }
}
