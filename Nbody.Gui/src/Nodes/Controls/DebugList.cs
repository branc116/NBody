using Godot;
using NBody.Gui.InputModels;

namespace NBody.Gui
{
    public class DebugList : ItemList
    {
        private readonly VisualizationModel _visualizationModel = SourceOfTruth.VisualizationModel;
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;

        public override void _Process(float delta)
        {
            var system = SourceOfTruth.System;
            if (!_visualizationModel.IsDebugShown || system is null)
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
