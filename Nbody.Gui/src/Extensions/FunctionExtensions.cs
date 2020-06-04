using Nbody.Gui.Core;
using System;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace Nbody.Gui.Extensions
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
            if (Math.Abs(r) > percision)
            {
                Console.WriteLine($"Numerical solution was not found, using the solution with error of {r}");
                Console.WriteLine(new System.Diagnostics.StackTrace().ToString());
            }
            return initValue;
        }
    }
}
