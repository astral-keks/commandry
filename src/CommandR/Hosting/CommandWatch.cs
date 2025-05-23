using System;

namespace CommandR.Hosting
{
    public abstract class CommandWatch : IDisposable
    {
        protected CommandWatch()
        {
        }

        public abstract void Dispose();

        public event EventHandler? CommandsChanged;

        protected void NotifyCommandsChanged(object? sender, EventArgs args) => CommandsChanged?.Invoke(sender, args);
    }
}
