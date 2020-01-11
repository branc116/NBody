using Godot;
using Nbody.Gui.Attributes;
using NBody.Core;
using NBody.Gui;
using NBody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.Controllers
{
    [PlotFunction]
    public class PlotFunctionsController
    {
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
            for(int i=0;i<to;i++)
            {
                var diff = history1[i] - history2[i];
                var phi = diff.AngleTo(Vector3.Right);
                var theta = diff.AngleTo(Vector3.Up);
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
            var stepsPerDiv = SourceOfTruth.PlotStepsPerDiv;
            for (int i = 0; i < to; i++)
            {
                var diff = history1[i] - history2[i];
                var phi = diff.AngleTo(Vector3.Right);
                var theta = diff.AngleTo(Vector3.Up);
                retArr[i] = new Vector2(i/stepsPerDiv, diff.Length());
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
            var stepsPerDiv = SourceOfTruth.PlotStepsPerDiv;
            for (int i = 0; i < to; i++)
            {
                var diff = history1[i] - history2[i];
                var phi = diff.AngleTo(Vector3.Right);
                var theta = diff.AngleTo(Vector3.Up);
                retArr[i] = new Vector2((float)(position - i)/stepsPerDiv, diff.Length());
            }
            return retArr;
        }
        public Vector2[] DistanceMomentum(Planet planet, Planet planet2)
        {
            var px1 = planet.PositionHistory.Select(i => i.ToV3()).ToArray();
            var px2 = planet2.PositionHistory.Select(i => i.ToV3()).ToArray();
            var pm1 = (float)planet.Mass;
            var pm2 = (float)planet2.Mass;
            var pv1 = planet.VelocityHistory.Select(i => i.ToV3().LengthSquared()*pm1/2f).ToArray();
            var pv2 = planet2.VelocityHistory.Select(i => i.ToV3().LengthSquared() * pm1 / 2f).ToArray();
            var to = Math.Min(px1.Length, px2.Length);
            var position = planet.Steps;
            var retArr = new Vector2[to];
            var stepsPerDiv = SourceOfTruth.PlotStepsPerDiv;
            for (int i = 0; i < to; i++)
            {
                var diff = px2[i].DistanceTo(px1[i]);
                var momentum = pv1[i]+pv2[i];
                retArr[i] = new Vector2(diff, momentum);
            }
            return retArr;
        }
        public Vector2[] KineticEnergy(Planet planet)
        {
            var j = 0;
            var step = planet.Steps;
            var stepsPerDiv = SourceOfTruth.PlotStepsPerDiv;
            var planetMass = (float)planet.Mass;
            var history1 = planet.VelocityHistory.Select(i => i.ToV3())
                .Select(i => i.LengthSquared() * (float)planetMass)
                .Select(i => new Vector2((j++)/stepsPerDiv, i))
                .ToArray();
            return history1;
        }
    }
}
