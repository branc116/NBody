using NBody.Core;

namespace NBody.Gui
{
    public interface IPlanetFab
    {
        Planet Planet { get; }
        void UpdateValue();
        void QueueFree();
        void Hide();
        void Show();
    }
}
