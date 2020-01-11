using Godot;
using NBody.Core;
using NBody.Gui.Extensions;

namespace NBody.Gui
{
    public class PlanetLable : Label, IPlanetFab
    {
        public Planet Planet { get; set; }
        public DisplayPlanetProperty ToDisplay { get; set; }
        public Node ControlledBy { get; set; }
        public PlanetLable(Planet planet, DisplayPlanetProperty displayPlanetProperty)
        {
            this.Planet = planet;
            this.ToDisplay = displayPlanetProperty;
            UpdateValue(-1);
        }
        public void UpdateValue(int index)
        {
            this.Text = GetTextPrivate();
            if (ControlledBy is ItemList il)
            {
                il.SetItemText(index, this.Text);
            }
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

}