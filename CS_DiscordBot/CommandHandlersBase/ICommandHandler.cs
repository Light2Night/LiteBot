using Discord.WebSocket;

namespace LiteBot.CommandHandlersBase;

public interface ICommandHandler {
	void HandleCommand(SocketMessage socketMessage);
	void HandleCommand(SocketMessage socketMessage, string command);
	bool IsCommand(string messageText);
	bool IsCommand(string messageText, out string commandText);
}
