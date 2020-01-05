using System;
using MathNet.Numerics.LinearAlgebra;
using NBody.Gui.Extensions;
namespace NBody.Core
{
    public class Planet
    {
        public Vector<double> Position { get; set; }
        public Vector<double> Velocity { get; set; }
        public double Mass { get; set; }
        public double Radius { get; set; }
        public string Name { get; set; }
        public Vector<double> Momentum =>
            Velocity.Multiply(Mass);
        public double KineticEnergy =>
            Mass * Math.Pow(Velocity.L2Norm(), 2) / 2;
        public void MegeWith(Planet planet)
        {
            
            Name += $" + {planet.Name}";
            Console.WriteLine(Name);
            var v1 = Velocity;
            var v2 = planet.Velocity;
            var mom1 = v1.Multiply(Mass);
            var mom2 = v2.Multiply(planet.Mass);
            Console.WriteLine($"v1: {v1.ToStrV3()}, v2: {v2.ToStrV3()}");
            Console.WriteLine($"m1: {mom1.ToStrV3()}, m2: {mom2.ToStrV3()}");
            Console.WriteLine($"M1: {Mass}, M2: {planet.Mass}");
            Velocity = mom1.Add(mom2).Multiply(1.0/(planet.Mass + Mass)); //Velocity.Multiply(Mass/(planet.Mass + Mass)).Add(planet.Velocity.Multiply(planet.Mass / (planet.Mass + Mass)));
            Mass += planet.Mass;
        }
        public override string ToString()
        {
            return $"{Name}:\n   Position: {Position.ToStrV3()}\n   Velocity: {Velocity.ToStrV3()}\n   Momentum: {Momentum.ToStrV3()}";
        }
    }
}

