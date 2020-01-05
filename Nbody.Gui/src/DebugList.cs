using Godot;
using System;
using System.Linq;
using NBody.Core;
using NBody.Gui.Extensions;

namespace NBody.Gui {
	public class DebugList : ItemList
	{
		// Declare member variables here. Examples:
		// private int a = 2;
		// private string b = "text";

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			//Items = new Godot.Collections.Array();
			//Nbody.Godot.SourceOfTruth.System.Planets.Select(i => i.ToString()).ToList().ForEach(i => Items.Add(i));
		}

		//  // Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			//Items = new Godot.Collections.Array();
			var system = SourceOfTruth.System;
			if (!SourceOfTruth.IsDebugShown || system is null)
				return;
			
			Clear();
			AddItem($"Time: {system.CurTime:F3}");
			AddItem($"#Steps: {system.NStep}");
			AddItem($"#Planets: {system.Planets.Count}");
			AddItem($"Total Mass: {system.TotalMass():F3}");
			AddItem($"Mass center: {system.MassCenter().ToStrV3()}");	
			AddItem($"Total Momentum: {system.TotalMomentum().ToStrV3()}");
			AddItem($"Planet System File: {SourceOfTruth.InputFile}");
			AddItem($"Fps: {1 / delta}");
			//NBody.Gui.SourceOfTruth.System.Planets.Select(i => i.ToString()).ToList().ForEach(i => AddItem(i));
		}
	}
}
