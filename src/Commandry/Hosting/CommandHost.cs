using Commandry.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Commandry.Hosting
{
    public class CommandHost(IEnumerable<CommandSource> commandSources,
        CommandErrorHandler? commandErrorHandler = default, CommandDispatcher? commandDispatcher = default, CommandSerializer? commandSerializer = default)
    {
        private readonly List<CommandSource> _commandSources = [.. commandSources];
        private readonly CommandDispatcher _commandDispatcher = commandDispatcher ?? CommandDispatcher.Default;
        private readonly CommandErrorHandler? _commandErrorHandler = commandErrorHandler;

        private CommandMonitor? _commandMonitor;
        private CommandServer? _commandServer;

        public bool IsRemote => _commandServer?.IsRemote == true;

        public Command? GetCommand(CommandIntent commandIntent)
        {
            Command? command = GetCommand(commandIntent.Command);
            if (command is not null)
                command.Parameters = commandIntent.Parameters;
            return command;
        }

        public Command? GetCommand(string commandName)
        {
            return GetCommands()
                .LastOrDefault(command => command.Name == commandName);
        }

        public IEnumerable<Command> GetCommands()
        {
            return _commandSources
                .SelectMany(commandSource => commandSource.DiscoverCommands())
                .Select(command => new CommandDecorator(command, _commandDispatcher, _commandErrorHandler));
        }

        public Command? ConnectCommand(CommandIntent commandIntent)
        {
            return _commandServer?.IsRemote == true
                ? _commandServer.ConnectCommand(commandIntent)
                : GetCommand(commandIntent);
        }

        public void BindCommands(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
                BindCommand(command);
        }

        public Command BindCommand(Command command)
        {
            if (command is CommandProxy bindable)
                bindable.Bind(this);
            return command;
        }

        public void ServeCommands(string serverName)
        {
            if (_commandServer is not null)
                throw new InvalidOperationException(
                    $"Cannot start server {serverName}. Server {_commandServer.Name} is already running.");

            if (commandSerializer is null)
                throw new InvalidOperationException(
                    $"Cannot start server {serverName}. Command serializer was not provided for the CommandHost.");

            _commandServer = new(serverName, commandSerializer, _commandErrorHandler, this);
        }

        public void ReceiveCommands()
        {
            if (_commandServer?.IsRemote == false)
                _commandServer.ReceiveCommands();
        }

        public void UnserveCommands()
        {
            _commandServer?.Dispose();
            _commandServer = default;
        }

        public void WatchCommands(EventHandler handler)
        {
            if (_commandMonitor is null)
                _commandMonitor = new(_commandSources);
            _commandMonitor.CommandsChanged += handler;
        }

        public void UnwatchCommands()
        {
            if (_commandMonitor is not null)
            {
                _commandMonitor.Dispose();
                _commandMonitor = default;
            }
        }
    }
}
