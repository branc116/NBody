using Godot;
using NBody.Core;
using NBody.Gui.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NBody.Gui.Nodes.Controls
{

    public class MethodModel
    {
        public MethodInfo Method { get; }
        public List<ParameterModel> ParameterModels { get; }
        public int PlanetParametes { get; }
        public object CallingObject { get; }
        public MethodModel(MethodInfo methodInfo, object callingObject)
        {
            this.CallingObject = callingObject;
            var ps = methodInfo.GetParameters();
            ParameterModels = ps
                .Where(i => i.ParameterType != typeof(Planet) && i.ParameterType != typeof(Planet[]))
                .Select(i => new ParameterModel(i))
                .ToList();

            PlanetParametes = ps.Length == 1 && ps[0].ParameterType == typeof(Planet[]) ? -1 : ps.Length;
        }
        public void Execute(Planet[] planets)
        {
            Method.Invoke(CallingObject, GetParameterValues(planets).ToArray());
        }
        public IEnumerable<object> GetParameterValues(Planet[] planets)
        {
            foreach (var planet in planets)
                yield return planet;
            foreach (var par in ParameterModels.Select(i => i.GetValue()))
                yield return par;
        }
        public IEnumerable<Node> GetNodes()
        {
            foreach(var param in ParameterModels.SelectMany(i => i.GetNodes()))
            {
                yield return param;
            }
        }
        public static List<MethodModel> GetAllMethodModels()
        {
            return typeof(PlanetCreatorParameters).Assembly
                .GetTypes()
                .SelectMany(mm)
                .ToList();
        }
        private static IEnumerable<MethodModel> mm(Type i)
        {
            var instance = i.GetConstructor(new Type[] { })?.Invoke(new object[] { }) ?? null;
            if (instance is null) yield break;
            foreach (var meth in i.GetMethods().Where(j => j.GetBaseDefinition().ReflectedType == j.ReflectedType).Select(j => new MethodModel(j, instance)))
                    yield return meth;
        }
    }
    public class ParameterModel
    {

        private ParameterInfo _parameter;
        private MethodInfo _parseMethod;

        public Label Lable { get; set; }
        public Node DynamicInput { get; set; }
        public ParameterInfo Parameter { get => _parameter; set { 
                _parameter = value;
                _parseMethod = Parameter.ParameterType.GetMethod("Parse", new[] { typeof(string) });
            } 
        }
        public ParameterModel(ParameterInfo parameterInfo)
        {
            Parameter = parameterInfo;
            var value = parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue : null;
            Lable = new Label { Text = parameterInfo.Name };
            if (parameterInfo.ParameterType == typeof(bool))
                DynamicInput = new CheckBox { Pressed = value is bool b ? b : false };
            else
                DynamicInput = new LineEdit { Text = value?.ToString() };
        }
        public object GetValue()
        {
            if (DynamicInput is CheckBox cb)
                return cb.Pressed;
            if (Parameter.ParameterType == typeof(string) && DynamicInput is LineEdit le)
                return le.Text;
            if (DynamicInput is LineEdit le1)
                return _parseMethod.Invoke(null, new object[] { le1.Text });
            else return null;
        }
        public IEnumerable<Node> GetNodes()
        {
            yield return Lable;
            yield return DynamicInput;
        }
    }
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
                var method = Get(selectedPlanets.Length, selectedMethod);
                method.Execute(selectedPlanets);
            };
        }
        private MethodModel Get(int n, string name) =>
            _methodModels.FirstOrDefault(i => i.Method.Name == name && n == i.PlanetParametes) ?? _methodModels.FirstOrDefault(i => i.Method.Name == name && -1 == i.PlanetParametes);
        public override void _Process(float delta)
        {
            if (_planetCreatorModel.SelectedPlanets.Length == _lastNumerOfPlanetsSelected && _lastMethodSelected == _planetCreatorModel.MethodSelected)
                return;
            _lastNumerOfPlanetsSelected = _planetCreatorModel.SelectedPlanets.Length;
            _lastMethodSelected = _planetCreatorModel.MethodSelected;
            if (!string.IsNullOrEmpty(_lastMethodSelected))
                return;
            foreach (var child in base.GetChildren())
                base.RemoveChild(child as Node);
            var method = Get(_lastNumerOfPlanetsSelected, _lastMethodSelected);
            if (method is MethodModel mm)
            {
                foreach (var node in mm.GetNodes())
                    base.AddChild(node);
            }
        }
    }
}
