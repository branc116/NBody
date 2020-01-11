using MathNet.Numerics.LinearAlgebra;
using Nbody.Gui.src.Extensions;
using NBody.Gui;
using NBody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace NBody.Core
{
    public class Planet
    {
        private CircularArray<Vector<real_t>> _position = new CircularArray<Vector<real_t>>(SourceOfTruth.MaxHistoy);
        private CircularArray<Vector<real_t>> _velocity = new CircularArray<Vector<real_t>>(SourceOfTruth.MaxHistoy);

        public Vector<real_t> Position { get => _position.Last(); set => _position.Add(value.Clone()); }
        public Vector<real_t> Velocity { get => _velocity.Last(); set => _velocity.Add(value.Clone()); }
        public int Steps => _position.Position;
        public IEnumerable<Vector<real_t>> PositionHistory => _position.Select(i => i);
        public IEnumerable<Vector<real_t>> VelocityHistory => _velocity.Select(i => i);
        public real_t Mass { get; set; }
        public real_t Radius { get; set; }
        public string Name { get; set; }
        public Vector<real_t> Momentum =>
            Velocity.Multiply(Mass);
        public double KineticEnergy =>
            Mass * Math.Pow(Velocity.L2Norm(), 2) / 2;
        public void MegeWith(Planet planet)
        {
            Name += $" + {planet.Name}";
            var v1 = Velocity;
            var v2 = planet.Velocity;
            var mom1 = v1.Multiply(Mass);
            var mom2 = v2.Multiply(planet.Mass);
            Velocity = mom1.Add(mom2).Multiply((real_t)1.0 / (planet.Mass + Mass));
            Mass += planet.Mass;
        }
        /// <summary>
        /// L1 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="planet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Vector<real_t> L1(Planet planet)
        {
            if (Mass > planet.Mass)
                return planet.L1(this);
            var diff =  Position - planet.Position;
            var R = diff.L2Norm();
            double M1 = planet.Mass, M2 = Mass, rinit = R / (Math.Pow(M2 / (3 * M1), 1 / 3.0));
            Func<double, double> func = (distance) => M2 / (distance * distance) + M1 / (R * R) - distance * (M1 + M2) / (R * R * R) - M1 / Math.Pow(R - distance, 2);
            var r = func.NumericSolver(rinit);
            var point = diff.Normalize(2).Multiply((real_t)r).Add(Position);
            return point;
        }
        public override string ToString()
        {
            return $"{Name}:\n   Position: {Position.ToStrV3()}\n   Velocity: {Velocity.ToStrV3()}\n   Momentum: {Momentum.ToStrV3()}";
        }
    }
}

