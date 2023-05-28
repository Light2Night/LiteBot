using LiteBot.Exceptions;
using Discord;
using Discord.WebSocket;

namespace LiteBot.CommandHandlersBase;

public abstract class CommandHandler : ICommandHandler {
	protected readonly string commandIdentifier;
	protected SocketMessage socketMessage = null!;

	public CommandHandler(string commandIdentifier) {
		this.commandIdentifier = commandIdentifier;
	}



	public void HandleCommand(SocketMessage socketMessage) {
		this.socketMessage = socketMessage;
		HandleCommand(socketMessage.Content);
	}

	public void HandleCommand(SocketMessage socketMessage, string command) {
		this.socketMessage = socketMessage;
		HandleCommand(command);
	}

	protected void HandleCommand(string command) {
		//Console.WriteLine($"Обробка команди {command}");

		if (!IsCommand(command, out string argumentsText)) {
			throw new IsNotCommandException();
		}

		ExecuteCommand(argumentsText);
	}

	protected virtual void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			DefaultAction();
		}
		else if (arguments == "?") {
			HelpMessage();
		}
		else {
			throw new UnknownCommandException();
		}
	}

	protected virtual void DefaultAction() {
		throw new UnknownCommandException();
	}

	protected virtual void HelpMessage() {
		throw new UnknownCommandException();
	}


	public bool IsCommand(string messageText) =>
		IsCommandBase(messageText, commandIdentifier, "");
	public bool IsCommand(string messageText, out string commandText) =>
		IsCommandBase(messageText, commandIdentifier, "", out commandText);

	public bool IsSubcommand(string messageText, string commandIdentifier) =>
		IsCommandBase(messageText, commandIdentifier, " ");
	public bool IsSubcommand(string messageText, string commandIdentifier, out string commandText) =>
		IsCommandBase(messageText, commandIdentifier, " ", out commandText);

	private bool IsCommandBase(string messageText, string commandIdentifier, string separator) {
		return messageText.ToLower().StartsWith(commandIdentifier + separator);
	}
	private bool IsCommandBase(string messageText, string commandIdentifier, string separator, out string commandText) {
		if (!IsCommandBase(messageText, commandIdentifier, separator)) {
			commandText = string.Empty;
			return messageText == commandIdentifier;
		}

		commandText = messageText.Substring(commandIdentifier.Length, messageText.Length - commandIdentifier.Length).Trim();
		return true;
	}


	protected void SendMessage(string text) {
		socketMessage.Channel.SendMessageAsync(text);
	}

	protected void SendMessage(string text, MessageReference messageReference) {
		socketMessage.Channel.SendMessageAsync(text, messageReference: messageReference);
	}
}
