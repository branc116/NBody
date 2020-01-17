using NBody.Gui.Core;
using NBody.Core;
using NBody.Gui.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBody.Gui.Extensions;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace NBody.Gui.Controllers
{
    [PlanetCreator]
    public class BodyCreatorController
    {
        public void BasicPlanet(Point3 position = default, Point3 velocity = default, real_t mass = 1, real_t radius = 1, string name = "Planet")
        {
            var planet = new Planet
            {
                Mass = mass,
                Name = name,
                Position = position,
                Velocity = velocity,
                Radius = radius
            };
            SourceOfTruth.System.AddPlanets(planet);
        }
        public void RandomPlanets(int n, real_t averagePosition = 0, real_t averageVelocity = 2, real_t averageMass = 10, real_t averageRadius = 1, real_t sigma = (real_t)19.1, string name = "Planet")
        {
            var random = new Random();
            real_t rand() =>
                ((real_t)random.NextDouble() - (real_t)0.5) * sigma;
            Point3 randP(real_t c) =>
                new Point3(c + rand(), c + rand(), c + rand());
            var planets = Enumerable.Range(0, n).Select(i => new Planet
            {
                Mass = averageMass + rand() / 10,
                Name = $"{name}#{i}",
                Position = randP(averagePosition),
                Velocity = randP(averageVelocity),
                Radius = averageRadius
            }).ToArray();
            SourceOfTruth.System.AddPlanets(planets);
        }
        public void CircularOrbit(Planet planet, real_t distance = 10, real_t angle = 0, real_t mass = 1, real_t radius = 1, string planetName = "{0} satelite")
        {
            var position = planet.Position + new Point3(distance, 0, 0);
            var absoluteSpeed = planet.GetCircularOrbitSpeed(mass, distance);
            var sin = MathReal.Sin(angle);
            var velocity = planet.Velocity + (new Point3(0, MathReal.Cos(angle), sin).Normalized() * absoluteSpeed);
            var newPlanet = new Planet
            {
                Mass = mass,
                Name = planetName.Replace("{0}", planet.Name),
                Position = position,
                Radius = radius,
                Velocity = velocity
            };
            SourceOfTruth.System.AddPlanets(newPlanet);
        }
        public void PutObjectInLagrangePoint(Planet planet, Planet planet1, int lagrangePoint = 1, Point3 pidCoeficient = default, real_t DV = 10, real_t offset = 0, real_t mass = 1, real_t radius = 1, string planetName = "{0}, {1} L{2}")
        {
            (Planet larger, Planet smaller) = planet.Mass > planet1.Mass ? (planet, planet1) : (planet1, planet);
            var newPlanet = new PlanetInLagrangePoint(smaller, larger, lagrangePoint, radius, offset, mass, planetName, pidCoeficient, DV);
            SourceOfTruth.System.AddPlanets(newPlanet);
        }
        public void StarSystem(Point3 centerLocation, Point3 velocity, real_t discSize = 10, real_t starSystemMass = 1000, int bodysInStarSystem = 10, string name = "System")
        {
            var random = new Random();
            var startMass = starSystemMass * (real_t)0.99;
            var massRemaining = starSystemMass - startMass;
            var star = new Planet
            {
                Mass = startMass,
                Name = $"{name} start",
                Position = centerLocation,
                Radius = 5,
                Velocity = velocity
            };
            SourceOfTruth.System.AddPlanets(star);
            for (var i = 1; i < bodysInStarSystem; i++)
            {
                var distance = discSize * i / bodysInStarSystem;
                var phaseShift = (real_t)(random.NextDouble() * MathReal.Pi * 2);
                
                var planetLocation = Point3.Right.RotateAround(Point3.Up, phaseShift) * distance + centerLocation;
                var mass = massRemaining / bodysInStarSystem; // * random.NextDouble();
                var r = (real_t)1;
                var speed = star.GetCircularOrbitSpeed(mass, distance);
                var planetVelocity = Point3.Forward.RotateAround(Point3.Up, phaseShift) * speed + velocity;
                var planet = new Planet
                {
                    Mass = mass,
                    Name = $"{name} Planet#{i}",
                    Position = planetLocation,
                    Radius = r,
                    Velocity = planetVelocity
                };
                SourceOfTruth.System.AddPlanets(planet);
                //massRemaining -= mass;
            }
        }
        public void Delete(Planet planet) =>
            SourceOfTruth.System.RemovePlanets(planet);
        public void Delete(Planet planet, Planet planet1) =>
            SourceOfTruth.System.RemovePlanets(planet, planet1);
        public void Delete(Planet planet, Planet planet1, Planet planet2) =>
            SourceOfTruth.System.RemovePlanets(planet, planet1, planet2);
        public void Delete(Planet planet, Planet planet1, Planet planet2, Planet planet3) =>
            SourceOfTruth.System.RemovePlanets(planet, planet1, planet2, planet3);
        public void Clear() =>
            SourceOfTruth.System.ClearPlanets();
    }
}
