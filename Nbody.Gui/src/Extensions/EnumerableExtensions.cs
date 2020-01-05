using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody.Gui.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Log<T>(this IEnumerable<T> src)
        {
            Console.WriteLine($"#{src.Count()} ");
            foreach(var obj in src)
            {
                Console.WriteLine(obj.ToString());
                yield return obj;
            }
        }
    }
}
