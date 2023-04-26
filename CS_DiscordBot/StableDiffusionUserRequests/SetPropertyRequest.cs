using DiscordBot.StableDiffusion;
using Discord;
using Discord.WebSocket;

namespace DiscordBot.StableDiffusionUserRequests;

public class SetPropertyRequest : UserRequest {
	protected StableDiffusionApi stableDiffusionInterface;

	protected string property;
	protected object value;

	public SetPropertyRequest(SocketMessage socketMessage, StableDiffusionApi stableDiffusionInterface, string property, object value) : base(socketMessage) {
		this.stableDiffusionInterface = stableDiffusionInterface;
		this.property = property;
		this.value = value;
	}

	public override void Exucute() {
		stableDiffusionInterface.SetJsonValue(property, value);

		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);
		SendMessage($"A new property value is set to: {property}", messageReference);
	}
}
