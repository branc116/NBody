using Godot;
using NBody.Gui.Extensions;

namespace NBody.Gui
{
    public class DebugList : ItemList
    {
        public override void _Process(float delta)
        {
            var system = SourceOfTruth.System;
            if (!SourceOfTruth.IsDebugShown || system is null)
                return;

            Clear();
            AddItem($"Time: {system.CurTime:F3}");
            AddItem($"#Steps: {system.NStep}");
            AddItem($"#Planets: {system.Planets.Count}");
            AddItem($"Total Mass: {system.TotalMass():F3}");
            AddItem($"Mass center: {system.MassCenter().ToStrV3()}");
            AddItem($"Total Momentum: {system.TotalMomentum().ToStrV3()}");
            AddItem($"Planet System File: {SourceOfTruth.InputFile}");
            AddItem($"Fps: {1 / delta}");
        }
    }
}
