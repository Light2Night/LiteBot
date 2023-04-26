using Discord;
using Discord.WebSocket;
using Discord.Rest;
using DiscordBot.StableDiffusion;

namespace DiscordBot.StableDiffusionUserRequests;

public class GenerationRequest : UserRequest {
	protected StableDiffusionApi stableDiffusionInterface;

	public GenerationRequest(SocketMessage socketMessage, StableDiffusionApi stableDiffusionInterface) : base(socketMessage) {
		this.stableDiffusionInterface = stableDiffusionInterface;
	}

	public override void Exucute() {
		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);

		RestUserMessage restUserMessage = SendMessage("Generation started...", messageReference: messageReference);
		using (MemoryStream memoryStream = stableDiffusionInterface.GenerateImage()) {
			socketMessage.Channel.SendFileAsync(memoryStream, "image.png", messageReference: messageReference);
		}

		//restUserMessage.ModifyAsync((a) => a.Content = "a");
		restUserMessage.DeleteAsync();
	}
}
