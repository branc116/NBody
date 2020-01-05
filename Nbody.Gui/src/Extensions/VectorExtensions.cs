using Godot;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using static Godot.Mathf;
namespace NBody.Gui.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 ToV3(this Vector<double> vs)
        {
            return new Vector3((float)vs[0], (float)vs[1], (float)vs[2]);
        }
        public static string ToStrV3(this Vector<double> vs)
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
            float eyex = eye[0],
                eyey = eye[1],
                eyez = eye[2],
                upx = up[0],
                upy = up[1],
                upz = up[2];

            float z0 = eyex - target[0],
                z1 = eyey - target[1],
                z2 = eyez - target[2];

            float len = z0 * z0 + z1 * z1 + z2 * z2;
            if (len > 0)
            {
                len = 1.0f / Mathf.Sqrt(len);
                z0 *= len;
                z1 *= len;
                z2 *= len;
            }
            float x0 = upy * z2 - upz * z1,
              x1 = upz * z0 - upx * z2,
              x2 = upx * z1 - upy * z0;

            len = x0 * x0 + x1 * x1 + x2 * x2;
            if (len > 0)
            {
                len = 1f / Mathf.Sqrt(len);
                x0 *= len;
                x1 *= len;
                x2 *= len;
            }
            return new Transform(new Vector3(x0, x1, x2), new Vector3(z1 * x2 - z2 * x1, z2 * x0 - z0 * x2, z0 * x1 - z1 * x0), new Vector3(z0, z1, z2), new Vector3(eyex, eyey, eyez));
        }
        public static Transform RotateZ(float rad)
        {
            return new Transform(
                new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0),
                new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 0));
        }
        public static Transform RotateY(float rad)
        {
            return new Transform(
                new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)),
                new Vector3(0, 1, 0),
                new Vector3(-Mathf.Sin(rad), 0, Mathf.Cos(rad)),
                new Vector3(0, 0, 0));
        }
        public static Transform RotateX(float rad)
        {
            return new Transform(
                new Vector3(1, 0, 0),
                new Vector3(0, Mathf.Cos(rad), Mathf.Sin(rad)),
                new Vector3(0, -Mathf.Sin(rad), Mathf.Cos(rad)),
                new Vector3(0, 0, 0));
        }
        public static Transform Rotate(Vector3 a, float rad)
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
            //return tran * RotateX(Mathf.Asin(lookAt.x)) * RotateZ(Mathf.Asin(lookAt.z)) * RotateY(Mathf.Acos(lookAt.y)); // * RotateY(Mathf.Acos(lookAt.y));
        }
        public static Transform Scale2(this Transform tran, Vector3 scale)
        {
            return tran * new Transform(
                new Vector3(scale.x, 0f, 0f),
                new Vector3(0f, scale.y, 0f),
                new Vector3(0f, 0f, scale.z),
                Vector3.Zero
                );
            //return tran * RotateX(Mathf.Asin(lookAt.x)) * RotateZ(Mathf.Asin(lookAt.z)) * RotateY(Mathf.Acos(lookAt.y)); // * RotateY(Mathf.Acos(lookAt.y));
        }
    }
}
