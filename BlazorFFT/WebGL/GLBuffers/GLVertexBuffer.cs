using Blazor.Extensions.Canvas.WebGL;
using BlazorFFT.WebGL.Models;
using System.Threading.Tasks;

namespace BlazorFFT.WebGL.GLBuffers
{
	public sealed class GLVertexBuffer : GLBuffer<VertexBufferModel, float>
	{
		public GLVertexBuffer(
			WebGLContext context, WebGLProgram program,
			BufferUsageHint bufferUsageHint = BufferUsageHint.STATIC_DRAW)
			: base(context, program, BufferType.ARRAY_BUFFER, bufferUsageHint)
		{
		}

		public override int ElementCount { get; protected set; }

		public override DataType DataType => DataType.FLOAT;

		protected override float[] GetBuffer(VertexBufferModel obj) => obj.VertexData;

		protected override async Task PostProcessBufferAsync(VertexBufferModel obj, WebGLBuffer glBuffer)
		{
			ElementCount = obj.ElementCount;

			foreach (var attr in obj.VertexAttributes)
			{
				var location = await _context.GetAttribLocationAsync(_program, attr.AttributeName);

				await _context.VertexAttribPointerAsync(
					index: (uint)location,
					size: attr.Size,
					type: DataType.FLOAT,
					normalized: attr.Normalized,
					stride: attr.Stride * sizeof(float),
					offset: attr.Offset * sizeof(float));

				await _context.EnableVertexAttribArrayAsync((uint)location);
			}
		}
	}
}
