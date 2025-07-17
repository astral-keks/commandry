using System.Collections.Generic;
using System.Text.Json;

namespace Commandry.Schemas
{
    public class PwshCommandSchema(PwshRunspace runspace) : CommandSchema
    {
        public override CommandParameters DeserializeParameters(IReadOnlyDictionary<string, JsonElement>? source)
        {
            using Pwsh pwsh = runspace.CreatePwsh();
            return pwsh.WithRunspace(() => base.DeserializeParameters(source));
        }
    }
}
