using NBody.Gui.Core;
using NBody.Core;
using System;
using System.Linq;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif
namespace NBody.Gui.InputModels
{
    public class PlanetSystemInputModel
    {
        public PlanetsInputModel[] Planets { get; set; }
        public real_t GravitationalConstant { get; set; }
        public real_t Dt { get; set; }
        public int StepsPerFrame { get; set; }
        public static PlanetSystemInputModel LoadFromFile(string fileName)
        {
            try
            {
                var json = System.IO.File.ReadAllText(fileName);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<PlanetSystemInputModel>(json);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public PlanetSystem ToPlanetSystem()
        {
            return new PlanetSystem
            {
                GravitationalConstant = GravitationalConstant,
                //DeltaTimePerStep = Dt,
                Planets = Planets?.Select(i => i.ToPlanet()).ToList()
            };
        }
    }
    public class PlanetsInputModel
    {
        public real_t[] Position { get; set; }
        public real_t[] Velocity { get; set; }
        public real_t Mass { get; set; }
        public real_t Radius { get; set; }
        public string Name { get; set; }

        internal Planet ToPlanet()
        {
            return new Planet
            {
                Mass = Mass,
                Name = Name,
                Position = new Point3(Position[0], Position[1], Position[2]),
                Velocity = new Point3(Velocity[0], Velocity[1], Velocity[2]),
                Radius = Radius
            };
        }
    }
}
