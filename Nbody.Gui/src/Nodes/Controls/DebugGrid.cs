using Godot;
using NBody.Gui.Controllers;
using System.Linq;

namespace NBody.Gui
{
    public enum DisplayPlanetProperty
    {
        Name = 0,
        Position = 1,
        Velocity = 2,
        KineticEnergy = 3,
        Momentum = 4
    }
    public class DebugGrid : GridContainer
    {
        private readonly PlanetFabController _fabController = new PlanetFabController();
        public override void _Ready()
        {
            _fabController.ShowSubset = true;
        }
        public override void _Draw()
        {
            base._Draw();
            VisualServer.CanvasItemSetClip(this.GetCanvas(), true);
        }
        public override void _Process(float delta)
        {
            if (!SourceOfTruth.IsDebugShown)
                return;
            var children = this.GetChildren();
            var system = SourceOfTruth.System;
            _fabController.progress = SourceOfTruth.DebugPlanetListScrollValue;
            _fabController.DeleteOld(system, this);
            _fabController.UpdateExisiting(system, this);
            _fabController.AddNew(system, this, (planet) =>
            {
                return Enumerable
                    .Range(0, 5)
                    .Select(j => new PlanetLable(planet, (DisplayPlanetProperty)j))
                    .Cast<IPlanetFab>()
                    .ToList();
            });
        }
    }

}