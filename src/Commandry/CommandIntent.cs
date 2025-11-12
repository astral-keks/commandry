namespace Commandry
{
    public record CommandIntent
    {
        public string Command { get; set; } = string.Empty;

        public CommandParameters Parameters { get; set; } = [];
    }
}
