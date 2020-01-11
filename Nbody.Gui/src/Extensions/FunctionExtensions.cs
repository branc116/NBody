using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.src.Extensions
{
    public static class FunctionExtensions
    {
        public static double NumericSolver(this Func<double, double> func, double initValue, double percision = 1e-6, int maxSteps = 1000)
        {
            double r = 0.0;
            do
            {
                r = func(initValue);
                var dt = percision * Math.Log10(Math.Abs(initValue));
                var dr = (r - func(initValue - dt)) / dt;
                initValue -= r / dr;
            } while (maxSteps-- > 0 && Math.Abs(r) > percision);
            return initValue;
        }
    }
}
