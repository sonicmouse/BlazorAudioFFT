using System;
using System.Linq;

namespace BlazorFFT.Utilities.NoteFinder
{
	public sealed class FrequencyNotes
	{
		private static readonly string[] _noteNames = new string[]
		{
			"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
		};
		private readonly int _fftSize;
		private readonly Note[] _noteTable;

		public FrequencyNotes(int fftSize, double sampleRate)
		{
			_fftSize = fftSize;

			// Set up frequencies based on sample rate
			_noteTable = Enumerable.Range(0, _fftSize).Select(i => new Note
			{
				Frequency = sampleRate * i / _fftSize,
				FftIndex = i
			}).ToArray();

			// Calculate pitch for equal-tempered scale: A4 = 440 Hz
			// Map to frequency
			for (var i = 0; i < 127; ++i)
			{
				var pitch = 440.0 / 32.0 * Math.Pow(2.0, (i - 9.0) / 12.0);
				if (pitch > sampleRate / 2.0) { break; }

				var min = double.MaxValue;
				var index = -1;
				for (var j = 0; j < _fftSize; ++j)
				{
					if (Math.Abs(_noteTable[j].Frequency - pitch) < min)
					{
						min = Math.Abs(_noteTable[j].Frequency - pitch);
						index = j;
					}
				}

				_noteTable[index].Name = _noteNames[i % 12];
				_noteTable[index].Pitch = pitch;
				_noteTable[index].Octave = i / 12 - 1;
			}
		}

		public DerivedNoteData DeriveNoteData(double[] fftData)
		{
			if (fftData.Length != _fftSize)
			{
				throw new InvalidOperationException(
					$"{nameof(fftData)} must have length of {_fftSize}");
			}

			var maxIndex = -1;
			{
				var maxVal = -1.0;
				for (var i = 0; i < _fftSize / 2; ++i)
				{
					var v = fftData[i] * fftData[i] +
						fftData[i] * fftData[i];
					if (v > maxVal)
					{
						maxVal = v;
						maxIndex = i;
					}
				}
			}

			var freqNote = _noteTable[maxIndex];

			var nearestNoteDelta = 0;
			while (true)
			{
				if ((nearestNoteDelta < maxIndex) &&
					_noteTable[maxIndex - nearestNoteDelta].HasName)
				{
					nearestNoteDelta = -nearestNoteDelta;
					break;
				}
				else if ((nearestNoteDelta + maxIndex < _fftSize) &&
					_noteTable[maxIndex + nearestNoteDelta].HasName)
				{
					break;
				}
				++nearestNoteDelta;
			}

			return new DerivedNoteData
			{
				FrequencyNote = freqNote,
				NearestNote = _noteTable[maxIndex + nearestNoteDelta],
			};
		}
	}
}
