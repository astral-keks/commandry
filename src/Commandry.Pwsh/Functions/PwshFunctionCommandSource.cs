using Commandry.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Commandry.Functions
{
    public sealed class PwshFunctionCommandSource : CommandSource
    {
        private readonly PwshRunspace _runspace;
        private readonly HashSet<string> _watchDirectories;
        private readonly HashSet<string> _moduleDirectories;
        private readonly HashSet<string> _moduleNamesOrPaths;

        public PwshFunctionCommandSource(PwshRunspace runspace)
        {
            _runspace = runspace;
            _watchDirectories = new(StringComparer.OrdinalIgnoreCase);
            _moduleDirectories = new(StringComparer.OrdinalIgnoreCase);
            _moduleNamesOrPaths = new(StringComparer.OrdinalIgnoreCase);
        }

        public void IncludeModules(DirectoryInfo moduleDirectory)
        {
            if (moduleDirectory.Exists)
            {
                _moduleDirectories.Add(moduleDirectory.FullName);
                _watchDirectories.Add(moduleDirectory.FullName);
            }
        }

        public void IncludeModule(string moduleNameOrPath)
        {
            _moduleNamesOrPaths.Add(moduleNameOrPath);

            FileInfo moduleFile = new(moduleNameOrPath);
            if (moduleFile.Exists && moduleFile.Directory is not null)
                _watchDirectories.Add(moduleFile.Directory.FullName);
        }

        public override IEnumerable<Command> DiscoverCommands()
        {
            using Pwsh pwsh = _runspace.CreatePwsh();

            return _moduleDirectories
                .SelectMany(moduleDirectory => Enumerable.Concat(
                    Directory.EnumerateFiles(moduleDirectory, "*.psm1", SearchOption.AllDirectories),
                    Directory.EnumerateFiles(moduleDirectory, "*.psd1", SearchOption.AllDirectories)))
                .Concat(_moduleNamesOrPaths)
                .Select(pwsh.ImportModule)
                .OfType<PSModuleInfo>()
                .SelectMany(module => module.ExportedFunctions.Values)
                .Select(function => new PwshFunctionCommand(_runspace, function))
                .ToList();
        }

        public override CommandWatch? WatchCommands()
        {
            PwshWatch watch = new(_watchDirectories, ["*.psm1", "*.psd1"]);
            watch.FileChanged += Watch_FileChanged;
            return watch;
        }

        private void Watch_FileChanged(FileSystemWatcher sender, FileSystemEventArgs e)
        {
            using Pwsh pwsh = _runspace.CreatePwsh();

            foreach (var module in pwsh.GetModules())
            {
                if (module.Path.StartsWith(sender.Path, StringComparison.OrdinalIgnoreCase))
                    pwsh.RemoveModule(module.Name);
            }
        }
    }
}
