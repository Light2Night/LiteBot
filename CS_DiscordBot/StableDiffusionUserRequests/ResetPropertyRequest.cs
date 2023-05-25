using DiscordBot.StableDiffusion;
using Discord;
using Discord.WebSocket;
using DiscordBot.CommandHandlers;

namespace DiscordBot.StableDiffusionUserRequests;

public class ResetPropertyRequest : UserRequest {
	protected PropertyAccessor propertyAccessor;

	public ResetPropertyRequest(SocketMessage socketMessage, PropertyAccessor propertyAccessor) : base(socketMessage) {
		this.propertyAccessor = propertyAccessor;
	}

	public override void Exucute() {
		propertyAccessor.SetDefaultValues(socketMessage.Author.Id);

		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);
		SendMessage("Default values are set", messageReference);
	}
}