using Discord;
using Discord.WebSocket;
using Discord.Rest;

namespace LiteBot.StableDiffusion.UserRequests;

public class GenerationRequest : UserRequest {
	protected ApiWithProperties apiWithProperties;

	public GenerationRequest(SocketMessage socketMessage, ApiWithProperties apiWithProperties) : base(socketMessage) {
		this.apiWithProperties = apiWithProperties;
	}

	public override void Exucute() {
		MessageReference messageReference = new MessageReference(socketMessage.Id, socketMessage.Channel.Id);
		RestUserMessage restUserMessage = SendMessage("Generation started...", messageReference: messageReference);

		try {
			GenerateImageAsync(restUserMessage).Wait();
		}
		catch (Exception ex) {
			restUserMessage.ModifyAsync(m => m.Content = $"Unexpected exception: {ex}");
		}
	}

	private async Task GenerateImageAsync(RestUserMessage restUserMessage) {
		StableDiffusionApi api = apiWithProperties.StableDiffusionApi;
		string properties = apiWithProperties.PropertyAccessor.GetProperies(socketMessage.Author.Id);

		Task<IEnumerable<MemoryStream>> imagesGenerationTask = api.GenerateImages(properties);

		while (!imagesGenerationTask.IsCompleted) {
			await Task.Delay(5000);
			//try {
			Progress progress = await api.GetProgress();
			using MemoryStream? image = progress.Image;
			if (image == null)
				continue;

			await restUserMessage.ModifyAsync(m => {
				m.Content = $"Progress: {progress.State.SamplingStep}/{progress.State.SamplingSteps}";
				m.Attachments = new List<FileAttachment> { new FileAttachment(image, "image.png") };
			});
			//}
			//catch (Exception) { }
		}

		IEnumerable<MemoryStream> imagesList = await imagesGenerationTask;
		await restUserMessage.ModifyAsync(m => {
			m.Content = "";
			m.Attachments = ImagesToAttachments(imagesList).ToList();
			//m.Components = CreateComponentBuilder().Build();
		});

		imagesList.ToList().ForEach(image => image.Dispose());
	}

	protected IEnumerable<FileAttachment> ImagesToAttachments(IEnumerable<MemoryStream> images) {
		return images.Select(stream => new FileAttachment(stream, "image.png"));
	}

	private ComponentBuilder CreateComponentBuilder() {
		ComponentBuilder builder = new ComponentBuilder()
			.WithButton("1", "sd 1")
			.WithButton("2", "sd 2");

		return builder;
	}
}
