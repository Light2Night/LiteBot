using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LiteBot.StableDiffusion;

public class PropertyAccessor {
	protected string folderPath = "Users properties";
	protected object jsonLocker = new();

	public PropertyAccessor() {
		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);
	}

	public string GetProperies(ulong authorId) {
		string filePath = GetPathToFile(authorId);

		lock (jsonLocker) {
			if (!File.Exists(filePath))
				filePath = GetDefaultPath();

			return File.ReadAllText(filePath);
		}
	}

	public void SetProperty(ulong authorId, string property, object value) {
		string filePath = GetPathToFile(authorId);

		if (!File.Exists(filePath))
			SetDefaultValues(authorId);

		JObject? obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath));
		if (obj is null)
			throw new NullReferenceException("PropertyAccessor.SetProperty JObject? obj");

		obj[property] = JToken.FromObject(value);

		lock (jsonLocker) {
			File.WriteAllText(filePath, JsonConvert.SerializeObject(obj));
		}
	}

	public void SetDefaultValues(ulong authorId) {
		lock (jsonLocker) {
			File.WriteAllText(GetPathToFile(authorId), File.ReadAllText(GetDefaultPath()));
		}
	}

	protected string GetPathToFile(ulong authorId) {
		return $"{folderPath}\\{authorId}.json";
	}
	protected string GetDefaultPath() {
		return $"{folderPath}\\default.json";
	}
}