using Godot;
using NBody.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.Nodes.Controls
{
    public class LinesDrawer : Node2D
    {
        public override void _Process(float delta)
        {
            base._Process(delta);
            (base.Material as ShaderMaterial).SetShaderParam("center", SourceOfTruth.PlotCenter);
            (base.Material as ShaderMaterial).SetShaderParam("offset", SourceOfTruth.PlotOffset);
            (base.Material as ShaderMaterial).SetShaderParam("width", SourceOfTruth.PlotWidth);
            (base.Material as ShaderMaterial).SetShaderParam("resolution", SourceOfTruth.PlotResoultion);
            
        }
        public override void _Draw()
        {
            DrawLine(new Vector2(0f, 0f), new Vector2(500f, 500f), new Color(1f, 0f, 0f));
            base._Draw();
            //VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
        }
    }
}
