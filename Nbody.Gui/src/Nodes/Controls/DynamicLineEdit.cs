using Godot;
using System;
using System.Reflection;

namespace NBody.Gui
{
    public class DynamicLineEdit : LineEdit
    {
        private readonly FieldInfo _fieldInfo;
        private readonly Func<object> _obj;
        private string _oldText;
        public DynamicLineEdit(FieldInfo fieldInfo, Func<object> obj)
        {
            _fieldInfo = fieldInfo;
            _obj = obj;
        }
        public override void _Ready()
        {
            base._Ready();

        }
        private void PropToText()
        {
            string newText = string.Empty;
            if (_fieldInfo.IsStatic)
            {
                newText = _fieldInfo.GetValue(null).ToString();
            }
            else
            {
                var instance = _obj();
                if (instance != null)
                    newText = _fieldInfo.GetValue(instance)?.ToString() ?? "";
            }
            if (_oldText != newText)
                Text = _oldText = newText;
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
                    _fieldInfo.SetValue(instance, _fieldInfo.FieldType.GetMethod("Parse", new[] { typeof(string) }).Invoke(null, new[] { Text }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Can't parse {_fieldInfo.Name}, {Text} {ex}");
                }
            }
        }
        public override void _Process(float delta)
        {
            if (IsVisibleInTree())
                return;
            if (_oldText != Text)
            {
                TextToProp();
                _oldText = Text;
                return;
            }
            //Text = _oldText = _fieldInfo.GetValue(_obj).ToString();
            PropToText();
        }

    }
}
