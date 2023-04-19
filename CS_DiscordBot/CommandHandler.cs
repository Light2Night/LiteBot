using Discord;

using ArtApp.Web;
using Discord.WebSocket;

namespace CS_DiscordBot;

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

public class TimeHandler : CommandHandler {
	public TimeHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void DefaultAction() {
		SendMessage($"{DateTime.Now.ToLongTimeString()}\n{DateTime.Now.ToLongDateString()}");
		if (DateTime.Now.Hour < 8)
			SendMessage("Іншими словами час спати");
	}
}

public class AuthorHandler : CommandHandler {
	public AuthorHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void DefaultAction() {
		SendMessage("<@!883836608963555339> Lite#5625");
	}
}

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

public class ArtHandler : CommandHandler {
	public ArtHandler(string commandIdentifier) : base(commandIdentifier) { }
	protected readonly string apiFilePath = "api.txt";

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			DefaultAction();
		}
		else if (arguments == "?") {
			HelpMessage();
		}
		else if (arguments == "джерело") {
			SendMessage(
				"Джерела що підтримуються:\n" +
				"	https://api.waifu.pics/sfw/neko\n" +
				"	https://api.waifu.im/search/?included_tags=maid\n" +
				"	https://api.waifu.im/search/?included_tags=waifu\n" +
				"	https://api.waifu.im/search/?included_tags=marin-kitagawa\n" +
				"	https://api.waifu.im/search/?included_tags=mori-calliope\n" +
				"	https://api.waifu.im/search/?included_tags=raiden-shogun\n" +
				"	https://api.waifu.im/search/?included_tags=oppai\n" +
				"	https://api.waifu.im/search/?included_tags=selfies\n" +
				"	https://api.waifu.im/search/?included_tags=uniform\n" +
				"Можа спробувати ввести інше джерело, можливо воно буде працювати"
			);
		}
		else if (arguments.StartsWith("джерело")) {
			File.WriteAllText(apiFilePath, arguments.Remove(0, "джерело".Length).Trim());

			SendMessage("Джерело змінено");
		}
		else if (TypeChecker.IsUInt32(arguments)) {
			uint numberOfPictures = Convert.ToUInt32(arguments);
			if (numberOfPictures > 10) {
				SendMessage("Занадто багато зображень");
				return;
			}

			for (uint i = 0; i < numberOfPictures; i++) {
				DefaultAction();
			}
		}
		else {
			throw new UnknownCommandException();
		}
	}

	protected override void DefaultAction() {
		SendMessage(WebLoad.GetPictureUrlFromApi(File.ReadAllText(apiFilePath), "\"url\":\"([^\"]*)\""));
	}

	protected override void HelpMessage() {
		SendMessage(
			"Доступні команди:\n" +
				"	\"Немає аргументів\" - вивід одного арту\n" +
				"	\"число\" - для надсилання кількох артів\n" +
				"	джерело - список стандартних API\n" +
				"	джерело \"посилання\" - змінити джерело на нове"
		);
	}
}

public class RandomHandler : CommandHandler {
	protected Random random = new Random(DateTime.Now.Millisecond);

	public RandomHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			SendMessage(random.Next().ToString());
		}
		else if (arguments == "?") {
			HelpMessage();
		}
		else if (IsRandRange(arguments, out uint first, out uint second)) {
			if (first > second) {
				SendMessage("Некоректний діапазон");
				return;
			}

			SendMessage(new Random(DateTime.Now.Millisecond).Next((int)first, (int)second + 1).ToString());
		}
		else {
			throw new UnknownCommandException();
		}
	}

	protected override void HelpMessage() {
		SendMessage(
			"Доступні команди:\n" +
				"	\"Немає аргументів\" - надсилає випадкове число\n" +
				"	\"число\"-\"число\" - надсилає число в заданому діапазоні"
		);
	}

	protected bool IsRandRange(string argument, out uint first, out uint second) {
		first = second = 0;

		string[] arguments = argument.Split("-");
		if (arguments.Length != 2)
			return false;

		if (!TypeChecker.IsUInt32(arguments[0]) && TypeChecker.IsUInt32(arguments[1]))
			return false;

		first = Convert.ToUInt32(arguments[0]);
		second = Convert.ToUInt32(arguments[1]);
		return true;
	}
}

public class StableDiffusionHandler : CommandHandler {
	protected StableDiffusionApi stableDiffusionInterface = new();
	protected StableDiffusionQueue diffusionQueue = new();

