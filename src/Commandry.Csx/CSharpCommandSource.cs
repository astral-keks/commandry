using Commandry.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Commandry
{
    public class CSharpCommandSource(ScriptBundle bundle) : CommandSource
    {
        private readonly ScriptBundle _bundle = bundle;

        public event EventHandler Reloaded
        {
            add { _bundle.Reloaded += value; }
            remove { _bundle.Reloaded -= value; }
        }

        public override IEnumerable<CommandDescription> DescribeCommands()
        {
            return _bundle.Scripts
                .Select(script => new CSharpCommandAssembly(script.Assembly))
                .SelectMany(assembly => assembly.Commands);
        }
    }
}
