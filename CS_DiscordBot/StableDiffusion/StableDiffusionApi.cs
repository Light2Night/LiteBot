using System.Net;
using System.Text.Json;
using System.Text;

namespace LiteBot.StableDiffusion;

public class StableDiffusionApi {
	private string url = "http://127.0.0.1:7860/sdapi/v1/";
	public StableDiffusionApi() { }

	public async Task<IEnumerable<MemoryStream>> GenerateImagesAsync(string postData) {
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "txt2img");
		request.Timeout = 1_200_000; // Timeout.Infinite
		request.Method = "POST";
		request.ContentType = "application/json";

		byte[] data = Encoding.UTF8.GetBytes(postData);
		request.ContentLength = data.Length;
		using (Stream stream = await request.GetRequestStreamAsync()) {
			stream.Write(data, 0, data.Length);
		}

		using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync()) {
			JsonElement images = JsonSerializer
				.Deserialize<JsonElement>(await GetJsonFromHttpWebResponseAsync(response))
				.GetProperty("images");

			return images.EnumerateArray()
				.Select(item => Convert.FromBase64String(
					item.GetString() ?? throw new NullReferenceException("item.GetString() is null"))
				)
				.Select(bytes => {
					MemoryStream stream = new();
					stream.Write(bytes, 0, bytes.Length);
					return stream;
				});
		}
	}

	public async Task<Progress> GetProgressAsync() {
		using HttpClient client = new HttpClient();
		HttpResponseMessage response = await client.GetAsync(url + "progress");

		if (!response.IsSuccessStatusCode)
			throw new Exception("Response with not success status code");

		string responseContent = await response.Content.ReadAsStringAsync();

		JsonElement json = JsonSerializer.Deserialize<JsonElement>(responseContent);
		JsonElement image = json.GetProperty("current_image");
		State state = GetStateFromStateJsonElement(json.GetProperty("state"));

		string imageInBase64 = image.GetString() ?? string.Empty;
		MemoryStream? stream = (imageInBase64 != string.Empty) ? new MemoryStream(Convert.FromBase64String(imageInBase64)) : null;

		return new Progress(state, stream);
	}

	private async Task<string> GetJsonFromHttpWebResponseAsync(HttpWebResponse response) {
		using (Stream responseStream = response.GetResponseStream())
		using (StreamReader reader = new StreamReader(responseStream)) {
			return await reader.ReadToEndAsync();
		}
	}

	private State GetStateFromStateJsonElement(JsonElement state) {
		return new State(
			state.GetProperty("skipped").GetBoolean(),
			state.GetProperty("interrupted").GetBoolean(),
			state.GetProperty("sampling_step").GetInt32(),
			state.GetProperty("sampling_steps").GetInt32()
		);
	}
}