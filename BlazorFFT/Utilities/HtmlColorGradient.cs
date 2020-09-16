using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlazorFFT.Utilities
{
	public static class HtmlColorGradient
	{
		public static string[] Calculate(Color from, Color to, int totalNumberOfColors) =>
			GetColorGradient(from, to, totalNumberOfColors).Select(ColorToHtml).ToArray();

		private static string ColorToHtml(Color c) =>
			$"#{c.R:x2}{c.G:x2}{c.B:x2}";

		private static List<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
		{
			var lst = new List<Color>(capacity: totalNumberOfColors);

			double diffR = to.R - from.R;
			double diffG = to.G - from.G;
			double diffB = to.B - from.B;

			var steps = totalNumberOfColors - 1;

			var stepR = diffR / steps;
			var stepG = diffG / steps;
			var stepB = diffB / steps;

			lst.Add(from);
			for (var i = 1; i < steps; ++i)
			{
				lst.Add(Color.FromArgb(
					c(from.R, stepR, i),
					c(from.G, stepG, i),
					c(from.B, stepB, i)));

				static int c(int fromC, double stepC, int i) =>
					(int)Math.Round(fromC + stepC * i);
			}
			lst.Add(to);

			return lst;
		}
	}
}
