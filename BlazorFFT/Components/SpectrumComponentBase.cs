using Blazor.Extensions;
using BlazorFFT.Utilities;
using System;
using System.Threading.Tasks;

namespace BlazorFFT.Components
{
	public abstract class SpectrumComponentBase<TRenderingContext>
		: AudioComponentBase<TRenderingContext> where TRenderingContext : RenderingContext
	{
		protected virtual int SpectrumSize => 64;
		private double[] _spectrumBuffer;
		private readonly BandPassFilter _audioFilter;

		public SpectrumComponentBase()
		{
			_audioFilter = new BandPassFilter(BufferSize, SampleRate);
		}

		protected virtual (double, double) GetDisplayFrequencyRange(double maxFrequency) =>
			(-1.0, -1.0);

		protected override Task OnInitializedAsync()
		{
			if (SpectrumSize > BufferSize / 2)
			{
				throw new InvalidOperationException(
					$"{nameof(SpectrumSize)} must be less than or " +
					$"equal do half the value of {nameof(BufferSize)}");
			}

			// setup audio filter (just allowing all bands through)
			_audioFilter.SetPassBands(1, _audioFilter.MaximumFrequency);

			return base.OnInitializedAsync();
		}

		protected override void OnProcessAudioBuffer(double[] buffer)
		{
			_audioFilter.CreateSpectrum(buffer);

			_spectrumBuffer ??= new double[SpectrumSize];

			var freqRange = GetDisplayFrequencyRange(_audioFilter.MaximumFrequency);
			_audioFilter.CompressPreviousSpectrum(_spectrumBuffer, freqRange.Item1, freqRange.Item2);
		}

		protected override Task OnRenderAsync(bool firstRender, TRenderingContext context, int width, int height) =>
			OnRenderAsync(firstRender, context, _spectrumBuffer, width, height);

		protected abstract Task OnRenderAsync(
			bool firstRender, TRenderingContext context, double[] spectrumBuffer, int width, int height);
	}
}
