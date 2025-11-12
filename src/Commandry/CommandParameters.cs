using System.Collections.Generic;

namespace Commandry
{
    public class CommandParameters : Dictionary<object, object?>
    {
        public CommandParameters(IDictionary<object, object?> source) : base(source)
        {
        }

        public CommandParameters()
        {
        }
    }
}
