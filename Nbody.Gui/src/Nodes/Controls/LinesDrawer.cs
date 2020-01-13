using Godot;
using Nbody.Gui.InputModels;
using Nbody.Gui.src.Nodes.Controls;
using NBody.Gui;
using NBody.Gui.Extensions;
using System;

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
            meteraial.SetShaderParam("center", _plotsModel.PlotCenter);
            meteraial.SetShaderParam("offset", _plotsModel.PlotOffset);
            meteraial.SetShaderParam("width", _plotsModel.PlotWidth);
            meteraial.SetShaderParam("resolution", _plotsModel.PlotResoultion);
            _points = _functionsManager.GetPoints();
            if (_points != null && _points.Length > 2)
            {
                Update();
                var (min, max) = _points.GetMinMax();
                _plotsModel.Min = min;
                _plotsModel.Max = max;
                if (_plotsModel.Follow)
                {
                    var diff = max - min;
                    _plotsModel.PlotWidth = Math.Max(diff.x * 1.5f, diff.y * 2f);
                    var center = (min + max) / 2;
                    _plotsModel.PlotCenterX = center.x;
                    _plotsModel.PlotCenterY = center.y - _plotsModel.PlotWidth * 0.1f;
                }
            }
        }
        public override void _Draw()
        {
            if (_points != null)
            {
                DrawPolyline(_points, Color.Color8(0, 0, 0, 255));
            }
            base._Draw();
        }
    }
}
