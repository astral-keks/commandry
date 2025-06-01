using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Hosting
{
    internal class CommandDecorator(Command command, CommandDispatcher commandDispatcher, CommandErrorHandler? commandErrorHandler) : Command
    {
        public override string Name => command.Name;

        public override Dictionary<object, object?> Parameters
        {
            get => command.Parameters;
            set => command.Parameters = value;
        }

        public override CommandResult? Result
        {
            get => command.Result;
            protected internal set => command.Result = value;
        }

        public override ILogger? Logger
        {
            protected internal get => command.Logger;
            set => command.Logger = value;
        }

        public override Task<CommandMetadata> DescribeAsync(CancellationToken cancellation) => command.DescribeAsync(cancellation);

        public override Task ExecuteAsync(CancellationToken cancellation) => commandDispatcher.InvokeAsync(async () =>
        {
            try
            {
                await command.ExecuteAsync(cancellation);
                Result ??= new();
            }
            catch (Exception commandError)
            {
                commandErrorHandler?.Invoke(commandError);
                Result = new() { Error = commandError };
            }

            return Result;
        });
    }
}
