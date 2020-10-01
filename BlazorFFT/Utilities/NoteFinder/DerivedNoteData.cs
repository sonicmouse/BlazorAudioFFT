using System;

namespace BlazorFFT.Utilities.NoteFinder
{
	public sealed class DerivedNoteData
	{
		public Note FrequencyNote { get; set; }
		public Note NearestNote { get; set; }
		public int NearestNoteDelta =>
			FrequencyNote.CompareTo(NearestNote);
		public double Cents =>
			1200.0 * Math.Log(FrequencyNote.Frequency / NearestNote.Pitch) / Math.Log(2.0);

		public override string ToString()
		{
			return $"Frequency Note: {FrequencyNote}\n" +
				$"Nearest Note: {NearestNote}\n" +
				$"Nearest Note Delta: {NearestNoteDelta}\n" +
				$"Cents: {Cents:0.000}";
		}
	}
}
