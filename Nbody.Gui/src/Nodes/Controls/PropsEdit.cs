using Godot;
using NBody.Gui.Attributes;
using NBody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBody.Gui
{
    public class PropsEdit : GridContainer
    {
        public override void _Ready()
        {
            var name = this.Name;
            var props = GetProps(typeof(SourceOfTruth), () => null, name).ToList();
            Console.WriteLine($"Number of pros: {props.Count}");
            foreach (var prop in props)
            {
                this.AddChild(prop);
            };
            //this.Columns = 4;
        }
        private IEnumerable<Control> GetProps(Type staticClass, Func<object> obj, string name = default)
        {
            return staticClass.GetFields()
                .Where(i => i.CustomAttributes.Select(j => j.AttributeType).Any(j => j == typeof(PropEditAttribute)))
                .Where(i =>
                {
                    if (name == null)
                        return true;
                    return i.GetCustomAttributes(typeof(PropEditAttribute), true).Cast<PropEditAttribute>().First().Name?.Contains(name) ?? true;
                })
                .SelectMany(i =>
                {
                    var atr = i.GetCustomAttributes(typeof(PropEditAttribute), true).Cast<PropEditAttribute>().First();
                    if (!i.FieldType.IsPrimitive && i.FieldType != typeof(string) && !i.FieldType.IsValueType)
                    {
                        return GetProps(i.FieldType, () => i.GetValue(obj()));
                    }

                    var lable = new Label
                    {
                        Text = i.Name
                    };
                    var input = i.FieldType == typeof(bool) ? new DynamicToggleButton(i, obj) as Control : new DynamicLineEdit(i, obj, atr.Editable) as Control;
                    return new Control[] { lable, input };
                });
        }
        public override void _Process(float delta)
        {

        }
    }
}
