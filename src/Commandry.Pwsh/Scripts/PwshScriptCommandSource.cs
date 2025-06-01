using Commandry.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Commandry.Scripts
{
    public sealed class PwshScriptCommandSource : CommandSource
    {
        private readonly PwshRunspace _runspace;
        private readonly List<DirectoryInfo> _directories;

        public PwshScriptCommandSource(PwshRunspace runspace, IEnumerable<DirectoryInfo>? directories = default)
        {
            _runspace = runspace;
            _directories = directories?.ToList() ?? [];
        }

        public PwshScriptCommandSource IncludeDirectory(string? commandDirectory) =>
            IncludeDirectory(commandDirectory is not null ? new DirectoryInfo(commandDirectory) : default);
        public PwshScriptCommandSource IncludeDirectory(DirectoryInfo? commandDirectory)
        {
            if (commandDirectory is not null)
                _directories.Add(commandDirectory);
            return this;
        }

        public PwshScriptCommandSource DefineVariable(string name, object value)
        {
            _runspace.SetVariable(name, value);
            return this;
        }

        public override IEnumerable<Command> DiscoverCommands() => _directories
            .SelectMany(directory => directory.EnumerateFiles("*.ps1", SearchOption.AllDirectories))
            .Select(ps1File => new PwshScriptCommand(_runspace, ps1File));

        public override CommandWatch? WatchCommands() => new PwshScriptCommandWatch(_directories);
    }
}
