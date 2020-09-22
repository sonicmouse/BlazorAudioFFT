using Blazor.Extensions;
using BlazorFFT.Interop;
using BlazorFFT.Utilities;
using BlazorFFT.Utilities.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlazorFFT.Components
{
	public abstract class AudioComponentBase<TRenderingContext>
		: ComponentBase, IJSAudioInteropDelegate, IJSSizeInteropDelegate
			where TRenderingContext : RenderingContext
	{
		protected string _centerButtonDivClass = "centerButtonDiv";
		protected string _centerButtonDivBgClass = "centerButtonDivBg";

		[Inject]
		private IJSAudioInterop AudioInterop { get; set; }

		[Inject]
		private IJSSizeInterop SizeInterop { get; set; }

		protected virtual double SampleRate => 44100;
		protected virtual int BufferSize => 512;
		protected virtual int SpectrumSize => 64;
		protected virtual double AudioAmplify => 25;

		private TRenderingContext _renderingContext;
		protected BECanvasComponent _canvasReference; // needs to be exposed

		private double[] _spectrumBuffer;
		private int _canvasWidth, _canvasHeight;
		private readonly BandPassFilter _audioFilter;
		private bool _bFirstRenderHandled;

		protected AudioComponentBase()
		{
			_audioFilter = new BandPassFilter(BufferSize, SampleRate);
		}

		protected abstract Task<TRenderingContext> CreateRenderingContextAsync(BECanvasComponent canvas);

		protected abstract Task OnRenderAsync(
			bool firstRender, TRenderingContext context, double[] spectrumBuffer, int width, int height);

		protected abstract Task OnResizedComponentAsync(TRenderingContext context, int width, int height);

		protected virtual (double, double) GetDisplayFrequencyRange(double maxFrequency) =>
			(-1.0, -1.0);

		protected abstract void OnAudioBufferProcessed();

		protected async Task OnStartListeningToAudio(MouseEventArgs e)
		{
			await AudioInterop.StartAudioListenAsync(@delegate: this);
			_centerButtonDivClass = "centerButtonDivHide";
			_centerButtonDivBgClass = "centerButtonDivHide";
		}

		protected override async Task OnInitializedAsync()
		{
			if(SpectrumSize > BufferSize / 2)
			{
				throw new InvalidOperationException(
					$"{nameof(SpectrumSize)} must be less than or " +
					$"equal do half the value of {nameof(BufferSize)}");
			}

			// shut down audio if it's currently running
			if (await AudioInterop.HasAudioListenStartedAsync())
			{
				await AudioInterop.StopAudioListenAsync();
			}

			// setup audio filter (just allowing all bands through)
			_audioFilter.SetPassBands(1, _audioFilter.MaximumFrequency);

			// initialize audio
			await AudioInterop.InitializeAudioListenAsync(
				inputChannels: 1, sampleRate: SampleRate, bufferSize: BufferSize);

			// start listening for resize events
			await SizeInterop.StartNotifyResizeEventAsync(this);

			await base.OnInitializedAsync();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			try
			{
				if (firstRender)
				{
					_renderingContext = await CreateRenderingContextAsync(_canvasReference);

					var w = await SizeInterop.GetWindowWidthAsync();
					var h = await SizeInterop.GetWindowHeightAsync();
					await SetComponentSizeAsync(w, h);

					_bFirstRenderHandled = true;
				}

				if (_bFirstRenderHandled)
				{
					if (_renderingContext == null)
					{
						throw new InvalidOperationException($"{nameof(_renderingContext)} is in an invalid state");
					}

					await OnRenderAsync(firstRender, _renderingContext, _spectrumBuffer, _canvasWidth, _canvasHeight);
				}
				else
				{
					Debug.WriteLine("WARNING: Skipped OnRenderAsync due to first-render not being called called");
				}
			}
			finally
			{
				await base.OnAfterRenderAsync(firstRender);
			}
		}

		private async Task SetComponentSizeAsync(int width, int height)
		{
			var id = _canvasReference.GetHtmlId();
			if (!string.IsNullOrEmpty(id))
			{
				await SizeInterop.ResizeComponentByIdAsync(id, width, height);
				_canvasWidth = width;
				_canvasHeight = height;
			}
		}

		#region IJSSizeInteropDelegate

		public virtual async Task OnResizedWindow(int width, int height)
		{
			await SetComponentSizeAsync(width, height);
			await OnResizedComponentAsync(_renderingContext, _canvasWidth, _canvasHeight);
		}

		#endregion

		#region IJSAudioInteropDelegate

		[JSInvokable]
		public Task OnAudioBufferReceived(object audioBuffer32bitJson)
		{
			var buffer = AudioInterop.
				ConvertJSFloat32ArrayToManaged(audioBuffer32bitJson, AudioAmplify);

			_audioFilter.CreateSpectrum(buffer);

			_spectrumBuffer ??= new double[SpectrumSize];

			var freqRange = GetDisplayFrequencyRange(_audioFilter.MaximumFrequency);
			_audioFilter.CompressPreviousSpectrum(_spectrumBuffer, freqRange.Item1, freqRange.Item2);
			
			if (_bFirstRenderHandled)
			{
				OnAudioBufferProcessed();
			}

			return Task.CompletedTask;
		}

		[JSInvokable]
		public virtual Task OnStartAudioListenError(string message)
		{
			return Task.CompletedTask;
		}

		[JSInvokable]
		public virtual Task OnStartAudioListenSuccess(string id)
		{
			return Task.CompletedTask;
		}

		#endregion
	}
}
