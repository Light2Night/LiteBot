using LiteBot.CommandHandlersBase;

namespace LiteBot.CommandHandlers;

public class BotCommandHandler : CommandHandlerWithCommandList {
	public BotCommandHandler(string commandIdentifier) : base(
		commandIdentifier,
		new List<CommandHandler>() {
			new TimeHandler("час"),
			new AuthorHandler("автор"),
			new HelloHandler("привіт"),
			new ArtHandler("арт"),
			new RandomHandler("рандом"),
			new StableDiffusionHandler("sd")
		}
	) { }

	protected override void HelpMessage() {
		SendMessage(
			"Доступні команди:\n" +
			"	Корисні:\n" +
			"		Час\n" +
			"		Арт\n" +
			"		sd\n" +
			"		Рандом\n" +
			"	Менш корисні:\n" +
			"		Привіт\n" +
			"		Автор\n"
		);
	}
}
