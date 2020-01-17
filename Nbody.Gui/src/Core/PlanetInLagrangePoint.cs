using NBody.Gui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace NBody.Core
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

        public PlanetInLagrangePoint(Planet smallerPlanet, Planet largerPlanet, int lagrangePoint, real_t radius, real_t offset = 0, real_t mass = 0, string name = default, Point3 controll = default, real_t dV = real_t.MaxValue) : base()
        {
            this.Position = smallerPlanet.LagrangePoint[lagrangePoint](largerPlanet, offset);
            this.Velocity = largerPlanet.GetNullVelocity(smallerPlanet, this.Position);
            this.Mass = mass;
            this.Radius = radius;
            this.Name = name?.Replace("{0}", largerPlanet.Name).Replace("{1}", smallerPlanet.Name).Replace("{2}", lagrangePoint.ToString());
            SmallerPlanet = smallerPlanet;
            LargerPlanet = largerPlanet;
            L = lagrangePoint;
            Controll = controll;
            DV = dV;
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
        }
    }
}
