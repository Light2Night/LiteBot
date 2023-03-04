using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_DiscordBot;

public interface ICommandHandler {
	void HandleCommand(string command);
	bool IsCommand(string messageText);
	bool IsCommand(string messageText, out string commandText);
}



public abstract class CommandHandler : ICommandHandler {
	protected string commandIdentifier;
	protected SocketMessage message;

	public CommandHandler(string commandIdentifier, SocketMessage message) {
		this.commandIdentifier = commandIdentifier;
		this.message = message;
	}



	public void HandleCommand(string command) {
		//Console.WriteLine($"Обробка команди {command}");
		command = command.Trim().ToLower();

		if (!IsCommand(command, out string argumentsText)) {
			throw new IsNotCommandException();
		}

		ExecuteCommand(argumentsText);
	}

	protected virtual void ExecuteCommand(string arguments) {
		if (arguments == "") {
			DefaultAction();
		}
		else {
			throw new UnknownCommandException();
		}
	}

	protected virtual void DefaultAction() {
		throw new UnknownCommandException();
	}

	public bool IsCommand(string messageText) {
		return messageText.StartsWith(commandIdentifier);
	}

	public bool IsCommand(string messageText, out string commandText) {
		if (!IsCommand(messageText)) {
			commandText = "";
			return false;
		}

		commandText = messageText.Substring(commandIdentifier.Length, messageText.Length - commandIdentifier.Length).Trim();
		return true;
	}

	protected void SendMessage(string text) {
		message.Channel.SendMessageAsync(text);
	}
}



public abstract class CommandHandlerWithCommandList : CommandHandler {
	protected List<CommandHandler> commandHandlers;

	public CommandHandlerWithCommandList(string commandIdentifier, SocketMessage message, List<CommandHandler> commandHandlers) : base(commandIdentifier, message) {
		this.commandHandlers = commandHandlers;
	}

	protected override void ExecuteCommand(string arguments) {
		if (arguments == "") {
			DefaultAction();
			return;
		}

		foreach (CommandHandler handler in commandHandlers) {
			try {
				handler.HandleCommand(arguments);
				return;
			}
			catch (IsNotCommandException) { }
		}
		throw new UnknownCommandException();
	}
}