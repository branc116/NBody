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
            var width = SourceOfTruth.PlotWidth;
            SourceOfTruth.PlotCenter -= iem.Relative / (new Vector2(-RectSize.x / width, RectSize.y / (width / (RectSize.x / RectSize.y))));
        }
        if (@event is InputEventMouseButton iemb && (iemb.ButtonMask == 8 || iemb.ButtonMask == 16))
        {
            SourceOfTruth.PlotWidth *= iemb.ButtonMask == 16 ? 1.1f : 0.90f;
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
        SourceOfTruth.PlotOffset = RectPosition;
        SourceOfTruth.PlotResoultion = base.RectSize;
        (base.Material as ShaderMaterial).SetShaderParam("center", SourceOfTruth.PlotCenter);
        (base.Material as ShaderMaterial).SetShaderParam("offset", SourceOfTruth.PlotOffset);
        (base.Material as ShaderMaterial).SetShaderParam("width", SourceOfTruth.PlotWidth);
        (base.Material as ShaderMaterial).SetShaderParam("resolution", SourceOfTruth.PlotResoultion);
        (base.Material as ShaderMaterial).SetShaderParam("modNum", Math.Pow(10, Math.Floor(Math.Log10(SourceOfTruth.PlotWidth) - 0.3)));
    }
    public override void _Draw()
    {
        base._Draw();
        VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
    }
}
