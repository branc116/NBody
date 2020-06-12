using Godot;
using Nbody.Gui.Attributes;
using Nbody.Gui.InputModels;
using Nbody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Nbody.Gui.Helpers;

namespace Nbody.Gui.Nodes.Controls
{
    public class FunctionsManager
    {
        private static readonly PlotsModel _plotsModel = SourceOfTruth.PlotModel;
        private readonly Dictionary<int, Dictionary<string, Func<Planet[], Vector2[]>>> _dict;
        private readonly SimpleObservable<PlanetSystem> _system = SourceOfTruth.System;
        public FunctionsManager()
        {
            _dict = typeof(FunctionsManager).Assembly.GetTypes().Where(i => i.CustomAttributes.Any(j => j.AttributeType == typeof(PlotFunctionAttribute)))
                .SelectMany(i =>
                {
                    var instance = i.GetConstructor(new Type[] { }).Invoke(new Type[] { });
                    var methods = i.GetMethods()
                        .Where(j => j.GetParameters().All(k => k.ParameterType == typeof(Planet) || k.ParameterType == typeof(Planet[])))
                        .GroupBy(j =>
                        {
                            var paramz = j.GetParameters();
                            if (paramz.Length == 1 && paramz[0].ParameterType == typeof(Planet[]))
                                return -1;
                            return paramz.Length;
                        });
                    return methods;
                }).ToDictionary(i => i.Key, j =>
                {
                    return j.Select(i =>
                    {
                        var instance = i.DeclaringType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                        var tuple = (Name: i.DeclaringType.Name + '.' + i.Name, (Func<Planet[], Vector2[]>)((planets) =>
                        {
                            return (Vector2[])i.Invoke(instance, planets);
                        }));
                        return tuple;
                    }).ToDictionary(i => i.Name, i => i.Item2);
                });
        }
        public Vector2[] GetPoints()
        {
            var selectdPlanets = _plotsModel.SelectedPlanets;
            var selectedFunc = _plotsModel.SelectedFunc;
            if (selectdPlanets is null || selectedFunc is null)
                return null;
            if (_dict.ContainsKey(selectdPlanets.Get.Length) && _dict[selectdPlanets.Get.Length].ContainsKey(_plotsModel.SelectedFunc))
            {
                return _dict[selectdPlanets.Get.Length][selectedFunc](selectdPlanets.Get);
            }
            if (_dict.ContainsKey(-1) && _dict[-1].ContainsKey(_plotsModel.SelectedFunc))
            {
                return _dict[-1][selectedFunc](selectdPlanets.Get);
            }
            return null;
        }
        public IEnumerable<string> GetFunctionsWithNParams(int n)
        {
            return _dict.ContainsKey(n) ? _dict[n].Keys : Enumerable.Empty<string>();
        }
        public string[] GetFunctions()
        {
            var selectedCount = _plotsModel.SelectedPlanets.Get.Length;
            var functions = GetFunctionsWithNParams(selectedCount)
                .Union(GetFunctionsWithNParams(-1))
                .ToArray();
            return functions;
        }
    }
}
