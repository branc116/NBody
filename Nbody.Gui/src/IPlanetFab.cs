using Nbody.Core;

namespace Nbody.Gui
{
    public interface IPlanetFab
    {
        Planet Planet { get; }
        void UpdateValue(int index);
        void QueueFree();
        void Hide();
        void Show();
    }
}
