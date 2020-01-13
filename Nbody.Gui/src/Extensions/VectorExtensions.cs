using Godot;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using static Godot.Mathf;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
#if GODOT_REAL_T_IS_DOUBLE
using godot_real_t = System.Double;
#else
using godot_real_t = System.Single;
#endif
namespace NBody.Gui.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 ToV3(this Nbody.Gui.Core.Point3 point)
        {
            return new Vector3((godot_real_t)point.x, (godot_real_t)point.y, (godot_real_t)point.z);
        }
        public static Vector3 ToV3(this Vector<real_t> vs)
        {
            return new Vector3((godot_real_t)vs[0], (godot_real_t)vs[1], (godot_real_t)vs[2]);
        }
        public static string ToStrV3(this Vector<real_t> vs)
        {
            return $"[{vs[0]:F1}, {vs[1]:F1}, {vs[2]:F1}]";
        }
        public static IEnumerable<Vector3> GenerateVectors(this (int n, int m, int o) tup, Func<int, int, int, Vector3> func)
        {
            for (int i = 0; i < tup.n; i++)
            {
                for (int j = 0; j < tup.m; j++)
                {
                    for (int k = 0; k < tup.o; k++)
                    {
                        yield return func(i, j, k);
                    }
                }
            }
            yield break;
        }
        public static Transform TargetTo(this Transform tran, Vector3 eye, Vector3 target, Vector3 up)
        {
            godot_real_t eyex = eye[0],
                eyey = eye[1],
                eyez = eye[2],
                upx = up[0],
                upy = up[1],
                upz = up[2];

            godot_real_t z0 = eyex - target[0],
                z1 = eyey - target[1],
                z2 = eyez - target[2];

            godot_real_t len = z0 * z0 + z1 * z1 + z2 * z2;
            if (len > 0)
            {
                len = 1.0f / Sqrt(len);
                z0 *= len;
                z1 *= len;
                z2 *= len;
            }
            godot_real_t x0 = upy * z2 - upz * z1,
              x1 = upz * z0 - upx * z2,
              x2 = upx * z1 - upy * z0;

            len = x0 * x0 + x1 * x1 + x2 * x2;
            if (len > 0)
            {
                len = 1f / Sqrt(len);
                x0 *= len;
                x1 *= len;
                x2 *= len;
            }
            return new Transform(new Vector3(x0, x1, x2), new Vector3(z1 * x2 - z2 * x1, z2 * x0 - z0 * x2, z0 * x1 - z1 * x0), new Vector3(z0, z1, z2), new Vector3(eyex, eyey, eyez));
        }
        public static Transform RotateZ(godot_real_t rad)
        {
            return new Transform(
                new Vector3(Cos(rad), Sin(rad), 0),
                new Vector3(-Sin(rad), Cos(rad), 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 0));
        }
        public static Transform RotateY(godot_real_t rad)
        {
            return new Transform(
                new Vector3(Cos(rad), 0, Sin(rad)),
                new Vector3(0, 1, 0),
                new Vector3(-Sin(rad), 0, Cos(rad)),
                new Vector3(0, 0, 0));
        }
        public static Transform RotateX(godot_real_t rad)
        {
            return new Transform(
                new Vector3(1, 0, 0),
                new Vector3(0, Cos(rad), Sin(rad)),
                new Vector3(0, -Sin(rad), Cos(rad)),
                new Vector3(0, 0, 0));
        }
        public static Transform Rotate(Vector3 a, godot_real_t rad)
        {
            float x = a.x, y = a.y, z = a.z;

            return new Transform(
                new Vector3(Cos(rad) + x * x * (1 - Cos(rad)), x * y * (1 - Cos(rad)) - z * Sin(rad), x * z * (1 - Cos(rad)) + y * Sin(rad)),
                new Vector3(y * x * (1 - Cos(rad)) + z * Sin(rad), Cos(rad) + y * y * (1 - Cos(rad)), y * z * (1 - Cos(rad)) - x * Sin(rad)),
                new Vector3(z * x * (1 - Cos(rad)) - y * Sin(rad), z * y * (1 - Cos(rad)) + x * Sin(rad), Cos(rad) + z * z * (1 - Cos(rad))),
                new Vector3(0, 0, 0));
        }
        public static Transform TargetTo2(this Transform tran, Vector3 lookAt, Vector3 up)
        {
            var arround = lookAt.Cross(up);
            var angle = lookAt.AngleTo(up);
            return tran * Rotate(arround, angle);
        }
        public static Transform Scale2(this Transform tran, Vector3 scale)
        {
            return tran * new Transform(
                new Vector3(scale.x, 0f, 0f),
                new Vector3(0f, scale.y, 0f),
                new Vector3(0f, 0f, scale.z),
                Vector3.Zero
                );
        }
        public static (Vector2 min, Vector2 max) GetMinMax(this Vector2[] array)
        {
            var min = new Vector2(godot_real_t.MaxValue, godot_real_t.MaxValue);
            var max = new Vector2(godot_real_t.MinValue, godot_real_t.MinValue);
            for (int i = 0; i < array.Length; i++)
            {
                var el = array[i];
                if (min.x > el.x) min.x = el.x;
                if (min.y > el.y) min.y = el.y;
                if (max.x < el.x) max.x = el.x;
                if (max.y < el.y) max.y = el.y;
            }
            return (min, max);
        }
    }
}
