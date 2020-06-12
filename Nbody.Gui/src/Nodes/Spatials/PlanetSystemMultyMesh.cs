using Godot;
using Nbody.Core;
using Nbody.Gui.Core;
using Nbody.Gui.Extensions;
using Nbody.Gui.Helpers;

namespace Nbody.Gui
{
    public class PlanetSystemMultyMesh : MultiMeshInstance
    {
        private readonly SimpleObservable<PlanetSystem> _system = SourceOfTruth.System;

        public override void _Process(float delta)
        {
            var system = _system.Get;
            Multimesh.InstanceCount = system.Planets.Count;
            for (int i = 0; i < system.Planets.Count; i++)
            {
                var planet = system.Planets[i];
                var scale = MathReal.Max(planet.Radius, 0.1f);
                Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, planet.Position.ToV3()).Scale2(new Vector3((float)scale, (float)scale, (float)scale)));
            }
        }
    }
}
