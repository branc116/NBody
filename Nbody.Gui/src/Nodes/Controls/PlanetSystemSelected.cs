using Godot;
using NBody.Gui.InputModels;
namespace NBody.Gui
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
        }
        public override void _Process(float delta)
        {
            if (_simulationModel.ShowOpenPlanetSystemDialog)
            {
                _simulationModel.ShowOpenPlanetSystemDialog = false;
                this.Visible = true;
            }
            if (!this.Visible)
                return;
            if (_ok.Pressed && !_handled)
            {
                _handled = true;
                var fullPath = System.IO.Path.Combine(base.CurrentDir, base.CurrentFile);
                //Console.WriteLine($"File: {fullPath}");
                _simulationModel.InputFile = fullPath;
                _simulationModel.RestartRequested = true;
                base.Visible = false;
            }
            else
            {
                _handled = false;
            }
        }
    }
}
