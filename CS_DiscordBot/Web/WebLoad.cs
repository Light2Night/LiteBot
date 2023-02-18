using System.Net;
using System.Text.RegularExpressions;

namespace ArtApp.Web {
	public static class WebLoad {
		private static string LoadHTML(string url) {
			HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
			HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
			StreamReader sr = new StreamReader(myHttpWebResponse.GetResponseStream());
			return sr.ReadToEnd();
		}

		public static string GetPictureUrlFromApi(string apiUrl, string regExPattern) {
			string htmlText = WebLoad.LoadHTML(apiUrl);

			Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
			Match match = regex.Match(htmlText);

			return match.Groups[1].Value;
		}
	}
}
