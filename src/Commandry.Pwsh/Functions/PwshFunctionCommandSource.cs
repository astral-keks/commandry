using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Commandry.Functions
{
    public sealed class PwshFunctionCommandSource : CommandSource
    {
        private readonly PwshRunspace _runspace;
        private readonly HashSet<string> _modules;

        public PwshFunctionCommandSource(PwshRunspace runspace)
        {
            _runspace = runspace;
            _modules = new(StringComparer.OrdinalIgnoreCase);
        }

        public void IncludeModules(DirectoryInfo moduleDirectory)
        {
            foreach (var moduleFile in moduleDirectory.EnumerateFiles("*.psm1", SearchOption.AllDirectories))
            {
                _runspace.ImportModule(moduleFile);
                _modules.Add(moduleFile.FullName);
            }
        }

        public void IncludeModule(string moduleName)
        {
            _runspace.ImportModule(moduleName);
            _modules.Add(moduleName);
        }

        public override IEnumerable<Command> DiscoverCommands()
        {
            using Pwsh pwsh = _runspace.CreatePwsh();

            return pwsh.GetCommands("*", CommandTypes.Function)
                .OfType<FunctionInfo>()
                .Where(function => function.Module is not null)
                .Where(function => _modules.Contains(function.Module.Name) || _modules.Contains(function.Module.Path))
                .Select(function => new PwshFunctionCommand(_runspace, function))
                .ToList();
        }
    }
}
