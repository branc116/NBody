using System;
using System.Collections.Generic;
using System.Linq;

namespace NBody.Gui.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Log<T>(this IEnumerable<T> src)
        {
            Console.WriteLine($"#{src.Count()} ");
            foreach (var obj in src)
            {
                Console.WriteLine(obj.ToString());
                yield return obj;
            }
        }
    }
}
