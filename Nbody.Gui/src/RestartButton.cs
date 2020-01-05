using Godot;
using System;
namespace NBody.Gui
{
    public class RestartButton : Button
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {

        }
        public override void _Pressed()
        {
            switch (Text)
            {
                case "Restart":
                    NBody.Gui.SourceOfTruth.RestartRequested = true;
                    break;
                case "Pause":
                    NBody.Gui.SourceOfTruth.Paused = true;
                    Text = "Resume";
                    break;
                case "Resume":
                    NBody.Gui.SourceOfTruth.Paused = false;
                    Text = "Pause";
                    break;
                case "Step":
                    NBody.Gui.SourceOfTruth.System.Step();
                    break;
                case "NSteps":
                    NBody.Gui.SourceOfTruth.System.Step(SourceOfTruth.StepsPerFrame);
                    break;
                default:
                    Console.WriteLine($"Command name unknown {Text}");
                    break;
            }

        }
        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}
