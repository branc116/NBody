﻿using MathNet.Numerics.LinearAlgebra;
using Nbody.Gui.Core;
using NBody.Gui;
using NBody.Gui.InputModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace NBody.Core
{
    public class PlanetSystem
    {
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        private readonly NBody.Gui.Kernels.NbodyClKernel _nbodyClKernel = SourceOfTruth.Kernel;
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
        public bool SimulateColitions { get => _simulationModel.SimulateColitions; set => _simulationModel.SimulateColitions = value; }
        public real_t GravitationalConstant { get => _simulationModel.GravitationalConstant; set => _simulationModel.GravitationalConstant = value; }
        public real_t Dt { get => _simulationModel.DeltaTimePerStep; set => _simulationModel.DeltaTimePerStep = value; }

        public PlanetSystem StepCL(int n = 1)
        {
            _nbodyClKernel.Step(this, n);
            NStep += n;
            CurTime += n * Dt;
            return this;
        }
        public PlanetSystem Step()
        {
            var mergeNeedeed = false;
            var positions = Positions;
            var dtps = _simulationModel.DeltaTimePerStep;
            var G = _simulationModel.GravitationalConstant;

            for (var i1 = 0; i1 < Planets.Count; i1++)
            {
                var planet = Planets[i1];
                var newPosition = planet.Position;
                var newVelocity = planet.Velocity;

                for (var j = 0; j < positions.Count; j++)
                {
                    if (i1 == j)
                        continue;
                    var interacts = Planets[j];
                    var position = positions[j];
                    var n = planet.Position - position;

                    var norm = (real_t)n.LengthSquared();
                    var absoluteAcceleration = G * interacts.Mass / (norm);
                    newVelocity += n.Normalized() * (-absoluteAcceleration * dtps);
                    var dist = interacts.Radius + planet.Radius;
                    if (norm < dist * dist)
                        mergeNeedeed = true;
                }
                newPosition += newVelocity * dtps;
                planet.Position = newPosition;
                planet.Velocity = newVelocity;
            }
            if (_simulationModel.SimulateColitions && mergeNeedeed)
                MergePlanets();
            NStep++;
            CurTime += dtps;
            return this;
        }
        public void MergePlanets()
        {
            var orderedPlanets = Planets.OrderByDescending(i => i.Mass).ToList();
            var normal = CreateVector.Dense(new[] { (real_t)0, (real_t)0, (real_t)0 });
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
                    }
                }
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
    }
}

