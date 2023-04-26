using Discord;
using Discord.WebSocket;

using DiscordBot.CommandHandlersBase;
using DiscordBot.CommandHandlers;
using DiscordBot.Exceptions;

namespace DiscordBot;
internal class Program {
	protected DiscordSocketClient client = null!;
	protected static Random random = new Random(DateTime.Now.Millisecond);
	protected const string commandIdentifier = "=";
	protected ICommandHandler commandHandler = new BotCommandHandler(commandIdentifier);

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
				GatewayIntents = GatewayIntents.All
			}
		);

		client.MessageReceived += CommandsHandler;
		client.Log += Log;
		client.Ready += () => {
			Console.WriteLine("Bot is ready to use!");
			return Task.CompletedTask;
		};
		client.MessageUpdated += MessageUpdated;
	}



	private Task CommandsHandler(SocketMessage message) {
		if (!(message.Channel.Id == 1003685097377116181 || message.Channel.Id == 906446658299117608 || message.Channel.Id == 1098194649895669801))
			return Task.CompletedTask;

		HandleMessage(message);

		return Task.CompletedTask;
	}

	private Task Log(LogMessage msg) {
		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
	}

	private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
		// If the message was not in the cache, downloading it will result in getting a copy of `after`.
		var message = await before.GetOrDownloadAsync();
		Console.WriteLine($"{message} -> {after}");
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

		//IReadOnlyCollection<Attachment> collection = message.Attachments;
		//foreach (Attachment item in collection) {
		//	Console.WriteLine("" + item.ToString());
		//	Console.WriteLine("ContentType: " + item.ContentType);
		//	Console.WriteLine("Description: " + item.Description);
		//	Console.WriteLine("URL: " + item.Url);
		//}

		try {
			commandHandler.HandleCommand(message);
		}
		catch (IsNotCommandException) { }
		catch (UnknownCommandException) {
			message.Channel.SendMessageAsync($"Невідома команда, для детальнішої інформації про команди спробуйте \"{commandIdentifier}?\"");
		}
		catch (Exception e) {
			message.Channel.SendMessageAsync($"Невідома помилка, код помилки {e}");
		}
	}
}