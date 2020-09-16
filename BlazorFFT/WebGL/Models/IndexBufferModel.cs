using System.Text.Json.Serialization;

namespace BlazorFFT.WebGL.Models
{
	public sealed class IndexBufferModel
	{
		[JsonPropertyName("indicesData")]
		public ushort[] IndicesData { get; set; }
	}
}
