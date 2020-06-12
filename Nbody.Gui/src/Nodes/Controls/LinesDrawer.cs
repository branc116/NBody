using Godot;
using Nbody.Gui.InputModels;
using Nbody.Gui;
using Nbody.Gui.Extensions;
using System;
using System.Linq;

namespace Nbody.Gui.Nodes.Controls
{
    public class LinesDrawer : Node2D
    {
        private readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
        private readonly FunctionsManager _functionsManager = new FunctionsManager();
        private Vector2[] _points;
        private ShaderMaterial _material;

        public override void _Ready()
        {
            _material = base.Material as ShaderMaterial;
            _plotsModel.PlotCenterX.RegisterAftersetting(value => 
                _material.SetShaderParam("center", _plotsModel.PlotCenter));
            _plotsModel.PlotCenterY.RegisterAftersetting((val) => 
                _material.SetShaderParam("center", _plotsModel.PlotCenter));
            _plotsModel.PlotOffset.RegisterAftersetting(val =>
                _material.SetShaderParam("offset", val));
            _plotsModel.PlotWidth.RegisterAftersetting(val =>
                _material.SetShaderParam("width", val));
            _plotsModel.PlotResoultion.RegisterAftersetting(val =>
                _material.SetShaderParam("resolution", val));
            _plotsModel.PlotVisible.RegisterAftersetting(val =>
                this.Visible = val);
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            if (!_plotsModel.PlotVisible.Get)
                return;
            if (_plotsModel.XLogScale.Get || _plotsModel.YLogScale.Get)
                _points = _functionsManager.GetPoints()?.Select(i => new Vector2(_plotsModel.XLogScale.Get ? Mathf.Log(i.x) : i.x, _plotsModel.YLogScale.Get ? Mathf.Log(i.y) : i.y)).ToArray();
            else 
                _points = _functionsManager.GetPoints();
            if (_points != null && _points.Length > 2)
            {
                var (min, max) = _points.GetMinMax();
                _plotsModel.Min.Set(min);
                _plotsModel.Max.Set(max);
                if (_plotsModel.Follow.Get)
                {
                    var diff = max - min;
                    _plotsModel.PlotWidth.Set(Math.Max(diff.x * 1.5f, diff.y * 2f));
                    var center = (min + max) / 2;
                    _plotsModel.PlotCenterX.Set(center.x);
                    _plotsModel.PlotCenterY.Set(center.y - _plotsModel.PlotWidth.Get * 0.1f);
                }
                Update();
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
