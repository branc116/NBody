using MathNet.Numerics.LinearAlgebra;
namespace Nbody.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = new Nbody.PlanetSystem
            {
                GravitationalConstant = 100.0,
                Planets = new System.Collections.Generic.List<Planet>{
                    new Planet {
                        Mass = 333030.0,
                        Position = CreateVector.Dense<double>(new [] {0.0, 0.0, 0.0}),
                        Velocity = CreateVector.Dense<double>(new [] {0.0, 0.0, 0.0}),
                        Name = "Sun",
                        Radius = 10.0
                    },
                    new Planet {
                        Mass = 1.0,
                        Position = CreateVector.Dense<double>(new [] {23481.4000942, 0.0, 0.0}),
                        Velocity = CreateVector.Dense<double>(new [] {0.0, 0.0, 0.0}),
                        Name = "Earth",
                        Radius = 1
                    },
                    new Planet {
                        Mass = 0.0123031469,
                        Position = CreateVector.Dense<double>(new [] {23481.4000942 + 60.3358970334, 0.0, 0.0}),
                        Velocity = CreateVector.Dense<double>(new [] {0.0, 0.0, 0.0}),
                        Name = "Moon",
                        Radius = 0.3
                    }
                }
            };
            while (true)
            {
                if ((system.NStep % 1000) == 0)
                {
                    System.Console.WriteLine($"step: {system.NStep}\ntime: {system.CurTime}\n{system.ToString()}");
                    System.Console.Read();
                }
                system.Step();
            }
        }
    }
}
