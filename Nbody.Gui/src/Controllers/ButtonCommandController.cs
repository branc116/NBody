using Godot;
using Nbody.Gui.src.Attributes;
using NBody.Gui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.src.Controllers
{
    public class ButtonCommandController
    {
        private readonly Dictionary<string, Action<Node>> _commands;
        public ButtonCommandController()
        {
            _commands = typeof(ButtonCommandController).Assembly.GetTypes()
                .Where(i => i.CustomAttributes.Any(j => j.AttributeType == typeof(ButtonCommandAttribute)))
                .SelectMany(i =>
                {
                    var controllerName = (i.GetCustomAttributes(true).FirstOrDefault(j => j is ButtonCommandAttribute) as ButtonCommandAttribute).Name;
                    var instance = i.GetConstructor(new Type[] { }).Invoke(new object[] { });
                    return i.GetMethods()
                        .Where(j => {
                            var param = j.GetParameters();
                            return param.Length == 1 && (param[0].ParameterType == typeof(Node) || param[0].ParameterType.IsSubclassOf(typeof(Node)));
                        })
                        //.Where(j => j.CustomAttributes.Any(k => k.AttributeType == typeof(ButtonCommandAttribute)))
                        .Select(j =>
                        {
                            var methodName = (j.GetCustomAttributes(true).FirstOrDefault(k => k is ButtonCommandAttribute) as ButtonCommandAttribute)?.Name;
                            methodName = methodName ?? j.Name;
                            void action(Node node)
                            {
                                j.Invoke(instance, new[] { node });
                            }
                            return ($"{controllerName}{methodName}", (Action<Node>)action);
                        });

                })
                .Log()
                .ToDictionary(i => i.Item1, i => i.Item2);
        }
        public void Do(string name, Node node)
        {
            _commands[name](node);
        }
    }
}
