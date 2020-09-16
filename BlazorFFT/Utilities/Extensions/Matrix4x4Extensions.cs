using System;
using System.Numerics;

namespace BlazorFFT.Utilities.Extensions
{
	public static class Matrix4x4Extensions
	{
		public static float[] Values1D(this Matrix4x4 matrix, bool transpose = false)
		{
			var arr = new float[16];

			var mat = transpose ? Matrix4x4.Transpose(matrix) : matrix;

			arr[0] = mat.M11;
			arr[1] = mat.M12;
			arr[2] = mat.M13;
			arr[3] = mat.M14;

			arr[4] = mat.M21;
			arr[5] = mat.M22;
			arr[6] = mat.M23;
			arr[7] = mat.M24;

			arr[8] = mat.M31;
			arr[9] = mat.M32;
			arr[10] = mat.M33;
			arr[11] = mat.M34;

			arr[12] = mat.M41;
			arr[13] = mat.M42;
			arr[14] = mat.M43;
			arr[15] = mat.M44;

			return arr;
		}

		// This rotate was taken from GlmSharp because I can't get CreateFromAxisAngle to work.
		public static Matrix4x4 CreateRotate(float angle, Vector3 v)
		{
			var c = (float)Math.Cos(angle);
			var s = (float)Math.Sin(angle);

			var axis = v / v.Length(); // nomalize
			var temp = (1 - c) * axis;

			var m = Matrix4x4.Identity;
			m.M11 = c + temp.X * axis.X;
			m.M12 = 0 + temp.X * axis.Y + s * axis.Z;
			m.M13 = 0 + temp.X * axis.Z - s * axis.Y;

			m.M21 = 0 + temp.Y * axis.X - s * axis.Z;
			m.M22 = c + temp.Y * axis.Y;
			m.M23 = 0 + temp.Y * axis.Z + s * axis.X;

			m.M31 = 0 + temp.Z * axis.X + s * axis.Y;
			m.M32 = 0 + temp.Z * axis.Y - s * axis.X;
			m.M33 = c + temp.Z * axis.Z;

			return m;
		}
	}
}
