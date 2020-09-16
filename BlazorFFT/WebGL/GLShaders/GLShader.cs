using Blazor.Extensions.Canvas.WebGL;
using BlazorFFT.Utilities;
using System.Threading.Tasks;

namespace BlazorFFT.WebGL.GLShaders
{
	public abstract class GLShader : GLObject<WebGLShader>
	{
		protected readonly WebGLContext _context;
		protected readonly WebGLProgram _program;
		protected readonly ShaderType _shaderType;

		public static implicit operator WebGLShader(GLShader shader) => shader.GLObjectValue;

		protected GLShader(WebGLContext context, WebGLProgram program, ShaderType type)
		{
			_context = context;
			_program = program;
			_shaderType = type;
		}

		public virtual async Task<GLShader> CreateFromResourceAsync(string resourceName)
		{
			ResetObject();

			var shader = await _context.CreateShaderAsync(_shaderType);

			await _context.ShaderSourceAsync(shader, await ResourceLoader.LoadTextAsync(resourceName));
			await _context.CompileShaderAsync(shader);
			if (!await _context.GetShaderParameterAsync<bool>(shader, ShaderParameter.COMPILE_STATUS))
			{
				LastError = await _context.GetShaderInfoLogAsync(shader);
				LastState = LastStateType.Failed;
				return this;
			}

			await _context.AttachShaderAsync(_program, shader);

			LastState = LastStateType.Success;
			GLObjectValue = shader;

			return this;
		}
	}
}
