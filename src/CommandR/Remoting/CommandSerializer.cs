using System.IO;

namespace CommandR.Remoting
{
    public abstract class CommandSerializer
    {
        public abstract void Serialize(TextWriter writer, CommandIntent intent);

        public abstract CommandIntent? Deserialize(TextReader reader);
    }
}
