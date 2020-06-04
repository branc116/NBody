using Godot;
using Nbody.Core;
using Nbody.Gui.Extensions;
using Nbody.Gui.InputModels;

namespace Nbody.Gui.Nodes.Spatials
{
    public class Nbody : Spatial
    {
        private readonly SimulationModel _simulationModel = SourceOfTruth.SimulationModel;
        private Spatial _arrowSpatial;
        private Spatial _globalArrow;
        public Nbody()
        {
            _simulationModel.RestartRequested.RegisterAftersetting(val =>
            {
                if (val)
                {
                    _Ready();
                    _simulationModel.RestartRequested.Set(false);
                }
            });
        }
        public override void _Ready()
        {
#if REAL_T_IS_DOUBLE
            System.Console.WriteLine("Using doubles");
#endif
            _arrowSpatial = _arrowSpatial is null ? this.GetNode<Spatial>(new NodePath("ArrowSpatial")) : _arrowSpatial;
            _globalArrow = _globalArrow is null ? _arrowSpatial.Duplicate() as Spatial : _globalArrow;
            AddChild(_globalArrow);
            var inputModel = PlanetSystemInputModel
                .LoadFromFile(_simulationModel.InputFile);
            Gui.SourceOfTruth.System = inputModel
                ?.ToPlanetSystem() ?? new PlanetSystem();
            
        }
        public override void _UnhandledKeyInput(InputEventKey @event)
        {
            if (@event.IsPressed())
                return;
            var str = @event.AsText();
            if (@event.Control && str == "R")
            {
                _simulationModel.RestartRequested.Set(true);
            }
        }
        public override void _Process(float delta)
        {
            int times = _simulationModel.StepsPerFrame;
            var system = SourceOfTruth.System;
            if (!_simulationModel.Paused)
            {
                if (_simulationModel.UseOpenCl)
                    system.StepCL(times);
                else
                    system.Step(times);
                UpdateArrow(system);
            }
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
