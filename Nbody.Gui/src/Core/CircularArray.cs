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
}

