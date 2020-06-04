using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.src.Extensions
{
    public static class ListExtensions
    {
        public static G Get<T, G>(this List<(T, G)> keyValuePair, T key)
        {
            var retValue = keyValuePair.FirstOrDefault(i => i.Item1.Equals(key));
            return retValue.Item2;
        }
    }
}
