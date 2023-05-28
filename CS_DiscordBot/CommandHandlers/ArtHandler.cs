using ArtApp.Web;
using LiteBot.CommandHandlersBase;
using LiteBot.Exceptions;

namespace LiteBot.CommandHandlers;

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
