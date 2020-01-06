using MathNet.Numerics.LinearAlgebra;
using NBody.Gui;
using NBody.Gui.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

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
            while(len-- > 0)
            {
                if (cur < 0) cur += _max;
                yield return _coll[cur];
                cur++; cur %= _max;
            }
            //for(int i = _position - _lenght; i < _position;i++)
            //{
            //    var index = i % _max;
            //    index = index < 0 ? index + _max : index;
            //    yield return _coll[index];
            //}
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

        public Vector<double> Position { get => _position.Last(); set => _position.Add(value); }
        public Vector<double> Velocity { get => _velocity.Last(); set => _velocity.Add(value); }
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
        public override string ToString()
        {
            return $"{Name}:\n   Position: {Position.ToStrV3()}\n   Velocity: {Velocity.ToStrV3()}\n   Momentum: {Momentum.ToStrV3()}";
        }
    }
}

