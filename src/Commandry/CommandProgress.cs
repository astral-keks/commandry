namespace Commandry
{
    public abstract class CommandProgress
    {
        public abstract void Report(float status, string message);
    }
}
