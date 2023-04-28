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

		List<MemoryStream> imagesList = stableDiffusionInterface.GenerateImage();

		List<FileAttachment> attachments = new List<FileAttachment>();
		foreach (MemoryStream stream in imagesList) {
			attachments.Add(new FileAttachment(stream, "image.png"));
		}

		//var builder = new ComponentBuilder()
		//	.WithButton("button 1", "custom-id")
		//	.WithButton("button 2", "custom-id2");

		socketMessage.Channel.SendFilesAsync(attachments, messageReference: messageReference/*, components: builder.Build()*/).Wait();

		foreach (MemoryStream stream in imagesList) {
			stream.Dispose();
		}

		//restUserMessage.ModifyAsync((a) => a.Content = "a");
		restUserMessage.DeleteAsync();
	}
}
