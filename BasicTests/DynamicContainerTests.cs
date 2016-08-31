using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using System.Reflection;

namespace BasicTests
{
    [TestClass]
    public class DynamicContainerTests
    {
        [TestMethod]
        public void DynamicContainer_Basic_Reflection()
        {
            var props = new List<ContainerPropDefinition>();
            props.Add(new ContainerPropDefinition() { Name = "Prop1", Type = typeof(int) });

            ContainerDefinition def = new ContainerDefinition();
            def.Name = "Foo";
            def.Properties = props.ToArray();

            Type t = ContainerEmmitter.Emit(def);

            dynamic obj = Activator.CreateInstance(t);

            t.GetProperty("Prop1").SetValue(obj, 10, new object[] { });
            Assert.AreEqual(10, t.GetProperty("Prop1").GetValue(obj, new object[] { }));

            Assert.AreEqual("Foo", obj.GetType().Name);
            Assert.AreEqual(1, obj.GetType().GetProperties().Length);            
        }

        [TestMethod]
        public void DynamicContainer_Basic_Props()
        {
            var props = new List<ContainerPropDefinition>();
            props.Add(new ContainerPropDefinition() { Name = "Prop1", Type = typeof(int) });

            ContainerDefinition def = new ContainerDefinition();
            def.Name = "Foo";
            def.Properties = props.ToArray();

            Type t = ContainerEmmitter.Emit(def);

            dynamic obj = Activator.CreateInstance(t);

            obj.Prop1 = 12;
            Assert.AreEqual(12, obj.Prop1);
        }

    }
}
