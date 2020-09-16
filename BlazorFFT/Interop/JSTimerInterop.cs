using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorFFT.Interop
{
	/// <summary>
	/// If you implement this delegate, you must add [JSInvokable] to
	/// each of your method implementations or they will *not* fire.
	/// Sometimes I wish method attributes were inherited.
	/// </summary>
	public interface IJSTimerInteropDelegate
	{
		[JSInvokable]
		Task OnIntervalTimer(string id);
	}

	public interface IJSTimerInterop
	{
		ValueTask SetIntervalAsync(string id, int interval, IJSTimerInteropDelegate @delegate);
		ValueTask ClearIntervalAsync(string id);
	}

	public sealed class JSTimerInterop : IJSTimerInterop
	{
		private readonly IJSRuntime _jsRuntime;

		public JSTimerInterop(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public ValueTask SetIntervalAsync(string id, int interval, IJSTimerInteropDelegate @delegate) =>
			_jsRuntime.InvokeVoidAsync("setCallbackTimer",
				DotNetObjectReference.Create(@delegate), "OnIntervalTimer", id, interval);

		public ValueTask ClearIntervalAsync(string id) =>
			_jsRuntime.InvokeVoidAsync("clearCallbackTimer", id);
	}
}
