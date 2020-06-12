using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Nbody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace Nbody.Gui.Core
{
    public class L2Calculation : PlanetInLagrangePoint
    {
        public override string Type => "L2 calculation";
        public real_t CalculationTime { get; }
        public real_t Tolerance { get; }
        public Action CalculationFinished { get; }
        public Action<(real_t bumpZ0, real_t bumpYdot0)> NextIteration { get; }

        public Point3 old;
        public Matrix<real_t> phi;
        public Vector<real_t> x0;
        public real_t PhiTimesFdt { get; set; }
        public real_t Mu => SmallerPlanet.Mass / (SmallerPlanet.Mass + LargerPlanet.Mass);
        public L2Calculation(Planet larger, Planet smaller, 
            Action<(real_t bumpZ0, real_t bumpYdot0)> nextIteration , Action calculationFinished, 
            real_t halfPeriod, Point3 x0Offset, Point3 xDot0Offset,
            real_t tolerance, real_t phiTimesFdt) : base(smaller, larger, 2, "Calculation")
        {
            Offset0 = x0Offset;
            DotOffset0 = xDot0Offset;
            CalculationTime = halfPeriod;
            Tolerance = tolerance;
            CalculationFinished = calculationFinished;
            NextIteration = nextIteration;
            Position += TransformToGlobalCoordinates(Offset0);
            Velocity += TransformToGlobalCoordinates(DotOffset0);
            var normDotX = TransformToNormalizedCoordinates(Velocity);
            var normX = TransformToNormalizedCoordinates(Position);
            this.old = new Point3(normDotX);
            phi = Matrix<real_t>.Build.Dense(6, 6, new real_t[]
            {
                1, 0, 0, 0, 0, 0,
                0, 1, 0, 0, 0, 0,
                0, 0, 1, 0, 0, 0,
                0, 0, 0, 1, 0, 0,
                0, 0, 0, 0, 1, 0,
                0, 0, 0, 0, 0, 1
            });
            x0 = Vector<real_t>.Build.Dense(normX.Append(normDotX.x).Append(normDotX.y).Append(normDotX.z).Select(i => 1/i).ToArray());
            PhiTimesFdt = phiTimesFdt;
        }
        public int? YDir = null;
        public override void AfterUpdate()
        {
            base.AfterUpdate();
            var normDotX = TransformToNormalizedCoordinates(this.Velocity);
            var normX = TransformToNormalizedCoordinates(this.Position);
            var cur = Vector<double>.Build.Dense(normX.Append(normDotX.x).Append(normDotX.y).Append(normDotX.z).ToArray());
            var mu = Mu;
            var f = F(normX, normDotX, mu);
            var phiD = f * this.phi;
            this.phi += phiD * PhiTimesFdt; // * SourceOfTruth.SimulationModel.DeltaTimePerStep;
            foreach(var el in phi.ToArray())
            {
                var e = el;
                if (!e.IsFinite())
                {
                    Console.WriteLine("Bad start..");
                    return;
                }
            }
            if (YDir == null && normX.y != 0)
            {
                YDir = Math.Sign(normX.y);
            }
            var time = false;
            if (YDir != null && Math.Sign(normX.y) != YDir.Value)
            {
                Console.WriteLine($"time: {SourceOfTruth.System.Get.CurTime}");
                YDir = Math.Sign(normX.y);
                time = true;
            }
            if (SourceOfTruth.System.Get.CurTime < CalculationTime && (!time || !SourceOfTruth.SimulationModel.RegressionStopOnTime))
            {
                old = normDotX;
                return;
            }

            if (Math.Abs(normDotX.x) < Tolerance && Math.Abs(normDotX.z) < Tolerance) {
                System.Console.WriteLine($"Success {normDotX.x} {normDotX.z}");
                CalculationFinished();
                SourceOfTruth.SimulationModel.StepsPerFrame.Set(1);
                return;
            }
            System.Console.WriteLine($"xdot = {normDotX.x}; zdot = {normDotX.z}");
            var dotDotX = (normDotX - old) / SourceOfTruth.SimulationModel.DeltaTimePerStep;
            var realDotDotX = A(normX, normDotX, mu);
            dotDotX = realDotDotX;
            var l = Matrix<real_t>.Build.Dense(2, 2, new real_t[]
            {
                phi[3, 2], phi[3, 4],
                phi[5, 2], phi[5, 4]
            });
            var r = Matrix<real_t>.Build.Dense(2, 2, new real_t[]
            {
                dotDotX.x * phi[1, 2], dotDotX.x * phi[1, 4],
                dotDotX.z * phi[1, 2], dotDotX.z * phi[1, 4]
            }) / normDotX.y;
            var inverse = (l - r).Inverse();
            var bumpZ = -normDotX.x * inverse[0, 0] - normDotX.z * inverse[0, 1];
            var bumpYdot = -normDotX.x * inverse[1, 0] - normDotX.z * inverse[1, 1];
            if (bumpYdot == real_t.NaN || bumpZ == real_t.NaN)
            {
                Console.WriteLine("Bad start..");
                return;
            }

            this.NextIteration((bumpZ, bumpYdot));
        }

        public override IEnumerable<(string name, string value)> GetPlanetInfo()
        {
            return base.GetPlanetInfo()
                .Append(("Jaccobi constant", JaccobiConstant().ToString()));
        }

        public override IEnumerable<Point3> GetTracePoints()
        {
            return base.GetTracePoints();
        }

        public override Point3 InternalAcceleration(bool recalculate, double dt)
        {
            return Point3.Zero;
        }
        public Matrix<real_t> F(Point3 l, Point3 v, real_t mu)
        {
            var s = SourceOfTruth.System.Get;
            var nOld = l;
            l = TransformToGlobalCoordinates(l);
            var n = TransformToNormalizedCoordinates(l);
            var dx = TransformToNormalizedCoordinates(s.DAD(Direction.X, l));
            var dy = TransformToNormalizedCoordinates(s.DAD(Direction.Y, l));
            var dzg = s.DAD(Direction.Z, l);
            var dz = TransformToNormalizedCoordinates(dzg);
            // return Matrix<real_t>.Build.Dense(6, 6, new real_t[]
            // {
            //     0, 0, 0, 1, 0, 0,
            //     0, 0, 0, 0, 1, 0,
            //     0, 0, 0, 0, 0, 1,
            //     dx[0], dx[1], dx[2], 0, 2, 0,
            //     dy[0], dy[1], dy[2], -2, 0, 0,
            //     dz[0], dz[1], dz[2], 0, 0, 0
            // });
            var dadx = DADX(n, v, mu);
            var dady = DADY(n, v, mu);
            var dadz = DADZ(n, v, mu);
            return Matrix<real_t>.Build.Dense(6, 6, new real_t[]
            {
                0, 0, 0, 1, 0, 0,
                0, 0, 0, 0, 1, 0,
                0, 0, 0, 0, 0, 1,
                dadx[0], dady[0], dadz[0], 0, 2, 0,
                dadx[1], dady[1], dadz[1], -2, 0, 0,
                dadx[2], dady[2], dadz[2], 0, 0, 0
            });
        }
        public static real_t R1Sq(Point3 n, real_t mu) {
            var _1 =  n.x + mu;
            var _2 = n.y;
            var _3 = n.z;
            return _1 * _1 + _2 * _2 + _3 * _3;
        }
        public static real_t R2Sq(Point3 n, real_t mu) {
            var _1 =  n.x -1.0 + mu;
            var _2 = n.y;
            var _3 = n.z;
            return _1 * _1 + _2 * _2 + _3 * _3;
        }
        public static Point3 A(Point3 n, Point3 v, real_t mu)
        {
            var r1 = Math.Pow(R1Sq(n, mu), 1.5);
            var r2 = Math.Pow(R2Sq(n, mu), 1.5);
            real_t x = n[0], y = n[1], z=n[2]; 
            var xdd = 2 * v[1] + x - (1 - mu) * (x + mu) / r1 - mu * (x - 1 + mu) / r2;
            var ydd = -2 * v[0] + y - (1 - mu) * y / r1 - mu * y / r2;
            var zdd = - (1 - mu) * z / r1 - mu * z / r2;
            return new Point3(xdd, ydd, zdd);
        }
        public static Point3 DADX(Point3 n, Point3 v, real_t mu) {
            var d = new Point3(10e-6, 0, 0);
            return (A(n + d, v, mu) - A(n, v, mu)) / 10e-6;
        }
        public static Point3 DADY(Point3 n, Point3 v, real_t mu) {
            var d = new Point3(0, 10e-6, 0);
            return (A(n + d, v, mu) - A(n, v, mu)) / 10e-6;
        }
        public static Point3 DADZ(Point3 n, Point3 v, real_t mu) {
            var d = new Point3(0, 0, 10e-6);
            return (A(n + d, v, mu) - A(n, v, mu)) / 10e-6;
        }
        public real_t JaccobiConstant()
        {
            var nx = TransformToNormalizedCoordinates(Position);
            var ndx = TransformToNormalizedCoordinates(Velocity);
            var U = 0.5 * (nx.x * nx.x + nx.y * nx.y) + (1 - Mu) / Math.Sqrt(R1Sq(nx, Mu)) + Mu / Math.Sqrt(R2Sq(nx, Mu));
            var Vsq = ndx.LengthSquared();
            return 2*U - Vsq;
        }
    }
}
