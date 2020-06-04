using Godot;
using Nbody.Gui.Helpers;

namespace Nbody.Gui.Nodes.Controls
{
    public class DynamicToggleButtonObservable : CheckBox
    {
        private readonly SimpleObservable<bool> _observable;
        private bool _change = false;
        public DynamicToggleButtonObservable(SimpleObservable<bool> obj)
        {
            _observable = obj;
            _observable.RegisterAftersetting(AfterPress);
        }
        ~DynamicToggleButtonObservable()
        {
            _observable.UnrgisterPost(AfterPress);
        }
        private void AfterPress(bool val)
        {
            _change = Pressed != val;
        }
        public override void _Ready()
        {
            base._Ready();
            ToggleMode = true;
        }
        public override void _Process(float delta)
        {
            if (_change)
            {
                Pressed = _observable.Get;
                return;
            }
            if (Pressed != _observable.Get)
                _observable.Set(Pressed);
        }
    }
}
