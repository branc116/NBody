using Godot;
using Nbody.Gui.src.Controllers;

namespace Nbody.Gui.Nodes.Controls
{
    public class ControlledButton : Button
    {
        public int mofo;
        private ButtonCommandController _buttonCommandController;
        public override void _Ready()
        {
            base._Ready();
            _buttonCommandController = new ButtonCommandController();
        }
        public override void _Pressed()
        {
            base._Pressed();
            _buttonCommandController.Do(Name, this);
        }
    }
}
