using Discord.WebSocket;

using ArtApp.Web;
using Discord;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Channels;

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
	protected string sdJsonPath = "sd.txt";

	public StableDiffusionHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void ExecuteCommand(string arguments) {
		new Thread(() => {
			lock (this) {
				if (arguments == string.Empty) {
					DefaultAction();
				}
				else if (arguments == "?") {
					HelpMessage();
				}
				else if (IsSubcommand(arguments, "p", out string prompt) || IsSubcommand(arguments, "prompt", out prompt)) {
					SetJsonValue("prompt", prompt);
				}
				else if (IsSubcommand(arguments, "np", out string nPrompt) || IsSubcommand(arguments, "negative prompt", out nPrompt)) {
					SetJsonValue("negative_prompt", nPrompt);
				}
				else if (arguments != string.Empty) {
					SetJsonValue("prompt", arguments);
					DefaultAction();
				}
				else {
					throw new UnknownCommandException();
				}
			}
		}).Start();
	}

	protected override void DefaultAction() {
		string path = "result.png";

		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);

		SendMessage("Generation started...", messageReference: messageReference);

		MemoryStream memoryStream = GenerateImage();
		socketMessage.Channel.SendFileAsync(memoryStream, "image.png", messageReference: messageReference);
		memoryStream.Close();
	}

	protected override void HelpMessage() {
		SendMessage("""
			Доступні команди:
				`"Немає аргументів"` - генерує зображення по раніше заданому промпту
				`p "текст промпту"` або `prompt "текст промпту"` - встановлює промпт для генерації
				`np "текст анти-промпту"` або `negative prompt "текст анти-промпту"` - встановлює анти-промпт для генерації
			""");
	}

	protected MemoryStream GenerateImage() {
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:7860/sdapi/v1/txt2img");
		request.Method = "POST";
		request.ContentType = "application/json";

		string postData = File.ReadAllText(sdJsonPath);
		byte[] data = Encoding.UTF8.GetBytes(postData);
		request.ContentLength = data.Length;
		using (Stream stream = request.GetRequestStream()) {
			stream.Write(data, 0, data.Length);
		}

		using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
			string jsonText;
			using (Stream responseStream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(responseStream)) {
				jsonText = reader.ReadToEnd();
			}

			JsonElement json = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(jsonText);

			JsonElement images = json.GetProperty("images");

			MemoryStream stream = new MemoryStream();
			foreach (JsonElement item in images.EnumerateArray().ToArray()) {
				byte[] bytes = Convert.FromBase64String(item.GetString() ?? throw new Exception("item.GetString() is null"));

				stream.Write(bytes, 0, bytes.Length);
				break;
			}
			return stream;
		}
	}

	public bool IsSubcommand(string messageText, string commandIdentifier) {
		return messageText.StartsWith(commandIdentifier + " ");
	}

	public bool IsSubcommand(string messageText, string commandIdentifier, out string commandText) {
		if (!IsSubcommand(messageText, commandIdentifier)) {
			commandText = string.Empty;
			return false;
		}

		commandText = messageText.Substring(commandIdentifier.Length, messageText.Length - commandIdentifier.Length).Trim();
		return true;
	}

	protected void SetJsonValue(string property, string value) {
		JObject? obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(sdJsonPath));
		if (obj == null)
			return;

		obj[property] = value;
		File.WriteAllText(sdJsonPath, JsonConvert.SerializeObject(obj));
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