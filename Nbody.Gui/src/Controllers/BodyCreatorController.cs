using Nbody.Gui.Core;
using Nbody.Core;
using Nbody.Gui.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nbody.Gui.Extensions;
using System.Runtime.InteropServices;
using Godot;
using Nbody.Gui.src.Controllers;
using Nbody.Gui.Helpers;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace Nbody.Gui.Controllers
{
    [PlanetCreator]
    public class BodyCreatorController
    {
        private readonly SimpleObservable<PlanetSystem> _system = SourceOfTruth.System;
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
            _system.Get.AddPlanets(planet);
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
            _system.Get.AddPlanets(planets);
        }
        public void EarthAndMoonL2NewTest(real_t min_x0 = 0.01, real_t max_x0 = 0.015,
            real_t min_z0 = 0.05, real_t max_z0 = 0.15,
            real_t min_yDot0 = -0.5, real_t max_yDot0 = 0,
            int n = 5, int steps=6000)
        {
            _system.Get.ClearPlanets();
            _system.Get.ResetTime();
            var (earth, moon) = GetEarthMoon();
            var lpoints = GetL2Spectar(earth, moon, min_x0, max_x0, min_z0, max_z0, min_yDot0, max_yDot0, n).Prepend(moon).Prepend(earth).ToArray();

            _system.Get.AddPlanets(lpoints);
            _system.Get.Step(steps);
        }
        public (Planet earth, Planet moon) GetEarthMoon()
        {
            var earth = new Planet()
            {
                Mass = 81.2800178,
                Position = new Point3(0, 0, 0),
                Name = "Earth",
                Radius = 3.6710602 * 3,
                Velocity = Point3.Zero
            };
            var moon = new Planet
            {
                Mass = 1,
                Position = new Point3(100, 0, 0),
                Name = "Moon",
                Radius = 3,
                Velocity = new Point3(0, 0, earth.GetCircularOrbitSpeed(1, 100))
            };
            earth.Velocity = moon.Velocity * (-moon.Mass / earth.Mass);
            return (earth, moon);
        }
        public void NewPopulationL2(int n = 100, int steps = 1000)
        {
            var pairs = _system.Get.Planets
                .Where(i => i is PlanetInLagrangePoint)
                .Cast<PlanetInLagrangePoint>()
                .Select(i => (cost: 1/(i.DistanceFromLPoint.Aggregate((i, j) => i + j).DistanceSquaredTo(Point3.Zero)/10e6), planet: i))
                .GetWaitedPairs(n);
            var (earth, moon) = GetEarthMoon();
            var arr = GetL2Mergers(moon, earth, pairs).Prepend(moon).Prepend(earth).ToArray();
            _system.Get.ClearPlanets();
            _system.Get.ResetTime();

            _system.Get.AddPlanets(arr);
            _system.Get.Step(steps);
        }
        public IEnumerable<PlanetInLagrangePoint> GetL2Mergers(Planet p1, Planet p2, IEnumerable<(PlanetInLagrangePoint, PlanetInLagrangePoint, double, double)> pairs)
        {
            var rand = new Random();
            var i = 0;
            foreach (var pair in pairs)
            {
                var v = pair.Item3 / (pair.Item3 + pair.Item4);
                Console.WriteLine($"v={v}");
                var off = (pair.Item1.Offset0 * (1-v) + pair.Item2.Offset0 * v);
                var dotOff = ((pair.Item1.DotOffset0 * (1-v)) + (pair.Item2.DotOffset0 * v));
                Console.WriteLine($"v={v}; dotx01={pair.Item1.DotOffset0 * (1 - v)}; dotx02={pair.Item2.DotOffset0 * v}; dotOff={dotOff}");
                yield return new PlanetInLagrangePoint(p1, p2, 2, off, dotOff, $"{i},{off.x},{off.y},{dotOff.z}");
                i++;
            }
        }
        public void NumerSolveL2(int n, int steps = 1000)
        {
            (real_t, PlanetInLagrangePoint) best = (real_t.PositiveInfinity, null);
            (real_t, PlanetInLagrangePoint) ndBest = (real_t.PositiveInfinity, null);
            foreach (var l in _system.Get.Planets.Where(i => i is PlanetInLagrangePoint).Cast<PlanetInLagrangePoint>())
            {
                var sum = l.DistanceFromLPoint.Aggregate((i, j) => i + j).DistanceSquaredTo(Point3.Zero);
                if (sum < best.Item1)
                {
                    best = (sum, l);
                    continue;
                }
                if (sum < ndBest.Item1)
                    ndBest = (sum, l);
            }
            EarthAndMoonL2NewTest(best.Item2.Offset0.x, ndBest.Item2.Offset0.x,
                best.Item2.Offset0.y, ndBest.Item2.Offset0.y,
                best.Item2.DotOffset0.z, ndBest.Item2.DotOffset0.z, n, steps);
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
            _system.Get.AddPlanets(newPlanet);
        }
        public void PutObjectInLagrangePoint(Planet planet, Planet planet1, int lagrangePoint = 1, Point3 pidCoeficient = default, real_t DV = 10, real_t offset = 0, real_t mass = 1, real_t radius = 1, string planetName = "{0}, {1} L{2}")
        {
            (Planet larger, Planet smaller) = planet.Mass > planet1.Mass ? (planet, planet1) : (planet1, planet);
            var newPlanet = new PlanetInLagrangePoint(smaller, larger, lagrangePoint, radius, offset, mass, planetName, pidCoeficient, DV);
            _system.Get.AddPlanets(newPlanet);
        }
        public void AddL2(Planet planet, Planet planet1, real_t x0 = 0, real_t z0 = 5, real_t yDot0 = 0.02, string name = null)
        {
            (Planet larger, Planet smaller) = planet.Mass > planet1.Mass ? (planet, planet1) : (planet1, planet);
            var newPlanet = new PlanetInLagrangePoint(smaller, larger, 2, new Point3(x0, z0, 0), new Point3(0, 0, yDot0), name);
            _system.Get.AddPlanets(newPlanet);
        }
        public Planet GetL2(Planet planet, Planet planet1, real_t x0 = 0, real_t z0 = 5, real_t yDot0 = 0.02, string name = null)
        {
            (Planet larger, Planet smaller) = planet.Mass > planet1.Mass ? (planet, planet1) : (planet1, planet);
            var newPlanet = new PlanetInLagrangePoint(smaller, larger, 2, new Point3(x0, z0, 0), new Point3(0, 0, yDot0), name);
            return newPlanet;
        }

        public void AddL2Spectar(Planet planet, Planet planet1,
            real_t min_x0 = 0, real_t max_x0 = 2,
            real_t min_z0 = 0, real_t max_z0 = 2,
            real_t min_yDot0 = 0.02, real_t max_yDot0 = 0.2,
            int n = 5)
        {
            real_t tmp;
            if (min_x0 > max_x0) { tmp = max_x0; max_x0 = min_x0; min_x0 = tmp; }
            if (min_yDot0 > max_yDot0) { tmp = max_yDot0; max_yDot0 = min_yDot0; min_yDot0 = tmp; }
            if (min_z0 > max_z0) { tmp = max_z0; max_z0 = min_z0; min_z0 = tmp; }
            var dx = max_x0 == min_x0 ? real_t.PositiveInfinity : (max_x0 - min_x0) / n;
            var dz = max_z0 == min_z0 ? real_t.PositiveInfinity : (max_z0 - min_z0) / n;
            var dDoty = max_yDot0 == min_yDot0 ? real_t.PositiveInfinity : (max_yDot0 - min_yDot0) / n;
            for (var x = min_x0; x <= max_x0; x += dx)
            {
                for (var z = min_z0; z <= max_z0; z += dz)
                {
                    for (var y = min_yDot0; y <= max_yDot0; y += dDoty)
                    {

                        AddL2(planet, planet1, x, z, y, $"L2_x={x}_z={z}_y={y}");

                    }
                }
            }
        }
        private IEnumerable<Planet> GetL2Spectar(Planet planet, Planet planet1,
            real_t min_x0 = 0, real_t max_x0 = 2,
            real_t min_z0 = 0, real_t max_z0 = 2,
            real_t min_yDot0 = 0.02, real_t max_yDot0 = 0.2,
            int n = 5)
        {
            real_t tmp;
            if (min_x0 > max_x0) { tmp = max_x0; max_x0 = min_x0; min_x0 = tmp; }
            if (min_yDot0 > max_yDot0) { tmp = max_yDot0; max_yDot0 = min_yDot0; min_yDot0 = tmp; }
            if (min_z0 > max_z0) { tmp = max_z0; max_z0 = min_z0; min_z0 = tmp; }

            var dx = max_x0 == min_x0 ? real_t.PositiveInfinity : (max_x0 - min_x0) / n;
            var dz = max_z0 == min_z0 ? real_t.PositiveInfinity : (max_z0 - min_z0) / n;
            var dDoty = max_yDot0 == min_yDot0 ? real_t.PositiveInfinity : (max_yDot0 - min_yDot0) / n;
            for (var x = min_x0; x <= max_x0; x += dx)
            {
                for (var z = min_z0; z <= max_z0; z += dz)
                {
                    for (var y = min_yDot0; y <= max_yDot0; y += dDoty)
                    {

                        yield return GetL2(planet, planet1, x, z, y, $"L2_x={x}_z={z}_y={y}");
                    }
                }
            }
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
            _system.Get.AddPlanets(star);
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
                _system.Get.AddPlanets(planet);
                //massRemaining -= mass;
            }
        }
        public void Delete(Planet planet) =>
            _system.Get.RemovePlanets(planet);
        public void Delete(Planet planet, Planet planet1) =>
            _system.Get.RemovePlanets(planet, planet1);
        public void Delete(Planet planet, Planet planet1, Planet planet2) =>
            _system.Get.RemovePlanets(planet, planet1, planet2);
        public void Delete(Planet planet, Planet planet1, Planet planet2, Planet planet3) =>
            _system.Get.RemovePlanets(planet, planet1, planet2, planet3);
        public void Clear() =>
            _system.Get.ClearPlanets();
        public void NewL2Calculate(real_t halfPeriod = 213, real_t x0 = 0.02, real_t z0 = 0.15,
             real_t initial_ydot = -0.0003, real_t phiTimesFDt = 1e-2, bool changePhiFdt = false)
        {
            //(Planet larger, Planet smaller) = _1.Mass > _2.Mass ? (_1, _2) : (_2, _1);
            var (e, m) = GetEarthMoon();
            _system.Set(new PlanetSystem());
            var Xoffset = new Point3(x0, 0, z0);
            var XdotOffset = new Point3(0, initial_ydot, 0);
            var stop = false;
            var changeing = false;
            var tolerance = 10e-9;
            void nextIteration((double bumpZ0, double bumpYdot0) bump)
            {
                _system.Get.ClearPlanets();
                if (changeing)
                    return;
                changeing = true;
                if (stop) {
                    changeing = false;
                    return;
                }
                if (real_t.IsNaN(bump.bumpYdot0)) {
                    changeing = false;
                    System.Console.WriteLine("real_t.IsNaN(bump.bumpYdot0)");
                    return;
                }
                if (real_t.IsNaN(bump.bumpZ0)) {
                    changeing = false;
                    System.Console.WriteLine("real_t.IsNaN(bump.bumpZ0)");
                    return;
                }
                if (changePhiFdt && Math.Abs(bump.bumpZ0) < 10e-12 && Math.Abs(bump.bumpYdot0) < 10e-12) {
                    phiTimesFDt *= 0.95;
                    System.Console.WriteLine($"phiTimesFdt: {phiTimesFDt}");
                }
                Xoffset += new Point3(0, 0, bump.bumpZ0);
                XdotOffset += new Point3(0, bump.bumpYdot0, 0);
                System.Console.WriteLine($"dz0= {bump.bumpZ0}, dydot0 ={bump.bumpYdot0}");
                Console.WriteLine(Xoffset);
                Console.WriteLine(XdotOffset);
                var (e, m) = GetEarthMoon();
                _system.Set(new PlanetSystem());
                var l2 = new L2Calculation(e, m, nextIteration, () => { stop = true; }, halfPeriod, Xoffset, XdotOffset, tolerance, phiTimesFDt);
                _system.Get.AddPlanets(e, m, l2);
                changeing = false;
            }
            var l2 = new L2Calculation(e, m, nextIteration, () => { }, halfPeriod, Xoffset, XdotOffset, tolerance, phiTimesFDt);
            _system.Get.AddPlanets(e, m, l2);
        }
        public void TestL2Calc(real_t x0 = 1.2, real_t z0 = 0, real_t yd0 = 0.12240913374305831)
        {
            var (e, m) = GetEarthMoon();
            _system.Set(new PlanetSystem());
            var Xoffset = new Point3(x0, 0, z0);
            var XdotOffset = new Point3(0, yd0, 0);
            var position = PlanetInLagrangePoint.TransfromNormalizedToGlobal(e, m, Xoffset);
            var planet = new Planet
            {
                Position = position,
                Velocity = e.GetNullVelocity(m, position) + PlanetInLagrangePoint.TransfromNormalizedToGlobalDontScale(e, m, XdotOffset),
                Mass = 0,
                Name = "Test l2",
                Radius = 0.3,
            };
            _system.Get.AddPlanets(e, m, planet);
        }
    }
}
