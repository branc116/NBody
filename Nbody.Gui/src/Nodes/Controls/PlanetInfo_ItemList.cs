using Godot;
using NBody.Gui.InputModels;
using System;
using System.Linq;

namespace NBody.Gui.Nodes.Controls
{
    public class PlanetInfo_ItemList : ItemList
    {
        private readonly PlanetInfoModel _planetInfoModel = SourceOfTruth.PlanetInfoModel;

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            var selectedPlanet = _planetInfoModel.SelectedPlanet;
            if (selectedPlanet is null || !this.IsVisibleInTree())
                return;
            this.Clear();
            foreach(var info in selectedPlanet.GetPlanetInfo().Select(i => $"{i.name}: {i.value}"))
            {
                this.AddItem(info, selectable: false);
            }
        }
    }
}
