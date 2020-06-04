using Godot;
using Nbody.Gui.InputModels;

namespace Nbody.Gui.Nodes.Controls
{
    public class PlanetSystemSelected : FileDialog
    {
        private Button _ok;
        private bool _handled = false;
        private bool _init = true;
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        public override void _Ready()
        {
            _ok = GetOk();
            _simulationModel.ShowOpenPlanetSystemDialog.RegisterAftersetting(val =>
            {
                Visible = val;
                _handled = false;
            });
        }
        public override void _Process(float delta)
        {
            if (!Visible)
                return;
            if (_ok.Pressed && !_handled)
            {
                _handled = true;
                var fullPath = System.IO.Path.Combine(CurrentDir, CurrentFile);
                _simulationModel.InputFile = fullPath;
                _simulationModel.RestartRequested.Set(true);
                _simulationModel.ShowOpenPlanetSystemDialog.Set(true);
            }
        }
    }
}
