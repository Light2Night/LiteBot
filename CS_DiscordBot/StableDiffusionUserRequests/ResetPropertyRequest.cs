using DiscordBot.StableDiffusion;
using Discord;
using Discord.WebSocket;

namespace DiscordBot.StableDiffusionUserRequests;

public class ResetPropertyRequest : UserRequest {
	protected StableDiffusionApi stableDiffusionInterface;

	public ResetPropertyRequest(SocketMessage socketMessage, StableDiffusionApi stableDiffusionInterface) : base(socketMessage) {
		this.stableDiffusionInterface = stableDiffusionInterface;
	}

	public override void Exucute() {
		stableDiffusionInterface.SetDefaultJson();

		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);
		SendMessage("Default values are set", messageReference);
	}
}