using Godot;
using Nbody.Gui.InputModels;
using Nbody.Gui.src.Nodes.Controls;
using NBody.Core;
using NBody.Gui;
using NBody.Gui.Extensions;
using NBody.Gui.Nodes.Spatials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.Nodes.Controls
{
    public class LinesDrawer : Node2D   
    {
        private readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
        private readonly FunctionsManager _functionsManager = new FunctionsManager();
        private Vector2[] _points;
        public override void _Process(float delta)
        {
            base._Process(delta);
            if (!_plotsModel.PlotVisible)
                return;
            var meteraial = base.Material as ShaderMaterial;
            meteraial.SetShaderParam("center", SourceOfTruth.PlotCenter);
            meteraial.SetShaderParam("offset", SourceOfTruth.PlotOffset);
            meteraial.SetShaderParam("width", SourceOfTruth.PlotWidth);
            meteraial.SetShaderParam("resolution", SourceOfTruth.PlotResoultion);
            _points = _functionsManager.GetPoints();
            if (_points != null && _points.Length > 2)
                Update();
        }
        public override void _Draw()
        {
            if (_points != null)
            {
                DrawPolyline(_points, Color.Color8(0, 0, 0));
                Console.WriteLine($"MaxX: {_points.Max(i => i.x)} MinX: {_points.Min(i => i.x)}");
            }
            base._Draw();
        }
    }
}
