using System.Collections.Generic;

namespace CommandR.Hosting
{
    internal class CommandMonitor : CommandWatch
    {
        private readonly List<CommandWatch> _watches = [];

        public CommandMonitor(List<CommandSource> commandSources)
        {
            foreach (var commandSource in commandSources)
            {
                CommandWatch? commandWatch = commandSource.WatchCommands();
                if (commandWatch is not null)
                {
                    commandWatch.CommandsChanged += NotifyCommandsChanged;
                    _watches.Add(commandWatch);
                }
            }
        }

        public override void Dispose()
        {
            foreach (var watch in _watches)
            {
                watch.CommandsChanged -= NotifyCommandsChanged;
                watch.Dispose();
            }
        }
    }
}
