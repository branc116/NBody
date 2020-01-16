using NBody.Gui.InputModels;
using NBody.Gui.Attributes;
using NBody.Core;

namespace NBody.Gui
{
    public static class SourceOfTruth
    {
        public static PlanetSystem System;
        [PropEdit("DebugProps")]
        public readonly static VisualizationModel VisualizationModel = new VisualizationModel();
        [PropEdit("DebugProps")]
        public readonly static SimulationModel SimulationModel = new SimulationModel();
        [PropEdit("PlotProps")]
        public readonly static PlotsModel PlotModel = new PlotsModel();
        public readonly static Kernels.NbodyClKernel Kernel = Kernels.NbodyClKernel.GetNbodyClKernel();
        public static readonly PlanetCreatorModel PlanetCreatorModel = new PlanetCreatorModel();
    }
}
