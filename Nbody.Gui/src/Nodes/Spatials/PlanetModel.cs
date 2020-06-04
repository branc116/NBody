using Godot;
using Nbody.Core;
using Nbody.Gui.Extensions;

namespace Nbody.Gui.Nodes.Spatials
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
        public void UpdateValue(int index)
        {
            Translation = Planet.Position.ToV3();
        }
    }
}
