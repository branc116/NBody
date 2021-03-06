using Godot;
using Nbody.Core;
using Nbody.Gui.Helpers;
using Nbody.Gui.InputModels;

namespace Nbody.Gui.Nodes.Controls
{
    public class DebugList : ItemList
    {
        private readonly VisualizationModel _visualizationModel = SourceOfTruth.VisualizationModel;
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        private readonly SimpleObservable<PlanetSystem> _system = SourceOfTruth.System;
        public override void _Process(float delta)
        {
            var system = _system.Get;
            if (system is null || !IsVisibleInTree())
                return;

            Clear();
            AddItem($"Time: {system.CurTime:F3}");
            AddItem($"#Steps: {system.NStep}");
            AddItem($"#Planets: {system.Planets.Count}");
            AddItem($"Total Mass: {system.TotalMass():F3}");
            AddItem($"Mass center: {system.MassCenter():F3}");
            AddItem($"Total Momentum: {system.TotalMomentum():F3}");
            AddItem($"Planet System File: {_simulationModel.InputFile}");
            AddItem($"Fps: {1 / delta}");
        }
    }
}
