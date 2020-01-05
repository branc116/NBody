using Godot;
using NBody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
			}else
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
				}catch(Exception ex)
				{
					Console.WriteLine($"Can't parse {_fieldInfo.Name}, {Text} {ex}");
				}
			}
		}
		public override void _Process(float delta)
		{
			if (!SourceOfTruth.IsDebugShown)
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
			if (!SourceOfTruth.IsDebugShown)
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
	public class PropEditAttribute : System.Attribute
	{

	}
	public class PropsEdit : GridContainer
	{
		public override void _Ready()
		{
			var props = GetProps(typeof(SourceOfTruth), () => null).ToList();
			Console.WriteLine($"Number of pros: {props.Count}");
			foreach (var prop in props) 
			{
				this.AddChild(prop);
			};
			this.Columns = 4;
		}
		private IEnumerable<Control> GetProps(Type staticClass, Func<object> obj)
		{
			return staticClass.GetFields()
				.Where(i => i.CustomAttributes.Select(j => j.AttributeType).Log().Any(j => j == typeof(PropEditAttribute)))
				.SelectMany(i =>
				{
					if (!i.FieldType.IsPrimitive && i.FieldType != typeof(string))
					{
						return GetProps(i.FieldType, () => i.GetValue(obj()));
					}
					
					var lable = new Label
					{
						Text = i.Name
					};
					var input = i.FieldType == typeof(bool) ? new DynamicToggleButton(i, obj) as Control : new DynamicLineEdit(i, obj) as Control;
					return new Control[] { lable, input };
				});
		}
		public override void _Process(float delta)
		{

		}
	}
}
