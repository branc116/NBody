using Nbody.Gui.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.src.Helpers
{
    public class BasicObserverSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Name == nameof(SimpleObservable<int>) + "`1";
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            var val = token.ToObject(objectType.GenericTypeArguments[0]);
            var ctor = objectType.GetConstructor(new[] { objectType.GenericTypeArguments[0] });
            var a = ctor.Invoke(new object[] { val });
            return a;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic a = value;
            writer.WriteValue((object)a.Get);
            return;
        }
    }
}
