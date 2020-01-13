using Godot;
using NBody.Core;
using NBody.Gui.Attributes;

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
        public float PlotStepsPerDiv = 1000f;
        [PropEdit]
        public float DivSize = 1f;
        [PropEdit]
        public godot_real_t PlotCenterX = 0;
        [PropEdit]
        public godot_real_t PlotCenterY = 0;
        [PropEdit]
        public float PlotWidth = 3f;
        [PropEdit(Editable = false)]
        public Vector2 Min;
        [PropEdit(Editable = false)]
        public Vector2 Max;
        [PropEdit]
        public bool Follow = false;

        public Vector2 PlotResoultion;
        public Planet[] SelectedPlanets;
        public string SelectedFunc;
        public bool PlotVisible = false;
        public Vector2 PlotOffset;
        public Vector2 PlotCenter { get => new Vector2(-PlotCenterX, -PlotCenterY); }
    }
}
