using Godot;
using NBody.Gui.Extensions;
using NBody.Gui.InputModels;

namespace NBody.Gui
{
    public class PlanetSystemArrowMultiMesh : MultiMeshInstance
    {
        private readonly VisualizationModel _visualizationModel = SourceOfTruth.VisualizationModel;

        public override void _Process(float delta)
        {
            if (!_visualizationModel.ShowPlanetArrows)
            {
                Visible = false;
                return;
            }
            else
            {
                Visible = true;
            }

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
