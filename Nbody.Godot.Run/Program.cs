using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NBody.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using NBody.Gui.Core;
using System.Collections.Generic;


#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

#if GODOT_REAL_T_IS_DOUBLE
using godot_real_t = System.Double;
#else
using godot_real_t = System.Single;
#endif

namespace NBody.Gui.Run
{
    class Program
    {
        static readonly string GodotExePath = @"C:\Users\Branimir\Source\Tools\Godot_v3.1.2-stable_mono_win64\Godot_v3.1.2-stable_mono_win64\Godot_v3.1.2-stable_mono_win64.exe";
        static readonly string ProjectFile = @"C:\Users\Branimir\Source\repos\nbody\Nbody.Godot\";
        static readonly string BenchamarkFolder = @"C:\Users\Branimir\Source\repos\nbody\Benchmarks\";
        static void Main(string[] args)
        {
            //var proc = System.Diagnostics.Process.Start(GodotExePath, $"--path {ProjectFile}");
            //proc.WaitForExit();
            using var kernel = new Kernels.NbodyClKernel();
            var outStr = "# of planets, with Collision, without collision, openCl\n";
            for (var i = 2; i < 5000; i *= 2)
            {
                var system1 = GenerateRandomSytem(i);
                var system2 = GenerateRandomSytem(i);
                var system3 = GenerateRandomSytem(i);
                var time1 = TimeSpan.FromTicks(BenchStep_WithColistions(system1)).ToString();
                var time2 = TimeSpan.FromTicks(BenchStep_WithoutCollisions(system2)).ToString();
                var time3 = TimeSpan.FromTicks(BenchStep_OpenCl(system3)).ToString();
                outStr += $"{i}, {time1}, {time2}, {time3}\n";
            }
            if (!Directory.Exists(BenchamarkFolder))
                Directory.CreateDirectory(BenchamarkFolder);
            var outPath = Path.Combine(BenchamarkFolder, $"benchmark.{DateTime.Now.Ticks}.csv");
            File.WriteAllText(outPath, outStr);
        }

        private static long BenchStep_OpenCl(PlanetSystem system3)
        {
            var sw = new Stopwatch();
            var s = 100;
            sw.Start();
            system3.StepCL(s);
            sw.Stop();
            return sw.ElapsedTicks / s;
        }

        static PlanetSystem GenerateRandomSytem(int numberOfPlanets)
        {
            var random = new Random(numberOfPlanets);
            real_t rP()
                => (real_t)random.NextDouble() * 100 - 50;
            var system = new PlanetSystem
            {
                Planets = Enumerable.Range(0, numberOfPlanets).Select(i => new Planet
                {
                    Mass = (real_t)random.NextDouble() * 100,
                    Velocity = new Point3(rP(), rP(), rP()),
                    Position = new Point3(rP(), rP(), rP()),
                    Name = $"Planet #{i}",
                    Radius = ((real_t)random.NextDouble() + (real_t)0.1) * 10
                }).ToList(),
                SimulateColitions = true,
                GravitationalConstant = (real_t)0.1
            };
            return system;
        }
        static long BenchStep_WithColistions(PlanetSystem system)
        {
            system.SimulateColitions = true;
            var sw = new Stopwatch();
            var s = 100;
            sw.Start();
            system.Step(s);
            sw.Stop();
            return sw.ElapsedTicks / s;
        }
        static long BenchStep_WithoutCollisions(PlanetSystem system)
        {
            system.SimulateColitions = false;
            var sw = new Stopwatch();
            var s = 100;
            sw.Start();
            system.Step(s);
            sw.Stop();
            return sw.ElapsedTicks / s;
        }
    }
}
