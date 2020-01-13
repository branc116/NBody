using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody.Gui.InputModels
{
    public class PlanetCreatorModel
    {
        public Planet[] SelectedPlanets { get; set; }
        public Action DoIt { get; set; }
        public string MethodSelected { get; set; }
    }
}
