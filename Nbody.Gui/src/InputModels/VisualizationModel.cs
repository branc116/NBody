using Nbody.Gui.Helpers;
using Nbody.Gui.Attributes;

namespace Nbody.Gui.InputModels
{
    public class VisualizationModel
    {
        [PropEdit]
        public SimpleObservable<bool> ShowPlanetArrows = true;
        public bool IsDebugShown = false;
    }
}
