using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nbody.Gui.Core;
using Nbody.Gui.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Tests
{
    [TestClass]
    public class TestL2Solver
    {
        [TestMethod]
        public void EarthMoon()
        {
            var mu = 1 / 82.2800178;
            var x0 = new Point3(1.1605, 0.0, 0.00776);
            var v0 = new Point3(0, 0.000420, 0);
            var solution = new SolveL2InitialConditions
            {
                Dt = 0.0001,
                Mu = mu,
                OrbitTime = 0.47570000000003576 * 2,
                Treshold = 1e-6,
                Position0 = x0,
                Velocity0 = v0
            };
            var finalSoulbtion = solution.Iterate();
        }
    }
}
