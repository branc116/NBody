using Godot;
using NBody.Core;
using NBody.Gui.Extensions;

namespace NBody.Gui.Nodes.Spatials
{
    public class PlanetModel : Spatial, IPlanetFab
    {
        public Planet Planet { get; set; }
        public PlanetModel(Planet planet, Spatial mesh)
        {
            Planet = planet;
            var m = mesh.Duplicate() as Spatial;
            var radius = (float)planet.Radius;
            m.Scale = new Vector3(radius, radius, radius);
            m.Visible = true;
            AddChild(m);
        }
        public void UpdateValue()
        {
            Translation = Planet.Position.ToV3();
        }
    }
}
