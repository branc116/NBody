using Godot;
using Nbody.Gui.Helpers;
using Nbody.Gui.Extensions;
using Nbody.Gui.InputModels;
using System.Collections.Generic;
using System.Linq;

namespace Nbody.Gui.Nodes.Controls
{
    public class PlanetCreatorParameters : GridContainer
    {
        private readonly List<MethodModel> _methodModels;
        private readonly PlanetCreatorModel _planetCreatorModel = SourceOfTruth.PlanetCreatorModel;
        private int _lastNumerOfPlanetsSelected = -1;
        private string _lastMethodSelected = null;
        public PlanetCreatorParameters()
        {
            _methodModels = MethodModel.GetAllMethodModels();

            _planetCreatorModel.DoIt = () =>
            {
                var selectedMethod = _planetCreatorModel.MethodSelected;
                var selectedPlanets = _planetCreatorModel.SelectedPlanets;
                var method = Get(selectedPlanets?.Get?.Length ?? 0, selectedMethod);
                method.Execute(selectedPlanets);
            };
        }
        private MethodModel Get(int n, string name) =>
            _methodModels.FirstOrDefault(i => i.Method.Name == name && n == i.PlanetParametes) ?? _methodModels.FirstOrDefault(i => i.Method.Name == name && -1 == i.PlanetParametes);
        public override void _Process(float delta)
        {
            var newLength = _planetCreatorModel.SelectedPlanets?.Get?.Length ?? 0;
            if (newLength == _lastNumerOfPlanetsSelected && _lastMethodSelected == _planetCreatorModel.MethodSelected)
                return;
            _lastNumerOfPlanetsSelected = newLength;
            _lastMethodSelected = _planetCreatorModel.MethodSelected;
            if (string.IsNullOrEmpty(_lastMethodSelected))
                return;
            foreach (var child in GetChildren())
                RemoveChild(child as Node);
            var method = Get(_lastNumerOfPlanetsSelected, _lastMethodSelected);
            if (method is MethodModel mm)
            {
                foreach (var node in mm.GetNodes())
                    AddChild(node);
            }
        }
    }
}
