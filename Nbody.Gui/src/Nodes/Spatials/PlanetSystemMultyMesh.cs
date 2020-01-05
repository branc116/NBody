using Godot;
using NBody.Gui.Extensions;
namespace NBody.Gui
{
    public class PlanetSystemMultyMesh : MultiMeshInstance
    {
        //private MeshInstance _planetMesh;

        public override void _Ready()
        {

            //_planetMesh = _planetMesh is null ? this.GetNode<MeshInstance>(new NodePath("PlanetMesh")) : _planetMesh;
            //Multimesh.Mesh = _planetMesh.Mesh;
            //Multimesh.InstanceCount;
        }

        public override void _Process(float delta)
        {
            var system = SourceOfTruth.System;
            Multimesh.InstanceCount = system.Planets.Count;
            for (int i = 0; i < system.Planets.Count; i++)
            {
                Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, system.Planets[i].Position.ToV3()));
            }
        }
    }
}
