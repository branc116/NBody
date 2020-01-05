using Godot;
using NBody.Core;
using NBody.Gui;
using NBody.Gui.Controllers;
using NBody.Gui.Extensions;
using NBody.Gui.InputModels;

namespace NBody.Gui.Nodes.Spatials
{
    public class Nbody : Spatial
    {
        private readonly PlanetFabController _fabController = new PlanetFabController();
        private Spatial _planetMesh;
        private Spatial _arrowSpatial;
        private Spatial _lightSpatial;
        private Spatial _globalArrow;
        private int _stepsPerFrame;
        public override void _Ready()
        {
            _planetMesh = _planetMesh is null ? this.GetNode<Spatial>(new NodePath("Spatial")) : _planetMesh;
            _arrowSpatial = _arrowSpatial is null ? this.GetNode<Spatial>(new NodePath("ArrowSpatial")) : _arrowSpatial;
            _lightSpatial = _lightSpatial is null ? this.GetNode<Spatial>(new NodePath("PlanetLightSpatial")) : _lightSpatial;
            _globalArrow = _globalArrow is null ? _arrowSpatial.Duplicate() as Spatial : _globalArrow;
            AddChild(_globalArrow);
            var inputModel = PlanetSystemInputModel
                .LoadFromFile(SourceOfTruth.InputFile);
            if (SourceOfTruth.RestartStepPerFrame)
                SourceOfTruth.StepsPerFrame = inputModel.StepsPerFrame;
            Gui.SourceOfTruth.System = inputModel
                .ToPlanetSystem();
        }
        public override void _UnhandledKeyInput(InputEventKey @event)
        {
            if (@event.IsPressed())
                return;
            var str = @event.AsText();
            if (str == "R")
            {
                _Ready();
            }
            else if (str == "L")
            {
                SourceOfTruth.ShowLights = !SourceOfTruth.ShowLights;
            }
        }
        public override void _Process(float delta)
        {
            int times = SourceOfTruth.StepsPerFrame;
            var system = SourceOfTruth.System;
            if (SourceOfTruth.RestartRequested)
            {
                _Ready();
                SourceOfTruth.RestartRequested = false;
            }
            if (!SourceOfTruth.Paused)
            {
                do
                {
                    system.Step();
                } while (times-- > 0);
            }
            //_fabController.DeleteOld(system, this);
            //_fabController.UpdateExisiting(system, this);
            //_fabController.AddNew(system, this, (planet) =>
            //{
            //	return new List<IPlanetFab>()
            //	{
            //		//new PlanetModel(planet, _planetMesh),
            //		new PlanetLight(_lightSpatial)
            //		{
            //			Planet = planet
            //		},
            //		new PlanetArrow(planet, _arrowSpatial)
            //	};
            //});
            UpdateArrow(system);
        }
        private void UpdateArrow(PlanetSystem system)
        {

            var massCenter = system.MassCenter().ToV3();
            var totalMom = system.TotalMomentum()
                .ToV3();
            var trans = new Transform(Basis.Identity, massCenter)
                .TargetTo2(totalMom.Normalized(), Vector3.Up)
                .Scale2(new Vector3(1f, Mathf.Log(1 + totalMom.Length()), 1f));
            _globalArrow.Transform = trans;
            _globalArrow.Visible = true;
            return;
        }
    }
}
