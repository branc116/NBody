using System;
using System.Collections;
using System.Collections.Generic;

#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace Nbody.Gui.Core
{
    public struct Point3 : IEquatable<Point3>, IEnumerable<real_t>
    {
        public enum Axis
        {
            X = 0,
            Y,
            Z
        }

        public real_t x;
        public real_t y;
        public real_t z;

        public real_t this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        return;
                    case 1:
                        y = value;
                        return;
                    case 2:
                        z = value;
                        return;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        internal void Normalize()
        {
            real_t lengthsq = LengthSquared();

            if (lengthsq == 0)
            {
                x = y = z = 0f;
            }
            else
            {
                real_t length = MathReal.Sqrt(lengthsq);
                x /= length;
                y /= length;
                z /= length;
            }
        }

        public Point3 Abs()
        {
            return new Point3(MathReal.Abs(x), MathReal.Abs(y), MathReal.Abs(z));
        }

        public real_t AngleTo(Point3 to)
        {
            return MathReal.Atan2(Cross(to).Length(), Dot(to));
        }

        public Point3 Bounce(Point3 n)
        {
            return -Reflect(n);
        }

        public Point3 Ceil()
        {
            return new Point3(MathReal.Ceil(x), MathReal.Ceil(y), MathReal.Ceil(z));
        }

        public Point3 Cross(Point3 b)
        {
            return new Point3
            (
                y * b.z - z * b.y,
                z * b.x - x * b.z,
                x * b.y - y * b.x
            );
        }

        public Point3 CubicInterpolate(Point3 b, Point3 preA, Point3 postB, real_t t)
        {
            var p0 = preA;
            var p1 = this;
            var p2 = b;
            var p3 = postB;

            real_t t2 = t * t;
            real_t t3 = t2 * t;

            return 0.5f * (
                        p1 * 2.0f + (-p0 + p2) * t +
                        (2.0f * p0 - 5.0f * p1 + 4f * p2 - p3) * t2 +
                        (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3
                    );
        }

        public Point3 DirectionTo(Point3 b)
        {
            return new Point3(b.x - x, b.y - y, b.z - z).Normalized();
        }

        public real_t DistanceSquaredTo(Point3 b)
        {
            return (b - this).LengthSquared();
        }

        public real_t DistanceTo(Point3 b)
        {
            return (b - this).Length();
        }

        public real_t Dot(Point3 b)
        {
            return x * b.x + y * b.y + z * b.z;
        }

        public Point3 Floor()
        {
            return new Point3(MathReal.Floor(x), MathReal.Floor(y), MathReal.Floor(z));
        }

        public Point3 Inverse()
        {
            return new Point3(1.0f / x, 1.0f / y, 1.0f / z);
        }

        public bool IsNormalized()
        {
            return MathReal.Abs(LengthSquared() - 1.0f) < MathReal.Epsilon;
        }

        public real_t Length()
        {
            real_t x2 = x * x;
            real_t y2 = y * y;
            real_t z2 = z * z;

            return MathReal.Sqrt(x2 + y2 + z2);
        }

        public real_t LengthSquared()
        {
            real_t x2 = x * x;
            real_t y2 = y * y;
            real_t z2 = z * z;

            return x2 + y2 + z2;
        }

        public Point3 LinearInterpolate(Point3 b, real_t t)
        {
            return new Point3
            (
                x + t * (b.x - x),
                y + t * (b.y - y),
                z + t * (b.z - z)
            );
        }

        public Point3 MoveToward(Point3 to, real_t delta)
        {
            var v = this;
            var vd = to - v;
            var len = vd.Length();
            return len <= delta || len < MathReal.Epsilon ? to : v + vd / len * delta;
        }

        public Axis MaxAxis()
        {
            return x < y ? (y < z ? Axis.Z : Axis.Y) : (x < z ? Axis.Z : Axis.X);
        }

        public Axis MinAxis()
        {
            return x < y ? (x < z ? Axis.X : Axis.Z) : (y < z ? Axis.Y : Axis.Z);
        }

        public Point3 Normalized()
        {
            var v = this;
            v.Normalize();
            return v;
        }

        public Point3 PosMod(real_t mod)
        {
            Point3 v;
            v.x = MathReal.PosMod(x, mod);
            v.y = MathReal.PosMod(y, mod);
            v.z = MathReal.PosMod(z, mod);
            return v;
        }

        public Point3 PosMod(Point3 modv)
        {
            Point3 v;
            v.x = MathReal.PosMod(x, modv.x);
            v.y = MathReal.PosMod(y, modv.y);
            v.z = MathReal.PosMod(z, modv.z);
            return v;
        }

        public Point3 Project(Point3 onNormal)
        {
            return onNormal * (Dot(onNormal) / onNormal.LengthSquared());
        }

        public Point3 Reflect(Point3 n)
        {
#if DEBUG
            if (!n.IsNormalized())
                throw new ArgumentException(String.Format("{0} is not normalized", n), nameof(n));
#endif
            return 2.0f * n * Dot(n) - this;
        }

        public Point3 Round()
        {
            return new Point3(MathReal.Round(x), MathReal.Round(y), MathReal.Round(z));
        }

        public Point3 Sign()
        {
            Point3 v;
            v.x = MathReal.Sign(x);
            v.y = MathReal.Sign(y);
            v.z = MathReal.Sign(z);
            return v;
        }

        public Point3 Slide(Point3 n)
        {
            return this - n * Dot(n);
        }

        public Point3 Snapped(Point3 by)
        {
            return new Point3
            (
                MathReal.Stepify(x, by.x),
                MathReal.Stepify(y, by.y),
                MathReal.Stepify(z, by.z)
            );
        }

        public static Point3 Zero { get; } = new Point3(0, 0, 0);
        public static Point3 One { get; } = new Point3(1, 1, 1);
        public static Point3 NegOne { get; } = new Point3(-1, -1, -1);
        public static Point3 Inf { get; } = new Point3(MathReal.Inf, MathReal.Inf, MathReal.Inf);

        public static Point3 Up { get; } = new Point3(0, 1, 0);
        public static Point3 Down { get; } = new Point3(0, -1, 0);
        public static Point3 Right { get; } = new Point3(1, 0, 0);
        public static Point3 Left { get; } = new Point3(-1, 0, 0);
        public static Point3 Forward { get; } = new Point3(0, 0, -1);
        public static Point3 Back { get; } = new Point3(0, 0, 1);

        // Constructors
        public Point3(real_t x, real_t y, real_t z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Point3(Point3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static Point3 operator +(Point3 left, Point3 right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            return left;
        }

        public static Point3 operator -(Point3 left, Point3 right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            return left;
        }

        public static Point3 operator -(Point3 vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;
            return vec;
        }

        public static Point3 operator *(Point3 vec, real_t scale)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        public static Point3 operator *(real_t scale, Point3 vec)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        public static Point3 operator *(Point3 left, Point3 right)
        {
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;
            return left;
        }

        public static Point3 operator /(Point3 vec, real_t scale)
        {
            vec.x /= scale;
            vec.y /= scale;
            vec.z /= scale;
            return vec;
        }

        public static Point3 operator /(Point3 left, Point3 right)
        {
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;
            return left;
        }

        public static Point3 operator %(Point3 vec, real_t divisor)
        {
            vec.x %= divisor;
            vec.y %= divisor;
            vec.z %= divisor;
            return vec;
        }

        public static Point3 operator %(Point3 vec, Point3 divisorv)
        {
            vec.x %= divisorv.x;
            vec.y %= divisorv.y;
            vec.z %= divisorv.z;
            return vec;
        }

        public static bool operator ==(Point3 left, Point3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point3 left, Point3 right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Point3 left, Point3 right)
        {
            if (MathReal.IsEqualApprox(left.x, right.x))
            {
                if (MathReal.IsEqualApprox(left.y, right.y))
                    return left.z < right.z;
                return left.y < right.y;
            }

            return left.x < right.x;
        }

        public static bool operator >(Point3 left, Point3 right)
        {
            if (MathReal.IsEqualApprox(left.x, right.x))
            {
                if (MathReal.IsEqualApprox(left.y, right.y))
                    return left.z > right.z;
                return left.y > right.y;
            }

            return left.x > right.x;
        }

        public static bool operator <=(Point3 left, Point3 right)
        {
            if (MathReal.IsEqualApprox(left.x, right.x))
            {
                if (MathReal.IsEqualApprox(left.y, right.y))
                    return left.z <= right.z;
                return left.y < right.y;
            }

            return left.x < right.x;
        }

        public static bool operator >=(Point3 left, Point3 right)
        {
            if (MathReal.IsEqualApprox(left.x, right.x))
            {
                if (MathReal.IsEqualApprox(left.y, right.y))
                    return left.z >= right.z;
                return left.y > right.y;
            }

            return left.x > right.x;
        }

        public override bool Equals(object obj)
        {
            if (obj is Point3)
            {
                return Equals((Point3)obj);
            }

            return false;
        }

        public bool Equals(Point3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public bool IsEqualApprox(Point3 other)
        {
            return MathReal.IsEqualApprox(x, other.x) && MathReal.IsEqualApprox(y, other.y) && MathReal.IsEqualApprox(z, other.z);
        }

        public override int GetHashCode()
        {
            return y.GetHashCode() ^ x.GetHashCode() ^ z.GetHashCode();
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", new object[]
            {
                x.ToString(format),
                y.ToString(format),
                z.ToString(format)
            });
        }

        public IEnumerator<real_t> GetEnumerator()
        {
            yield return x;
            yield return y;
            yield return z;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
