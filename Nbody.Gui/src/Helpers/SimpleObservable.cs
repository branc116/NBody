using Nbody.Gui.src.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nbody.Gui.Helpers
{
    [JsonConverter(typeof(BasicObserverSerializer))]
    public class SimpleObservable<T>
    {
        private T _value;
        private readonly List<Action<T, T>> _actionsPre;
        private readonly List<Action<T>> _actionsPost;
        private readonly MethodInfo _parseMethod = typeof(T).GetMethod("Parse", new[] { typeof(string) });
        public SimpleObservable()
        {
            _actionsPre = new List<Action<T, T>>();
            _actionsPost = new List<Action<T>>();
        }
        public SimpleObservable(T value) : this()
        {
            _value = value;
        }
        public T Get => _value;
        public T Set(T value)
        {
            if (value.Equals(_value))
                return _value;
            foreach (var action in _actionsPre)
            {
                try
                {
                    action(_value, value);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine($"One of registered functions faild on observable {this}, {_value}");
                    Console.WriteLine(Ex);
                }
            }
            _value = value;
            foreach (var action in _actionsPost)
            {
                try
                {
                    action(_value);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine($"One of registered functions faild on observable {this}, {_value}");
                    Console.WriteLine(Ex);
                }
            }
            return _value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">Firsts param is old value, second parameter is new value</param>
        public void RegisterPresetting(Action<T, T> action)
        {
            _actionsPre.Add(action);
        }
        public void RegisterAftersetting(Action<T> action)
        {
            _actionsPost.Add(action);
        }
        public void UnrgisterPre(Action<T, T> action)
        {
            _actionsPre.Remove(action);
        }
        public void UnrgisterPost(Action<T> action)
        {
            _actionsPost.Remove(action);
        }
        public static implicit operator SimpleObservable<T>(T value)
        {
            return new SimpleObservable<T>(value);
        }
        public static implicit operator T(SimpleObservable<T> observable)
        {
            return observable._value;
        }
        public T Parse(string str)
        {
            try
            {
                var val = _parseMethod.Invoke(null, new[] { str });
                if (val is T ok)
                {
                    return ok;
                }else
                {
                    throw new Exception("Parse method does not return T");
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
            }
        }
    }
}
