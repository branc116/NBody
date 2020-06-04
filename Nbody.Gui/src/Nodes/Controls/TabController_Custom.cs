using Godot;
namespace Nbody.Gui.Nodes.Controls
{
    public class TabController_Custom : TabContainer
    {
        public override void _Process(float delta)
        {
            Modulate = this.GetCurrentTabControl().Name == "Hide" ? new Color(1, 1, 1, 0.1f) : new Color(1, 1, 1, 1);
        }
    }
}
