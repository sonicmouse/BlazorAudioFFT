using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorFFT.Interop
{
	/// <summary>
	/// If you implement this delegate, you must add [JSInvokable] to
	/// each of your method implementations or they will *not* fire.
	/// Sometimes I wish method attributes were inherited.
	/// </summary>
	public interface IJSAudioInteropDelegate
	{
		[JSInvokable]
		Task OnStartAudioListenError(string message);

		[JSInvokable]
		Task OnStartAudioListenSuccess(string id);

		[JSInvokable]
		Task OnAudioBufferReceived(object audioBuffer32bitJson);
	}

	public interface IJSAudioInterop
	{
		ValueTask<bool> HasAudioListenStartedAsync();
		ValueTask InitializeAudioListenAsync(int inputChannels, double sampleRate, int bufferSize);
		ValueTask StartAudioListenAsync(IJSAudioInteropDelegate @delegate);
		ValueTask StopAudioListenAsync();
		double[] ConvertJSFloat32ArrayToManaged(object audioBufferFloat32, double amp);
	}

	public sealed class JSAudioInterop : IJSAudioInterop
	{
		private readonly IJSRuntime _jsRuntime;

		public JSAudioInterop(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public ValueTask InitializeAudioListenAsync(
			int inputChannels, double sampleRate, int bufferSize) =>
			_jsRuntime.InvokeVoidAsync(
				"initializeAudioListen", inputChannels, sampleRate, bufferSize);

		public ValueTask StartAudioListenAsync(IJSAudioInteropDelegate @delegate) =>
			_jsRuntime.InvokeVoidAsync("startAudioListen", DotNetObjectReference.Create(@delegate));

		public ValueTask<bool> HasAudioListenStartedAsync() =>
			_jsRuntime.InvokeAsync<bool>("hasAudioListenStarted");

		public ValueTask StopAudioListenAsync() =>
			_jsRuntime.InvokeVoidAsync("stopAudioListen");

		public double[] ConvertJSFloat32ArrayToManaged(object audioBufferFloat32, double amp)
		{
			audioBufferFloat32 = audioBufferFloat32 ??
				throw new ArgumentNullException(nameof(audioBufferFloat32));

			if (audioBufferFloat32.GetType() == typeof(JsonElement))
			{
				// there *must* be a better way to do this. Tracking here:
				// https://stackoverflow.com/questions/63759822/blazor-webassembly-send-float32array-from-javascript-to-net

				return ((JsonElement)audioBufferFloat32).
					EnumerateObject().Select(x => x.Value.GetDouble() * amp).ToArray();
			}

			throw new ArgumentException("Must be of type JsonElement", nameof(audioBufferFloat32));
		}
	}
}
