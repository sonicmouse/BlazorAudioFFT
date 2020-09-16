using Blazor.Extensions.Canvas.WebGL;
using BlazorFFT.WebGL.Models;
using System.Threading.Tasks;

namespace BlazorFFT.WebGL.GLBuffers
{
	public class GLIndexbuffer : GLBuffer<IndexBufferModel, ushort>
	{
		public GLIndexbuffer(
			WebGLContext context, BufferUsageHint bufferUsageHint = BufferUsageHint.STATIC_DRAW) : 
			base(context, null, BufferType.ELEMENT_ARRAY_BUFFER, bufferUsageHint)
		{
		}

		public override int ElementCount { get; protected set; }

		public override DataType DataType => DataType.UNSIGNED_SHORT;

		protected override ushort[] GetBuffer(IndexBufferModel obj) => obj.IndicesData;

		protected override Task PostProcessBufferAsync(IndexBufferModel obj, WebGLBuffer glBuffer)
		{
			ElementCount = obj.IndicesData.Length;
			return Task.CompletedTask;
		}
	}
}
