using BlazorFFT.Utilities.NoteFinder;
using Lomont;
using System;

namespace BlazorFFT.Utilities
{
	public sealed class NoteFinderFilter
	{
		private readonly int _fftSize;
		private readonly double _sampleRate;
		private readonly double[] _fftBufferTmp;

		private readonly HanWindow _hanWindow;
		private readonly SecondOrderLowPassFilter _soFilter;
		private readonly FrequencyNotes _frequencyNotes;
		private readonly LomontFFT _fft;

		public NoteFinderFilter(int fftSize, double sampleRate, double cornerFrequency = 440.0)
		{
			_fftSize = fftSize;
			_sampleRate = sampleRate;
			_fftBufferTmp = new double[_fftSize * 2];
			_hanWindow = new HanWindow(_fftSize);
			_soFilter = new SecondOrderLowPassFilter(_sampleRate, cornerFrequency);
			_frequencyNotes = new FrequencyNotes(_fftSize, _sampleRate);
			_fft = new LomontFFT
			{
				A = 1,
				B = -1
			};
		}

		public DerivedNoteData FindNote(double[] audioBuffer)
		{
			if(audioBuffer.Length != _fftSize)
			{
				throw new InvalidOperationException(
					$"{nameof(audioBuffer)} must be of length {_fftSize}");
			}

			_soFilter.Process(audioBuffer);
			_hanWindow.ApplyWindow(audioBuffer);

			{
				for (var i = 0; i < _fftSize; ++i)
				{
					_fftBufferTmp[i * 2] = audioBuffer[i];
					_fftBufferTmp[i * 2 + 1] = 0;
				}
				_fft.TableFFT(new Span<double>(_fftBufferTmp), true);
				for (var i = 0; i < _fftSize; ++i)
				{
					audioBuffer[i] = _fftBufferTmp[i * 2];
				}
			}

			return _frequencyNotes.DeriveNoteData(audioBuffer);
		}
	}
}
