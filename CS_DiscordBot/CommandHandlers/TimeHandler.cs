using DiscordBot.CommandHandlersBase;

namespace DiscordBot.CommandHandlers;

public class TimeHandler : CommandHandler {
	public TimeHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void DefaultAction() {
		SendMessage($"{DateTime.Now.ToLongTimeString()}\n{DateTime.Now.ToLongDateString()}");
		if (DateTime.Now.Hour < 8)
			SendMessage("Іншими словами час спати");
	}
}
