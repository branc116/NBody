using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nbody.Gui.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Tests
{
    [TestClass]
    public class LagrangeTesting
    {
        [TestMethod]
        public void TestingL2()
        {
            var controller = new TestingLagrangeManufolds();
            controller.Test();
            //Assert.IsNotNull(a);
        }
    }
}
