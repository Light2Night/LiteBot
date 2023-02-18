using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;

namespace CS_DiscordBot {
	internal class Program {
		protected DiscordSocketClient client;
		protected static Random random = new Random(DateTime.Now.Millisecond);

		private static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync() {
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			string token = File.ReadAllText("Token.txt");

			CreateClientInstance();

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			await Task.Delay(-1);
		}

		protected void CreateClientInstance() {
			client = new DiscordSocketClient(
				new DiscordSocketConfig {
					GatewayIntents = GatewayIntents.All,
					//LogLevel = LogSeverity.Verbose,
					//AlwaysDownloadUsers = true,
					//MessageCacheSize = 200
				}
			);

			client.MessageReceived += CommandsHandler;
			client.Log += Log;
		}

		// Обробники подій
		private Task CommandsHandler(SocketMessage message) {
			HandleMessage(message);
			return Task.CompletedTask;
		}

		private Task Log(LogMessage msg) {
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}



		protected void PrintMessageInfo(SocketMessage message) {
			Console.WriteLine(
				$"Channel: {message.Channel}\n" +
				$"Author: {message.Author}\n" +
				$"Id: {message.Id}\n" +
				$"EditedTimestamp: {message.EditedTimestamp}\n" +
				$"CreatedAt: {message.CreatedAt}\n" +
				$"Application {message.Application}\n" +
				$"CleanContent: {message.CleanContent}\n" +
				$"Content: {message.Content}\n"
			);
		}

		protected void HandleMessage(SocketMessage message) {
			if (message.Author.IsBot)
				return;

			PrintMessageInfo(message);

			ICommandHandler commandHandler = new BotCommandHandler("=", message);
			try {
				commandHandler.HandleCommand(message.Content);
			}
			catch (IsNotCommandException) { }
			catch (UnknownCommandException) {
				message.Channel.SendMessageAsync("Невідома команда");
			}
		}
	}
}