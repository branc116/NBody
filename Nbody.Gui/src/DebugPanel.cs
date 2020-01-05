using Godot;
using System;
namespace NBody.Gui {
	public class DebugPanel : Panel
	{
		// Declare member variables here. Examples:
		// private int a = 2;
		// private string b = "text";

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			this.Visible = SourceOfTruth.IsDebugShown;
		}
		public override void _UnhandledKeyInput(InputEventKey @event)
		{
			
			if (!@event.Pressed && @event.AsText() == "D")
			{
				Console.WriteLine(@event.AsText());
				this.Visible = SourceOfTruth.IsDebugShown = !SourceOfTruth.IsDebugShown;
			}
		}
		//  // Called every frame. 'delta' is the elapsed time since the previous frame.
		//  public override void _Process(float delta)
		//  {
		//      
		//  }
	}
}
