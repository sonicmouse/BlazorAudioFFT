using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorFFT.Utilities;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFFT.Components
{
	public class Canvas2dAudioComponentBase : AudioComponentBase<Canvas2DContext>
	{
		private string[] _spectrumGradientColors;
		private LevelDropper[] _spectrumLevels;

		protected override double SampleRate => 44100;
		protected override int BufferSize => 256;
		protected override int SpectrumSize => 25;
		protected override double AudioAmplify => 25;

		protected override Task<Canvas2DContext> CreateRenderingContextAsync(BECanvasComponent canvas)
		{

			return canvas.CreateCanvas2DAsync();
		}

		protected override Task OnResizedComponentAsync(Canvas2DContext context, int width, int height)
		{
			StateHasChanged();
			return Task.CompletedTask;
		}

		protected override void OnAudioBufferProcessed(long processTimeMilliseconds)
		{
			StateHasChanged();
		}

		protected override async Task OnRenderAsync(
			bool firstRender, Canvas2DContext context, double[] spectrumBuffer, int width, int height)
		{
			if(spectrumBuffer == null) { return; }

			var sw = new Stopwatch();
			sw.Start();

			var portrait = width < height;
			var barWidth = portrait ? height / (double)spectrumBuffer.Length : width / (double)spectrumBuffer.Length;

			_spectrumGradientColors ??= HtmlColorGradient.Calculate(Color.Red, Color.Orange, spectrumBuffer.Length);
			_spectrumLevels ??= spectrumBuffer.Select(x => new LevelDropper(x)).ToArray();

			for (var i = 0; i < _spectrumLevels.Length; ++i)
			{
				_spectrumLevels[i].ResetLevelIfHigher(spectrumBuffer[i]);
			}

			await context.BeginBatchAsync();
			await context.SetFillStyleAsync("black");
			await context.FillRectAsync(0, 0, width, height);
			await context.EndBatchAsync();

			await context.BeginBatchAsync();
			for (var i = 0; i < spectrumBuffer.Length; ++i)
			{
				await context.SetFillStyleAsync(_spectrumGradientColors[i]);
				var val = spectrumBuffer[i];
				if (portrait)
				{
					await context.FillRectAsync(0, i * barWidth, val, barWidth - 1);
				}
				else
				{
					await context.FillRectAsync(i * barWidth, height - val, barWidth - 1, val);
				}
			}
			await context.EndBatchAsync();

			await context.BeginBatchAsync();
			await context.SetStrokeStyleAsync("white");
			await context.SetLineWidthAsync(2);
			await context.BeginPathAsync();
			for (var i = 0; i < spectrumBuffer.Length; ++i)
			{
				var levelDrop = _spectrumLevels[i].RenderedLevel;
				if (levelDrop > 0)
				{
					if (portrait)
					{
						await context.MoveToAsync(levelDrop, i * barWidth);
						await context.LineToAsync(levelDrop, i * barWidth + barWidth - 1);
					}
					else
					{
						await context.MoveToAsync(i * barWidth, height - levelDrop);
						await context.LineToAsync(i * barWidth + barWidth - 1, height - levelDrop);
					}
				}
			}
			await context.StrokeAsync();
			await context.EndBatchAsync();

			sw.Stop();
			Debug.WriteLine($"Render time: {sw.ElapsedMilliseconds} milliseconds");
		}
	}
}
