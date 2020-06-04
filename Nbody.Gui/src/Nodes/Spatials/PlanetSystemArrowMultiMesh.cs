using Godot;
using Nbody.Gui.Extensions;
using Nbody.Gui.InputModels;

namespace Nbody.Gui
{
    public class PlanetSystemArrowMultiMesh : MultiMeshInstance
    {
        private readonly VisualizationModel _visualizationModel = SourceOfTruth.VisualizationModel;
        public override void _Ready()
        {
            _visualizationModel.ShowPlanetArrows.RegisterAftersetting(val => this.Visible = val);
        }

        public override void _Process(float delta)
        {
            if (!Visible)
                return;

            var system = SourceOfTruth.System;
            Multimesh.InstanceCount = system.Planets.Count;
            for (int i = 0; i < system.Planets.Count; i++)
            {
                var planet = system.Planets[i];
                Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, planet.Position.ToV3())
                    .TargetTo2(planet.Velocity.ToV3().Normalized(), Vector3.Up)
                    .Scale2(new Vector3(1f, Mathf.Log(1f + planet.Velocity.ToV3().LengthSquared())/2, 1f)));
            }
        }
    }
}
