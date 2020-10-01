using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorFFT.Utilities;
using BlazorFFT.Utilities.NoteFinder;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorFFT.Components
{
	public class NoteFinderComponentBase
		: AudioComponentBase<Canvas2DContext>
	{
		protected override int BufferSize => 8192;
		protected override double SampleRate => 48000;

		private readonly NoteFinderFilter _noteFinder;

		private DerivedNoteData _note;
		private Timer _timerClear;

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
			if(note.FrequencyNote.Frequency < 30) { return; }

			_note = note;
			if(_timerClear != null)
			{
				_timerClear.Dispose();
			}

			_timerClear = new Timer(_ =>
			{
				_timerClear.Dispose();
				_timerClear = null;
				_note = null;
				StateHasChanged();
			}, null, 5000, Timeout.Infinite);
		}

		protected override async Task OnRenderAsync(bool firstRender, Canvas2DContext context, int width, int height)
		{
			const int NoteSize = 122;
			const int InfoSize = 18;

			var centerx = width / 2.0;
			var centery = height / 2.0;

			await context.SetFillStyleAsync("#0B6603");
			await context.FillRectAsync(0, 0, width, height);

			await context.SetTextAlignAsync(TextAlign.Center);
			await context.SetFillStyleAsync("white");
			

			if (_note == null)
			{
				await context.SetFontAsync($"bold {InfoSize}px Space Mono");
				await context.FillTextAsync("Listening...", centerx, centery);
				return;
			}

			await context.SetFontAsync($"{NoteSize}px Space Mono");
			await context.FillTextAsync(_note.NearestNote.Name, centerx, centery);

			await context.SetFontAsync($"{InfoSize}px Space Mono");
			await context.FillTextAsync($"{_note.NearestNote.Pitch:0.000} Hz.", centerx, centery + (NoteSize / 2));

			var cents = string.Empty;
			if (_note.NearestNoteDelta != 0)
			{
				if (_note.Cents < 0)
				{
					cents = $"FLAT ({Math.Abs(_note.Cents):0.000} ¢)";
				}
				else if (_note.Cents > 0)
				{
					cents = $"SHARP ({_note.Cents:0.000} ¢)";
				}
			}
			await context.FillTextAsync(cents, centerx, centery + (NoteSize / 2) + InfoSize + 10);
		}

		protected override Task OnResizedComponentAsync(Canvas2DContext context, int width, int height)
		{
			StateHasChanged();
			return Task.CompletedTask;
		}
	}
}
