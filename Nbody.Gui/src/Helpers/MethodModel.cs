using Godot;
using Nbody.Core;
using Nbody.Gui.Attributes;
using Nbody.Gui.Nodes.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nbody.Gui.Helpers
{
    public class MethodModel
    {
        public MethodInfo Method { get; }
        public List<ParameterModel> ParameterModels { get; }
        public int PlanetParametes { get; }
        public object CallingObject { get; }
        public MethodModel(MethodInfo methodInfo, object callingObject)
        {
            CallingObject = callingObject;
            var ps = methodInfo.GetParameters();
            ParameterModels = ps
                .Where(i => i.ParameterType != typeof(Planet) && i.ParameterType != typeof(Planet[]))
                .Select(i => new ParameterModel(i))
                .ToList();
            Method = methodInfo;
            PlanetParametes = ps.Length == 1 && ps[0].ParameterType == typeof(Planet[]) ? -1 : ps.TakeWhile(i => i.ParameterType == typeof(Planet)).Count();
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
            foreach (var param in ParameterModels.SelectMany(i => i.GetNodes()))
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
        private static IEnumerable<MethodModel> mm(Type type)
        {
            var hasAttribute = type.CustomAttributes.Any(j => j.AttributeType == typeof(PlanetCreatorAttribute));
            if (!hasAttribute)
                yield break;
            var instance = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) ?? null;
            if (instance is null) yield break;
            foreach (var meth in type.GetMethods().Where(j =>
            {
                var ret = j.GetBaseDefinition().ReflectedType == j.ReflectedType;
                if (!ret)
                    return false;
                var nonValidParams = j.GetParameters()
                    .SkipWhile(k => k.ParameterType == typeof(Planet))
                    .Where(k => k.ParameterType != typeof(bool) &&
                        k.ParameterType != typeof(string) &&
                        k.ParameterType.GetMethod("Parse", new Type[] { typeof(string) }) is null)
                    .ToList();
                if (nonValidParams.Any())
                    Console.WriteLine(nonValidParams.Select(k => k.ParameterType.Name).Aggregate((ii, jj) => $"{ii}, {jj}"));
                return !nonValidParams.Any();
            }).Select(j => new MethodModel(j, instance)))
                yield return meth;
        }
    }
}
