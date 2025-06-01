using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Commandry.Scripting
{
    internal class ScriptLoadContext() : AssemblyLoadContext(isCollectible: true)
    {
        public Assembly LoadFromCompilation(Compilation compilation)
        {
            using MemoryStream assemblyStream = new();
            var result = compilation.Emit(assemblyStream);
            if (!result.Success)
            {
                List<ScriptDiagnostic> errors = result.Diagnostics
                    .Select(diagnostic => new ScriptDiagnostic(diagnostic))
                    .ToList();
                throw new ScriptLoadException(errors);
            }

            assemblyStream.Seek(0, SeekOrigin.Begin);
            return LoadFromStream(assemblyStream);
        }
    }
}
