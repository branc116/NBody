using Godot;
using System;
namespace NBody.Gui
{
    public class DebugPanel : Panel
    {
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
    }
}
