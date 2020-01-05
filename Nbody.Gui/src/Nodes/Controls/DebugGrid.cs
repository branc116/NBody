using Godot;
using NBody.Core;
using NBody.Gui.Controllers;
using NBody.Gui.Extensions;
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
    public class PlanetLable : Label, IPlanetFab
    {
        public Planet Planet { get; set; }
        public DisplayPlanetProperty ToDisplay { get; set; }
        public PlanetLable(Planet planet, DisplayPlanetProperty displayPlanetProperty)
        {
            this.Planet = planet;
            this.ToDisplay = displayPlanetProperty;
            UpdateValue();
        }
        public void UpdateValue()
        {
            this.Text = GetTextPrivate();
        }
        private string GetTextPrivate()
        {
            switch (ToDisplay)
            {
                case DisplayPlanetProperty.Name:
                    return Planet.Name.Length > 20 ? Planet.Name.Substring(0, 20) : Planet.Name;
                case DisplayPlanetProperty.Velocity:
                    return Planet.Velocity.ToStrV3();
                case DisplayPlanetProperty.Position:
                    return Planet.Position.ToStrV3();
                case DisplayPlanetProperty.KineticEnergy:
                    return Planet.KineticEnergy.ToString("F3");
                case DisplayPlanetProperty.Momentum:
                    return Planet.Momentum.ToStrV3();
            }
            return "";
        }

    }
    public class DebugGrid : GridContainer
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        private readonly PlanetFabController _fabController = new PlanetFabController();
        public override void _Ready()
        {
            _fabController.ShowSubset = true;
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
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