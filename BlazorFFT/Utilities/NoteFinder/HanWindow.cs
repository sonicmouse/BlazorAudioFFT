using System;
using System.IO;

namespace BlazorFFT.Utilities.NoteFinder
{
	public class HanWindow
	{
		private readonly double[] _window;

		public HanWindow(int fftSize)
		{
			_window = new double[fftSize];
			for (int i = 0; i < fftSize; ++i)
			{
				_window[i] = .5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (fftSize - 1.0)));
			}
		}

		public void ApplyWindow(double[] data)
		{
			if(data.Length != _window.Length)
			{
				throw new InvalidDataException(
					$"{nameof(data)} must be of length {_window.Length}");
			}

			for (var i = 0; i < _window.Length; ++i)
			{
				data[i] *= _window[i];
			}
		}
	}
}
