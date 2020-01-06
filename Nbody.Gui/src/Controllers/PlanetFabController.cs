﻿using Godot;
using NBody.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBody.Gui.Controllers
{
    public class PlanetFabController
    {
        public bool ShowSubset = false;
        public int TakeMax = 5;
        public int progress = 0;
        public Dictionary<string, List<IPlanetFab>> Planets { get; set; } = new Dictionary<string, List<IPlanetFab>>();
        List<string> _planetNames = new List<string>();
        public void DeleteOld(PlanetSystem system, Node parent)
        {
            Planets.Where(i => !system.HasPlanet(i.Value.First().Planet))
                .Select(i => i.Key)
                .ToList()
                .ForEach(i =>
                {
                    Planets[i].ForEach(j => j.QueueFree());
                    Planets.Remove(i);
                    _planetNames.RemoveAll(j => j == i);
                });
        }
        public void UpdateExisiting(PlanetSystem system, Node parent)
        {
            if (ShowSubset)
            {
                for (int i = 0; i < Planets.Count; i++)
                {
                    var planet = Planets[_planetNames[i]];
                    if (i >= progress * Planets.Count / 100 && i < (progress * Planets.Count / 100) + TakeMax)
                    {
                        planet.ForEach(j =>
                        {
                            j.UpdateValue();
                            j.Show();
                        });
                    }
                    else
                    {
                        planet.ForEach(j =>
                        {
                            j.Hide();
                        });
                    }
                }
            }
            else
            {
                foreach (var lab in Planets)
                {
                    lab.Value.ForEach(i => i.UpdateValue());
                }
            }
        }
        public void AddNew(PlanetSystem system, Node parent, Func<Planet, List<IPlanetFab>> getFabs)
        {
            system.Planets.Where(i => !Planets.Any(j => j.Value.First().Planet == i))
                .ToList()
                .ForEach(i =>
                {
                    var newLables = getFabs(i);
                    Planets.Add(i.Name, newLables);
                    _planetNames.Add(i.Name);
                    newLables.ForEach(j =>
                    {
                        parent.AddChild(j as Node);
                    });
                });
        }
    }
}