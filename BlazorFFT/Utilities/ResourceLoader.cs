using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorFFT.Utilities
{
	public static class ResourceLoader
	{
		public static async Task<string> LoadTextAsync(string resourceName)
		{
			using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			using var sr = new StreamReader(s);
			return await sr.ReadToEndAsync();
		}

		public static async Task<TResult> LoadAsJson<TResult>(string resourceName)
		{
			using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			return await JsonSerializer.DeserializeAsync<TResult>(s, new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
		}
	}
}
