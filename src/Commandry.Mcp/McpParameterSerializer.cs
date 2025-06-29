using System.Text.Json;

namespace Commandry.Mcp
{
    public class McpParameterSerializer(PwshRunspace runspace)
    {
        public Dictionary<object, object?>? Deserialize(IReadOnlyDictionary<string, JsonElement>? arguments, CommandSchema commandSchema)
        {
            using Pwsh pwsh = runspace.CreatePwsh();
            return pwsh.WithRunspace(() =>
                arguments
                    ?.Join(commandSchema.Parameters, arg => arg.Key, par => par.Name, (arg, par) => (Argument: arg, Schema: par))
                    .ToDictionary(
                        x => x.Schema.Name as object,
                        x => x.Argument.Value.Deserialize(x.Schema.Type))
            );
        }
    }
}
