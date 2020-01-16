using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody.Gui.Extensions
{
    public static class RealExtensions
    {
        public static float SQ(this float r)
        {
            return r * r;
        }
        public static double SQ(this double r)
        {
            return r * r;
        }
    }
}
