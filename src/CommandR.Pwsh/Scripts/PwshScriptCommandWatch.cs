using CommandR.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommandR.Scripts
{
    internal class PwshScriptCommandWatch : CommandWatch
    {
        private readonly List<FileSystemWatcher> _watchers;

        public PwshScriptCommandWatch(List<DirectoryInfo> directories)
        {
            _watchers = [.. directories.Select(directory =>
            {
                FileSystemWatcher watcher = new(directory.FullName)
                {
                    Filter = "*.ps1",
                    NotifyFilter = NotifyFilters.FileName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };
                watcher.Created += NotifyCommandsChanged;
                watcher.Deleted += NotifyCommandsChanged;
                watcher.Renamed += NotifyCommandsChanged;
                return watcher;
            })];
        }

        public override void Dispose()
        {
            foreach (FileSystemWatcher watcher in _watchers)
            {
                watcher.Renamed -= NotifyCommandsChanged;
                watcher.Deleted -= NotifyCommandsChanged;
                watcher.Created -= NotifyCommandsChanged;
                watcher.Dispose();
            }
        }
    }
}
