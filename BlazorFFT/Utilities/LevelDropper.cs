using System;

namespace BlazorFFT.Utilities
{
	public sealed class LevelDropper
	{
		private const double Gravity = 9.81 * 4.0;

		private double _baseLevel;
		private DateTime _timeCreated;

		public LevelDropper(double level)
		{
			_baseLevel = level;
			_timeCreated = DateTime.Now;
		}

		public double RenderedLevel
		{
			get
			{
				double e = (DateTime.Now - _timeCreated).TotalSeconds;
				var level = _baseLevel - (Gravity * (e * e) / 2.0);
				if (level < 0) { level = 0; }
				return level;
			}
		}

		public void ResetLevelIfHigher(double level)
		{
			if(RenderedLevel < level)
			{
				_baseLevel = level;
				_timeCreated = DateTime.Now;
			}
		}
	}
}
