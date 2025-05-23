using System;
using System.Threading.Tasks;

namespace CommandR.Hosting
{
    public class CommandDispatcher
    {
        public static readonly CommandDispatcher Default = new();

        public virtual Task<CommandResult> InvokeAsync(Func<Task<CommandResult>> commandAction) => commandAction();
    }
}
