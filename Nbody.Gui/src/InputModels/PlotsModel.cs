using Godot;
using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.InputModels
{
    public class PlotsModel
    {
        public bool PlotVisible = false;
        public float PlotStepsPerDiv = 1000f;
        public Vector2 PlotCenter = Vector2.Zero;
        public Vector2 PlotOffset;
        public float PlotWidth = 3f;
        public Vector2 PlotResoultion;
        public Planet[] SelectedPlanets;
        public string SelectedFunc;
    }
}
