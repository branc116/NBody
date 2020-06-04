using Godot;
using Nbody.Gui.Helpers;
using Nbody.Gui.Attributes;
using Nbody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nbody.Gui.Nodes.Controls
{
    public class PropsEdit : GridContainer
    {
        [Export]
        public string PropGroup { get; set; }
        public override void _Ready()
        {
            var name = PropGroup ?? Name;
            var props = GetProps(typeof(SourceOfTruth), () => null, name).ToList();
            Console.WriteLine($"Number of pros: {props.Count}");
            foreach (var prop in props)
            {
                AddChild(prop);
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
                    var isSimpleObservable = i.FieldType.Name == typeof(SimpleObservable<int>).Name;

                    var lable = new Label
                    {
                        Text = i.Name
                    };
                    if (isSimpleObservable)
                    {
                        var internalType = i.FieldType.GenericTypeArguments[0];

                        var ctr = i.GetValue(obj());
                        if (ctr is SimpleObservable<bool> sw)
                        {
                            return new Control[] { lable, new DynamicToggleButtonObservable(sw) };
                        }
                        else if (ctr is null)
                        {
                            return new Control[] { };
                        }
                        else
                        {
                            var control = typeof(DynamicLineEdit<>)
                                .MakeGenericType(internalType)
                                .GetConstructor(new Type[] { ctr.GetType(), typeof(bool) })
                                .Invoke(new object[] { ctr, atr.Editable }) as Control;
                            return new Control[] { lable, control };
                        }
                    }
                    if (!i.FieldType.IsPrimitive && i.FieldType != typeof(string) && !i.FieldType.IsValueType)
                    {
                        return GetProps(i.FieldType, () => i.GetValue(obj()));
                    }

                    Control input = i.FieldType switch
                    {
                        { } x when x == typeof(bool) => new DynamicToggleButton(i, obj),
                        _ => new DynamicLineEdit(i, obj, atr.Editable)
                    }; // == typeof(bool) ? new DynamicToggleButton(i, obj) as Control : new DynamicLineEdit(i, obj, atr.Editable) as Control;
                    return new Control[] { lable, input };
                });
        }
    }
}
