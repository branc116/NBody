using Nbody.Gui.InputModels;
using Nbody.Gui.Attributes;
using Nbody.Core;
using Godot.Collections;
using Nbody.Gui.Helpers;
using System.Collections.Generic;

namespace Nbody.Gui
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
        public static readonly PlanetInfoModel PlanetInfoModel = new PlanetInfoModel();
        public static List<(string, SimpleObservable<Planet[]>)> PlanetCollection = new List<(string, SimpleObservable<Planet[]>)>();
    }
}
