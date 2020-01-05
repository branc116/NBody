using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
namespace Nbody.Core
{

    public class Planet
    {
        public Vector<double> Position { get; set; }
        public Vector<double> Velocity { get; set; }
        public double Mass { get; set; }
        public double Radius { get; set; }
        public string Name { get; set; }
        public void MegeWith(Planet planet) {
            Mass += planet.Mass;
            Name += $" + {planet.Name}";
            Velocity = planet.Velocity.Add(planet.Velocity.Multiply(planet.Mass/(planet.Mass+Mass)));
        }
        public override string ToString()
        {
            return $"{Name}:\n   Position: {Position.ToString()}\n   Velocity: {Velocity}\n";
        }
    }
    public class PlanetSystem
    {
        public List<Planet> Planets { get; set; }
        public double GravitationalConstant { get; set; }
        public int NStep { get; private set; } = 0;
        public double CurTime { get => NStep * DeltaTimePerStep; }
        public double DeltaTimePerStep { get; set; } = 0.001;
        public PlanetSystem Step()
        {
            var mergeNeedeed = false;
            Vector<double> normal = CreateVector.Dense(new [] {0.0, 0.0, 0.0});
            Vector<double> directedAcceleration = CreateVector.Dense(new [] {0.0, 0.0, 0.0});
            foreach (var planet in Planets)
            {
                var newPosition = planet.Position;
                var newVelocity = planet.Velocity;

                foreach (var interacts in Planets.Where(i => i.Name != planet.Name))
                {
                    planet.Position.Subtract(interacts.Position, normal);
                    var norm = normal.Norm(2.0);
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
            if (mergeNeedeed)
                MergePlanets();
            NStep++;
            return this;
        }
        public void MergePlanets() {
            var orderedPlanets = Planets.OrderByDescending(i => i.Mass).ToList();
            Vector<double> normal = CreateVector.Dense(new [] {0.0, 0.0, 0.0});
            System.Console.ReadKey();
            for(var i = 0;i<orderedPlanets.Count;i++) {
                var mergeTo = orderedPlanets[i];
                for(var j = i+1;j<orderedPlanets.Count;j++) {
                    var mergeFrom = orderedPlanets[j];
                    mergeTo.Position.Subtract(mergeFrom.Position, normal);
                    var norm = normal.Norm(2.0);
                    if (norm < (mergeTo.Radius + mergeFrom.Radius)) {
                        Planets.Remove(mergeFrom);
                        mergeTo.MegeWith(mergeFrom);
                    }
                }
            }
        }
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

