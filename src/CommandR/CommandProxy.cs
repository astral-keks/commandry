using CommandR.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CommandR
{
    public class CommandProxy(CommandIntent commandIntent, CommandHost? commandHost = default) : Command
    {
        private readonly CommandIntent _intent = commandIntent;
        private CommandHost? _host = commandHost;

        public override string Name => _intent.Command;

        public override Dictionary<object, object?> Parameters 
        { 
            get => _intent.Parameters; 
            set => _intent.Parameters = value; 
        }

        public override CommandResult? Result 
        { 
            get => InnerCommand.Result; 
            protected internal set => InnerCommand.Result = value; 
        }

        public override ILogger? Logger
        {
            protected internal get => InnerCommand.Logger;
            set => InnerCommand.Logger = value;
        }

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            await InnerCommand.ExecuteAsync(cancellation);
            Result = InnerCommand.Result;
        }

        public override async Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            return await InnerCommand.DescribeAsync(cancellation);
        }

        public void Bind(CommandHost commandHost) => _host = commandHost;

        private Command InnerCommand => CommandHost.GetCommand(_intent) ?? throw new CommandException($"Command {_intent.Command} is was not found");
        private CommandHost CommandHost => _host ?? throw new CommandException($"Command {_intent.Command} is detached");

    }
}
