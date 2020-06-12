using Godot;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra;
using Nbody.Gui;
using Nbody.Gui.Core;
using Nbody.Gui.Extensions;
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

        private const double intConstant = 10e-6;
        private Point3 _pError = Point3.Zero;
        private Point3 _iError = Point3.Zero;
        private Point3 _dError = Point3.Zero; 
        public Point3 Controll { get; }
        public double DV { get; set; }
        private Point3 _internalAcceleration = Point3.Zero;
        public CircularArray<Point3> DistanceFromLPoint { get; set; } = new CircularArray<Point3>(SourceOfTruth.SimulationModel.MaxHistoy, SourceOfTruth.SimulationModel.RememberEvery);
        public real_t UnitDistanceInRatationFrame { get; }
        public Point3 Offset0 { get; protected set; }
        public Point3 DotOffset0 { get; protected set; }
        public Point3 PositionInNormalizedFrame => TransformToNormalizedCoordinates(Position);
        public int? PastYPositionSign { get; private set; }
        public CircularArray<(Point3 distanceToLPoint, Point3 velocityRelativeToLPoint, real_t time)> VelocityChanges { get; } = new CircularArray<(Point3, Point3, real_t)>(SourceOfTruth.SimulationModel.MaxHistoy, SourceOfTruth.SimulationModel.RememberEvery);
        public PlanetInLagrangePoint(Planet smallerPlanet, Planet largerPlanet, int lagrangePoint, real_t radius, real_t offset = 0, real_t mass = 0, string name = default, Point3 controll = default, real_t dV = real_t.MaxValue) : this(smallerPlanet, largerPlanet, lagrangePoint, null)
        {
            Mass = mass;
            Radius = radius;
            Name = name?.Replace("{0}", largerPlanet.Name).Replace("{1}", smallerPlanet.Name).Replace("{2}", lagrangePoint.ToString());
            Controll = controll;
            DV = dV;
        }
        public PlanetInLagrangePoint(Planet smallerPlanet, Planet largerPlanet, int lagrangePoint, Point3 offset, Point3 dotOffset, string name) : this(smallerPlanet, largerPlanet, lagrangePoint, null)
        {
            UnitDistanceInRatationFrame = smallerPlanet.Position.DistanceTo(largerPlanet.Position);
            Velocity += dotOffset;
            Position += offset * UnitDistanceInRatationFrame;
            Name = name ?? $"temp_{DateTime.Now.Ticks}" ;
            Offset0 = offset;
            DotOffset0 = dotOffset;
        }
        protected PlanetInLagrangePoint(Planet smallerPlanet, Planet largerPlanet, int lagrangePoint, string name) : base()
        {
            UnitDistanceInRatationFrame = smallerPlanet.Position.DistanceTo(largerPlanet.Position);
            Position = smallerPlanet.LagrangePoint[lagrangePoint](largerPlanet, (real_t)0);
            Velocity = largerPlanet.GetNullVelocity(smallerPlanet, this.Position);
            Mass = (real_t)0;
            Radius = (real_t)0.5;
            SmallerPlanet = smallerPlanet;
            LargerPlanet = largerPlanet;
            L = lagrangePoint;
            Name = name;
        }
        protected PlanetInLagrangePoint()
        {

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
            yield return ("Normalized position", PositionInNormalizedFrame.ToString("F3"));
            var (position, velocity, time) = VelocityChanges.Last();
            yield return ("Last velocity direction change time", time.ToString("F3"));
            yield return ("Last velocity direction change veloctiy", velocity.ToString("F3"));
            yield return ("Last velocity direction change position", position.ToString("F3"));
        }
        public override void AfterUpdate()
        {
            var lx = SmallerPlanet.LagrangePoint[L](LargerPlanet, 0);
            var diff = Position - lx;
            var diffNorm = TransformToNormalizedCoordinates(diff);
            DistanceFromLPoint.Add(diff);
            var LdotX0 = LargerPlanet.GetNullVelocity(SmallerPlanet, lx);
            var dotX = this.Velocity;
            var diffDotX = TransformToNormalizedCoordinates(dotX - LdotX0);
            var sign = Math.Sign(diffNorm.y);
            if ((PastYPositionSign is null || sign != PastYPositionSign.Value) && diffNorm.y != 0)
            {
                this.PastYPositionSign = sign;
                this.VelocityChanges.Add((diffNorm, diffDotX, SourceOfTruth.System.Get.CurTime));
            }
        }
        public real_t U(Point3 location, real_t mu)
        {
            var scaled = location;
            var largeDistance = location.Length();
            var smallDistance = location.DistanceTo(new Point3(1, 0, 0));
            var energy = (scaled.x * scaled.x) * (scaled.y * scaled.y) / 2 + (1 - mu) / smallDistance + mu / largeDistance;
            return energy;
        }
        public real_t DuDx(Point3 location, real_t mu)
        {
            var dx = new Point3(location.x / (real_t)intConstant + intConstant, (real_t)0, (real_t)0);
            var dUdx = (U(location + dx, mu) - U(location, mu)) / dx.x;
            return dUdx;
        }
        public real_t DuDy(Point3 location, real_t mu)
        {
            var dy = new Point3((real_t)0, location.y / (real_t)intConstant + intConstant, (real_t)0);
            var dUdy = (U(location + dy, mu) - U(location, mu)) / dy.y;
            return dUdy;
        }
        public real_t DuDz(Point3 location, real_t mu)
        {
            var dz = new Point3((real_t)0, (real_t)0, location.z / (real_t)10e-3 + intConstant);
            var dUdz = (U(location + dz, mu) - U(location, mu)) / dz.z;
            return dUdz;
        }
        public real_t DuDxDz(Point3 location, real_t mu)
        {
            var d = new Point3((real_t)0, (real_t)0, location.z / (real_t)10e-3 + intConstant);
            var dxz = (DuDx(location + d, mu) - DuDx(location, mu)) / d.z;
            return dxz;
        }
        public real_t DuDxDy(Point3 location, real_t mu)
        {
            var d = new Point3((real_t)0, location.y / (real_t)10e-3 + intConstant, (real_t)0);
            var dxy = (DuDx(location + d, mu) - DuDx(location, mu)) / d.y;
            return dxy;
        }
        public real_t DuDyDz(Point3 location, real_t mu)
        {
            var d = new Point3((real_t)0, (real_t)0, location.z / (real_t)10e-3 + intConstant);
            var dyz = (DuDy(location + d, mu) - DuDy(location, mu)) / d.z;
            return dyz;
        }
        public real_t DuDxDx(Point3 location, real_t mu)
        {
            var d = new Point3(location.x / (real_t)10e-3 + intConstant, (real_t)0, (real_t)0);
            var dxx = (DuDx(location + d, mu) - DuDx(location, mu)) / d.x;
            return dxx;
        }
        public real_t DuDyDy(Point3 location, real_t mu)
        {
            var d = new Point3((real_t)0, location.y / (real_t)10e-3 + intConstant, (real_t)0);
            var dxy = (DuDy(location + d, mu) - DuDy(location, mu)) / d.y;
            return dxy;
        }
        public real_t DuDzDz(Point3 location, real_t mu)
        {
            var d = new Point3((real_t)0, (real_t)0, location.z / (real_t)10e-3 + intConstant);
            var dyz = (DuDz(location + d, mu) - DuDz(location, mu)) / d.z;
            return dyz;
        }
        public override IEnumerable<Point3> GetTracePoints()
        {
            IEnumerable<Point3> p((Point3 position, Point3 driftFromL) i) {
                yield return i.position;
                yield return i.position - i.driftFromL;
            }
            return this.PositionHistory.GetPairs(this.DistanceFromLPoint).SelectMany(p);
        }
        public Point3 TransformToNormalizedCoordinates(Point3 globalCoordinates)
        {
            var basis_x = (SmallerPlanet.Position - LargerPlanet.Position).Normalized();
            var basis_y = SmallerPlanet.Velocity.Normalized();
            var basis_z = basis_x.Cross(basis_y).Normalized();
            return new Point3(globalCoordinates.Dot(basis_x), globalCoordinates.Dot(basis_y), globalCoordinates.Dot(basis_z)) / UnitDistanceInRatationFrame;
        }
        public Point3 TransformToGlobalCoordinates(Point3 normalized)
        {
            var basis_x = (SmallerPlanet.Position - LargerPlanet.Position).Normalized();
            var basis_y = SmallerPlanet.Velocity.Normalized();
            var basis_z = basis_x.Cross(basis_y).Normalized();
            var trans = Transform.Identity;
            trans.basis.Column0 = basis_x.ToV3();
            trans.basis.Column1 = basis_y.ToV3();
            trans.basis.Column2 = basis_z.ToV3();
            var basies = trans.basis.Transposed().Inverse().Transposed();
            var a = new Point3(basies.Column0.Dot(normalized.ToV3()), basies.Column1.Dot(normalized.ToV3()), basies.Column2.Dot(normalized.ToV3()));
            return a * UnitDistanceInRatationFrame;
        }
        public static Point3 TransfromNormalizedToGlobal(Planet larger, Planet smaller, Point3 normalized)
        {
            var basis_x = (smaller.Position - larger.Position).Normalized();
            var basis_y = smaller.Velocity.Normalized();
            var basis_z = basis_x.Cross(basis_y).Normalized();
            var trans = Transform.Identity;
            trans.basis.Column0 = basis_x.ToV3();
            trans.basis.Column1 = basis_y.ToV3();
            trans.basis.Column2 = basis_z.ToV3();
            var basies = trans.basis.Transposed().Inverse().Transposed();
            var a = new Point3(basies.Column0.Dot(normalized.ToV3()), basies.Column1.Dot(normalized.ToV3()), basies.Column2.Dot(normalized.ToV3()));
            return a * larger.Position.DistanceTo(smaller.Position);
        }
        public static Point3 TransfromNormalizedToGlobalDontScale(Planet larger, Planet smaller, Point3 normalized)
        {
            var basis_x = (smaller.Position - larger.Position).Normalized();
            var basis_y = smaller.Velocity.Normalized();
            var basis_z = basis_x.Cross(basis_y).Normalized();
            var trans = Transform.Identity;
            trans.basis.Column0 = basis_x.ToV3();
            trans.basis.Column1 = basis_y.ToV3();
            trans.basis.Column2 = basis_z.ToV3();
            var basies = trans.basis.Transposed().Inverse().Transposed();
            var a = new Point3(basies.Column0.Dot(normalized.ToV3()), basies.Column1.Dot(normalized.ToV3()), basies.Column2.Dot(normalized.ToV3()));
            return a;
        }
        //public real_t Fittness()
        //{
        //    var arr = VelocityChanges.GetNeigbros().ToArray();
        //    if (arr.Length == 1)
        //        return 1/ 
        //    arr.Length / arr.Select(i => ? i.Item1.time - i.Item2.time);
        //}
    }
}
