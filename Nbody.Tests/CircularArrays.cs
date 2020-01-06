using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBody.Core;

namespace Nbody.Tests
{
    [TestClass]
    public class CircularArrays
    {
        [TestMethod]
        public void CurcularArray_NoOverflow()
        {
            var a = new CircularArray<int>(10);
            Enumerable.Range(0, 9).ToList().ForEach(i => a.Add(i));
            Assert.AreEqual(9, a.Position);
            Assert.AreEqual(9, a.Count());
            var cur = 0;
            foreach (var b in a)
            {
                Assert.AreEqual(cur, b);
                cur++;
            }

        }
        [TestMethod]
        public void CurcularArray_Overflow()
        {
            var a = new CircularArray<int>(10);
            Enumerable.Range(0, 11).ToList().ForEach(i => a.Add(i));
            Assert.AreEqual(1, a.Position);
            Assert.AreEqual(11, a.Length);
            Assert.AreEqual(10, a.Count());
            var cur = 1;
            foreach (var b in a)
            {
                Assert.AreEqual(cur, b);
                cur++;
            }

        }
        [TestMethod]
        public void CurcularArray_Last()
        {
            var a = new CircularArray<int>(10);
            Enumerable.Range(0, 10).ToList().ForEach(i => a.Add(i));
            Assert.AreEqual(9, a.Last());
            a.Add(10);
            Assert.AreEqual(10, a.Last());
            for (int i = 0; i < 30; i++)
            {
                a.Add(i);
                Assert.AreEqual(i, a.Last());
            }
        }
        [TestMethod]
        public void CurcularArray_Clear()
        {
            var a = new CircularArray<int>(10);
            Enumerable.Range(0, 10).ToList().ForEach(i => a.Add(i));
            Assert.AreEqual(9, a.Last());
            a.Clear();
            Assert.AreEqual(0, a.Count());
        }
    }
}
