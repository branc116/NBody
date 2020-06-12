using Godot;
using Nbody.Gui.Attributes;
using Nbody.Gui.InputModels;
using Nbody.Core;
using Nbody.Gui;
using Nbody.Gui.Extensions;
using System;
using System.Linq;
using Nbody.Gui.Core;
using Newtonsoft.Json.Schema;

namespace Nbody.Gui.Controllers
{
    [PlotFunction]
    public class PlotFunctionsController
    {
        private readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
        public Vector2[] PhaseSpace(Planet planet)
        {
            var points = planet.PositionHistory.Select(i =>
            {
                var position = i.ToV3().Normalized();
                var phi = position.AngleTo(Vector3.Right);
                var theta = position.AngleTo(Vector3.Up);
                return new Vector2(phi, theta);
            }).ToArray();

            return points;
        }
        public Vector2[] PhaseSpace2(Planet planet)
        {
            var points = planet.PositionHistory.Select(i =>
            {
                var p = i.ToV3();
                var r = p.Length();
                var lon = Mathf.Acos(p.x / Mathf.Sqrt(p.x * p.x + p.y * p.y)) * (p.y < 0 ? -1 : 1);
                var lat = Mathf.Acos(p.z / r);
                return new Vector2(lon, lat);
            }).ToArray();

            return points;
        }
        public Vector2[] PhaseSpace(Planet planet, Planet planet2)
        {
            var history1 = planet.PositionHistory.Select(i => i.ToV3()).ToArray();
            var history2 = planet2.PositionHistory.Select(i => i.ToV3()).ToArray();
            var to = Math.Min(history1.Length, history2.Length);
            var retArr = new Vector2[to];
            for (int i = 0; i < to; i++)
            {
                var diff = history1[i] - history2[i];
                var phi = Mathf.Pow(diff.AngleTo(Vector3.Right), 2);
                var theta = Mathf.Pow(diff.AngleTo(Vector3.Up), 2);
                retArr[i] = new Vector2(phi, theta);
            }
            return retArr;
        }
        public Vector2[] Distance(Planet planet, Planet planet2)
        {
            var history1 = planet.PositionHistory.Select(i => i.ToV3()).ToArray();
            var history2 = planet2.PositionHistory.Select(i => i.ToV3()).ToArray();
            var to = Math.Min(history1.Length, history2.Length);
            var retArr = new Vector2[to];
            var stepsPerDiv = _plotsModel.PlotStepsPerDiv;
            for (int i = 0; i < to; i++)
            {
                var diff = history1[i] - history2[i];
                var phi = diff.AngleTo(Vector3.Right);
                var theta = diff.AngleTo(Vector3.Up);
                retArr[i] = new Vector2(i / stepsPerDiv, diff.Length());
            }
            return retArr;
        }
        public Vector2[] DistanceTraveling(Planet planet, Planet planet2)
        {
            var history1 = planet.PositionHistory.Select(i => i.ToV3()).ToArray();
            var history2 = planet2.PositionHistory.Select(i => i.ToV3()).ToArray();
            var to = Math.Min(history1.Length, history2.Length);
            var position = planet.Steps;
            var retArr = new Vector2[to];
            var stepsPerDiv = _plotsModel.PlotStepsPerDiv;
            for (int i = 0; i < to; i++)
            {
                var diff = history1[i] - history2[i];
                var phi = diff.AngleTo(Vector3.Right);
                var theta = diff.AngleTo(Vector3.Up);
                retArr[i] = new Vector2((float)(position - i) / stepsPerDiv, diff.Length());
            }
            return retArr;
        }
        public Vector2[] DistanceMomentum(Planet planet, Planet planet2)
        {
            var px1 = planet.PositionHistory.Select(i => i.ToV3()).ToArray();
            var px2 = planet2.PositionHistory.Select(i => i.ToV3()).ToArray();
            var pm1 = (float)planet.Mass;
            var pm2 = (float)planet2.Mass;
            var pv1 = planet.VelocityHistory.Select(i => i.ToV3().LengthSquared() * pm1 / 2f).ToArray();
            var pv2 = planet2.VelocityHistory.Select(i => i.ToV3().LengthSquared() * pm1 / 2f).ToArray();
            var to = Math.Min(px1.Length, px2.Length);
            var position = planet.Steps;
            var retArr = new Vector2[to];
            var stepsPerDiv = _plotsModel.PlotStepsPerDiv;
            for (int i = 0; i < to; i++)
            {
                var diff = px2[i].DistanceTo(px1[i]);
                var momentum = pv1[i] + pv2[i];
                retArr[i] = new Vector2(diff, momentum);
            }
            return retArr;
        }
        public Vector2[] KineticEnergy(Planet planet)
        {
            var j = 0;
            var step = planet.Steps;
            var stepsPerDiv = _plotsModel.PlotStepsPerDiv;
            var planetMass = (float)planet.Mass;
            var history1 = planet.VelocityHistory.Select(i => i.ToV3())
                .Select(i => i.LengthSquared() * (float)planetMass)
                .Select(i => new Vector2((j++) / stepsPerDiv, i))
                .ToArray();
            return history1;
        }
        public Vector2[] ProjectToOrbitPlane(Planet p1, Planet p2)
        {
            (Planet sun, Planet earth) = p1.Mass > p2.Mass ? (p1, p2) : (p2, p1);
            var normal = (earth.Position - sun.Position).Cross(earth.Velocity);
            var d = -normal.Dot(sun.Position);
            var ls = normal.LengthSquared();

            var oR = (sun.Position + Point3.Right);
            var xOs = (oR - (((oR.Dot(normal) + d) / ls) * normal)).Normalized();
            var yOs = xOs.Cross(normal).Normalized();
            var points = earth.PositionHistory
                .Select(i => i - (((i.Dot(normal) + d)/ls) * normal))
                .Select(i => new Vector2((float)i.Dot(xOs), (float)i.Dot(yOs)))
                .ToArray();
            return points;
        }
        public Vector2[] ProjectToOrbitPlaneRelative(Planet p1, Planet p2)
        {
            (Planet sun, Planet earth) = p1.Mass > p2.Mass ? (p1, p2) : (p2, p1);
            var normal = (earth.Position - sun.Position).Cross(earth.Velocity);
            var d = -normal.Dot(sun.Position);
            var ls = normal.LengthSquared();

            var oR = (sun.Position + Point3.Right);
            var xOs = (oR - (((oR.Dot(normal) + d) / ls) * normal)).Normalized();
            var yOs = xOs.Cross(normal).Normalized();

            var sPs = sun.PositionHistory.ToList();
            var ePs = earth.PositionHistory.ToList();
            var n = Math.Min(sPs.Count, ePs.Count);
            var arr = new Vector2[n];
            for (int i = 0;i<n;i++)
            {
                var relative = ePs[ePs.Count - 1 - i] - sPs[sPs.Count - 1 - i];
                var onPlain = relative - (((relative.Dot(normal) + d) / ls) * normal);
                var point = new Vector2((float)onPlain.Dot(xOs), (float)onPlain.Dot(yOs));
                arr[n - i - 1] = point;
            }
            return arr;
        }
        public Vector2[] DriftFromLagrangeXY(Planet p1)
        {
            if (p1 is PlanetInLagrangePoint pilp)
            {
                var retArr = pilp.DistanceFromLPoint.Select(i => new Vector2((float)i.x, (float)i.y))
                    .ToArray();
                return retArr;
            }
            return Array.Empty<Vector2>();
        }
        public Vector2[] DriftFromLagrangeXZ(Planet p1)
        {
            if (p1 is PlanetInLagrangePoint pilp)
            {
                var retArr = pilp.DistanceFromLPoint.Select(i => new Vector2((float)i.x, (float)i.z))
                    .ToArray();
                return retArr;
            }
            return Array.Empty<Vector2>();
        }
        public Vector2[] DriftFromLagrangeYZ(Planet p1)
        {
            if (p1 is PlanetInLagrangePoint pilp)
            {
                var retArr = pilp.DistanceFromLPoint.Select(i => new Vector2((float)i.y, (float)i.z))
                    .ToArray();
                return retArr;
            }
            return Array.Empty<Vector2>();
        }
        public Vector2[] ErrorLgrangeIntegral(Planet p1)
        {
            if (p1 is PlanetInLagrangePoint pilp)
            {
                var curSum = Point3.Zero;
                var errors = pilp.DistanceFromLPoint.ToArray();
                var dt = SourceOfTruth.SimulationModel.DeltaTimePerStep;
                return errors.Select((cur, i) =>
                {
                    curSum += cur * dt;
                    return new Vector2((float)i, (float)curSum.DistanceSquaredTo(Point3.Zero)/1000);
                }).ToArray();
            }
            return Array.Empty<Vector2>();
        }
        public Vector2[] DirectionChangesPosition(Planet p1)
        {
            if (p1 is PlanetInLagrangePoint pilp)
            {
                var curSum = Point3.Zero;
                var errors = pilp.VelocityChanges.ToArray();
                return errors.Select(i => new Vector2((float)i.time, (float)i.distanceToLPoint.Length())).ToArray();
            }
            return Array.Empty<Vector2>();
        }
        public Vector2[] DirectionChangesSpeedY(Planet p1)
        {
            if (p1 is PlanetInLagrangePoint pilp)
            {
                var curSum = Point3.Zero;
                var errors = pilp.VelocityChanges.ToArray();
                return errors.Select(i => new Vector2((float)i.time, (float)i.velocityRelativeToLPoint.Length())).ToArray();
            }
            return Array.Empty<Vector2>();
        }
    }
}
