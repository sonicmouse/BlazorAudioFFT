using BlazorFFT.Interop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorFFT
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			builder.Services.AddTransient<IJSAudio1Interop, JSAudio1Interop>();
			builder.Services.AddTransient<IJSAudio2Interop, JSAudio2Interop>();
			builder.Services.AddTransient<IJSSizeInterop, JSSizeInterop>();
			builder.Services.AddTransient<IJSTimerInterop, JSTimerInterop>();

			return builder.Build().RunAsync();
		}
	}
}
