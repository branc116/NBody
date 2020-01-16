using NBody.Core;
using System;

namespace NBody.Gui.InputModels
{
    public class PlanetCreatorModel
    {
        public Planet[] SelectedPlanets { get; set; }
        public Action DoIt { get; set; }
        public string MethodSelected { get; set; }
    }
}
