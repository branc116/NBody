using Godot;
using System;
using System.Reflection;

namespace NBody.Gui
{
    public class DynamicToggleButton : CheckBox
    {
        private readonly FieldInfo _fieldInfo;
        private readonly Func<object> _obj;
        private bool? _oldToggle;
        public DynamicToggleButton(FieldInfo fieldInfo, Func<object> obj)
        {
            _fieldInfo = fieldInfo;
            _obj = obj;
        }
        public override void _Ready()
        {
            base._Ready();
            ToggleMode = true;
        }
        private void PropToText()
        {
            bool? newText = null;
            if (_fieldInfo.IsStatic)
            {
                newText = _fieldInfo.GetValue(null) as bool?;
            }
            else
            {
                var instance = _obj();
                if (instance != null)
                    newText = _fieldInfo.GetValue(instance) as bool?;
            }
            if (_oldToggle != newText)
                Pressed = (_oldToggle = newText) ?? false;
        }
        public void TextToProp()
        {
            var instance = _fieldInfo.IsStatic ? null : _obj();
            if (_fieldInfo.IsStatic || instance != null)
            {
                if (_fieldInfo.FieldType == typeof(string))
                {
                    _fieldInfo.SetValue(instance, Text);
                }
                try
                {
                    _fieldInfo.SetValue(instance, base.Pressed);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Can't parse {_fieldInfo.Name}, {Text} {ex}");
                }
            }
        }
        public override void _Process(float delta)
        {
            if (!IsVisibleInTree())
                return;
            if (_oldToggle != Pressed)
            {
                TextToProp();
                _oldToggle = Pressed;
                return;
            }
            //Text = _oldText = _fieldInfo.GetValue(_obj).ToString();
            PropToText();
        }
    }
}
