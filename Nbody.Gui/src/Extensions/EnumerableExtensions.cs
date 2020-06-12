using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Nbody.Gui.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Log<T>(this IEnumerable<T> src, bool block = false)
        {
            Console.WriteLine($"#{src.Count()} ");
            foreach (var obj in src)
            {
                Console.WriteLine(obj.ToString());
                yield return obj;
            }
            if (block)
                Console.ReadKey();
        }
        public static IEnumerable<(T, T, double, double)> GetWaitedPairs<T>(this IEnumerable<(double, T)> src, int n)
        {
            var list = src.ToList();
            var sum = list.Sum(i => i.Item1);
            var rand = new Random();
            while(--n != 0)
            {
                var randSort = list.OrderBy(i => rand.Next()).ToList();
                var t1 = rand.NextDouble();
                T _1 = default, _2 = default;
                double _1c = default, _2c = default;
                for(int i = 0;i<list.Count;i++)
                {
                    t1 -= randSort[i].Item1 / sum;
                    if (t1 < 0)
                    {
                        _1 = randSort[i].Item2;
                        _1c = randSort[i].Item1;
                        break;
                    }
                }
                var t2 = rand.NextDouble();
                for (int j = 0; j < list.Count; j++)
                {
                    t2 -= randSort[j].Item1 / sum;
                    if (t2 < 0)
                    {
                        _2 = randSort[j].Item2;
                        _2c = randSort[j].Item1;
                        break;
                    }
                }
                if (_1 is null)
                {
                    Console.WriteLine("_1 is null");
                    _1 = list[0].Item2; 
                }
                if (_2 is null)
                {
                    Console.WriteLine("_2 is null");
                    _2 = list[0].Item2;
                }
                yield return (_1, _2, _1c, _2c);
            }
        }
        public static IEnumerable<(T, G)> GetPairs<T, G>(this IEnumerable<T> src, IEnumerable<G> p)
        {
            var frst = src.GetEnumerator();
            var snd = p.GetEnumerator();
            while(frst.MoveNext() && snd.MoveNext())
            {
                yield return (frst.Current, snd.Current);
            }
        }
        public static IEnumerable<(T, T)> GetNeigbros<T>(this IEnumerable<T> src)
        {
            var en = src.GetEnumerator();
            if (!en.MoveNext())
                yield return (default, default);

            var old = en.Current;
            while(en.MoveNext())
            {
                yield return (old, en.Current);
                old = en.Current;
            }
            yield return (old, default);
        }
        public static double Std(this IEnumerable<double> src)
        {
            var mean = src.Average();
            var std = src.Sum(i => (i + mean) * (i + mean));
            return std;
        }
    }
}
