using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandR
{
    internal static class CommandReflection
    {
        public static IEnumerable<(string CommandName, Type ParametersType, MethodInfo MethodInfo)> ReflectCommands(this Type commandSourceType)
        {
            foreach (var commandMethod in commandSourceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Type? commandParametersType = commandMethod.GetParameters()?.FirstOrDefault()?.ParameterType;
                string? commandName = commandParametersType?.ReflectCommandName();
                if (!string.IsNullOrWhiteSpace(commandName) && commandParametersType is not null)
                    yield return (commandName, commandParametersType, commandMethod);
            }
        }

        public static string? ReflectCommandName(this Type? commandParametersType)
        {
            return commandParametersType?.GetCustomAttribute<CommandHandlerAttribute>()?.CommandName;
        }
    }
}
