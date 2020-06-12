using Godot;
using Nbody.Gui.InputModels;
using Nbody.Gui;
using System.Linq;
using Nbody.Gui.Nodes.Controls;
using Nbody.Gui.Helpers;

namespace Nbody.Gui.src.Nodes.Controls
{
    public class FunctionList : ItemList
    {
        private static readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
        private static readonly FunctionsManager _functionsManager = new FunctionsManager();
        private int _lastSelectedNumerOfPlanets = 0;
        public override void _Ready()
        {
            base._Ready();
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            if (!_plotsModel.PlotVisible || _plotsModel.SelectedPlanets is null)
                return;
            UpdateSelected();
            var selectedPlanetsCount = _plotsModel.SelectedPlanets.Get.Length;
            if (selectedPlanetsCount == _lastSelectedNumerOfPlanets)
                return;
            this.Clear();
            var funcs = _functionsManager.GetFunctions();
            foreach (var func in funcs)
            {
                this.AddItem(func);
            }
            _lastSelectedNumerOfPlanets = selectedPlanetsCount;
        }

        private void UpdateSelected()
        {
            _plotsModel.SelectedFunc = this.GetSelectedItems()
                .Select(i => GetItemText(i))
                .FirstOrDefault();
        }
    }
}
