using Godot;
using Nbody.Gui.src.Attributes;
using Nbody.Gui;
using Nbody.Gui.InputModels;

namespace Nbody.Gui.src.Controllers
{
    [ButtonCommand(Name = "")]
    public class DebugController
    {
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        private readonly VisualizationModel _visualizationModel = SourceOfTruth.VisualizationModel;
        private readonly PlanetCreatorModel _planetCreatorModel = SourceOfTruth.PlanetCreatorModel;
        public void Restart(Node node)
        {
            _simulationModel.RestartRequested.Set(true);
        }
        public void Pause(Node node)
        {
            _simulationModel.Paused = true;
            (node as Button).Text = node.Name = "Resume";
        }
        public void Resume(Node node)
        {
            _simulationModel.Paused = false;
            (node as Button).Text = node.Name = "Pause";
        }
        public void Step(Node node)
        {
            SourceOfTruth.System.Step();
        }
        public void NSteps(Node node)
        {
            SourceOfTruth.System.Step(_simulationModel.StepsPerFrame);
        }
        [ButtonCommand(Name = "Open")]
        public void OpenFileDialog(Node node)
        {
            _simulationModel.ShowOpenPlanetSystemDialog.Set(true);
        }
        public void CloseDebug(Node node)
        {
            _visualizationModel.IsDebugShown = false;
        }
        public void DoIt(Node node)
        {
            _planetCreatorModel.DoIt();
        }
    }
}
