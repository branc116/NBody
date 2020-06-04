using Godot;
using Nbody.Gui.Core;
using System.Collections.Generic;
using System.Reflection;

namespace Nbody.Gui.Helpers
{
    public class ParameterModel
    {

        private ParameterInfo _parameter;
        private MethodInfo _parseMethod;

        public Label Lable { get; set; }
        public Node DynamicInput { get; set; }
        public ParameterInfo Parameter
        {
            get => _parameter; set
            {
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
            else if (parameterInfo.ParameterType == typeof(Point3))
                DynamicInput = new LineEdit { Text = "(0, 0, 0)", SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill };
            else
                DynamicInput = new LineEdit { Text = value?.ToString(), SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill };
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
}
