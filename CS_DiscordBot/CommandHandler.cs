using Discord;
using Discord.WebSocket;
using Discord.Rest;

namespace CS_DiscordBot;

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

public class StableDiffusionQueue {
	protected Queue<Task> queue = new();
	protected object queueLocker = new();

	public StableDiffusionQueue() { }

	public void Enqueue(UserRequest request) {
		Task task = new Task(() => ExecuteAction(request.Exucute));

		lock (queueLocker) {
			queue.Enqueue(task);

			if (queue.Count == 1) {
				queue.Peek().Start();
			}
		}
	}

	protected void ExecuteAction(Action action) {
		try {
			action();
		}
		catch (Exception e) {
			//new EventHandler<Exception>(this, e);
		}
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