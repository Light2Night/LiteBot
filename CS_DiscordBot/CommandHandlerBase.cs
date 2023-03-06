using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_DiscordBot;

public interface ICommandHandler {
	void HandleCommand(SocketMessage socketMessage);
	void HandleCommand(SocketMessage socketMessage, string command);
	bool IsCommand(string messageText);
	bool IsCommand(string messageText, out string commandText);
}



public abstract class CommandHandler : ICommandHandler {
	protected readonly string commandIdentifier;
	protected SocketMessage socketMessage = null;

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
		command = command.Trim().ToLower();

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

	public bool IsCommand(string messageText) {
		return messageText.StartsWith(commandIdentifier);
	}

	public bool IsCommand(string messageText, out string commandText) {
		if (!IsCommand(messageText)) {
			commandText = string.Empty;
			return false;
		}

		commandText = messageText.Substring(commandIdentifier.Length, messageText.Length - commandIdentifier.Length).Trim();
		return true;
	}

	protected void SendMessage(string text) {
		socketMessage.Channel.SendMessageAsync(text);
	}
}



public abstract class CommandHandlerWithCommandList : CommandHandler {
	protected List<CommandHandler> commandHandlers;

	public CommandHandlerWithCommandList(string commandIdentifier, List<CommandHandler> commandHandlers) : base(commandIdentifier) {
		this.commandHandlers = commandHandlers;
	}

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			DefaultAction();
			return;
		}
		else if (arguments == "?") {
			HelpMessage();
			return;
		}

		foreach (CommandHandler handler in commandHandlers) {
			try {
				handler.HandleCommand(socketMessage, arguments);
				return;
			}
			catch (IsNotCommandException) { }
		}
		throw new UnknownCommandException();
	}
}