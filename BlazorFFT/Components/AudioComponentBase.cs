using Blazor.Extensions;
using BlazorFFT.Interop;
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
		#region Properties and Members

		[Inject]
		private IJSAudioInterop AudioInterop { get; set; }

		[Inject]
		private IJSSizeInterop SizeInterop { get; set; }

		protected virtual double SampleRate => 44100;
		protected virtual int BufferSize => 512;
		protected virtual double AudioAmplify => 1.0;

		// These 3 members must be exosed
		protected string _centerButtonDivClass = "centerButtonDiv";
		protected string _centerButtonDivBgClass = "centerButtonDivBg";
		protected BECanvasComponent _canvasReference;

		private TRenderingContext _renderingContext;
		private int _canvasWidth, _canvasHeight;
		private bool _bFirstRenderHandled;

		#endregion

		#region Abstract
		protected abstract Task<TRenderingContext> CreateRenderingContextAsync(BECanvasComponent canvas);

		protected abstract void OnProcessAudioBuffer(double[] buffer);

		protected abstract Task OnRenderAsync(bool firstRender, TRenderingContext context, int width, int height);

		protected abstract Task OnResizedComponentAsync(TRenderingContext context, int width, int height);

		protected abstract void OnAudioBufferProcessed();
		#endregion

		#region Page Methods

		/// <summary>
		/// This should be
		/// </summary>
		protected async Task OnStartListeningToAudio(MouseEventArgs e)
		{
			await AudioInterop.StartAudioListenAsync(@delegate: this);
			_centerButtonDivClass = "centerButtonDivHide";
			_centerButtonDivBgClass = "centerButtonDivHide";
		}

		#endregion

		#region Component Overrides

		protected override async Task OnInitializedAsync()
		{
			// shut down audio if it's currently running
			if (await AudioInterop.HasAudioListenStartedAsync())
			{
				await AudioInterop.StopAudioListenAsync();
			}

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

					var width = await SizeInterop.GetWindowWidthAsync();
					var height = await SizeInterop.GetWindowHeightAsync();
					await SetComponentSizeAsync(width, height);

					_bFirstRenderHandled = true;
				}

				if (_bFirstRenderHandled)
				{
					if (_renderingContext == null)
					{
						throw new InvalidOperationException($"{nameof(_renderingContext)} is in an invalid state");
					}

					await OnRenderAsync(firstRender, _renderingContext, _canvasWidth, _canvasHeight);
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
		#endregion

		#region Utilities

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

		#endregion

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

			OnProcessAudioBuffer(buffer);
			
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
