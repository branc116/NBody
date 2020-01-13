using Godot;
using NBody.Core;

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
                    return Planet.Velocity.ToString();
                case DisplayPlanetProperty.Position:
                    return Planet.Position.ToString();
                case DisplayPlanetProperty.KineticEnergy:
                    return Planet.KineticEnergy.ToString();
                case DisplayPlanetProperty.Momentum:
                    return Planet.Momentum.ToString();
            }
            return "";
        }

    }

}