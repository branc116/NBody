using Godot;
using MathNet.Numerics.LinearAlgebra;
using NBody.Gui;
using NBody.Gui.Attributes;
using System;
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
        public List<Planet> Planets { get; set; }
        private int _lastStep = -1;
        private List<Vector<real_t>> _oldPositions;
        public List<Vector<real_t>> Positions
        {
            get
            {
                if (_lastStep == NStep)
                    return _oldPositions;
                _lastStep = NStep;
                return _oldPositions = Planets.Select(i => i.Position.Clone()).ToList();
            }
        }
        [PropEdit]
        public real_t GravitationalConstant;
        public int NStep { get; private set; } = 0;

        public void Step(int numberOfSteps)
        {
            while (numberOfSteps-- > 0)
            {
                Step();
            }
        }

        public real_t CurTime { get; private set; } = (real_t)0;
        [PropEdit]
        public real_t DeltaTimePerStep = (real_t)0.001;
        [PropEdit]
        public bool SimulateColitions = true;
        public PlanetSystem Step()
        {
            var mergeNeedeed = false;
            var normal = CreateVector.Dense(new[] { (real_t)0, (real_t)0, (real_t)0 });
            var directedAcceleration = CreateVector.Dense(new[] { (real_t)0, (real_t)0, (real_t)0 });
            var positions = Positions;
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

                    planet.Position.Subtract(position, normal);
                    var norm = (real_t)normal.L2Norm();
                    var absoluteAcceleration = this.GravitationalConstant * interacts.Mass / (norm*norm);
                    normal.Normalize(2).Multiply(-absoluteAcceleration, directedAcceleration);
                    newVelocity.Add(directedAcceleration.Multiply(DeltaTimePerStep), newVelocity);
                    if (norm < (interacts.Radius + planet.Radius))
                        mergeNeedeed = true;
                }
                newPosition = newPosition.Add(newVelocity.Multiply(DeltaTimePerStep));
                planet.Position = newPosition;
                planet.Velocity = newVelocity;
            }
            if (SimulateColitions && mergeNeedeed)
                MergePlanets();
            NStep++;
            CurTime += DeltaTimePerStep;
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
                    mergeTo.Position.Subtract(mergeFrom.Position, normal);
                    var norm = normal.Norm(2.0);
                    if (norm < (mergeTo.Radius + mergeFrom.Radius))
                    {
                        Planets.Remove(mergeFrom);
                        mergeTo.MegeWith(mergeFrom);
                    }
                }
            }
        }
        public real_t TotalMass()
            => Planets.Sum(i => i.Mass);

        public Vector<real_t> MassCenter()
        {
            var totalMass = TotalMass();
            var a = CreateVector.Dense<real_t>(new[] { (real_t)0, (real_t)0, (real_t)0 });
            foreach (var planet in Planets)
            {
                a.Add(planet.Position.Multiply(planet.Mass / totalMass), a);
            }
            return a;
        }
        public Vector<real_t> TotalMomentum()
        {
            var a = CreateVector.Dense<real_t>(new[] { (real_t)0, (real_t)0, (real_t)0 });
            foreach (var planet in Planets)
            {
                a.Add(planet.Momentum, a);
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

