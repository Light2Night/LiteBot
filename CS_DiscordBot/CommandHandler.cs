using Discord.WebSocket;

using ArtApp.Web;

namespace Tea_and_Tea_Discord_Bot {
	public class CommandHandler {
		protected string commandIdentifier;
		protected SocketMessage message;

		public CommandHandler(string commandIdentifier, SocketMessage message) {
			this.commandIdentifier = commandIdentifier;
			this.message = message;
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

		public void HandleCommand(SocketMessage message, string command) {
			Console.WriteLine($"Обробка команди {command}");
			command = command.Trim().ToLower();

			if (command == "?") {
				SendMessage(
					message,
					"Доступні команди:\n" +
					"Час\n" +
					"Автор\n" +
					"Привіт"
				);
			}
			else if (command == "час") {
				SendMessage(message, $"{DateTime.Now.ToLongTimeString()}\n{DateTime.Now.ToLongDateString()}");
				if (DateTime.Now.Hour < 8)
					SendMessage(message, "Іншими словами час спати");
			}
			else if (command == "автор") {
				SendMessage(message, "<@!883836608963555339> Lite#5625");
			}
			else if (command == "привіт") {
				if (message.Author.Id == 883836608963555339) {
					SendMessage(message, "Вітаю!");
					return;
				}
				else {
					SendMessage(message, "Ти хто такий?");
					return;
				}
			}
			else if (command == "арт") {
				IsCommand(command, out string subCommand);
				string[] parameters = subCommand.Split(" ");

				SendMessage(message, WebLoad.GetPictureUrlFromApi("https://api.waifu.pics/sfw/neko", "\"url\":\"([^\"]*)\""));
			}
			else {
				SendMessage(message, "Невідома команда");
			}
		}

		protected void SendMessage(SocketMessage message, string text) {
			message.Channel.SendMessageAsync(text);
		}
	}
}