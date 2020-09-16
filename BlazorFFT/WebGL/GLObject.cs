using Blazor.Extensions.Canvas.WebGL;
using System;

namespace BlazorFFT.WebGL
{
	public enum LastStateType
	{
		Uninitialized, Success, Failed
	}

	public abstract class GLObject<TWebGLObject> where TWebGLObject: WebGLObject
	{
		public string LastError { get; protected set; }
		public LastStateType LastState { get; protected set; }

		private TWebGLObject _object;

		public TWebGLObject GLObjectValue
		{
			get
			{
				ThrowIfNotValid();
				return _object;
			}
			protected set => _object = value;
		}

		public void ThrowIfNotValid()
		{
			if (LastState != LastStateType.Success)
			{
				throw new InvalidOperationException(
					$"Type {GetType().Name}:{nameof(GLObjectValue)} is in an " +
					$"invalid state ({LastState}) with error ({LastError ?? "<none>"})");
			}
		}

		protected void ResetObject()
		{
			LastError = string.Empty;
			LastState = LastStateType.Uninitialized;
			GLObjectValue = null;
		}
	}
}
