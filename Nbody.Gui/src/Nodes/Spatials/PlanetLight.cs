using Godot;
using NBody.Core;
using NBody.Gui.Extensions;

namespace NBody.Gui.Nodes.Spatials
{
    public class PlanetLight : Spatial, IPlanetFab
    {
        public Planet Planet { get; set; }
        public PlanetLight(Spatial lightSpatial)
        {
            var light = lightSpatial.Duplicate() as Spatial;
            this.Visible = false;
            light.Visible = true;
            AddChild(light);
        }
        public void UpdateValue()
        {

            if (SourceOfTruth.ShowLights)
            {
                Transform = new Transform(Basis.Identity, Planet.Position.ToV3())
                    .TargetTo2(Planet.Velocity.ToV3().Normalized(), Vector3.Up);
                this.Visible = true;
            }
            else
            {
                this.Visible = false;
            }
        }
    }
}
