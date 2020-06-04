using Godot;
using Nbody.Gui.InputModels;
using System;

namespace Nbody.Gui.Nodes.Controls
{
    public class GraphContainer : PanelContainer
    {
        private readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
        private Panel _parent;
        private ShaderMaterial _material;

        public override void _Ready()
        {
            base._Ready();
            _parent = GetParent<Panel>();
            _parent.Visible = _plotsModel.PlotVisible.Get;
            _material = Material as ShaderMaterial;
            _plotsModel.PlotCenterX.RegisterAftersetting(value => _material.SetShaderParam("center", _plotsModel.PlotCenter));
            _plotsModel.PlotCenterY.RegisterAftersetting((val) => _material.SetShaderParam("center", _plotsModel.PlotCenter));
            _plotsModel.DivSize.RegisterAftersetting((val) => _material.SetShaderParam("modNum", val));
            _plotsModel.PlotWidth.RegisterAftersetting((val) =>
            {
                _material.SetShaderParam("width", val);
                _plotsModel.DivSize.Set((float)Math.Pow(10, Math.Floor(Math.Log10(val) - 0.3)));
            });
            _plotsModel.PlotVisible.RegisterAftersetting(val => _parent.Visible = val);
            _plotsModel.PlotResoultion.RegisterAftersetting(val => _material.SetShaderParam("resolution", val));
            _plotsModel.PlotOffset.RegisterAftersetting(val => _material.SetShaderParam("offset", val));
        }
        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);
            if (@event is InputEventMouseMotion iem && iem.ButtonMask == 1)
            {
                var width = _plotsModel.PlotWidth.Get;
                var move = iem.Relative / new Vector2(-RectSize.x / width, RectSize.y / (width / (RectSize.x / RectSize.y)));
                _plotsModel.PlotCenterX.Set(_plotsModel.PlotCenterX + move.x);
                _plotsModel.PlotCenterY.Set(_plotsModel.PlotCenterY + move.y);
            }
            if (@event is InputEventMouseButton iemb && (iemb.ButtonMask == 8 || iemb.ButtonMask == 16))
            {
                _plotsModel.PlotWidth.Set(_plotsModel.PlotWidth.Get * (iemb.ButtonMask == 16 ? 1.1f : 0.90f));
                //_plotsModel.DivSize.Set((float)Math.Pow(10, Math.Floor(Math.Log10(_plotsModel.PlotWidth) - 0.3)));
            }
        }
        public override void _UnhandledKeyInput(InputEventKey @event)
        {
            base._UnhandledKeyInput(@event);
            if (!@event.IsPressed() && @event.AsText() == "P")
            {
                _plotsModel.PlotVisible.Set(!_parent.Visible);
            }
        }
        public override void _Process(float delta)
        {
            if (!_plotsModel.PlotVisible)
                return;
            _plotsModel.PlotOffset.Set(RectPosition - RectPivotOffset);
            _plotsModel.PlotResoultion.Set(RectSize);

            //(base.Material as ShaderMaterial).SetShaderParam("center", _plotsModel.PlotCenter);
            //(base.Material as ShaderMaterial).SetShaderParam("resolution", _plotsModel.PlotResoultion);
            //(base.Material as ShaderMaterial).SetShaderParam("offset", _plotsModel.PlotOffset);
            //(base.Material as ShaderMaterial).SetShaderParam("width", _plotsModel.PlotWidth.Get);
            //(base.Material as ShaderMaterial).SetShaderParam("modNum", _plotsModel.DivSize.Get);
        }
        public override void _Draw()
        {
            base._Draw();
            VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
        }
    }
}