using Blazor.Extensions.Canvas.WebGL;
using BlazorFFT.Utilities;
using BlazorFFT.Utilities.Extensions;
using BlazorFFT.WebGL.GLBuffers;
using BlazorFFT.WebGL.GLPrograms;
using BlazorFFT.WebGL.GLShaders;
using BlazorFFT.WebGL.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace BlazorFFT.Components.WebGL
{
	public sealed class AudioFFTGLProgram : GLProgram
	{
		public double[] SpectrumBuffer { get; set; }

		private WebGLUniformLocation _matLocWorld;
		private WebGLUniformLocation _matLocView;
		private WebGLUniformLocation _matLocProj;
		private GLVertexBuffer _vertexBuffer;
		private GLIndexbuffer _indexBuffer;

		private readonly Matrix4x4 _matWorld = Matrix4x4.Identity;
		private readonly int _tickStart = Environment.TickCount;

		public AudioFFTGLProgram(WebGLContext context) : base(context)
		{
		}

		protected override async Task BindShadersToProgramAsync(WebGLProgram program)
		{
			var vertexShader = await new GLVertexShader(_context, program)
				.CreateFromResourceAsync("BlazorFFT.Components.WebGL.GLSL.VertexShader.glsl");

			var fragmentShader = await new GLFragmentShader(_context, program)
				.CreateFromResourceAsync("BlazorFFT.Components.WebGL.GLSL.FragmentShader.glsl");

			vertexShader.ThrowIfNotValid();
			fragmentShader.ThrowIfNotValid();
		}

		protected override async Task BindDataToProgramAsync(WebGLProgram program)
		{
			_vertexBuffer = new GLVertexBuffer(_context, program);
			await _vertexBuffer.ConsumeAndBindBufferAsync(
				await ResourceLoader.LoadAsJsonAsync<VertexBufferModel>(
				"BlazorFFT.Components.WebGL.JSON.BoxVertexBuffer.json"));
			_vertexBuffer.ThrowIfNotValid();

			_indexBuffer = new GLIndexbuffer(_context);
			await _indexBuffer.ConsumeAndBindBufferAsync(
				await ResourceLoader.LoadAsJsonAsync<IndexBufferModel>(
				"BlazorFFT.Components.WebGL.JSON.BoxIndexBuffer.json"));
			_indexBuffer.ThrowIfNotValid();

			_matLocWorld = await _context.GetUniformLocationAsync(program, "mWorld");
			_matLocView = await _context.GetUniformLocationAsync(program, "mView");
			_matLocProj = await _context.GetUniformLocationAsync(program, "mProj");
		}

		private static float ConvertToRadians(float angle) =>
			(float)(Math.PI / 180.0 * angle);

		public async Task UpdateSizeAsync(int width, int height)
		{
			if(height <= 0) { return; }

			await _context.ViewportAsync(0, 0, width, height);

			var mProj = Matrix4x4.CreatePerspectiveFieldOfView(ConvertToRadians(45f), width / (float)height, .1f, 1000f);
			await _context.UniformMatrixAsync(_matLocProj, false, mProj.Values1D());
		}

		protected override async Task OnRenderAsync(bool firstRender, int width, int height)
		{
			if (firstRender)
			{
				await _context.EnableAsync(EnableCap.DEPTH_TEST);

				// set up world matrix
				await _context.UniformMatrixAsync(_matLocWorld, false, _matWorld.Values1D());

				// set up view matrix
				var mView = Matrix4x4.CreateLookAt(new Vector3(0f, 0f, -25f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f));
				await _context.UniformMatrixAsync(_matLocView, false, mView.Values1D());

				// this will set up projection matrix
				await UpdateSizeAsync(width, height);
			}

			if (SpectrumBuffer == null || SpectrumBuffer.Length != 25) { return; } // must match x/y iterations below

			await _context.ClearColorAsync(0.184f, 0.310f, 0.310f, 1f);
			await _context.ClearAsync(BufferBits.COLOR_BUFFER_BIT | BufferBits.DEPTH_BUFFER_BIT);

			// Please note: I have no idea why I am transposing the matrix's so much. I obvioulsy don't
			// understand how the matrix is stored in System.Numerics.Matrix4x4

			var rotate = (float)((Environment.TickCount - _tickStart) / 1000.0 / 6.0 * 2.0 * Math.PI);
			var matRotate = Matrix4x4.Transpose(Matrix4x4Extensions.CreateRotate(rotate, new Vector3(.5f, 1f, -.2f)));
			var matWorld = Matrix4x4.Transpose(_matWorld);

			var ind = 0;
			await _context.BeginBatchAsync();
			for (var y = -2; y < 3; ++y)
			{
				for (var x = -2; x < 3; ++x)
				{
					var mFin =
						matWorld *
						matRotate *
						Matrix4x4.Transpose(Matrix4x4.CreateTranslation(
							new Vector3(2.1f * x, 2.1f * y, 0f))) *
						Matrix4x4.Transpose(Matrix4x4.CreateScale(
							new Vector3(1f, 1f, (float)SpectrumBuffer[_dicMap[new Point(x, y)]] * .5f)));

					await _context.UniformMatrixAsync(_matLocWorld, false, mFin.Values1D(true));
					await _context.DrawElementsAsync(
						Primitive.TRIANGLES, _indexBuffer.ElementCount, _indexBuffer.DataType, 0);

					++ind;
				}
			}
			await _context.EndBatchAsync();
		}

		// This is based on spectrum size of 25. There is probably an algorithm that could
		// be made that would simplify this greatly, but I just don't have the time.
		private static readonly Dictionary<Point, int> _dicMap =
			new Dictionary<Point, int>
			{
				{ new Point(0, 0), 0 },
				{ new Point(-1, 0), 1 },
				{ new Point(-1, -1), 2 },
				{ new Point(0, -1), 3 },
				{ new Point(1, -1), 4 },
				{ new Point(1, 0), 5 },
				{ new Point(1, 1), 6 },
				{ new Point(0, 1), 7 },
				{ new Point(-1, 1), 8 },
				{ new Point(-2, 0), 9 },
				{ new Point(-2, -1), 10 },
				{ new Point(-2, -2), 11 },
				{ new Point(-1, -2), 12 },
				{ new Point(0, -2), 13 },
				{ new Point(1, -2), 14 },
				{ new Point(2, -2), 15 },
				{ new Point(2, -1), 16 },
				{ new Point(2, 0), 17 },
				{ new Point(2, 1), 18 },
				{ new Point(2, 2), 19 },
				{ new Point(1, 2), 20 },
				{ new Point(0, 2), 21 },
				{ new Point(-1, 2), 22 },
				{ new Point(-2, 2), 23 },
				{ new Point(-2, 1), 24 }
			};
	}
}
