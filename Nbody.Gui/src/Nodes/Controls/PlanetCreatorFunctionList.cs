using Godot;
using NBody.Gui.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody.Gui.Nodes.Controls
{
    public class PlanetCreatorFunctionList : ItemList
    {
        private readonly List<MethodModel> _methodModels;
        private readonly PlanetCreatorModel _planetCreatorModel = SourceOfTruth.PlanetCreatorModel;
        private int _lastNumerOfPlanetsSelected = -1;
        public PlanetCreatorFunctionList()
        {
            _methodModels = MethodModel.GetAllMethodModels();
        }
        public override void _Process(float delta)
        {
            var numOfPlanetsSelected = _planetCreatorModel.SelectedPlanets?.Length ?? 0;
            if (this.IsAnythingSelected())
                _planetCreatorModel.MethodSelected = this.GetItemText(this.GetSelectedItems().First());
            if (_lastNumerOfPlanetsSelected != numOfPlanetsSelected)
            {
                _lastNumerOfPlanetsSelected = numOfPlanetsSelected;
                base.Clear();
                foreach(var methodName in _methodModels.Where(i => i.PlanetParametes == numOfPlanetsSelected || i.PlanetParametes == -1).Select(i => i.Method.Name))
                {
                    base.AddItem(methodName);
                }

            }
        }
    }
}
