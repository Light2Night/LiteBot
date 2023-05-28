using LiteBot.CommandHandlersBase;

namespace LiteBot.CommandHandlers;

public class HelloHandler : CommandHandler {
	public HelloHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void DefaultAction() {
		if (socketMessage.Author.Id == 883836608963555339) {
			SendMessage("Вітаю!");
		}
		else {
			SendMessage("Ти хто такий?");
		}
	}
}