	public StableDiffusionHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			diffusionQueue.Enqueue(new GenerationRequest(GenerateAndSend, socketMessage));
		}
		else if (arguments == "?") {
			HelpMessage();
		}
		else if (IsSubcommand(arguments, "p", out string prompt) || IsSubcommand(arguments, "prompt", out prompt)) {
			diffusionQueue.Enqueue(new SetPromptRequest(stableDiffusionInterface.SetJsonValue, socketMessage, "prompt", prompt));
		}
		else if (IsSubcommand(arguments, "np", out prompt) || IsSubcommand(arguments, "negative prompt", out prompt)) {
			diffusionQueue.Enqueue(new SetPromptRequest(stableDiffusionInterface.SetJsonValue, socketMessage, "negative_prompt", prompt));
		}
		else if (arguments != string.Empty) {
			diffusionQueue.Enqueue(new SetPromptRequest(stableDiffusionInterface.SetJsonValue, socketMessage, "prompt", arguments));
			diffusionQueue.Enqueue(new GenerationRequest(GenerateAndSend, socketMessage));
		}
		else {
			throw new UnknownCommandException();
		}
	}

	protected override void DefaultAction() {
		GenerateAndSend(socketMessage);
	}

	protected void GenerateAndSend(SocketMessage socketMessage) {
		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);

		SendMessage("Generation started...", messageReference: messageReference);

		MemoryStream memoryStream = stableDiffusionInterface.GenerateImage();
		socketMessage.Channel.SendFileAsync(memoryStream, "image.png", messageReference: messageReference);
		memoryStream.Close();
	}

	protected override void HelpMessage() {
		SendMessage("""
			Доступні команди:
				`"Немає аргументів"` - генерує зображення по раніше заданому промпту
				`p "текст промпту"` або `prompt "текст промпту"` - встановлює промпт для генерації
				`np "текст анти-промпту"` або `negative prompt "текст анти-промпту"` - встановлює анти-промпт для генерації
				`"текст промпту"` - встановлює промпт та запускає генерацію
			""");
	}
}

public abstract class UserRequest { }

public class GenerationRequest : UserRequest {
	public delegate void Generation(SocketMessage socketMessage);

	protected Generation generationMethod;
	protected SocketMessage socketMessage;

	public GenerationRequest(Generation generationMethod, SocketMessage socketMessage) {
		this.generationMethod = generationMethod;
		this.socketMessage = socketMessage;
	}

	public Generation GenerationMethod {
		get => generationMethod;
	}
	public SocketMessage SocketMessage {
		get => socketMessage;
	}
}

public class SetPromptRequest : UserRequest {
	public delegate void SetJsonValue(string property, string value);

	protected SetJsonValue setJsonValueMethod;
	protected SocketMessage socketMessage;
	protected string property;
	protected string value;

	public SetPromptRequest(SetJsonValue setJsonValueMethod, SocketMessage socketMessage, string property, string value) {
		this.setJsonValueMethod = setJsonValueMethod;
		this.socketMessage = socketMessage;
		this.property = property;
		this.value = value;
	}

	public SetJsonValue SetJsonValueMethod {
		get => setJsonValueMethod;
	}
	public SocketMessage SocketMessage {
		get => socketMessage;
	}
	public string Property {
		get => property;
	}
	public string Value {
		get => value;
	}
}

public class StableDiffusionQueue {
	protected Queue<Task> queue = new();
	protected object queueLocker = new();

	public StableDiffusionQueue() { }

	public void Enqueue(UserRequest request) {
		Task? task = null;

		if (request is GenerationRequest generation) {
			task = new Task(() => {
				StartAction(() => generation.GenerationMethod(generation.SocketMessage));
			});
		}
		else if (request is SetPromptRequest promptRequest) {
			task = new Task(() => {
				StartAction(() => promptRequest.SetJsonValueMethod(promptRequest.Property, promptRequest.Value));
			});
		}

		if (task == null)
			return;

		lock (queueLocker) {
			queue.Enqueue(task);

			if (queue.Count == 1) {
				queue.Peek().Start();
			}
		}
	}

	protected void StartAction(Action action) {
		try {
			action();
		}
		//catch (Exception e) {

		//}
		finally {
			DequeueAndStartNext();
		}
	}

	protected void DequeueAndStartNext() {
		lock (queueLocker) {
			queue.Dequeue();
			if (queue.Count > 0)
				queue.Peek().Start();
		}
	}
}

public class IsNotCommandException : Exception {
	public IsNotCommandException() : this("Is not command cxception") { }
	public IsNotCommandException(string message) : base(message) { }
}
public class UnknownCommandException : Exception {
	public UnknownCommandException() : this("Unknown command cxception") { }
	public UnknownCommandException(string message) : base(message) { }
}