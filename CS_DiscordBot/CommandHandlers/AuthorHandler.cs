using DiscordBot.CommandHandlersBase;

namespace DiscordBot.CommandHandlers;

public class AuthorHandler : CommandHandler {
	public AuthorHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void DefaultAction() {
		SendMessage("<@!883836608963555339> Lite#5625");
	}
}
