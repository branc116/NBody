using MathNet.Numerics.LinearAlgebra;
using NBody.Gui;
using NBody.Gui.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NBody.Core
{
    public class PlanetSystem
    {
        public List<Planet> Planets { get; set; }
        private int _lastStep = -1;
        private List<Vector<double>> _oldPositions;
        public List<Vector<double>> Positions
        {
            get
            {
                if (_lastStep == NStep)
                    return _oldPositions;
                _lastStep = NStep;
                return _oldPositions = Planets.Select(i => i.Position.Multiply(1)).ToList();
            }
        }
        [PropEdit]
        public double GravitationalConstant;
        public int NStep { get; private set; } = 0;

        public void Step(int numberOfSteps)
        {
            while (numberOfSteps-- > 0)
            {
                Step();
            }
        }

        public double CurTime { get; private set; } = 0.0;
        [PropEdit]
        public double DeltaTimePerStep = 0.001;
        [PropEdit]
        public bool SimulateColitions = true;
        public PlanetSystem Step()
        {
            var mergeNeedeed = false;
            Vector<double> normal = CreateVector.Dense(new[] { 0.0, 0.0, 0.0 });
            Vector<double> directedAcceleration = CreateVector.Dense(new[] { 0.0, 0.0, 0.0 });
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
                    var norm = normal.L2Norm();
                    var absoluteAcceleration = this.GravitationalConstant * interacts.Mass / Math.Pow(norm, 2.0);
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
            Vector<double> normal = CreateVector.Dense(new[] { 0.0, 0.0, 0.0 });
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
        public double TotalMass()
            => Planets.Sum(i => i.Mass);

        public Vector<double> MassCenter()
        {
            var totalMass = TotalMass();
            var a = CreateVector.Dense<double>(new[] { 0.0, 0.0, 0.0 });
            foreach (var planet in Planets)
            {
                a.Add(planet.Position.Multiply(planet.Mass / totalMass), a);
            }
            return a;
        }
        public Vector<double> TotalMomentum()
        {
            var a = CreateVector.Dense<double>(new[] { 0.0, 0.0, 0.0 });
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

