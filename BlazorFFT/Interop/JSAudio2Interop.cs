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
	public interface IJSAudio2InteropDelegate
	{
		[JSInvokable]
		Task OnStartAudioListenError(string message);

		[JSInvokable]
		Task OnStartAudioListenSuccess(string id);

		[JSInvokable]
		Task OnAudioBufferReceived(object audioBuffer32bitJson);
	}

	public interface IJSAudio2Interop
	{
		ValueTask<bool> HasAudioListenStartedAsync();
		ValueTask InitializeAudioListenAsync(
			IJSAudio1InteropDelegate @delegate, int inputChannels, double sampleRate, int bufferSize);
		ValueTask StopAudioListenAsync();
		double[] ConvertJSFloat32ArrayToManaged(object audioBufferFloat32, double amp);
	}

	public sealed class JSAudio2Interop : IJSAudio2Interop
	{
		private readonly IJSRuntime _jsRuntime;

		public JSAudio2Interop(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public ValueTask InitializeAudioListenAsync(
			IJSAudio1InteropDelegate @delegate, int inputChannels, double sampleRate, int bufferSize) =>
			_jsRuntime.InvokeVoidAsync(
				"initializeAudio2Listen", DotNetObjectReference.Create(@delegate),
				inputChannels, sampleRate, bufferSize);

		public ValueTask<bool> HasAudioListenStartedAsync() =>
			_jsRuntime.InvokeAsync<bool>("hasAudio2ListenStarted");

		public ValueTask StopAudioListenAsync() =>
			_jsRuntime.InvokeVoidAsync("stopAudio2Listen");

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
