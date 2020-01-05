using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
