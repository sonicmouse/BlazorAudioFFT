using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFFT.Interop
{
	public interface IJSSizeInteropDelegate
	{
		public Task OnResizedWindow(int width, int height);
	}

	public interface IJSSizeInterop
	{
		ValueTask<int> GetWindowWidthAsync();
		ValueTask<int> GetWindowHeightAsync();
		ValueTask StartNotifyResizeEventAsync(IJSSizeInteropDelegate @delegate);
		ValueTask ResizeComponentByIdAsync(string id, int width, int height);
	}

	public sealed class JSSizeInterop : IJSSizeInterop
	{
		private readonly IJSRuntime _jsRuntime;
		private static readonly List<IJSSizeInteropDelegate>
			_resizeDelegates = new List<IJSSizeInteropDelegate>();

		public JSSizeInterop(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public ValueTask<int> GetWindowWidthAsync() =>
			_jsRuntime.InvokeAsync<int>("getWindowWidth");

		public ValueTask<int> GetWindowHeightAsync() =>
			_jsRuntime.InvokeAsync<int>("getWindowHeight");

		public async ValueTask StartNotifyResizeEventAsync(IJSSizeInteropDelegate @delegate)
		{
			_resizeDelegates.Add(@delegate);
			if (_resizeDelegates.Count == 1)
			{
				// `OnResizedWindow` targeting the static method below
				await _jsRuntime.InvokeVoidAsync("addResizeEventCallback", "BlazorFFT", "OnResizedWindow");
			}
		}

		[JSInvokable]
		public static Task OnResizedWindow(int width, int height) =>
			Task.WhenAll(_resizeDelegates.Select(x => x.OnResizedWindow(width, height)));

		public ValueTask ResizeComponentByIdAsync(string id, int width, int height) =>
			_jsRuntime.InvokeVoidAsync("resizeComponentById", id, width, height);
	}
}
