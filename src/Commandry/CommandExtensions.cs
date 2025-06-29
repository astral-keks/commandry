namespace Commandry;

public static class CommandExtensions
{
    public static CommandResult Inspect(this Command command)
    {
        if (command.Result?.Error is not null)
            throw command.Result.Error;
        return command.Result ?? new();
    }

}
