using Blazor.Extensions.Canvas.WebGL;
using System.Threading.Tasks;

namespace BlazorFFT.WebGL.GLPrograms
{
	public abstract class GLProgram : GLObject<WebGLProgram>
	{
		protected readonly WebGLContext _context;

		public static implicit operator WebGLProgram(GLProgram program) => program.GLObjectValue;

		protected GLProgram(WebGLContext context)
		{
			_context = context;
		}

		protected abstract Task BindShadersToProgramAsync(WebGLProgram program);
		protected abstract Task BindDataToProgramAsync(WebGLProgram program);
		protected abstract Task OnRenderAsync(bool firstRender, int width, int height);

		public async Task<GLProgram> CreateAsync()
		{
			ResetObject();

			var program = await _context.CreateProgramAsync();

			await BindShadersToProgramAsync(program);

			await _context.LinkProgramAsync(program);
			if (!await _context.GetProgramParameterAsync<bool>(program, ProgramParameter.LINK_STATUS))
			{
				LastState = LastStateType.Failed;
				LastError = await _context.GetProgramInfoLogAsync(program);
				return this;
			}

			await _context.ValidateProgramAsync(program);
			if (!await _context.GetProgramParameterAsync<bool>(program, ProgramParameter.VALIDATE_STATUS))
			{
				LastState = LastStateType.Failed;
				LastError = await _context.GetProgramInfoLogAsync(program);
				return this;
			}

			await BindDataToProgramAsync(program);
			await _context.UseProgramAsync(program);

			LastState = LastStateType.Success;
			GLObjectValue = program;

			return this;
		}

		public async Task RenderAsync(bool firstRender, int width, int height)
		{
			ThrowIfNotValid();
			await OnRenderAsync(firstRender, width, height);
		}
	}
}
