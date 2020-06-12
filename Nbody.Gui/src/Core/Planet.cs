using Nbody.Gui.Core;
using Nbody.Gui;
using Nbody.Gui.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Nbody.Gui.Extensions;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace Nbody.Core
{
    public class Planet
    {
        private static readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        private CircularArray<Point3> _position = new CircularArray<Point3>(_simulationModel.MaxHistoy, _simulationModel.RememberEvery);
        private CircularArray<Point3> _velocity = new CircularArray<Point3>(_simulationModel.MaxHistoy, _simulationModel.RememberEvery);

        public Point3 Position { get => _position.Last(); set => _position.Add(value); }
        public Point3 Velocity { get => _velocity.Last(); set => _velocity.Add(value); }
        public int Steps => _position.Position;
        public IEnumerable<Point3> PositionHistory => _position.Select(i => i);
        public IEnumerable<Point3> VelocityHistory => _velocity.Select(i => i);
        public real_t Mass { get; set; }
        public real_t Radius { get; set; }
        public string Name { get; set; }
        public virtual string Type => "Basic planet";
        public Point3 Momentum =>
            Velocity * Mass;
        public double KineticEnergy =>
            Mass * Velocity.LengthSquared() / 2;
        public void MegeWith(Planet planet)
        {
            Name += $" + {planet.Name}";
            var v1 = Velocity;
            var v2 = planet.Velocity;
            var mom1 = v1 * Mass;
            var mom2 = v2 * planet.Mass;
            Velocity = mom1 + mom2 / (planet.Mass + Mass);
            Mass += planet.Mass;
        }
        public Dictionary<int, Func<Planet, real_t, Point3>> LagrangePoint => new Dictionary<int, Func<Planet, real_t, Point3>>
        {
            {1, L1 }, {2, L2 }, {3, L3 }, {4, L4 }, {5, L5 }
        };
        /// <summary>
        /// L1 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="planet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Point3 L1(Planet planet, real_t offset = default)
        {
            if (Mass > planet.Mass)
                return planet.L1(this, offset);
            var diff = Position - planet.Position;
            var R = diff.Length();
            double M1 = planet.Mass, M2 = Mass, rinit = R * (Math.Pow(M2 / (3 * M1), 1 / 3.0));
            Func<double, double> func = (distance) => M2 / (distance * distance) + M1 / (R * R) - distance * (M1 + M2) / (R * R * R) - M1 / Math.Pow(R - distance, 2);
            var r = func.NumericSolver(rinit);
            var rpercent = r / R;
            rpercent += offset;
            r = rpercent * R;
            var point = -diff.Normalized() * (real_t)r + Position;
            return point;
        }
        /// <summary>
        /// L2 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="planet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Point3 L2(Planet planet, real_t offset = default)
        {
            if (Mass > planet.Mass)
                return planet.L2(this, offset);
            var diff = Position - planet.Position;
            var R = diff.Length();
            double M1 = planet.Mass, M2 = Mass, rinit = R * (Math.Pow(M2 / (3 * M1), 1 / 3.0));
            Func<double, double> func = (distance) =>
                -M2 / (distance * distance) + M1 / (R * R) - distance * (M1 + M2) / (R * R * R) - M1 / Math.Pow(R - distance, 2);
            var r = func.NumericSolver(rinit);
            var rpercent = r / R;
            rpercent += offset;
            r = rpercent * R;
            var point = -diff.Normalized() * (real_t)r + Position;
            return point;
        }
        /// <summary>
        /// L3 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="planet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Point3 L3(Planet planet, real_t offset = default)
        {
            if (Mass > planet.Mass)
                return planet.L3(this, offset);
            var diff = Position - planet.Position;
            var R = diff.Length();
            double M1 = planet.Mass, M2 = Mass, rinit = 7 * M2 * R / 12 / M1;
            Func<double, double> func = (rr) =>
                M1 / Math.Pow((R - rr), 2) + M2 / Math.Pow((2 * R - rr), 2) - (M2 * R / (M1 + M2) + R - rr) * (M1 + M2) / Math.Pow(R, 3);
            var r = func.NumericSolver(rinit);
            var position = planet.Position - (Position + Position.Normalized() * (real_t)r);
            return position;
        }
        /// <summary>
        /// L4 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="largerPlanet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Point3 L4(Planet largerPlanet, real_t offset = default)
        {
            if (Mass > largerPlanet.Mass)
                return largerPlanet.L2(this, offset);
            var diff = Position - largerPlanet.Position;
            var R = diff.Length();
            var T1 = diff.Normalized() * R / 2;
            var T2 = Velocity.Normalized() * MathReal.Sqrt3 / 2 * R;
            var L4 = largerPlanet.Position + T1 + T2;
            return L4;
        }
        /// <summary>
        /// L5 point for the pair of planets planet.Mass > Mass;
        /// </summary>
        /// <param name="planet"><see cref="Planet.Mass"/> > <see cref="Mass"></see>/></param>
        /// <returns></returns>
        public Point3 L5(Planet largerPlanet, real_t offset = default)
        {
            if (Mass > largerPlanet.Mass)
                return largerPlanet.L2(this, offset);
            var diff = Position - largerPlanet.Position;
            var R = diff.Length();
            var T1 = diff.Normalized() * R / 2;
            var T2 = Velocity.Normalized() * MathReal.Sqrt3 / 2 * R;
            var L4 = largerPlanet.Position + T1 - T2;
            return L4;
        }
        public override string ToString()
        {
            return $"{Name}:\n   Position: {Position}\n   Velocity: {Velocity}\n   Momentum: {Momentum}";
        }

        public real_t GetCircularOrbitSpeed(real_t mass, real_t distance)
        {
            return MathReal.Sqrt(SourceOfTruth.System.Get.GravitationalConstant * (Mass + mass) / distance);
        }
        public Point3 GetNullVelocity(Planet smallerPlanet, Point3 position)
        {
            var vdas = position - Position;
            var vdae = position - smallerPlanet.Position;
            var das = Position.DistanceTo(position);
            var dae = smallerPlanet.Position.DistanceTo(position);
            var beryCenter = (smallerPlanet.Mass * smallerPlanet.Position + Mass * Position) / (smallerPlanet.Mass + Mass);
            var distanceToBeryCenter = position.DistanceTo(beryCenter);
            var speedSquared = SourceOfTruth.SimulationModel.GravitationalConstant *
                    (Mass * vdas.Normalized() / das / das + smallerPlanet.Mass * vdae.Normalized() / dae / dae).Length() * distanceToBeryCenter;
            var rotateFor = position.AngleTo(smallerPlanet.Position);

            var rotateAroud = vdae.Normalized().Cross(smallerPlanet.Position - Position).Normalized();
            var velocity = smallerPlanet.Velocity.RotateAround(rotateAroud, rotateFor).Normalized() * MathReal.Sqrt(speedSquared);
            return velocity;
        }
        public virtual IEnumerable<(string name, string value)> GetPlanetInfo()
        {
            yield return ("Name", Name);
            yield return ("Type", Type);
            yield return ("Position", Position.ToString("F3"));
            yield return ("Velocity", Velocity.ToString("F3"));
            yield return ("Momentum", Momentum.ToString("F3"));
            yield return ("Internal force", InternalAcceleration(false, 0).ToString("F3"));

            yield return ("Kinetic energy", KineticEnergy.ToString("F3"));
            yield return ("Speed", Velocity.Length().ToString("F3"));
            yield return ("Mass", Mass.ToString("F3"));
            yield return ("Radius", Radius.ToString("F3"));
            yield return ("Number of history points", _position.Count.ToString());

        }
        public virtual Point3 InternalAcceleration(bool recalculate, real_t dt)
        {
            return Point3.Zero;
        }
        public virtual void AfterUpdate()
        {
            return;
        }
        public virtual IEnumerable<Point3> GetTracePoints()
        {
            return PositionHistory.ToList();
        }
    }
    
}

