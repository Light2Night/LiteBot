using Discord;
using Discord.WebSocket;
using Discord.Rest;

namespace LiteBot.StableDiffusion.UserRequests;

public abstract class UserRequest {
	protected SocketMessage socketMessage;

	public UserRequest(SocketMessage socketMessage) {
		this.socketMessage = socketMessage;
	}

	public abstract void Exucute();

	protected RestUserMessage SendMessage(string text) {
		return socketMessage.Channel.SendMessageAsync(text).Result;
	}
	protected RestUserMessage SendMessage(string text, MessageReference messageReference) {
		return socketMessage.Channel.SendMessageAsync(text, messageReference: messageReference).Result;
	}
}
