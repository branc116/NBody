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
