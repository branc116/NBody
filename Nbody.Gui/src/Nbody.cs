using Godot;
using NBody.Core;
using NBody.Gui;
using NBody.Gui.Extensions;

namespace NBody
{
    public class PlanetModel : Spatial, IPlanetFab
    {
        public Planet Planet { get; set; }
        public PlanetModel(Planet planet, Spatial mesh)
        {
            Planet = planet;
            var m = mesh.Duplicate() as Spatial;
            var radius = (float)planet.Radius;
            m.Scale = new Vector3(radius, radius, radius);
            m.Visible = true;
            AddChild(m);
        }
        public void UpdateValue()
        {
            Translation = Planet.Position.ToV3();
        }
    }
    public class PlanetLight : Spatial, IPlanetFab
    {
        public Planet Planet { get; set; }
        public PlanetLight(Spatial lightSpatial)
        {
            var light = lightSpatial.Duplicate() as Spatial;
            this.Visible = false;
            light.Visible = true;
            AddChild(light);
        }
        public void UpdateValue()
        {

            if (SourceOfTruth.ShowLights)
            {
                Transform = new Transform(Basis.Identity, Planet.Position.ToV3())
                    .TargetTo2(Planet.Velocity.ToV3().Normalized(), Vector3.Up);
                this.Visible = true;
            }
            else
            {
                this.Visible = false;
            }
        }
    }
    public class PlanetArrow : Spatial, IPlanetFab
    {
        public Planet Planet { get; set; }
        public PlanetArrow(Planet planet, Spatial mesh)
        {
            Planet = planet;
            var m = mesh.Duplicate() as Spatial;
            var radius = 0.5f;
            m.Scale = new Vector3(radius, radius, radius);
            m.Visible = true;
            AddChild(m);
        }
        public void UpdateValue()
        {
            Transform = new Transform(Basis.Identity, Planet.Position.ToV3())
                .TargetTo2(Planet.Velocity.ToV3().Normalized(), Vector3.Up)
                .Scale2(new Vector3(1f, Mathf.Log(1f + Planet.Velocity.ToV3().LengthSquared()), 1f));

        }
    }
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
            var inputModel = Gui.PlanetSystemInputModel
                .LoadFromFile(Gui.SourceOfTruth.InputFile);
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
