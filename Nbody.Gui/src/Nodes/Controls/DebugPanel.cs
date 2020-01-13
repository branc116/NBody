using Godot;
using NBody.Gui.InputModels;
using System;
namespace NBody.Gui
{
    public class DebugPanel : Panel
    {
        private readonly VisualizationModel _visualizationModel = SourceOfTruth.VisualizationModel;

        public override void _Ready()
        {
            this.Visible = _visualizationModel.IsDebugShown;
        }
        public override void _UnhandledKeyInput(InputEventKey @event)
        {

            if (!@event.Pressed && @event.AsText() == "D")
            {
                Console.WriteLine(@event.AsText());
                this.Visible = _visualizationModel.IsDebugShown = !_visualizationModel.IsDebugShown;
            }
        }
        public override void _Process(float delta)
        {
            if (Visible != _visualizationModel.IsDebugShown)
                Visible = _visualizationModel.IsDebugShown;
        }
    }
}
