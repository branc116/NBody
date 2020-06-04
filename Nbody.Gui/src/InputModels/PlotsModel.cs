using Godot;
using Nbody.Gui.Helpers;
using Nbody.Core;
using Nbody.Gui.Attributes;
using System.Text;

#if GODOT_REAL_T_IS_DOUBLE
using godot_real_t = System.Double;
#else
using godot_real_t = System.Single;
#endif

namespace Nbody.Gui.InputModels
{
    public class PlotsModel
    {
        [PropEdit]
        public readonly SimpleObservable<float> PlotStepsPerDiv = 1000f;
        [PropEdit]
        public readonly SimpleObservable<float> DivSize = 1f;
        [PropEdit]
        public readonly SimpleObservable<godot_real_t> PlotCenterX = 0;
        [PropEdit]
        public readonly SimpleObservable<godot_real_t> PlotCenterY = 0;
        [PropEdit]
        public readonly SimpleObservable<float> PlotWidth = 3f;
        [PropEdit(Editable = false)]
        public readonly SimpleObservable<Vector2> Min = new SimpleObservable<Vector2>(default);
        [PropEdit(Editable = false)]
        public readonly SimpleObservable<Vector2> Max = new SimpleObservable<Vector2>(default);
        [PropEdit]
        public readonly SimpleObservable<bool> Follow = false;
        [PropEdit]
        public readonly SimpleObservable<bool> XLogScale = false;
        [PropEdit]
        public readonly SimpleObservable<bool> YLogScale = false;
        public readonly SimpleObservable<Vector2> PlotResoultion = new SimpleObservable<Vector2>(Vector2.Zero);
        public readonly SimpleObservable<Planet[]> SelectedPlanets = (Planet[])null;
        public string SelectedFunc;
        public readonly SimpleObservable<bool> PlotVisible = true;
        public readonly SimpleObservable<Vector2> PlotOffset = new SimpleObservable<Vector2>(default);
        public Vector2 PlotCenter { get => new Vector2(-PlotCenterX, -PlotCenterY); }
    }
}
