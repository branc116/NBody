using Nbody.Gui.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nbody.Core
{
    public class CircularArray<T> : IEnumerable<T>
    {
        private T[] _coll;
        private int _position;
        private int _positionRead;
        private int _max;
        private int _rememberEvery;
        private int _lenght;
        private int _mod;
        public int Position => _position;
        public int Length => _lenght;
        public int Count => Math.Min(_lenght, _max);
        public CircularArray(SimpleObservable<int> N, SimpleObservable<int> rememberEvery)
        {
            _coll = new T[N.Get];
            _position = _positionRead = _lenght = 0;
            _max = N.Get == 0 ? N.Set(1) : N.Get;
            _rememberEvery = rememberEvery.Get;
            N.RegisterAftersetting(i =>
            {
                var arr = new T[i];
                var span = arr.AsSpan();
                _coll.AsSpan(0, Math.Min(arr.Length, _coll.Length)).CopyTo(span);
                _position %= i;
                _positionRead %= i;
                _max = i;
                _coll = arr;
            });
            rememberEvery.RegisterAftersetting(AlterRememberEvery);
        }
        ~CircularArray()
        {
            _coll = null;
        }
        private void AlterRememberEvery(int i)
        {
            _rememberEvery = i;
        }
        public void Add(T item)
        {
            _coll[_position] = item;

            if (_mod % _rememberEvery == 0)
            {
                _position++;
                _position %= _max;
                _lenght++;
                _mod = 0;
                _positionRead = _position == 0 ? _max - 1 : _position - 1;
            }
            else
            {
                _positionRead = _position;
            }
            _mod++;
        }
        public void RemoveLast()
        {
            if (_lenght == 0)
                return;
            _lenght = Math.Min(_lenght, _max) - 1;
            _mod = 0;
            _position = _position == 0 ? _max - 1 : _position - 1;
            _positionRead = _position == 0 ? _max - 1 : _position - 1;
        }
        public T Last()
        {
            return _coll[_positionRead];
        }
        public T Last(int i)
        {
            if (_positionRead - i < 0)
                return _coll[_positionRead - i + _max];
            return _coll[_positionRead - i];
        }
        public IEnumerable<T> Reverse()
        {
            var i = 0;
            var n = Math.Min(_max, _lenght);
            for (; i < n; i++)
                yield return Last(i);
        }
        public void Clear()
        {
            _position = 0;
            _lenght = 0;
            _mod = 0;
        }
        public IEnumerator<T> GetEnumerator()
        {
            var len = Math.Min(_lenght, _max);
            var cur = _positionRead - len + 1;
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

