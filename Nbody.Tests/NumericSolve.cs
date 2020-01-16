using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBody.Gui.Extensions;

namespace NBody.Tests
{
    [TestClass]
    public class NumericSolve
    {
        [TestMethod]
        public void TestEasy()
        {
            Func<double, double> func = (x) => x * x;
            var value = func.NumericSolver(10, 1e-10);
            Assert.IsTrue(Math.Abs(func(value)) <= 1e-10);
        }
        [TestMethod]
        public void TestEasy2()
        {
            Func<double, double> func = (x) => x * x;
            var value = func.NumericSolver(-10, 1e-10);
            Assert.IsTrue(Math.Abs(func(value)) <= 1e-10);
        }
    }
}
