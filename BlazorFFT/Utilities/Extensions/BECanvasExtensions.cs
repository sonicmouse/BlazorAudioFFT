using Blazor.Extensions;
using System.Reflection;

namespace BlazorFFT.Utilities.Extensions
{
	public static class BECanvasExtensions
	{
		public static string GetHtmlId(this BECanvasComponent beCanvas)
		{
			var field = typeof(BECanvasComponent).GetField("Id", BindingFlags.Instance | BindingFlags.NonPublic);
			return field == null ? null : field.GetValue(beCanvas) as string;
		}
	}
}
