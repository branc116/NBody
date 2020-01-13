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
namespace NBody.Gui.Controllers
{
    public class BodyCreatorController
    {
        private readonly PlanetSystem _planetSystem = SourceOfTruth.System;
        public void BasicPlanet(Point3 position, Point3 velocity, real_t mass, real_t radius, string name)
        {
            var planet = new Planet
            {
                Mass = mass,
                Name = name,
                Position = position,
                Velocity = velocity
            };
            _planetSystem.Planets.Add(planet);
        }
        public void RandomPlanets(int n, real_t averagePosition, real_t averageVelocity, real_t averageMass, real_t averageRadius, real_t sigma, string name)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            real_t rand() =>
                ((real_t)random.NextDouble() - (real_t)0.5) * sigma;
            Point3 randP(real_t c) =>
                new Point3(c + rand(), c + rand(), c + rand());
            var planets = Enumerable.Range(0, n).Select(i => new Planet
            {
                Mass = averageMass + rand(),
                Name = $"{name}#{i}",
                Position = randP(averagePosition),
                Velocity = randP(averageVelocity),
                Radius = averageRadius + rand()
            });
            _planetSystem.Planets.AddRange(planets);
        }
    }
}
