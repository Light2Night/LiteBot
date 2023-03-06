using Discord.WebSocket;

using ArtApp.Web;

namespace CS_DiscordBot;

public class BotCommandHandler : CommandHandlerWithCommandList {
	public BotCommandHandler(string commandIdentifier)
		: base(commandIdentifier,
			  new List<CommandHandler>() {
				  new BotHelpHandler("?"),
				  new TimeHandler("час"),
				  new AuthorHandler("автор"),
				  new HelloHandler("привіт"),
				  new ArtHandler("арт"),
				  new RandomHandler("рандом")
			  }
		) { }
}

public class BotHelpHandler : CommandHandler {
	public BotHelpHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void DefaultAction() {
		SendMessage(
			"Доступні команди:\n" +
			"	Корисні:\n" +
			"		? - меню доступних можливостей\n" +
			"		Час\n" +
			"		Арт\n" +
			"		Рандом \"від\"-\"до\"\n" +
			"	Менш корисні\n" +
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
			SendMessage(
				"Доступні команди:\n" +
				"	\"число\" - для надсилання кількох артів\n" +
				"	джерело - список стандартних API\n" +
				"	джерело \"посилання\" - змінити джерело на нове"
			);
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
}

public class RandomHandler : CommandHandler {
	public RandomHandler(string commandIdentifier) : base(commandIdentifier) { }

	protected override void ExecuteCommand(string arguments) {
		if (arguments == string.Empty) {
			SendMessage(new Random(DateTime.Now.Millisecond).Next().ToString());
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



public class IsNotCommandException : Exception {
	public IsNotCommandException() : this("Is not command cxception") { }
	public IsNotCommandException(string message) : base(message) { }
}
public class UnknownCommandException : Exception {
	public UnknownCommandException() : this("Unknown command cxception") { }
	public UnknownCommandException(string message) : base(message) { }
}