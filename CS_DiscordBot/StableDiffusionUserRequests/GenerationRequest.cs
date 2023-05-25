using Discord;
using Discord.WebSocket;
using Discord.Rest;
using DiscordBot.StableDiffusion;
using DiscordBot.CommandHandlers;

namespace DiscordBot.StableDiffusionUserRequests;

public class GenerationRequest : UserRequest {
	protected Pair pair;

	public GenerationRequest(SocketMessage socketMessage, Pair pair) : base(socketMessage) {
		this.pair = pair;
	}

	public override void Exucute() {
		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);
		RestUserMessage restUserMessage = SendMessage("Generation started...", messageReference: messageReference);
		StableDiffusionApi sdApi = pair.StableDiffusionApi;

		string properties = pair.PropertyAccessor.GetProperies(socketMessage.Author.Id);
		Task<List<MemoryStream>> imageGenerationTask = sdApi.GenerateImage(properties);
		//ComponentBuilder builder = new ComponentBuilder()
		//	.WithButton("1", "sd 1")
		//	.WithButton("2", "sd 2");

		while (!imageGenerationTask.IsCompleted) {
			Thread.Sleep(5000);

			using MemoryStream? image = sdApi.GetProgress().Result;
			if (image == null)
				continue;

			restUserMessage.ModifyAsync(m => {
				m.Attachments = new List<FileAttachment> { new FileAttachment(image, "image.png") };
				m.Content = "";
			}).Wait();
		}

		List<MemoryStream> imagesList = imageGenerationTask.Result;
		restUserMessage.ModifyAsync(m => {
			m.Attachments = ImagesToAttachments(imagesList).ToList();
			m.Content = "";
			//m.Components = builder.Build();
		}).Wait();

		imagesList.ForEach(image => image.Dispose());
	}

	protected IEnumerable<FileAttachment> ImagesToAttachments(IEnumerable<MemoryStream> images) {
		return images.Select(stream => new FileAttachment(stream, "image.png"));
	}
}
