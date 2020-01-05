using Godot;
namespace NBody.Gui
{
    public class OpenFileDialogButton : Button
    {
        public override void _Ready()
        {

        }
        public override void _Pressed()
        {
            base._Pressed();
            SourceOfTruth.ShowOpenPlanetSystemDialog = true;
        }
    }
}
