using System;
using System.Numerics;

namespace BlazorFFT.Utilities
{
	public sealed class BandPassFilter
	{
		private readonly int _fftSize;
		private readonly double _sampleRate;
		private readonly Lomont.LomontFFT _fft;

		private double _lowBand;
		private double _hiBand;
		private double[] _filterBuffer;

		private readonly double[] _spectrumComplexBuffer;
		private readonly double[] _spectrumBuffer;

		public BandPassFilter(int fftSize, double sampleRate)
		{
			if(!IsPositivePowerOfTwo(fftSize))
			{
				throw new ArgumentException("Must be positive power of 2", nameof(fftSize));
			}

			_lowBand = -1;
			_hiBand = -1;
			_fftSize = fftSize;
			_sampleRate = sampleRate;
			_fft = new Lomont.LomontFFT
			{
				A = 1,
				B = -1
			};

			// double the size for the imaginary part
			_spectrumComplexBuffer = new double[2 * _fftSize];
			// in an FFT, the bottom half is the valid spectrum
			_spectrumBuffer = new double[_fftSize / 2];
		}

		public void SetPassBands(double low, double high)
		{
			_lowBand = low;
			_hiBand = high;
			UpdateFilter();
		}

		public double MaximumFrequency => _sampleRate / 2.0;

		private static bool IsPositivePowerOfTwo(int v) =>
			(v > 0) && (v & (v - 1)) == 0;

		private void ApplyFFT(double[] arr, bool forward)
		{
			_fft.FFT(new Span<double>(arr), forward);
		}

		private void UpdateFilter()
		{
			if ((_lowBand <= 0) || (_hiBand <= 0))
			{
				_filterBuffer = null;
				return;
			}

			// derive filter
			var filterBuff = new double[_fftSize];
			for (var i = 0; i < _fftSize; ++i)
			{
				double calcSinc(double freq, int index)
				{
					static double sinc(double x) =>
						x == 0 ? 1 : Math.Sin(Math.PI * x) / (Math.PI * x);
					return 2.0 * freq / _sampleRate * sinc(2.0 * freq * (index - (_fftSize / 2.0)) / _sampleRate);
				}

				double calcBlackman(int index) =>
					0.42 - 0.5 * Math.Cos(2.0 * Math.PI * index / _fftSize) + 0.08 * Math.Cos(4.0 * Math.PI * index / _fftSize);

				filterBuff[i] = calcBlackman(i) * (calcSinc(_lowBand, i) - calcSinc(_hiBand, i));
			}

			// convert filter to complex
			var filterComplexTemp = new double[_fftSize * 2];
			for(var i = 0; i < _fftSize; ++i)
			{
				filterComplexTemp[i * 2] = filterBuff[i];
			}

			// apply forward FFT
			ApplyFFT(filterComplexTemp, true);

			// save
			_filterBuffer = filterComplexTemp;
		}

		public double[] CreateSpectrum(double[] audioBuffer)
		{
			if(audioBuffer.Length != _fftSize)
			{
				throw new ArgumentException("Invalid length", nameof(audioBuffer));
			}

			var spectrumComplexBuff = new Span<double>(_spectrumComplexBuffer);
			var audioBuff = new Span<double>(audioBuffer);
			for (var i = 0; i < _fftSize; ++i)
			{
				spectrumComplexBuff[i * 2] = audioBuff[i];
				spectrumComplexBuff[i * 2 + 1] = 0;
			}

			ApplyFFT(_spectrumComplexBuffer, true);

			if (_filterBuffer != null)
			{
				var filterBuff = new Span<double>(_filterBuffer);
				for (var i = 0; i < _fftSize; ++i)
				{
					var cAudio = new Complex(spectrumComplexBuff[i * 2], spectrumComplexBuff[i * 2 + 1]);
					var cFilter = new Complex(filterBuff[i * 2], filterBuff[i * 2 + 1]);
					var cMult = cAudio * cFilter;
					spectrumComplexBuff[i * 2] = cMult.Real;
					spectrumComplexBuff[i * 2 + 1] = cMult.Imaginary;
				}
				// ApplyFFT(_spectrumComplexBuffer, false); // not doing what I had hoped.
			}

			var spectrumBuff = new Span<double>(_spectrumBuffer);
			for (var i = 0; i < _spectrumBuffer.Length; ++i)
			{
				spectrumBuff[i] = new Complex(
					spectrumComplexBuff[i * 2],
					spectrumComplexBuff[i * 2 + 1]).Magnitude;
			}
			return _spectrumBuffer;
		}

		public void CompressPreviousSpectrum(
			double[] compressed, double freqLowCutoff = -1.0,
			double freqHighCutoff = -1.0)
		{
			var spectrumSpan = new Span<double>(_spectrumBuffer);
			var compressedSpan = new Span<double>(compressed);

			if ((freqLowCutoff >= 0) && (freqHighCutoff >= 0))
			{
				var spectrumSpanTemp = new Span<double>(_spectrumBuffer);

				var lo = freqLowCutoff / MaximumFrequency;
				var hi = freqHighCutoff / MaximumFrequency;

				var tLo = (int)Math.Round(_spectrumBuffer.Length * lo);
				var tHi = (int)Math.Round(_spectrumBuffer.Length * hi);

				spectrumSpan = spectrumSpanTemp[tLo..tHi];
			}

			var groupSize = Math.Max(1,
				(int)(spectrumSpan.Length / (double)compressedSpan.Length));
			var cind = 0;
			for (var i = 0; (i < spectrumSpan.Length) &&
				(cind < compressedSpan.Length); i += groupSize, ++cind)
			{
				var t = 0.0;
				for (var x = 0; x < groupSize; ++x)
				{
					t += spectrumSpan[i + x];
				}
				compressedSpan[cind] = t / groupSize;
			}
		}
	}
}
