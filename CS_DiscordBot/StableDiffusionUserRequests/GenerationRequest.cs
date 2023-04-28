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

		try {
			List<MemoryStream> imagesList = stableDiffusionInterface.GenerateImage();

			List<FileAttachment> attachments = new List<FileAttachment>();
			foreach (MemoryStream stream in imagesList) {
				attachments.Add(new FileAttachment(stream, "image.png"));
			}

			//ComponentBuilder builder = new ComponentBuilder()
			//	.WithButton("1", "sd 1")
			//	.WithButton("2", "sd 2");

			socketMessage.Channel.SendFilesAsync(attachments, messageReference: messageReference/*, components: builder.Build()*/).Wait();

			foreach (MemoryStream stream in imagesList) {
				stream.Dispose();
			}
		}
		finally {
			//restUserMessage.ModifyAsync((a) => a.Content = "a");
			restUserMessage.DeleteAsync();
		}
	}
}
