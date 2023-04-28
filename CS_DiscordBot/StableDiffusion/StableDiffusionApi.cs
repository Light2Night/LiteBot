using System.Net;
using System.Text.Json;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.StableDiffusion;

public class StableDiffusionApi {
	protected string sdJsonPath = "sd.txt";
	protected object jsonLocker = new();
	protected string defaultSdJsonPath = "sd_default.json";

	public StableDiffusionApi() {

	}

	public List<MemoryStream> GenerateImage() {
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:7860/sdapi/v1/txt2img");
		request.Timeout = 300 * 1000; // Timeout.Infinite
		request.Method = "POST";
		request.ContentType = "application/json";

		string postData;
		lock (jsonLocker) {
			postData = File.ReadAllText(sdJsonPath);
		}

		byte[] data = Encoding.UTF8.GetBytes(postData);
		request.ContentLength = data.Length;
		using (Stream stream = request.GetRequestStream()) {
			stream.Write(data, 0, data.Length);
		}

		using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
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

	public void SetJsonValue(string property, object value) {
		JObject? obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(sdJsonPath));
		if (obj == null)
			return;

		obj[property] = JToken.FromObject(value);
		lock (jsonLocker) {
			File.WriteAllText(sdJsonPath, JsonConvert.SerializeObject(obj));
		}
	}

	public void SetDefaultJson() {
		lock (jsonLocker) {
			File.WriteAllText(sdJsonPath, File.ReadAllText(defaultSdJsonPath));
		}
	}
}
