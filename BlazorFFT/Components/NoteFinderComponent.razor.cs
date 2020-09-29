using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorFFT.Utilities;
using System.Threading.Tasks;

namespace BlazorFFT.Components
{
	public class NoteFinderComponentBase
		: AudioComponentBase<Canvas2DContext>
	{
		protected override double AudioAmplify => 1.0;
		protected override int BufferSize => 8192;
		protected override double SampleRate => 48000;

		private readonly NoteFinderFilter _noteFinder;

		public NoteFinderComponentBase()
		{
			_noteFinder = new NoteFinderFilter(BufferSize, SampleRate);
		}

		protected override Task<Canvas2DContext> CreateRenderingContextAsync(BECanvasComponent canvas)
		{
			return canvas.CreateCanvas2DAsync();
		}

		protected override void OnAudioBufferProcessed()
		{
			StateHasChanged();
		}

		protected override void OnProcessAudioBuffer(double[] buffer)
		{
			var note = _noteFinder.FindNote(buffer);
			if(note.FrequencyNote.Frequency > 30)
			{
				System.Diagnostics.Debug.WriteLine(note);
			}
		}

		protected override async Task OnRenderAsync(bool firstRender, Canvas2DContext context, int width, int height)
		{
			await context.SetFillStyleAsync("black");
			await context.FillRectAsync(0, 0, width, height);
		}

		protected override Task OnResizedComponentAsync(Canvas2DContext context, int width, int height)
		{
			StateHasChanged();
			return Task.CompletedTask;
		}
	}
}
