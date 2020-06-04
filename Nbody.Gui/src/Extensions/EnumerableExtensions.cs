using System;
using System.Collections.Generic;
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
    }
}
