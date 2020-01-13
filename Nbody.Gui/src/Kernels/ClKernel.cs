using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.OpenCL;
using Nbody.Gui.Core;
using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace NBody.Gui.Kernels
{
    public readonly struct NbodyKernelModel
    {
        public readonly real_t G;
        public readonly real_t dt;
        public readonly int N;
        public NbodyKernelModel(real_t G, real_t dt, int n)
        {
            this.G = G;
            this.dt = dt;
            N = n;
        }
    }
    public readonly struct PlanetKernelModel
    {
        public readonly Point3 Position;
        public readonly Point3 Velocity;
        public readonly real_t Mass;
        public readonly real_t Radius;
        public readonly int WillMerge;

        public PlanetKernelModel(Point3 position, Point3 velocity, real_t mass, real_t radius, int willMerge)
        {
            Position = position;
            Velocity = velocity;
            Mass = mass;
            WillMerge = willMerge;
            Radius = radius;
        }
    }
    public sealed class NbodyClKernel : IDisposable
    {
        private readonly Context _context;
        private readonly Accelerator _accelerator;
        private readonly Action<Index, ArrayView<PlanetKernelModel>, NbodyKernelModel, ArrayView<PlanetKernelModel>> _kernel;

        public static void NbodyKernel(Index i, ArrayView<PlanetKernelModel> planets, NbodyKernelModel model, ArrayView<PlanetKernelModel> outPlanet)
        {
            
            var planet = planets[i];
            var newPosition = planet.Position;
            var newVelocity = planet.Velocity;
            int willMerge = -1;
            for (var j = 0; j < model.N; j++)
            {
                if (i == j)
                    continue;
                var interacts = planets[j];
                var position = interacts.Position;
                var n = planet.Position - position;

                var norm = (real_t)n.LengthSquared();
                var absoluteAcceleration = model.G * interacts.Mass / (norm);
                var aTdT = -1 * absoluteAcceleration * model.dt;
                newVelocity = newVelocity + n.NormalizedGPU() * aTdT;
                var dist = interacts.Radius + planet.Radius;
                if (norm < dist * dist)
                    willMerge = j;
            }
            newPosition += newVelocity * model.dt;
            outPlanet[i] = new PlanetKernelModel(newPosition, newVelocity, planet.Mass, planet.Radius, willMerge);
        }
        
        public void Step(PlanetSystem planetSystem, int n = 1)
        {
            var model = new NbodyKernelModel(planetSystem.GravitationalConstant, planetSystem.Dt, planetSystem.Planets.Count);
            var planets = planetSystem.Planets.Select(i => new PlanetKernelModel(i.Position, i.Velocity, i.Mass, i.Radius, -1)).ToArray();
            using (var dataSource = _accelerator.Allocate<PlanetKernelModel>(model.N))
            //using (var dataSource2 = _accelerator.Allocate<NbodyKernelModel>(1))
            using (var dataTarget = _accelerator.Allocate<PlanetKernelModel>(model.N))
            {
                dataSource.CopyFrom(planets, 0, 0, model.N);
                dataTarget.MemSetToZero();
                do
                {
                    _kernel(model.N, dataSource.ToArrayView(), model, dataTarget.ToArrayView());
                    _accelerator.Synchronize();
                    if (n > 0)
                    {
                        dataSource.CopyFrom(dataTarget, 0, 0, model.N);
                        dataTarget.MemSetToZero();
                    }
                } while (n-- >= 0);
                var outPlanets = dataTarget.GetAsArray();
                for (int i = 0; i < planetSystem.Planets.Count; i++)
                {
                    var planet = planetSystem.Planets[i];
                    planet.Position = outPlanets[i].Position;
                    planet.Velocity = outPlanets[i].Velocity;
                }
                return;
            }

        }

        public void Dispose()
        {
            _context.Dispose();
            _accelerator.Dispose();

        }

        public NbodyClKernel()
        {
            try
            {
                var context = new Context();
                _context = context;
                _accelerator = CLAccelerator.Create(_context, CLAccelerator.CLAccelerators[0]);
                _kernel = _accelerator.LoadStreamKernel<Index, ArrayView<PlanetKernelModel>, NbodyKernelModel, ArrayView<PlanetKernelModel>>(NbodyKernel);
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
