using Godot;
using Nbody.Gui.InputModels;
using NBody.Gui;
using System;

public class GraphContainer : PanelContainer
{
    private readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
    private Panel _parent;
    public override void _Ready()
    {
        base._Ready();
        _parent = GetParent<Panel>();
        _parent.Visible = _plotsModel.PlotVisible;
    }
    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseMotion iem && iem.ButtonMask == 1)
        {
            var width = _plotsModel.PlotWidth;
            var move = iem.Relative / (new Vector2(-RectSize.x / width, RectSize.y / (width / (RectSize.x / RectSize.y))));
            _plotsModel.PlotCenterX += move.x;
            _plotsModel.PlotCenterY += move.y;
        }
        if (@event is InputEventMouseButton iemb && (iemb.ButtonMask == 8 || iemb.ButtonMask == 16))
        {
            _plotsModel.PlotWidth *= iemb.ButtonMask == 16 ? 1.1f : 0.90f;
            _plotsModel.DivSize = (float)Math.Pow(10, Math.Floor(Math.Log10(_plotsModel.PlotWidth) - 0.3));
        }
    }
    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        base._UnhandledKeyInput(@event);
        if (!@event.IsPressed() && @event.AsText() == "P")
        {
            _plotsModel.PlotVisible = _parent.Visible = !_parent.Visible;
        }
    }
    public override void _Process(float delta)
    {
        if (!_plotsModel.PlotVisible)
            return;
        _plotsModel.PlotOffset = RectPosition;
        _plotsModel.PlotResoultion = base.RectSize;

        (base.Material as ShaderMaterial).SetShaderParam("center", _plotsModel.PlotCenter);
        (base.Material as ShaderMaterial).SetShaderParam("offset", _plotsModel.PlotOffset);
        (base.Material as ShaderMaterial).SetShaderParam("width", _plotsModel.PlotWidth);
        (base.Material as ShaderMaterial).SetShaderParam("resolution", _plotsModel.PlotResoultion);
        (base.Material as ShaderMaterial).SetShaderParam("modNum", _plotsModel.DivSize);
    }
    public override void _Draw()
    {
        base._Draw();
        VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
    }
}
