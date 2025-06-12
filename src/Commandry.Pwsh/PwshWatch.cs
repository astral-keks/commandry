using Commandry.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Commandry
{
    internal class PwshWatch : CommandWatch
    {
        private readonly List<FileSystemWatcher> _watchers;

        public PwshWatch(IEnumerable<string> directories, IEnumerable<string> filters)
        {
            _watchers = [.. directories.SelectMany(directory => filters.Select(filter =>
            {
                FileSystemWatcher watcher = new(directory)
                {
                    Filter = filter,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    IncludeSubdirectories = true
                };
                watcher.Created += NotifyCommandsChanged;
                watcher.Deleted += NotifyCommandsChanged;
                watcher.Renamed += NotifyCommandsChanged;
                watcher.Changed += NotifyCommandsChanged;
                watcher.EnableRaisingEvents = true;
                return watcher;
            }))];

            CommandsChanged += PwshWatch_CommandsChanged;
        }

        public override void Dispose()
        {
            CommandsChanged -= PwshWatch_CommandsChanged;

            foreach (FileSystemWatcher watcher in _watchers)
            {
                watcher.Changed -= NotifyCommandsChanged;
                watcher.Renamed -= NotifyCommandsChanged;
                watcher.Deleted -= NotifyCommandsChanged;
                watcher.Created -= NotifyCommandsChanged;
                watcher.Dispose();
            }
        }

        public event FileChangedHandler? FileChanged;
        public delegate void FileChangedHandler(FileSystemWatcher sender, FileSystemEventArgs e);

        private void PwshWatch_CommandsChanged(object? sender, EventArgs e)
        {
            if (sender is FileSystemWatcher watcher && e is FileSystemEventArgs fsEventArgs)
                FileChanged?.Invoke(watcher, fsEventArgs);
        }
    }
}
