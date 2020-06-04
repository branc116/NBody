using Godot;
using Nbody.Gui.Helpers;
using System;
using System.Reflection;

namespace Nbody.Gui.Nodes.Controls
{
    public class DynamicLineEdit : LineEdit
    {
        private readonly FieldInfo _fieldInfo;
        private readonly Func<object> _obj;
        private string _oldText;
        public DynamicLineEdit()
        {
            _fieldInfo = null;
            _obj = null;
            _oldText = null;
        }
        public DynamicLineEdit(FieldInfo fieldInfo, Func<object> obj, bool isEditable = true)
        {
            _fieldInfo = fieldInfo;
            _obj = obj;
            Editable = isEditable;
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
        }
        public override void _Ready()
        {
            base._Ready();

        }
        private void PropToText()
        {
            var newText = string.Empty;
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
                    //Console.WriteLine($"Can't parse {_fieldInfo.Name}, {Text} {ex}");
                }
            }
        }
        public override void _Process(float delta)
        {
            if (HasFocus())
                return;
            if (Editable && _oldText != Text)
            {
                TextToProp();
                _oldText = Text;
                return;
            }
            //Text = _oldText = _fieldInfo.GetValue(_obj).ToString();
            PropToText();
        }

    }
    public class DynamicLineEdit<T> : LineEdit
    {
        private readonly SimpleObservable<T> _observable;
        private string _oldText;
        public DynamicLineEdit(SimpleObservable<T> observable, bool isEditable = true)
        {
            _observable = observable;
            Editable = isEditable;
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _observable.RegisterAftersetting(AfterChange);
            AfterChange(_observable.Get);
        }
        ~DynamicLineEdit()
        {
            _observable.UnrgisterPost(AfterChange);
        }
        private void AfterChange(T val)
        {
            var newValue = val.ToString();
            if (newValue != _oldText)
            {
                _oldText = newValue;
                Text = newValue;
            }
        }
        public void TextToProp()
        {
            if (_observable is SimpleObservable<string> strOb)
            {
                strOb.Set(Text);
            }
            var parsed = _observable.Parse(Text);
            if (parsed is T ok)
            {
                _observable.Set(ok);
            }
        }
        public override void _Process(float delta)
        {
            if (HasFocus())
                return;
            if (Editable && _oldText != Text)
            {
                _oldText = Text;
                TextToProp();
                return;
            }
        }
    }
}
