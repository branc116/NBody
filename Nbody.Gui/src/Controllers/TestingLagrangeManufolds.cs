using Nbody.Gui.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Text;
using System.Threading.Tasks;
using Nbody.Core;
using Nbody.Gui.Core;
using MathNet.Numerics.Integration;

namespace Nbody.Gui.Controllers
{
    //[PlotFunction]
    public class TestingLagrangeManufolds
    {
        private readonly PlanetInLagrangePoint l2;
        public TestingLagrangeManufolds()
        {
            var left = new Planet
            {
                Mass = 1000,
                Name = "Sun",
                Position = Point3.Zero,
                Velocity = Point3.Zero
            };
            var right = new Planet
            {
                Mass = 1,
                Name = "Earth",
                Position = Point3.Right * 100,
                Velocity = Point3.Zero
            };

            var l2p = left.L1(right, 0);
            l2 = new PlanetInLagrangePoint(left, right, 2, 1, 0, 0, "l2");
        }

        public void Test()
        {
            var dict = new Dictionary<(Point3, Point3), double>();
            for (var x0=-5f;x0<5f;x0+=1f)
            {
                for (var yDot0 = 0f; yDot0 < 0.12f; yDot0 += 0.01f)
                {
                    var tasks = new List<Task>();
                    for (var z0 = -5f; z0 < 5f; z0 += 1f)
                    {
                        var x = new Point3(x0, 0f, z0);
                        var xDot = new Point3(0f, yDot0, 0f);
                        var task = new Task(() =>
                        {
                            var res = l2.Calc(x, xDot, 0.001f);
                            dict.Add((x, xDot), res);
                        });
                        task.Start();
                        tasks.Add(task);
                    }
                    Task.WaitAll(tasks.ToArray());
                }
            }
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            System.IO.File.WriteAllText(@"C:\_Projects\_temp\data.json", str);
            var count = dict.Count;
        }
        public Vector2[] GetY0Xdot0Zdot0()
        {
            return Array.Empty<Vector2>();
        }
    }
}
