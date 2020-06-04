using MathNet.Numerics;
using Nbody.Gui;
using Nbody.Gui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace Nbody.Core
{
    public class PlanetInLagrangePoint : Planet
    {
        public Planet SmallerPlanet { get; }
        public Planet LargerPlanet { get; }
        public int L { get; }
        public override string Type => $"Planet in L{L}";
        private Point3 _pError = Point3.Zero;
        private Point3 _iError = Point3.Zero;
        private Point3 _dError = Point3.Zero; 
        public Point3 Controll { get; }
        public double DV { get; set; }
        private Point3 _internalAcceleration = Point3.Zero;
        public CircularArray<Point3> DistanceFromLPoint { get; set; } = new CircularArray<Point3>(SourceOfTruth.SimulationModel.MaxHistoy, SourceOfTruth.SimulationModel.RememberEvery);
        public real_t UnitDistanceInRatationFrame { get; }
        public Point3 PositionInNormalizedFrame
        {
            get
            {
                var basis_x = (SmallerPlanet.Position - LargerPlanet.Position).Normalized();
                var basis_y = SmallerPlanet.Velocity.Normalized();
                var basis_z = basis_x.Cross(basis_y).Normalized();
                return Position / UnitDistanceInRatationFrame;
            }
        }

        public PlanetInLagrangePoint(Planet smallerPlanet, Planet largerPlanet, int lagrangePoint, real_t radius, real_t offset = 0, real_t mass = 0, string name = default, Point3 controll = default, real_t dV = real_t.MaxValue) : base()
        {
            this.Position = smallerPlanet.LagrangePoint[lagrangePoint](largerPlanet, offset);
            this.Velocity = largerPlanet.GetNullVelocity(smallerPlanet, this.Position);
            this.Mass = mass;
            this.Radius = radius;
            this.Name = name?.Replace("{0}", largerPlanet.Name).Replace("{1}", smallerPlanet.Name).Replace("{2}", lagrangePoint.ToString());
            SmallerPlanet = smallerPlanet;
            LargerPlanet = largerPlanet;
            UnitDistanceInRatationFrame = smallerPlanet.Position.DistanceTo(largerPlanet.Position);
            L = lagrangePoint;
            Controll = controll;
            DV = dV;
        }
        public PlanetInLagrangePoint(Planet smallerPlanet, Planet largerPlanet, int lagrangePoint, Point3 offset, Point3 dotOffset, string name) : base()
        {
            UnitDistanceInRatationFrame = smallerPlanet.Position.DistanceTo(largerPlanet.Position);
            this.Position = smallerPlanet.LagrangePoint[lagrangePoint](largerPlanet, (real_t)0);
            this.Velocity = largerPlanet.GetNullVelocity(smallerPlanet, this.Position) + dotOffset;
            this.Position += offset * UnitDistanceInRatationFrame;
            this.Mass = (real_t)0;
            this.Radius = (real_t)0.5;
            this.Name = name ?? $"temp_{DateTime.Now.Ticks}" ;
            SmallerPlanet = smallerPlanet;
            LargerPlanet = largerPlanet;
            L = lagrangePoint;
        }
        public override Point3 InternalAcceleration(bool recalculate, real_t dt)
        {
            if (recalculate && DV > 0)
            {
                var oldP = _pError;
                var wantedPosition = SmallerPlanet.LagrangePoint[L](LargerPlanet, 0);
                var wantedVelocity = LargerPlanet.GetNullVelocity(SmallerPlanet, wantedPosition);
                _pError = Velocity - wantedVelocity;
                _iError = Position - SmallerPlanet.LagrangePoint[L](LargerPlanet, 0);
                //_iError += _pError * dt;
                _dError = (oldP - _pError);
                _internalAcceleration = (_pError * Controll.x + _iError * Controll.y + _dError * Controll.z) * (real_t)(-1);
                DV -= _internalAcceleration.Length()*dt;
            }
            return _internalAcceleration;
            //return base.InternalAcceleration(false);
        }
        public override IEnumerable<(string name, string value)> GetPlanetInfo()
        {
            var baseInfos = base.GetPlanetInfo();
            foreach (var baseInfo in baseInfos)
                yield return baseInfo;
            yield return ($"Drift from L{L}", SmallerPlanet.LagrangePoint[L](LargerPlanet, 0).DistanceTo(Position).ToString("F3"));
            yield return ("Smaller planet", SmallerPlanet.Name);
            yield return ("Distance to Smaller planet", SmallerPlanet.Position.DistanceTo(Position).ToString("F3"));
            yield return ("Relative speed to Smaller planet", SmallerPlanet.Velocity.DistanceTo(Velocity).ToString("F3"));
            yield return ("Relative velocity to Smaller planet", SmallerPlanet.Velocity.DistanceTo(Velocity).ToString("F3"));
            yield return ("Larger planet", LargerPlanet.Name);
            yield return ("Distance to Larger planet", LargerPlanet.Position.DistanceTo(Position).ToString("F3"));
            yield return ("Relative speed to Larger planet", LargerPlanet.Velocity.DistanceTo(Velocity).ToString("F3"));
            yield return ("Relative velocity to Larger planet", LargerPlanet.Velocity.DistanceTo(Velocity).ToString("F3"));
            yield return ("Proportional error", _pError.ToString("F3"));
            yield return ("Integral error", _iError.ToString("F3"));
            yield return ("Derivation error", _dError.ToString("F3"));
            yield return ("DV left", DV.ToString("F3"));
            yield return ("Normalized position", )
        }
        public override void AfterUpdate()
        {
            var diff = Position - SmallerPlanet.LagrangePoint[L](LargerPlanet, 0);
            DistanceFromLPoint.Add(diff);
        }
        public real_t U(Point3 location, real_t mu)
        {
            location += this.Position;
            var fact = (LargerPlanet.Position.DistanceTo(SmallerPlanet.Position));
            var scaled = location / fact;
            var largeDistance = location.DistanceTo(LargerPlanet.Position) / fact;
            var smallDistance = location.DistanceTo(SmallerPlanet.Position) / fact;
            var energy = (scaled.x * scaled.x) * (scaled.y * scaled.y) / 2 + (1 - mu) / smallDistance + mu / largeDistance;
            return energy;
        }
        public real_t DuDx(Point3 location, real_t mu)
        {
            var dx = new Point3(location.x / (real_t)10e-5 + 10e-5, (real_t)0, (real_t)0);
            var dUdx = (U(location + dx, mu) - U(location, mu)) / dx.x;
            return dUdx;
        }
        public real_t DuDy(Point3 location, real_t mu)
        {
            var dy = new Point3((real_t)0, location.y / (real_t)10e-5 + 10e-5, (real_t)0);
            var dUdy = (U(location + dy, mu) - U(location, mu)) / dy.y;
            return dUdy;
        }
        public real_t DuDz(Point3 location, real_t mu)
        {
            var dz = new Point3((real_t)0, (real_t)0, location.z / (real_t)10e-3 + 10e-5);
            var dUdz = (U(location + dz, mu) - U(location, mu)) / dz.z;
            return dUdz;
        }
        public real_t Calc(Point3 x0, Point3 xDot0, real_t mu)
        {
            int? ySign = null;
            real_t t = 0;
            while (true)
            { 
                var dt = (real_t)10e-3;
                t += dt;
                var xdotdot0 = new Point3(DuDx(x0, mu) + 2 * xDot0.y,
                    DuDy(x0, mu) - 2 * xDot0.x,
                    DuDz(x0, mu));
                xDot0 += xdotdot0 * dt;
                x0 += xDot0 * dt;
                if (ySign != null && Math.Sign(x0.y) != ySign)
                    return t;
                if (ySign == null && x0.y != 0)
                    ySign = Math.Sign(x0.y);
                if (x0.DistanceTo(LargerPlanet.Position) > 2 * SmallerPlanet.Position.DistanceTo(LargerPlanet.Position))
                    return -t;
                if (t > 500)
                    return t;
            }
        }
    }
}
