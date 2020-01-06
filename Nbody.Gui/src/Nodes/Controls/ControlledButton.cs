using Godot;
using Nbody.Gui.src.Controllers;

namespace Nbody.Gui.src.Nodes.Controls
{
    public class ControlledButton : Button
    {
        private ButtonCommandController _buttonCommandController;
        public override void _Ready()
        {
            base._Ready();
            _buttonCommandController = new ButtonCommandController();
        }
        public override void _Pressed()
        {
            base._Pressed();
            _buttonCommandController.Do(this.Name, this);
        }
    }
}
