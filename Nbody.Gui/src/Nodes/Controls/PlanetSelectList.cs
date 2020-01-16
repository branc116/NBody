using Godot;
using NBody.Gui.InputModels;
using NBody.Core;
using NBody.Gui;
using NBody.Gui.Controllers;
using NBody.Gui.InputModels;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NBody.Gui.Nodes.Controls
{
    public class PlanetSelectList : ItemList
    {
        private readonly PlanetFabController _planetFabController = new PlanetFabController();
        private readonly PlotsModel _plotModel = SourceOfTruth.PlotModel;
        private readonly PlanetCreatorModel _planetCreatorModel = SourceOfTruth.PlanetCreatorModel;
        private int _lastSelected = -1;
        private DateTime _lastUpdated = DateTime.Now.AddDays(-1);
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
            if (!IsVisibleInTree())
                return;
            var arr = base.IsAnythingSelected() ? base.GetSelectedItems()
                    .Select(i => _planetFabController[i])
                    .ToArray() : Enumerable.Empty<Planet>().ToArray();
            if (Name == "CreatorPlanetsList")
                _planetCreatorModel.SelectedPlanets = arr;
            else if (Name == "PlotsPlanetsList")
                _plotModel.SelectedPlanets = arr;

            var lastchange = SourceOfTruth.System.PlanetNamesLastChanged;
            if (_lastUpdated >= lastchange)
                return;
            _lastUpdated = lastchange;
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
            
        }
    }
}
