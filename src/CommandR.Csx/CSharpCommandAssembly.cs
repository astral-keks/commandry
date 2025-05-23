using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandR
{
    internal class CSharpCommandAssembly(Assembly assembly)
    {
        private readonly Assembly _assembly = assembly;
        private readonly List<CommandDescription> _commands = assembly.ExportedTypes
            .Where(type => type.IsSubclassOf(typeof(Command)))
            .Select(Activator.CreateInstance)
            .Cast<Command>()
            .Select(prototype => new CommandDescription
            {
                CommandName = prototype.Name,
                CommandFactory = parameters => CSharpCommandMapper.MapParameters(parameters, (Command)prototype.Clone())
            })
            .ToList();

        public IEnumerable<CommandDescription> Commands => _commands;
    }
}
