using System.Text.Json.Serialization;

namespace BlazorFFT.WebGL.Models
{
	public sealed class VertexBufferModel
	{
		public sealed class VertexAttributesModel
		{
			[JsonPropertyName("attributeName")]
			public string AttributeName { get; set; }

			[JsonPropertyName("size")]
			public int Size { get; set; }

			[JsonPropertyName("stride")]
			public int Stride { get; set; }

			[JsonPropertyName("offset")]
			public int Offset { get; set; }

			[JsonPropertyName("normalized")]
			public bool Normalized { get; set; }
		}

		[JsonPropertyName("vertexData")]
		public float[] VertexData { get; set; }

		[JsonPropertyName("elementCount")]
		public int ElementCount { get; set; }

		[JsonPropertyName("vertexAttributes")]
		public VertexAttributesModel[] VertexAttributes { get; set; }
			= new VertexAttributesModel[] { };
	}
}
