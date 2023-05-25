using DiscordBot.CommandHandlersBase;
using DiscordBot.StableDiffusion;
using Discord;
using DiscordBot.Exceptions;
using DiscordBot.StableDiffusionUserRequests;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DiscordBot.CommandHandlers;

public class StableDiffusionHandler : CommandHandler {
	protected StableDiffusionApi stableDiffusionApi = new();
	protected StableDiffusionQueue diffusionQueue = new();
	protected PropertyAccessor propertyAccessor = new();

	public StableDiffusionHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			diffusionQueue.Enqueue(new GenerationRequest(socketMessage, new Pair(stableDiffusionApi, propertyAccessor)));
		}
		else if (arguments == "?") {
			HelpMessage();
		}
		else if (IsSubcommand(arguments, "p", out string value) || IsSubcommand(arguments, "prompt", out value)) {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, propertyAccessor, "prompt", value));
		}
		else if (IsSubcommand(arguments, "np", out value) || IsSubcommand(arguments, "negative prompt", out value)) {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, propertyAccessor, "negative_prompt", value));
		}
		else if (IsSubcommand(arguments, "s", out value) || IsSubcommand(arguments, "steps", out value)) {
			if (!TypeChecker.IsUInt32(value)) {
				SendMessage("Uncorrect value type", new MessageReference(socketMessage.Id, socketMessage.Channel.Id));
				return;
			}

			uint steps = Convert.ToUInt32(value);

			if (!(1 <= steps && steps <= 100)) {
				SendMessage("The value of the property must be between 1 and 100", new MessageReference(socketMessage.Id, socketMessage.Channel.Id));
				return;
			}

			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, propertyAccessor, "steps", steps));
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

			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, propertyAccessor, "cfg_scale", cfgScale));
		}
		else if (arguments == "default") {
			diffusionQueue.Enqueue(new ResetPropertyRequest(socketMessage, propertyAccessor));
		}
		else if (arguments != string.Empty) {
			diffusionQueue.Enqueue(new SetPropertyRequest(socketMessage, propertyAccessor, "prompt", arguments));
			diffusionQueue.Enqueue(new GenerationRequest(socketMessage, new Pair(stableDiffusionApi, propertyAccessor)));
		}
		else {
			throw new UnknownCommandException();
		}

		socketMessage.AddReactionAsync(new Emoji("✅"));
	}

	//protected override void DefaultAction() {
	//
	//}

	protected override void HelpMessage() {
		SendMessage("""
			Доступні команди:
				`"Немає аргументів"` - генерує зображення по раніше заданим параметрах
				`"текст промпту"` - встановлює промпт та запускає генерацію
				`p "текст промпту"` або `prompt "текст промпту"` - встановлює промпт для генерації
				`np "текст анти-промпту"` або `negative prompt "текст анти-промпту"` - встановлює анти-промпт для генерації
				`s "число" або steps "число"` - встановлює кількість ітерацій яку виконує AI над зображенням. Стандартне значення 20
				`cfg "число"` - встановлює значення властивості cfg_scale. Вона вплиає на силу дії промптів та анти-промптів. Стандартне значення 7
				`default` - встановлює стандартне значення властивостей
			""");
	}
}

public record Pair(StableDiffusionApi StableDiffusionApi, PropertyAccessor PropertyAccessor);

public class PropertyAccessor {
	protected string folderPath = "Users properties";
	protected object jsonLocker = new();

	public PropertyAccessor() {
		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);
	}

	public string GetProperies(ulong authorId) {
		string filePath = GetPathToFile(authorId);

		lock (jsonLocker) {
			if (!File.Exists(filePath))
				filePath = GetDefaultPath();

			return File.ReadAllText(filePath);
		}
	}

	public void SetProperty(ulong authorId, string property, object value) {
		string filePath = GetPathToFile(authorId);

		if (!File.Exists(filePath))
			SetDefaultValues(authorId);

		JObject? obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath));
		if (obj is null)
			throw new NullReferenceException("PropertyAccessor.SetProperty JObject? obj");

		obj[property] = JToken.FromObject(value);

		lock (jsonLocker) {
			File.WriteAllText(filePath, JsonConvert.SerializeObject(obj));
		}
	}

	public void SetDefaultValues(ulong authorId) {
		lock (jsonLocker) {
			File.WriteAllText(GetPathToFile(authorId), File.ReadAllText(GetDefaultPath()));
		}
	}

	protected string GetPathToFile(ulong authorId) {
		return $"{folderPath}\\{authorId}.json";
	}
	protected string GetDefaultPath() {
		return $"{folderPath}\\default.json";
	}
}