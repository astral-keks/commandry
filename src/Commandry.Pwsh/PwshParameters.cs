using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Commandry
{
    internal static class PwshParameters
    {
        private static readonly HashSet<string> _commonParameters = [.. typeof(CommonParameters).GetProperties().Select(property => property.Name)];

        public static bool IsCommon(this ParameterMetadata parameterMetadata) =>_commonParameters.Contains(parameterMetadata.Name);
    }
}
