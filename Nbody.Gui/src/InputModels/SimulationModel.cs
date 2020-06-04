using Nbody.Gui.Helpers;
using Nbody.Gui.Attributes;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace Nbody.Gui.InputModels
{
    public class SimulationModel
    {
        [PropEdit]
        public readonly SimpleObservable<int> StepsPerFrame = 100;
        [PropEdit]
        public readonly SimpleObservable<int> MaxHistoy = 10000;
        [PropEdit]
        public readonly SimpleObservable<int> RememberEvery = 100;
        [PropEdit]
        public readonly SimpleObservable<real_t> DeltaTimePerStep = (real_t)0.001;
        [PropEdit]
        public readonly SimpleObservable<bool> SimulateColitions = false;
        [PropEdit]
        public readonly SimpleObservable<real_t> GravitationalConstant = (real_t)0.1;
        [PropEdit]
        public readonly SimpleObservable<bool> UseOpenCl = false;
        [PropEdit]
        public readonly SimpleObservable<int> DoPidEveryStep = 10;
        public string InputFile = "PlanetSystem.json";
        public readonly  SimpleObservable<bool> RestartRequested = false;
        public readonly SimpleObservable<bool> ShowOpenPlanetSystemDialog = false;
        public bool Paused = false;
    }
}
