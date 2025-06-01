using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Commandry.Scripting
{
    public class ScriptSettings
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Debug;

        public IEnumerable<string> GlobalImports { get; set; } = [];
    }
}
