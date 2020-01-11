using Godot;
using Nbody.Gui.InputModels;
using NBody.Gui;
using NBody.Gui.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.Nodes.Controls
{
    public class PlanetSelectList : ItemList
    {
        private readonly PlanetFabController _planetFabController = new PlanetFabController();
        private readonly PlotsModel _plotModel = SourceOfTruth.PlotModel;
        private int _lastSelected = -1;
        public override void _Ready()
        {
            base._Ready();
        }
        public new void AddChild(Node node, bool legibleuniquename = false)
        {
            if (node is PlanetLable pl)
                AddItem(pl.Text);
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            if (!_plotModel.PlotVisible)
                return;
            _planetFabController.DeleteOld(SourceOfTruth.System, this);
            _planetFabController.AddNew(SourceOfTruth.System, this, (planet) =>
            {
                return new List<IPlanetFab>()
                {
                    new PlanetLable(planet, DisplayPlanetProperty.Name)
                    {
                        ControlledBy = this
                    }
                };
            });
            _planetFabController.UpdateExisiting(SourceOfTruth.System, this);
            
            if (base.IsAnythingSelected())
            {
                _plotModel.SelectedPlanets = base.GetSelectedItems()
                    .Select(i => _planetFabController[i])
                    .ToArray();
                //var selected = base.GetSelectedItems().First();
                //if (selected != _lastSelected)
                //{
                //    SourceOfTruth.SelectedPlanet = _planetFabController[selected];
                //    _lastSelected = selected;
                //}
            }
        }
    }
}
