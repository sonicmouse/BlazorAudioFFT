using Blazor.Extensions.Canvas.WebGL;

namespace BlazorFFT.WebGL.GLShaders
{
	public sealed class GLFragmentShader : GLShader
	{
		public GLFragmentShader(WebGLContext context, WebGLProgram program)
			: base(context, program, ShaderType.FRAGMENT_SHADER)
		{
		}
	}
}
