using MathNet.Numerics;
using MathNet.Numerics.Integration;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using Nbody.Gui.Core;
using Nbody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.Solvers
{
    public class CorrectionState {
        public Vector<double> X { get; set; }
        public Matrix<double> Phi { get; set; }
        public Matrix<double> F { get; set; }
        public Vector<double> XDot { get; set; }
        public Vector<double> Corrections { get; set; }
        public double Time { get; set; }
        public double? LastTime { get; set; }
        public double Error { get; set; }
        public int StepLeft { get; set; }
        public override string ToString()
        {
            return X.Select(i => i.ToString()).Aggregate((i, j) => $"{i};{j}");
        }
        public CorrectionState Clone()
        {
            return new CorrectionState
            {
                Corrections = Corrections?.Clone(),
                F = F?.Clone(),
                Phi = Phi?.Clone(),
                Time = Time,
                X = X?.Clone(),
                XDot = XDot?.Clone(),
                Error = Error,
                LastTime = LastTime,
                StepLeft = StepLeft
            };
        }
    }
    public class SolveL2InitialConditions
    {
        public  double dConst = 10e-9;

        public double Mu { get; set; }
        public Point3 Position0 { get; set; }
        public Point3 Velocity0 { get; set; }
        public double Dt { get; set; }
        public double Treshold { get; set; }
        public double OrbitTime { get; set; }
        public Vector<double> X0 => Vector<double>.Build.Dense(new[] { Position0.x, Position0.y, Position0.z, Velocity0.x, Velocity0.y, Velocity0.z });
        public (Point3 finalP, Point3 finalV) Iterate()
        {
            SingleShot();
            return default;
        }
        public void SingleShot()
        {
            var x0 = X0;
            //var initCorrected1 = Reverse(init, p1c, Patch1, 0, 1, 2, 3, 5);
            //var p1cc = Patch1(initCorrected1);
            
            var init = new CorrectionState { X = x0, LastTime = null };
            var initTime = Patch2(init);
            init.LastTime = initTime.Time;
            while (Dt > 10e-12)
            {
            bad_y_val:
                var j = JaccobiConstant(x0);
                if (j < 3.0)
                {
                    x0[4] *= 0.95;
                    goto bad_y_val;
                }
                var newIni = StartCorrecting(init, Patch2, 0, 1, 3, 5);
                newIni.LastTime = null;
                var tst = Patch2(newIni);
                var j1 = JaccobiConstant(tst.X);
                var j2 = JaccobiConstant(newIni.X);
                init.LastTime = tst.Time;
                newIni.X.CopyTo(init.X);
                Dt *= 0.95;
            }
        }
        public void Manager()
        {
            var x0 = X0;
            //var initCorrected1 = Reverse(init, p1c, Patch1, 0, 1, 2, 3, 5);
            //var p1cc = Patch1(initCorrected1);
            while (Dt > 10e-12)
            {
                bad_y_val:
                var init = new CorrectionState { X = x0 };
                var j = JaccobiConstant(x0);
                if (j < 3.0)
                {
                    x0[4] *= 0.95;
                    goto bad_y_val;
                }
                var p1 = Patch1(init);
                var p1c = StartCorrecting(p1, Patch2, 3);
                var p2 = Patch2(p1c);
                var p2c = StartCorrecting(p2, Patch3, 4);
                var p3 = Patch3(p2c);
                
                var p2r = Reverse(p2c, p3, Patch3, 4);
                var p1r = Reverse(p1c, p2r, Patch2, 3);
                var ir = Reverse(init, p1r, Patch1, 4);

                var p3c = StartCorrecting(p3, i => Patch4(i, ir.X[0], ir.X[5], ir.X[2]), 3);
                p2r = Reverse(p2c, p3c, Patch3, 4);
                p1r = Reverse(p1c, p2r, Patch2, 3);
                ir = Reverse(init, p1r, Patch1, 4);
                var test = Patch3(Patch2(Patch1(ir)));
                ir.X.CopyTo(x0);
                Dt *= 0.95;
            }
        }
        public CorrectionState StartCorrecting(CorrectionState init, Func<CorrectionState, CorrectionState> stator, params int[] mask)
        {
            var error = double.PositiveInfinity;
            var maxSteps = 400;
            var ret = init.Clone();
            var div = 100.0;
            var oldX = ret.X.Clone();
            var succcesSteps = 1;
            var stack = new Stack<dynamic>();
            while (maxSteps-- > 0 && Math.Abs(error) > this.Treshold)
            {
                var cur = stator(ret);
                var bum = cur.Corrections.PointwiseMultiply(cur.XDot);
                
                for (int i = 0; i < mask.Length; i++)
                {
                    bum[mask[i]] = 0;
                }
                error = bum.Norm(2);
                var move = bum * 1; // PwdIfNotZero(bum, ret.XDot) * 0.1; //.PointwiseMultiply(cur;
                stack.Push(new
                {
                    cors = cur.Corrections.Clone(),
                    bump = bum.Clone(),
                    move = move.Clone(),
                    cs = cur.Clone()
                });
                ret.X.CopyTo(oldX);
                ret.X -= move ; //.XDot.Norm(2);
                succcesSteps++;
                ret.LastTime = cur.Time;
            }
            ret.Error = error;
            ret.StepLeft = maxSteps;
            return ret;
            Vector<double> PwdIfNotZero(Vector<double> a, Vector<double> b)
            {
                var c = a.Clone();
                for (int i = 0; i < a.Count; i++)
                    c[i] = i < 3 ? -a[i] : a[i];
                return c / Math.Pow(b.L2Norm(), 2) * 0.1;
            }
        }
        
        public CorrectionState Reverse(CorrectionState initial, CorrectionState wanted, Func<CorrectionState, CorrectionState> func, params int[] mask)
        {
            //var x = wanted.X.PointwiseMultiply(wanted.XDot.PointwiseSign().PointwiseMultiply(wanted.X.PointwiseSign()));
            
            return StartCorrecting(initial, (c) =>
            {
                var tmp = func(c);
                c.XDot = Xdot(initial);
                tmp.Corrections = (tmp.Phi.Inverse() *
                    (wanted.X - tmp.X).ToColumnMatrix())
                    .Column(0)
                    .PointwiseMultiply(c.XDot);
                return tmp;
            }, mask);
        }
        public CorrectionState Patch1(CorrectionState correctionState)
        {
            var X = correctionState.X;
            var startSign = Math.Sign(X[4]);
            var CSAfter = Step(X, (i, time) => Math.Sign(i[4]) == startSign);
            CSAfter.Corrections = Vector<double>.Build.Dense(6);
            return CSAfter;
        }
        public CorrectionState Patch2(CorrectionState correctionState)
        {
            var X = correctionState.X;
            int? startSign = null; // Math.Sign(X[1]);
            var CSAfter = Step(X, (i, time) => {
                //return correctionState.LastTime > time;
                if (startSign == null)
                {
                    if (i[1] != 0)
                        startSign = Math.Sign(i[1]);
                    return true;
                }
                if (correctionState.LastTime != null)
                {
                    return correctionState.LastTime > time;
                    if (correctionState.LastTime * 0.9 > time)
                        return true;
                    if (correctionState.LastTime * 1.1 < time)
                        return false;
                } else
                {
                    return Math.Sign(i[1]) == startSign;
                }
                
            });
            correctionState.XDot = Xdot(correctionState);
            var xd1 = CSAfter.XDot;
            //var evd = CSAfter.Phi.Evd(Symmetricity.Unknown).EigenValues.GetNeigbros().Select((i) => ((i.Item1 + i.Item2)/2.0)).ToList();
            //var evd0 = F(correctionState.X, Mu).Evd(Symmetricity.Unknown);
            var l = Matrix<double>.Build.Dense(2, 2, new[] { CSAfter.Phi[3, 2], CSAfter.Phi[3, 4], CSAfter.Phi[5, 2], CSAfter.Phi[5, 4] });
            var r = Vector<double>.Build.Dense(new[] { xd1[3], xd1[5] }).ToColumnMatrix() * Vector<double>.Build.Dense(new[] { CSAfter.Phi[1, 2], CSAfter.Phi[1, 4] }).ToRowMatrix();
            r /= xd1[1];
            var correnctions = (l - r).Inverse() * Vector<double>.Build.Dense(new double[] { 0 - xd1[0], 0 - xd1[2] }).ToColumnMatrix();
            CSAfter.Corrections = Vector<double>.Build.Dense(new[] { 0, 0, correnctions[0, 0], 0, correnctions[1, 0], 0 });
            //CSAfter.Corrections = (CSAfter.Phi.Inverse() *
            //    Vector<double>.Build.Dense(new[] { 0, 0 - CSAfter.X[1], 0, 0 - CSAfter.X[3], 0, 0 - CSAfter.X[5] })
            //    .ToColumnMatrix())
            //    .Column(0)
            //    //.PointwiseMultiply(correctionState.XDot)
            //    ;
            return CSAfter;
        }
        public CorrectionState Patch3(CorrectionState correctionState)
        {
            var lastTime = correctionState.Time;
            var X = correctionState.X;
            var startSign = Math.Sign(X[1]);
            var CSAfter = Step(X, (i, time) => time < lastTime);
            correctionState.XDot = Xdot(correctionState);
            CSAfter.Corrections = (CSAfter.Phi.Inverse() *
                Vector<double>.Build.Dense(new[] { 0, 0, 0, 0, -CSAfter.X[4], 0 })
                .PointwiseMultiply(CSAfter.XDot)
                .ToColumnMatrix())
                .Column(0)
                .PointwiseMultiply(correctionState.XDot);
            return CSAfter;
        }
        public CorrectionState Patch4(CorrectionState correctionState, double x0, double yd0, double z0)
        {
            var lastTime = correctionState.Time;
            var X = correctionState.X;
            var CSAfter = Step(X, (i, time) => time < lastTime);
            correctionState.XDot = Xdot(correctionState);

            CSAfter.Corrections = (CSAfter.Phi.Inverse() *
                Vector<double>.Build.Dense(new[] {
                    x0 - CSAfter.X[0],
                    0 - CSAfter.X[1],
                    z0 - CSAfter.X[2],
                    0 - CSAfter.X[3],
                    yd0 - CSAfter.X[4],
                    0 - CSAfter.X[5] }).ToColumnMatrix())
                .Column(0)
                .PointwiseMultiply(correctionState.XDot);
            return CSAfter;
        }

        public CorrectionState Step(Vector<double> X0, Func<Vector<double>, double, bool> doWile) {
            var X = X0.Clone();
            var phi = Matrix<double>.Build.DenseIdentity(6);
            double x = X[0], y = X[1], z = X[2], xd = X[3], yd = X[4], zd = X[5];
            Point3 p = new Point3(x, y, z), v = new Point3(xd, yd, zd);
            var time = 0.0;
            while (doWile(X, time))
            {
                x = X[0]; y = X[1]; z = X[2]; xd = X[3]; yd = X[4]; zd = X[5];
                p = new Point3(x, y, z); v = new Point3(xd, yd, zd);
                var f = F(X, Mu);
                var a = A(p, v, Mu);

                var phiD = f * phi * Dt;
                phi += phiD;
                X[0] += X[3] * Dt; X[1] += X[4] * Dt; X[2] += X[5] * Dt;
                X[3] += a.x * Dt; X[4] += a.y * Dt; X[5] += a.z * Dt;
                time += Dt;
            }
            var aa = A(p, v, Mu);
            return new CorrectionState
            {
                F = F(X, Mu),
                X = X,
                Phi = phi,
                XDot = Vector<double>.Build.Dense(new double[] { X[3], X[4], X[5], aa.x, aa.y, aa.z }),
                Time = time
            };
        }
        public Vector<double> Xdot(CorrectionState correctionState)
        {
            var X = correctionState.X;
            double x = X[0], y = X[1], z = X[2], xd = X[3], yd = X[4], zd = X[5];
            Point3 p = new Point3(x, y, z), v = new Point3(xd, yd, zd);
            var aa = A(p, v, Mu);
            return Vector<double>.Build.Dense(new double[] { X[3], X[4], X[5], aa.x, aa.y, aa.z });
        }
        public (Point3 changeP, Point3 changeV, double changeT, double score) Step(Vector<double> initX, double orbit)
        {
            var X = initX;
            var phi = Matrix<double>.Build.DenseIdentity(6);
            double x = X[0], y = X[1], z = X[2], xd = X[3], yd = X[4], zd = X[5];
            Point3 p = new Point3(x, y, z), v = new Point3(xd, yd, zd);
            var str = "x; y; z";
            while ((orbit -= Dt) > 0) {
                x = X[0]; y = X[1]; z = X[2]; xd = X[3]; yd = X[4]; zd = X[5];
                p = new Point3(x, y, z); v = new Point3(xd, yd, zd);
                var f = F(X, Mu);
                var a = A(p, v, Mu);

                var phiD = f * phi * Dt;
                phi += phiD;
                X[0] += X[3] * Dt; X[1] += X[4] * Dt; X[2] += X[5] * Dt;
                X[3] += a.x * Dt; X[4] += a.y * Dt; X[5] += a.z * Dt;
                //X += (X.ToRowMatrix() * f).Row(0);
                //str += $"{x}; {y}; {z}\n";
            }
            var dX = phi.Inverse() * Vector<double>.Build.Dense(new[] { 0, 0 - y, 0, 0 - xd, 0, 0 - zd }).ToColumnMatrix();
            var r = dX.Column(0).ToArray();
            //System.IO.File.WriteAllText(@"C:\_Projects\_temp\orbit.csv", str);
            //foreach (var el in phi.ToArray())
            //{
            //    var e = el;
            //    if (!e.IsFinite())
            //    {
            //        Console.WriteLine("Bad start..");
            //        return default;
            //    }
            //}
            //var dotDotX = A(p, v, Mu);
            //var l = Matrix<double>.Build.Dense(2, 2, new double[]
            //{
            //    phi[3, 2], phi[3, 4],
            //    phi[5, 2], phi[5, 4]
            //});
            //var r = Matrix<double>.Build.Dense(2, 2, new double[]
            //{
            //    dotDotX.x * phi[1, 2], dotDotX.x * phi[1, 4],
            //    dotDotX.z * phi[1, 2], dotDotX.z * phi[1, 4]
            //}) / yd;
            //var inverse = (l - r).Inverse();
            //var bumpZ = -xd * inverse[0, 0] - zd * inverse[0, 1];
            //var bumpYdot = -xd * inverse[1, 0] - zd * inverse[1, 1];
            //if (!bumpYdot.IsFinite() || !bumpZ.IsFinite())
            //{
            //    Console.WriteLine("Bad start..");
            //    return default;
            //}
            var sq = Math.Sqrt(r.Sum(i => i * i));
            for (int i = 0;i<r.Length;i++)
            {
                r[i] /= -sq;
            }
            //var bumpZ = Math.Abs(retArr[2]) < 0.5 ? retArr[2] : Math.Sign(retArr[2]) * 0.001;
            //var bumpYdot = Math.Abs(retArr[4]) < 0.5 ? retArr[4] : Math.Sign(retArr[4]) * 0.001;
            return (new Point3(r[0], r[1], r[2]), new Point3(r[3], r[4], r[5]), 0, new Point3(xd, 0, zd).Length());
        }
        public Matrix<double> F(Vector<double> X, double mu)
        {
            var l = new Point3(X[0], X[1], X[2]);
            var v = new Point3(X[3], X[4], X[5]);
            var dadx = DADX(l, v, mu);
            var dady = DADY(l, v, mu);
            var dadz = DADZ(l, v, mu);
            return Matrix<double>.Build.Dense(6, 6, new double[]
            {
                0, 0, 0, 1, 0, 0,
                0, 0, 0, 0, 1, 0,
                0, 0, 0, 0, 0, 1,
                dadx[0], dadx[1], dadx[1], 0, 2, 0,
                dadx[1], dady[1], dady[2], -2, 0, 0,
                dadx[2], dady[2], dadz[2], 0, 0, 0
            });
        }
        public static double R1Sq(Point3 n, double mu)
        {
            var _1 = n.x + mu;
            var _2 = n.y;
            var _3 = n.z;
            return _1 * _1 + _2 * _2 + _3 * _3;
        }
        public static double R2Sq(Point3 n, double mu)
        {
            var _1 = n.x - 1.0 + mu;
            var _2 = n.y;
            var _3 = n.z;
            return _1 * _1 + _2 * _2 + _3 * _3;
        }
        public static Point3 A(Point3 n, Point3 v, double mu)
        {
            var r1 = Math.Pow(R1Sq(n, mu), 1.5);
            var r2 = Math.Pow(R2Sq(n, mu), 1.5);
            double x = n[0], y = n[1], z = n[2];
            var xdd = 2 * v[1] + x - (1 - mu) * (x + mu) / r1 - mu * (x - 1 + mu) / r2;
            var ydd = -2 * v[0] + y - (1 - mu) * y / r1 - mu * y / r2;
            var zdd = -(1 - mu) * z / r1 - mu * z / r2;
            return new Point3(xdd, ydd, zdd);
        }
        public Point3 DADX(Point3 n, Point3 v, double mu)
        {
            var d = new Point3(dConst, 0, 0);
            return (A(n + d, v, mu) / dConst - A(n, v, mu) / dConst);
        }
        public Point3 DADY(Point3 n, Point3 v, double mu)
        {
            var d = new Point3(0, dConst, 0);
            return (A(n + d, v, mu) / dConst - A(n, v, mu) / dConst) ;
        }
        public Point3 DADZ(Point3 n, Point3 v, double mu)
        {
            var d = new Point3(0, 0, dConst);
            var z1 = A(n + d, v, mu);
            var z2 = A(n, v, mu);
            var dz = (z1 / dConst - z2 / dConst) ;
            return dz;
        }
        public double JaccobiConstant(Vector<double> X)
        {
            var nx = new Point3(X[0], X[1], X[2]);
            var ndx = new Point3(X[3], X[4], X[5]);
            var U = 0.5 * (nx.x * nx.x + nx.y * nx.y) + (1 - Mu) / Math.Sqrt(R1Sq(nx, Mu)) + Mu / Math.Sqrt(R2Sq(nx, Mu));
            var Vsq = ndx.LengthSquared();
            return 2 * U - Vsq;
        }
    }
}
