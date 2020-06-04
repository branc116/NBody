using Godot;
using Nbody.Gui.Helpers;
using Nbody.Core;

namespace Nbody.Gui.Nodes.Controls
{
    public class PlanetLable : Label, IPlanetFab
    {
        public Planet Planet { get; set; }
        public DisplayPlanetProperty ToDisplay { get; set; }
        public Node ControlledBy { get; set; }
        public PlanetLable(Planet planet, DisplayPlanetProperty displayPlanetProperty)
        {
            Planet = planet;
            ToDisplay = displayPlanetProperty;
            UpdateValue(-1);
        }
        public void UpdateValue(int index)
        {
            Text = GetTextPrivate();
            if (ControlledBy is ItemList il)
            {
                il.SetItemText(index, Text);
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