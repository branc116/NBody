﻿using MathNet.Numerics.LinearAlgebra;
using Nbody.Gui.Core;
using Nbody.Gui;
using Nbody.Gui.InputModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using MathNet.Numerics;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace Nbody.Core
{
    public enum Direction
    {
        X = 0,
        Y = 1,
        Z = 2
    }
    public class PlanetSystem
    {
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        private readonly Nbody.Gui.Kernels.NbodyClKernel _nbodyClKernel = SourceOfTruth.Kernel;
        public List<Planet> Planets { get; set; }
        private int _lastStep = -1;
        private List<Point3> _oldPositions;
        public List<Point3> Positions
        {
            get
            {
                if (_lastStep == NStep)
                    return _oldPositions;
                _lastStep = NStep;
                return _oldPositions = Planets.Select(i => i.Position).ToList();
            }
        }
        public int NStep { get; private set; } = 0;

        public void Step(int numberOfSteps)
        {
            while (numberOfSteps-- > 0)
            {
                Step();
            }
        }

        public real_t CurTime { get; private set; } = (real_t)0;
        public bool SimulateColitions { get => _simulationModel.SimulateColitions; set => _simulationModel.SimulateColitions.Set(value); }
        public real_t GravitationalConstant { get => _simulationModel.GravitationalConstant; set => _simulationModel.GravitationalConstant.Set(value); }
        public real_t Dt { get => _simulationModel.DeltaTimePerStep; set => _simulationModel.DeltaTimePerStep.Set(value); }
        public DateTime PlanetNamesLastChanged { get; private set; } = DateTime.Now;
        public PlanetSystem()
        {
            Planets = new List<Planet>();
        }
        public PlanetSystem StepCL(int n = 1)
        {
            if (_nbodyClKernel is null)
            {
                Step(n);
                return this;
            }
            _nbodyClKernel.Step(this, n);
            NStep += n;
            CurTime += n * Dt;
            return this;
        }
        public PlanetSystem Step()
        {
            var mergeNeedeed = false;
            var positions = Positions.Select((p, i) => (p, i)).Where(i => Planets[i.i].Mass != 0).ToArray();
            var dtps = _simulationModel.DeltaTimePerStep;
            var G = _simulationModel.GravitationalConstant;
            var doPid = NStep % _simulationModel.DoPidEveryStep == 0;
            for (var i1 = 0; i1 < Planets.Count; i1++)
            {
                var planet = Planets[i1];
                var newPosition = planet.Position;
                var newVelocity = planet.Velocity;

                for (var index = 0; index < positions.Length; index++)
                {
                    var j = positions[index].i;
                    if (i1 == j)
                        continue;
                    var interacts = Planets[j];
                    if (interacts.Mass == 0)
                        continue;
                    var position = positions[index].p;
                    var n = planet.Position - position;

                    var norm = (real_t)n.LengthSquared();
                    var absoluteAcceleration = G * interacts.Mass / (norm);
                    newVelocity += n.Normalized() * (-absoluteAcceleration * dtps);
                    var dist = interacts.Radius + planet.Radius;
                    if (norm < dist * dist)
                        mergeNeedeed = true;
                }
                if (doPid)
                    newVelocity += planet.InternalAcceleration(true, dtps * _simulationModel.DoPidEveryStep) * dtps;
                newPosition += newVelocity * dtps;
                planet.Position = newPosition;
                planet.Velocity = newVelocity;
                planet.AfterUpdate();
            }
            if (_simulationModel.SimulateColitions && mergeNeedeed)
                MergePlanets();
            NStep++;
            CurTime += dtps;
            return this;
        }
        public Point3 A(Point3 point)
        {
            var planets = Planets.Where(i => i.Mass != 0);
            var cur = Point3.Zero;
            foreach (var planet in planets)
            {
                cur += _simulationModel.GravitationalConstant * planet.Mass / planet.Position.DistanceSquaredTo(point) * planet.Position.Normalized();
            }
            return cur;
        }
        public Point3 DAD(Direction d, Point3 point)
        {
            var val = 10e-7;
            var v = d switch {
                { } dir when dir == Direction.X => new Point3(val, 0, 0),
                { } dir when dir == Direction.Y => new Point3(0, val, 0),
                { } dir when dir == Direction.Z => new Point3(0, 0, val),
                _ => Point3.Zero
            };
            return ((A(point + v) - A(point)) / val);
        }

        internal void ClearPlanets()
        {
            Planets.Clear();
            PlanetNamesLastChanged = DateTime.Now;
        }

        public void MergePlanets()
        {
            var orderedPlanets = Planets.OrderByDescending(i => i.Mass).ToList();
            for (var i = 0; i < orderedPlanets.Count; i++)
            {
                var mergeTo = orderedPlanets[i];
                for (var j = i + 1; j < orderedPlanets.Count; j++)
                {
                    var mergeFrom = orderedPlanets[j];
                    var n = mergeTo.Position - mergeFrom.Position;
                    var norm = n.LengthSquared();
                    if (norm < (mergeTo.Radius + mergeFrom.Radius) * (mergeTo.Radius + mergeFrom.Radius))
                    {
                        Planets.Remove(mergeFrom);
                        mergeTo.MegeWith(mergeFrom);
                        PlanetNamesLastChanged = DateTime.Now;
                    }
                }
                var ft = new ILGPU.Util.Double3(0, 0, 0);
                var it = ft* 2f;
            }
        }
        public real_t TotalMass()
            => Planets.Sum(i => i.Mass);

        public Point3 MassCenter()
        {
            var totalMass = TotalMass();
            var a = Point3.Zero; // CreateVector.Dense<real_t>(new[] { (real_t)0, (real_t)0, (real_t)0 });
            foreach (var planet in Planets)
            {
                a += planet.Position * (planet.Mass / totalMass);
            }
            return a;
        }
        public Point3 TotalMomentum()
        {
            var a = Point3.Zero;
            foreach (var planet in Planets)
            {
                a += planet.Momentum;
            }
            return a;
        }
        public bool HasPlanet(string planetName) =>
            Planets.Any(i => i.Name == planetName);
        public bool HasPlanet(Planet planet) =>
            Planets.Any(i => i == planet);
        public override string ToString()
        {
            var a = new StringBuilder();
            foreach (var planet in Planets)
            {
                a.Append(planet.ToString());
            }
            return a.ToString();
        }
        public void AddPlanets(params Planet[] planets)
        {
            if (planets is null || planets.Length == 0)
                return;
            var names = Planets.Select(j => j.Name).ToHashSet();
            var planetsToAdd = planets.Select(i => { i.Name += names.Contains(i.Name) ? DateTime.Now.Ticks.ToString() : "";return i; }).ToList();
            if (planetsToAdd.Count == 0)
                return;
            Planets.AddRange(planetsToAdd);
            PlanetNamesLastChanged = DateTime.Now;
        }
        public void RemovePlanets(params Planet[] planets)
        {
            if (planets is null | planets.Length == 0)
            {
                return;
            }
            Planets.RemoveAll(i => planets.Select(j => j.Name).Contains(i.Name));
            PlanetNamesLastChanged = DateTime.Now;
        }
        public void ResetTime()
        {
            this.NStep = 0;
        }
    }
}

