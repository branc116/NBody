using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.OpenCL;
using NBody.Gui.Core;
using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Util;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace NBody.Gui.Kernels
{
    public readonly struct NbodyKernelModel
    {
        public readonly float G;
        public readonly float dt;
        public readonly int N;
        public NbodyKernelModel(float G, float dt, int n)
        {
            this.G = G;
            this.dt = dt;
            N = n;
        }
    }
    public readonly struct PlanetKernelModel
    {
        public readonly Point3Float Position;
        public readonly Point3Float Velocity;
        public readonly float Mass;
        public readonly float Radius;
        public readonly int WillMerge;

        public PlanetKernelModel(Point3Float position, Point3Float velocity, float mass, float radius, int willMerge)
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

        public bool Ok { get; }

        public static void NbodyKernelPnt(Index i, ArrayView<PlanetKernelModel> planets, NbodyKernelModel model, ArrayView<PlanetKernelModel> outPlanet)
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

                var norm = n.LengthSquared();
                var absoluteAcceleration = model.G * interacts.Mass / (norm);
                var aTdT = -1 * absoluteAcceleration * model.dt;
                newVelocity += n.Normalized() * aTdT;
                var dist = interacts.Radius + planet.Radius;
                if (norm < dist * dist)
                    willMerge = j;
            }
            newPosition += (newVelocity * model.dt);
            outPlanet[i] = new PlanetKernelModel(newPosition, newVelocity, planet.Mass, planet.Radius, willMerge);
        }

        public void StepPnt(PlanetSystem planetSystem, int n = 1)
        {
            var model = new NbodyKernelModel((float)planetSystem.GravitationalConstant, (float)planetSystem.Dt, planetSystem.Planets.Count);
            var planets = planetSystem.Planets.Select(i => new PlanetKernelModel(i.Position, i.Velocity, (float)i.Mass, (float)i.Radius, -1)).ToArray();
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
                    if (n > 1)
                    {
                        dataSource.CopyFrom(dataTarget, 0, 0, model.N);
                        dataTarget.MemSetToZero();
                    }
                } while (--n > 0);
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
        public void Step(PlanetSystem planetSystem, int n = 1)
        {
            StepPnt(planetSystem, n);
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
                _kernel = _accelerator.LoadStreamKernel<Index, ArrayView<PlanetKernelModel>, NbodyKernelModel, ArrayView<PlanetKernelModel>>(NbodyKernelPnt);
                Ok = true;
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static NbodyClKernel GetNbodyClKernel()
        {
            var nbody = new NbodyClKernel();
            return nbody.Ok ? nbody : null;
        }
    }
}
