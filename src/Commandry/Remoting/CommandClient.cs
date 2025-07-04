using Commandry.Schemas;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Remoting
{
    internal class CommandClient(CommandIntent commandIntent, CommandServer commandServer) : Command
    {
        private readonly CommandIntent _intent = commandIntent;
        private readonly CommandServer _server = commandServer;

        public override string Name => _intent.Command;

        public override CommandParameters Parameters
        {
            get => _intent.Parameters;
            set => _intent.Parameters = value;
        }

        public override Task ExecuteAsync(CancellationToken cancellation) =>
            _server.SendCommandAsync(_intent, cancellation);

        public override Task<CommandMetadata> DescribeAsync(CancellationToken cancellation) =>
            Task.FromResult(new CommandMetadata() { Name = Name, Schema = CommandSchema.Empty });
    }
}
