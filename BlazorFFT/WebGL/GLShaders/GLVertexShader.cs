using Blazor.Extensions.Canvas.WebGL;

namespace BlazorFFT.WebGL.GLShaders
{
	public sealed class GLVertexShader : GLShader
	{
		public GLVertexShader(WebGLContext context, WebGLProgram program)
			: base(context, program, ShaderType.VERTEX_SHADER)
		{
		}
	}
}
