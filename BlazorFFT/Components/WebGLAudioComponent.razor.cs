using Blazor.Extensions;
using Blazor.Extensions.Canvas.WebGL;
using BlazorFFT.Components.WebGL;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlazorFFT.Components
{
	public class WebGLAudioComponentBase : AudioComponentBase<WebGLContext>
	{
		protected override double SampleRate => 8000.0;
		protected override int SpectrumSize => 25;
		protected override double AudioAmplify => 5.0;
		protected override int BufferSize => 256;

		private AudioFFTGLProgram _program;

		protected override (double, double) GetDisplayFrequencyRange(double maxFrequency)
		{
			// 8000 hz freq gives us 4000 hz max range, no need to adjust anything here
			return base.GetDisplayFrequencyRange(maxFrequency);
		}

		protected override async Task<WebGLContext> CreateRenderingContextAsync(BECanvasComponent canvas)
		{
			var context = await canvas.CreateWebGLAsync();
			_program = new AudioFFTGLProgram(context);
			return context;
		}

		protected override async Task OnResizedComponentAsync(WebGLContext context, int width, int height)
		{
			await _program.UpdateSizeAsync(width, height);
			StateHasChanged();
		}

		protected override void OnAudioBufferProcessed()
		{
			StateHasChanged();
		}

		protected override async Task OnRenderAsync(
			bool firstRender, WebGLContext context, double[] spectrumBuffer, int width, int height)
		{
			var sw = new Stopwatch();
			sw.Start();

			if (firstRender)
			{
				(await _program.CreateAsync()).ThrowIfNotValid();
			}

			_program.SpectrumBuffer = spectrumBuffer;
			await _program.RenderAsync(firstRender, width, height);

			sw.Stop();
			Debug.WriteLine($"Render time: {sw.ElapsedMilliseconds} milliseconds");
		}
	}
}
