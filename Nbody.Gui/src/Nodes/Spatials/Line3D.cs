using Godot;
using Nbody.Gui.Extensions;
using System;
using System.Linq;

namespace Nbody.Gui.Nodes.Spatials
{
    public class Line3D : MultiMeshInstance
    {
        public override void _Ready()
        {
            
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            var planet = SourceOfTruth.PlanetInfoModel.SelectedPlanet;
            if (planet is null)
                return;
            var arr = planet.GetTracePoints().ToArray();
            Multimesh.InstanceCount = arr.Length;
            for(int i =0;i<arr.Length;i++)
            {
                Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, arr[i].ToV3()));
            }
        }
    }
}