using Discord;
using Discord.WebSocket;

using LiteBot.CommandHandlersBase;
using LiteBot.CommandHandlers;
using LiteBot.Exceptions;

namespace LiteBot;
internal class Program {
	protected DiscordSocketClient client = null!;
	protected const string commandIdentifier = "=";
	protected ICommandHandler commandHandler = new BotCommandHandler(commandIdentifier);

	protected List<ulong> channelsWhiteList = new() {
		906446658299117608, // бот
		1098194649895669801, // бот генератор
		1003685097377116181 // флуд-нсфв
	};

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

		client.MessageReceived += CommandsHandlerAsync;
		client.ButtonExecuted += ButtonHandlerAsync;
		client.Log += LogAsync;
		client.Ready += () => {
			Console.WriteLine("Bot is ready to use!");
			return Task.CompletedTask;
		};
		//client.MessageUpdatedAsync += MessageUpdatedAsync;
	}

	private async Task CommandsHandlerAsync(SocketMessage message) {
		if (!channelsWhiteList.Contains(message.Channel.Id))
			return;

		await Task.Run(() => HandleMessage(message));
	}

	private async Task ButtonHandlerAsync(SocketMessageComponent arg) {
		if (arg.Data.CustomId == "sd 1") {
			await arg.Channel.SendMessageAsync("b1");
		}
		else if (arg.Data.CustomId == "sd 2") {
			await arg.Channel.SendMessageAsync("b2", messageReference: new MessageReference(arg.Message.Id, arg.Message.Channel.Id));
		}

		await arg.RespondAsync("Click handled");
	}

	private async Task LogAsync(LogMessage msg) {
		await Task.Run(() => Console.WriteLine(msg.ToString()));
	}

	private async Task MessageUpdatedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
		// If the message was not in the cache, downloading it will result in getting a copy of `after`.
		var message = await before.GetOrDownloadAsync();
		Console.WriteLine($"{message} -> {after}");
	}



	protected void PrintMessageInfo(SocketMessage message) {
		Console.WriteLine($"""
			Channel: {message.Channel}
			Author: {message.Author}
			Id: {message.Id}
			EditedTimestamp: {message.EditedTimestamp}
			CreatedAt: {message.CreatedAt}
			CleanContent: {message.CleanContent}
			Content: {message.Content}

			""");
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