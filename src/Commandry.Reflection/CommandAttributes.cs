using System;

namespace Commandry
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CommandHandlerAttribute(string commandName) : Attribute
    {
        public string CommandName { get; } = commandName;
    }
}
