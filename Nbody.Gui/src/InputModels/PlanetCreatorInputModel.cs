using Nbody.Gui.Helpers;
using Nbody.Core;
using System;

namespace Nbody.Gui.InputModels
{
    public class PlanetCreatorModel
    {
        public SimpleObservable<Planet[]> SelectedPlanets { get; } = new SimpleObservable<Planet[]>(null);
        public Action DoIt { get; set; }
        public string MethodSelected { get; set; }
    }
}
