using Blazor.Extensions.Canvas.WebGL;
using System.Threading.Tasks;

namespace BlazorFFT.WebGL.GLBuffers
{
	public abstract class GLBuffer<TBufferObject, TBufferType> : GLObject<WebGLBuffer>
	{
		protected readonly WebGLContext _context;
		protected readonly WebGLProgram _program;
		private readonly BufferUsageHint _bufferUsageHint;
		private readonly BufferType _bufferType;

		protected GLBuffer(WebGLContext context, WebGLProgram program, BufferType bufferType,
			BufferUsageHint bufferUsageHint = BufferUsageHint.STATIC_DRAW)
		{
			_context = context;
			_bufferUsageHint = bufferUsageHint;
			_program = program;
			_bufferType = bufferType;
		}

		public abstract int ElementCount { get; protected set; }
		public abstract DataType DataType { get; }

		protected abstract TBufferType[] GetBuffer(TBufferObject obj);
		protected abstract Task PostProcessBufferAsync(TBufferObject obj, WebGLBuffer glBuffer);

		public async Task<GLBuffer<TBufferObject, TBufferType>> ConsumeAndBindBufferAsync(TBufferObject obj)
		{
			ResetObject();

			var buffer = await _context.CreateBufferAsync();

			await _context.BindBufferAsync(_bufferType, buffer); // select
			await _context.BufferDataAsync(_bufferType, GetBuffer(obj), _bufferUsageHint);

			await PostProcessBufferAsync(obj, buffer);

			LastState = LastStateType.Success;
			GLObjectValue = buffer;

			return this;
		}

		public Task BindBufferAsync()
		{
			ThrowIfNotValid();
			return _context.BindBufferAsync(_bufferType, GLObjectValue);
		}
	}
}
