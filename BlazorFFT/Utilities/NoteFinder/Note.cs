using System;
using System.Collections.Generic;

namespace BlazorFFT.Utilities.NoteFinder
{
	public sealed class Note : IComparer<Note>, IComparable<Note>, IEquatable<Note>
	{
		public string Name { get; set; }
		public double Pitch { get; set; } = -1.0;
		public double Frequency { get; set; } = -1.0;
		public int Octave { get; set; } = -1;
		public int FftIndex { get; set; } = -1;
		public bool HasName => !string.IsNullOrEmpty(Name);

		public int Compare(Note x, Note y) =>
			x.FftIndex - y.FftIndex;

		public int CompareTo(Note other) =>
			FftIndex - other.FftIndex;

		public bool Equals(Note other) =>
			FftIndex == other.FftIndex;

		public override string ToString()
		{
			return
				$"{Name ?? "<no name>"}: " +
				$"Frequency: {Frequency:0.000} Hz. " +
				$"Pitch: {Pitch:0.000} Hz. " +
				$"Octave: {Octave}; " +
				$"Index: {FftIndex}";
		}
	}
}
