using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Commandry
{
    public abstract class Command : ICommand
    {
        public abstract string Name { get; }

        public virtual CommandParameters Parameters { get; set; } = [];
        public virtual CommandResult? Result { get; protected internal set; }

        public virtual CommandProgress? Progress { protected internal get; set; }
        public virtual ILogger? Logger { protected internal get; set; }

        public event EventHandler? CanExecuteChanged;
        protected void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object? parameter) => CanExecute();
        public virtual bool CanExecute() => true;

        public void Execute() => Execute(null);
        public async void Execute(object? parameter) => await ExecuteAsync(CancellationToken.None);
        public abstract Task ExecuteAsync(CancellationToken cancellation);

        public CommandMetadata Describe() => DescribeAsync(CancellationToken.None).GetAwaiter().GetResult();
        public abstract Task<CommandMetadata> DescribeAsync(CancellationToken cancellation);
    }
}
