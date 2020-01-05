using Godot;
using System;
namespace NBody.Gui {
	public class PlanetSystemSelected : FileDialog
	{
		// Declare member variables here. Examples:
		// private int a = 2;
		// private string b = "text";

		// Called when the node enters the scene tree for the first time.
		private Button ok;
		private Button cancel;
		private bool handled = false;
		private bool init = true;
		public override void _Ready()
		{
			ok = GetOk();
			cancel = GetCancel();
		}
		public override void _Process(float delta)
		{
			if (SourceOfTruth.ShowOpenPlanetSystemDialog) {
				SourceOfTruth.ShowOpenPlanetSystemDialog = false;
				this.Visible = true;
			}
			if (!this.Visible)
				return;
			if (ok.Pressed && !handled)
			{
				handled = true;
				var fullPath = System.IO.Path.Combine(base.CurrentDir, base.CurrentFile);
				Console.WriteLine($"File: {fullPath}");
				SourceOfTruth.InputFile = fullPath;
				SourceOfTruth.RestartRequested = true;
				base.Visible = false;
			}
			else
			{
				handled = false;
			}
		}
	}
}
