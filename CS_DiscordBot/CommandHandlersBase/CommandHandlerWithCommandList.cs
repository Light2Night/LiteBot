using DiscordBot.Exceptions;

namespace DiscordBot.CommandHandlersBase;

public abstract class CommandHandlerWithCommandList : CommandHandler
{
    protected List<CommandHandler> commandHandlers;

    public CommandHandlerWithCommandList(string commandIdentifier, List<CommandHandler> commandHandlers) : base(commandIdentifier)
    {
        this.commandHandlers = commandHandlers;
    }

    protected override void ExecuteCommand(string arguments)
    {
        if (arguments == string.Empty)
        {
            DefaultAction();
            return;
        }
        if (arguments == "?")
        {
            HelpMessage();
            return;
        }

        foreach (CommandHandler handler in commandHandlers)
        {
            try
            {
                handler.HandleCommand(socketMessage, arguments);
                return;
            }
            catch (IsNotCommandException) { }
        }
        throw new UnknownCommandException();
    }
}