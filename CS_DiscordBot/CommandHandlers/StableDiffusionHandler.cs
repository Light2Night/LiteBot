using Discord;
using CS_DiscordBot.Exceptions;

namespace CS_DiscordBot.CommandHandlers;

public class StableDiffusionHandler : CommandHandler {
	protected StableDiffusionApi stableDiffusionInterface = new();
	protected StableDiffusionQueue diffusionQueue = new();

	public StableDiffusionHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			diffusionQueue.Enqueue(new GenerationRequest(socketMessage, stableDiffusionInterface));
		}
		else if (arguments == "?") {
			HelpMessage();
		}
		else if (arguments == "p") {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, stableDiffusionInterface, "prompt", ""));
		}
		else if (IsSubcommand(arguments, "p", out string value) || IsSubcommand(arguments, "prompt", out value)) {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, stableDiffusionInterface, "prompt", value));
		}
		else if (arguments == "np") {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, stableDiffusionInterface, "negative_prompt", ""));
		}
		else if (IsSubcommand(arguments, "np", out value) || IsSubcommand(arguments, "negative prompt", out value)) {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, stableDiffusionInterface, "negative_prompt", value));
		}
		else if (arguments == "default") {
			diffusionQueue.Enqueue(new ResetPropertyRequest(socketMessage, stableDiffusionInterface));
		}
		else if (IsSubcommand(arguments, "cfg", out value)) {
			if (!TypeChecker.IsUInt32(value)) {
				SendMessage("Uncorrect value type", new MessageReference(socketMessage.Id, socketMessage.Channel.Id));
				return;
			}

			uint cfgScale = Convert.ToUInt32(value);

			if (!(1 <= cfgScale && cfgScale <= 30)) {
				SendMessage("The value of the property must be between 1 and 30", new MessageReference(socketMessage.Id, socketMessage.Channel.Id));
				return;
			}

			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, stableDiffusionInterface, "cfg_scale", cfgScale));
		}
		else if (arguments != string.Empty) {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, stableDiffusionInterface, "prompt", arguments));
			diffusionQueue.Enqueue(new GenerationRequest(socketMessage, stableDiffusionInterface));
		}
		else {
			throw new UnknownCommandException();
		}
	}

	//protected override void DefaultAction() {
	//
	//}

	protected override void HelpMessage() {
		SendMessage("""
			Доступні команди:
				`"Немає аргументів"` - генерує зображення по раніше заданому промпту
				`"текст промпту"` - встановлює промпт та запускає генерацію
				`p "текст промпту"` або `prompt "текст промпту"` - встановлює промпт для генерації
				`np "текст анти-промпту"` або `negative prompt "текст анти-промпту"` - встановлює анти-промпт для генерації
				`default` - встановлює стандартне значення властивостей
				`cfg "число"` - встановлює значення властивості cfg_scale. Вона вплиає на силу дії промптів та анти-промптів. Стандартне значення 7
			""");
	}
}
