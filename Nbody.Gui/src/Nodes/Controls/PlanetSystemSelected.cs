using Godot;
using System;
namespace NBody.Gui
{
    public class PlanetSystemSelected : FileDialog
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        private Button _ok;
        private bool _handled = false;
        private bool _init = true;
        public override void _Ready()
        {
            _ok = GetOk();
        }
        public override void _Process(float delta)
        {
            if (SourceOfTruth.ShowOpenPlanetSystemDialog)
            {
                SourceOfTruth.ShowOpenPlanetSystemDialog = false;
                this.Visible = true;
            }
            if (!this.Visible)
                return;
            if (_ok.Pressed && !_handled)
            {
                _handled = true;
                var fullPath = System.IO.Path.Combine(base.CurrentDir, base.CurrentFile);
                Console.WriteLine($"File: {fullPath}");
                SourceOfTruth.InputFile = fullPath;
                SourceOfTruth.RestartRequested = true;
                base.Visible = false;
            }
            else
            {
                _handled = false;
            }
        }
    }
}
