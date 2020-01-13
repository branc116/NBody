using NBody.Gui.Attributes;
#if REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace NBody.Gui.InputModels
{
    public class SimulationModel
    {
        [PropEdit]
        public int StepsPerFrame = 1;
        [PropEdit]
        public int MaxHistoy = 10000;
        [PropEdit]
        public int RememberEvery = 100;
        [PropEdit]
        public real_t DeltaTimePerStep = (real_t)0.001;
        [PropEdit]
        public bool SimulateColitions = false;
        [PropEdit]
        public real_t GravitationalConstant;
        [PropEdit]
        public bool UseOpenCl;
        public string InputFile = "PlanetSystem.json";
        public bool RestartRequested = false;
        public bool ShowOpenPlanetSystemDialog = false;
        public bool Paused = false;
    }
}
