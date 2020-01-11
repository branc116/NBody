using MathNet.Numerics.LinearAlgebra;
using Nbody.Gui.src.Extensions;
using NBody.Gui;
using NBody.Gui.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NBody.Core
{
    public class CircularArray<T> : IEnumerable<T>
    {
        private readonly T[] _coll;
        private int _position;
        private readonly int _max;
        private int _lenght;
        public int Position => _position;
        public int Length => _lenght;
        public CircularArray(int N)
        {
            _coll = new T[N];
            _position = 0;
            _lenght = 0;
            _max = N;
        }
        public void Add(T item)
        {
            _coll[_position] = item;
            _lenght++;
            _position++;
            _position %= _max;
        }
        public T Last()
        {
            return _position == 0 ? _coll[_max - 1] : _coll[_position - 1];
        }
        public void Clear()
        {
            _position = 0;
            _lenght = 0;
        }
        public IEnumerator<T> GetEnumerator()
        {
            var len = Math.Min(_lenght, _max);
            var cur = _position - len;
            while (len-- > 0)
            {
                if (cur < 0) cur += _max;
                yield return _coll[cur];
                cur++; cur %= _max;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _coll.GetEnumerator();
        }
    }
    public class Planet
    {
        private CircularArray<Vector<double>> _position = new CircularArray<Vector<double>>(SourceOfTruth.MaxHistoy);
        private CircularArray<Vector<double>> _velocity = new CircularArray<Vector<double>>(SourceOfTruth.MaxHistoy);

        public Vector<double> Position { get => _position.Last(); set => _position.Add(value.Clone()); }
        public Vector<double> Velocity { get => _velocity.Last(); set => _velocity.Add(value.Clone()); }
        public int Steps => _position.Position;
        public IEnumerable<Vector<double>> PositionHistory => _position.Select(i => i);
        public IEnumerable<Vector<double>> VelocityHistory => _velocity.Select(i => i);
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
            var v1 = Velocity;
            var v2 = planet.Velocity;
            var mom1 = v1.Multiply(Mass);
            var mom2 = v2.Multiply(planet.Mass);
            Velocity = mom1.Add(mom2).Multiply(1.0 / (planet.Mass + Mass));
            Mass += planet.Mass;
        }
        /// <summary>
        /// L1 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="planet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Vector<double> L1(Planet planet)
        {
            if (Mass > planet.Mass)
                return planet.L1(this);
            var diff =  Position - planet.Position;
            var R = diff.L2Norm();
            double M1 = planet.Mass, M2 = Mass, rinit = R / Math.Pow(M2 / (3 * M1), 1 / 3.0);
            Func<double, double> func = (distance) => M2 / (distance * distance) + M1 / (R * R) - distance * (M1 + M2) / (R * R * R) - M1 / Math.Pow(R - distance, 2);
            var r = func.NumericSolver(rinit);
            var point = diff.Normalize(2).Multiply(r).Add(Position);
            return point;
        }
        public override string ToString()
        {
            return $"{Name}:\n   Position: {Position.ToStrV3()}\n   Velocity: {Velocity.ToStrV3()}\n   Momentum: {Momentum.ToStrV3()}";
        }
    }
}

