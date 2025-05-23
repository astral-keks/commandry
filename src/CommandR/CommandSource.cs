using CommandR.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandR
{
    public class CommandSource(IEnumerable<Command>? commands = default)
    {
        protected IEnumerable<Command> _commands = commands ?? [];

        public static CommandSource Resolve(Func<CommandSource?> resolver)
        {
            IEnumerable<Command> resolve()
            {
                foreach (var command in resolver()?.DiscoverCommands() ?? [])
                    yield return command;
            }
            return new(resolve());
        }

        public static CommandSource Combine(IEnumerable<CommandSource> commandSources) => new(
            commandSources.SelectMany(commandSource => commandSource.DiscoverCommands()));

        public virtual IEnumerable<Command> DiscoverCommands() => _commands;

        public virtual CommandWatch? WatchCommands() => default;
    }
}
