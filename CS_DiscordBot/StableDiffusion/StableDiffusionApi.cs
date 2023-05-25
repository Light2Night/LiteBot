using System.Net;
using System.Text.Json;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.StableDiffusion;

public class StableDiffusionApi {
	public StableDiffusionApi() { }

	public async Task<List<MemoryStream>> GenerateImage(string postData) {
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:7860/sdapi/v1/txt2img");
		request.Timeout = 1_200_000; // Timeout.Infinite
		request.Method = "POST";
		request.ContentType = "application/json";

		byte[] data = Encoding.UTF8.GetBytes(postData);
		request.ContentLength = data.Length;
		using (Stream stream = await request.GetRequestStreamAsync()) {
			stream.Write(data, 0, data.Length);
		}

		using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync()) {
			string jsonText;
			using (Stream responseStream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(responseStream)) {
				jsonText = reader.ReadToEnd();
			}

			JsonElement json = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(jsonText);

			JsonElement images = json.GetProperty("images");

			List<MemoryStream> imagesList = new List<MemoryStream>();
			foreach (JsonElement item in images.EnumerateArray().ToArray()) {
				MemoryStream stream = new MemoryStream();
				byte[] bytes = Convert.FromBase64String(item.GetString() ?? throw new Exception("item.GetString() is null"));
				stream.Write(bytes, 0, bytes.Length);

				imagesList.Add(stream);
			}
			return imagesList;
		}
	}

	public async Task<MemoryStream?> GetProgress() {
		string url = "http://127.0.0.1:7860/sdapi/v1/progress";

		using (HttpClient client = new HttpClient()) {
			HttpResponseMessage response = await client.GetAsync(url);

			if (!response.IsSuccessStatusCode)
				return null;

			string responseContent = await response.Content.ReadAsStringAsync();

			JsonElement image = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseContent)
				.GetProperty("current_image");

			string result = image.GetString() ?? "null";

			if (result == "null")
				return null;

			return new MemoryStream(Convert.FromBase64String(result));
		}
	}
}