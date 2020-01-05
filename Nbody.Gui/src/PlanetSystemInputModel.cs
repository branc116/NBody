using MathNet.Numerics.LinearAlgebra;
using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody.Gui
{
    public class PlanetSystemInputModel
    {
        public PlanetsInputModel[] Planets { get; set; }
        public double GravitationalConstant { get; set; }
        public double Dt { get; set; }
        public int StepsPerFrame { get; set; }
        public static PlanetSystemInputModel LoadFromFile(string fileName)
        {
            try
            {
                var json = System.IO.File.ReadAllText(fileName);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<PlanetSystemInputModel>(json);
            } catch (Exception ex)
            {
                return null;
            }
        }
        public PlanetSystem ToPlanetSystem()
        {
            return new PlanetSystem
            {
                GravitationalConstant = GravitationalConstant,
                DeltaTimePerStep = Dt,
                Planets = Planets?.Select(i => i.ToPlanet()).ToList()
            };
        }
    }
    public class PlanetsInputModel
    {
        public double[] Position { get; set; }
        public double[] Velocity { get; set; }
        public double Mass { get; set; }
        public double Radius { get; set; }
        public string Name { get; set; }

        internal Planet ToPlanet()
        {
            return new Planet
            {
                Mass = Mass,
                Name = Name,
                Position = CreateVector.Dense<double>(Position),
                Velocity = CreateVector.Dense<double>(Velocity),
                Radius = Radius
            };
        }
    }
}
