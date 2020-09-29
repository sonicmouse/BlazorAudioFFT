using System;

namespace BlazorFFT.Utilities.NoteFinder
{
	public class SecondOrderLowPassFilter
	{
		private readonly double[] _coeffA = new double[2];
		private readonly double[] _coeffB = new double[3];
		private readonly double[] _mem1 = new double[4];
		private readonly double[] _mem2 = new double[4];

		public SecondOrderLowPassFilter(double sampleRate, double cornerFrequency)
		{
			var w0 = 2.0 * Math.PI * cornerFrequency / sampleRate;
			var cosw0 = Math.Cos(w0);
			var sinw0 = Math.Sin(w0);
			var alpha = sinw0 / 2.0 * Math.Sqrt(2.0);
			var a0 = 1.0 + alpha;

			_coeffA[0] = -2.0 * cosw0 / a0;
			_coeffA[1] = (1.0 - alpha) / a0;
			_coeffB[0] = (1.0 - cosw0) / 2.0 / a0;
			_coeffB[1] = (1.0 - cosw0) / a0;
			_coeffB[2] = _coeffB[0];
		}

		private double ProcessMem(double x, double[] mem)
		{
			var ret = _coeffB[0] * x + _coeffB[1] * mem[0] + _coeffB[2] * mem[1]
						 - _coeffA[0] * mem[2] - _coeffA[1] * mem[3];

			mem[1] = mem[0];
			mem[0] = x;
			mem[3] = mem[2];
			mem[2] = ret;

			return ret;
		}

		public double Process(double x) =>
			ProcessMem(ProcessMem(x, _mem1), _mem2);

		public void Process(double[] buffer)
		{
			for (var i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = Process(buffer[i]);
			}
		}
	}
}
