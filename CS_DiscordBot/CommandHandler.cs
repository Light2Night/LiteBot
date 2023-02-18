using Discord.WebSocket;

using ArtApp.Web;

namespace Tea_and_Tea_Discord_Bot {
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

			DoCommand(argumentsText);
		}

		protected virtual void DoCommand(string arguments) {
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

			commandText = messageText.Substring(commandIdentifier.Length, messageText.Length - commandIdentifier.Length);
			return true;
		}

		protected void SendMessage(SocketMessage message, string text) {
			message.Channel.SendMessageAsync(text);
		}
	}

	public abstract class CommandHandlerWithCommandList : CommandHandler {
		protected List<CommandHandler> commandHandlers;

		public CommandHandlerWithCommandList(string commandIdentifier, SocketMessage message, List<CommandHandler> commandHandlers) : base(commandIdentifier, message) {
			this.commandHandlers = commandHandlers;
		}

		protected override void DoCommand(string arguments) {
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



	public class BotCommandHandler : CommandHandlerWithCommandList {
		public BotCommandHandler(string commandIdentifier, SocketMessage message)
			: base(commandIdentifier, message,
				  new List<CommandHandler>() {
					  new BotHelpHandler("?", message),
					  new TimeHandler("час", message),
					  new AuthorHandler("автор", message),
					  new HelloHandler("привіт", message),
					  new ArtHandler("арт", message)
				  }
			) { }
	}

	public class BotHelpHandler : CommandHandler {
		public BotHelpHandler(string commandIdentifier, SocketMessage message) : base(commandIdentifier, message) { }

		protected override void DefaultAction() {
			SendMessage(
				message,
				"Доступні команди:\n" +
				"Час\n" +
				"Автор\n" +
				"Привіт"
			);
		}
	}

	public class TimeHandler : CommandHandler {
		public TimeHandler(string commandIdentifier, SocketMessage message) : base(commandIdentifier, message) { }

		protected override void DefaultAction() {
			SendMessage(message, $"{DateTime.Now.ToLongTimeString()}\n{DateTime.Now.ToLongDateString()}");
			if (DateTime.Now.Hour < 8)
				SendMessage(message, "Іншими словами час спати");
		}
	}

	public class AuthorHandler : CommandHandler {
		public AuthorHandler(string commandIdentifier, SocketMessage message) : base(commandIdentifier, message) { }

		protected override void DefaultAction() {
			SendMessage(message, "<@!883836608963555339> Lite#5625");
		}
	}

	public class HelloHandler : CommandHandler {
		public HelloHandler(string commandIdentifier, SocketMessage message) : base(commandIdentifier, message) { }

		protected override void DefaultAction() {
			if (message.Author.Id == 883836608963555339) {
				SendMessage(message, "Вітаю!");
			}
			else {
				SendMessage(message, "Ти хто такий?");
			}
		}
	}

	public class ArtHandler : CommandHandlerWithCommandList {
		public ArtHandler(string commandIdentifier, SocketMessage message)
			: base(commandIdentifier, message,
				new List<CommandHandler>() {
					new ArtWithCountHandler("кількість", message),
					new ArtHelpHandler("?", message)
				}
			) { }

		protected override void DefaultAction() {
			SendMessage(message, WebLoad.GetPictureUrlFromApi("https://api.waifu.pics/sfw/neko", "\"url\":\"([^\"]*)\""));
		}
	}

	public class ArtHelpHandler : CommandHandler {
		public ArtHelpHandler(string commandIdentifier, SocketMessage message) : base(commandIdentifier, message) { }

		protected override void DefaultAction() {
			SendMessage(
				message,
				"Доступні команди:\n" +
				"Кількість \"число\""
			);
		}
	}

	public class ArtWithCountHandler : CommandHandler {
		public ArtWithCountHandler(string commandIdentifier, SocketMessage message) : base(commandIdentifier, message) { }

		protected override void DoCommand(string arguments) {
			if (IsUInt32(arguments)) {
				uint numberOfPictures = Convert.ToUInt32(arguments);
				if (numberOfPictures > 10) {
					SendMessage(message, "Занадто багато зображень");
					return;
				}

				for (uint i = 0; i < numberOfPictures; i++) {
					SendMessage(message, WebLoad.GetPictureUrlFromApi("https://api.waifu.pics/sfw/neko", "\"url\":\"([^\"]*)\""));
				}
			}
			else {
				throw new UnknownCommandException();
			}
		}

		protected bool IsUInt32(string number) {
			try {
				Convert.ToUInt32(number);
				return true;
			}
			catch (Exception) { }
			return false;
		}
	}



	public class IsNotCommandException : Exception {
		public IsNotCommandException() : this("Is not command cxception") { }
		public IsNotCommandException(string message) : base(message) { }
	}
	public class UnknownCommandException : Exception {
		public UnknownCommandException() : this("Unknown command cxception") { }
		public UnknownCommandException(string message) : base(message) { }
	}
}