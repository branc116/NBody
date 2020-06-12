using Godot;
using Nbody.Gui.InputModels;
using Nbody.Core;
using Nbody.Gui.Controllers;
using System.Collections.Generic;
using System.Linq;
using System;
using Nbody.Gui.src.Extensions;
using Nbody.Gui.Helpers;

namespace Nbody.Gui.Nodes.Controls
{
    public class PlanetSelectList : ItemList
    {
        private readonly PlanetFabController _planetFabController = new PlanetFabController();
        private readonly PlotsModel _plotModel = SourceOfTruth.PlotModel;
        private readonly PlanetCreatorModel _planetCreatorModel = SourceOfTruth.PlanetCreatorModel;
        private readonly PlanetInfoModel _planetInfoModel = SourceOfTruth.PlanetInfoModel;
        private readonly SimpleObservable<PlanetSystem> _planetSystem = SourceOfTruth.System;
        private int _lastSelected = -1;
        private DateTime _lastUpdated = DateTime.Now.AddDays(-1);
        [Export]
        public string PlanetListName { get; set; }

        public override void _Ready()
        {
            SourceOfTruth.PlanetCollection.Add((PlanetListName ?? Name, new Nbody.Gui.Helpers.SimpleObservable<Planet[]>(new Planet[1])));
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
            SourceOfTruth.PlanetCollection.Get(PlanetListName ?? Name).Set(arr);
            if (Name == "CreatorPlanetsList")
                _planetCreatorModel.SelectedPlanets.Set(arr);
            else if (Name == "PlotsPlanetsList")
                _plotModel.SelectedPlanets.Set(arr);
            else if (Name == "PlanetInfo" && arr.Length > 0)
                _planetInfoModel.SelectedPlanet = arr[0];

            var lastchange = _planetSystem.Get.PlanetNamesLastChanged;
            if (_lastUpdated >= lastchange)
                return;
            _lastUpdated = lastchange;
            _planetFabController.DeleteOld(SourceOfTruth.System.Get, this);
            _planetFabController.AddNew(SourceOfTruth.System.Get, this, (planet) =>
            {
                return new List<IPlanetFab>()
                {
                    new PlanetLable(planet, DisplayPlanetProperty.Name)
                    {
                        ControlledBy = this
                    }
                };
            });
            _planetFabController.UpdateExisiting(SourceOfTruth.System.Get, this);
            
        }

    }
}
