using Godot;
using NBody.Core;
using NBody.Gui.Extensions;

namespace NBody.Gui.Nodes.Spatials
{
    public class PlanetArrow : Spatial, IPlanetFab
    {
        public Planet Planet { get; set; }
        public PlanetArrow(Planet planet, Spatial mesh)
        {
            Planet = planet;
            var m = mesh.Duplicate() as Spatial;
            var radius = 0.5f;
            m.Scale = new Vector3(radius, radius, radius);
            m.Visible = true;
            AddChild(m);
        }
        public void UpdateValue(int index)
        {
            Transform = new Transform(Basis.Identity, Planet.Position.ToV3())
                .TargetTo2(Planet.Velocity.ToV3().Normalized(), Vector3.Up)
                .Scale2(new Vector3(1f, Mathf.Log(1f + Planet.Velocity.ToV3().LengthSquared()), 1f));

        }
    }
}
