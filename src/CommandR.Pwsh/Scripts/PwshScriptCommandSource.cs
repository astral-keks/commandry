using CommandR.Hosting;
using Microsoft.PowerShell;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace CommandR.Scripts
{
    public sealed class PwshScriptCommandSource : CommandSource
    {
        private readonly List<DirectoryInfo> _directories;
        private readonly Runspace _runspace;

        public PwshScriptCommandSource(IEnumerable<DirectoryInfo>? directories = default, ApartmentState apartmentState = ApartmentState.STA)
        {
            _directories = directories?.ToList() ?? [];

            InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.RemoteSigned;
            initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            initialSessionState.ApartmentState = apartmentState;

            _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            _runspace.Open();
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
            _runspace.SessionStateProxy.SetVariable(name, value);
            return this;
        }

        public override IEnumerable<Command> DiscoverCommands() => _directories
            .SelectMany(directory => directory.EnumerateFiles("*.ps1", SearchOption.AllDirectories))
            .Select(DiscoverCommand);

        private Command DiscoverCommand(FileInfo ps1File) => new PwshScriptCommand(_runspace, ps1File);

        public override CommandWatch? WatchCommands() => new PwshScriptCommandWatch(_directories);
    }
}
