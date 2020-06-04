using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nbody.Gui.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Tests
{
    [TestClass]
    public class SerializerTest
    {
        public class TestClass<T>
        {
            public SimpleObservable<T> TestInt { get; set; }
        }
        [TestMethod]
        public void Test_SimpleObserverSerialize_long()
        {
            var json = @"
{
    'TestInt': 1
}";
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass<long>>(json);
            Assert.AreEqual(1, obj.TestInt.Get);
        }
        [TestMethod]
        public void Test_SimpleObserverSerialize_double()
        {
            var json = @"
{
    'TestInt': 1
}";
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass<double>>(json);
            Assert.AreEqual(1, obj.TestInt.Get);
        }
        [TestMethod]
        public void Test_SimpleObserverSerialize_float()
        {
            var json = @"
{
    'TestInt': 1
}";
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass<float>>(json);
            Assert.AreEqual(1f, obj.TestInt.Get);
        }
        [TestMethod]
        public void Test_SimpleObserverSerialize_complex()
        {
            var json = @"
{
    'TestInt': {
        'TestInt': 1
    }
}";
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass<TestClass<float>>>(json);
            Assert.AreEqual(1f, obj.TestInt.Get.TestInt.Get);
        }
    }
    [TestClass]
    public class DeserializeTest
    {
        public class TestClass<T>
        {
            public SimpleObservable<T> TestT { get; set; }
        }
        [TestMethod]
        public void Test_SimpleObserverSerialize_int()
        {
            var value = new TestClass<int> { TestT = 1 };
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            Assert.AreEqual("{\"TestT\":1}", str);
        }
    }
}
