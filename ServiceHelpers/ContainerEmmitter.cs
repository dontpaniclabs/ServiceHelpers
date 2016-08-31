using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;

namespace DontPanic.Helpers
{
    public class ContainerBase : DynamicObject
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prop = GetType().GetProperty(binder.Name);
            if (prop != null && prop.CanRead)
            {
                result = prop.GetValue(this, new object[] { });
                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var prop = GetType().GetProperty(binder.Name);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(this, value, new object[] { });
                return true;
            }

            return base.TrySetMember(binder, value);
        }
    }

    public static class ContainerEmmitter
    {
        private static object _lockObject = new object();
        private static Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public static Type Emit(ContainerDefinition def)
        {
            Type result = null;

            var key = def.Name;

            if (!_types.ContainsKey(key))
            {
                lock (_lockObject)
                {
                    if (!_types.ContainsKey(key))
                    {
                        // setup dynamic assembly, module, and class
                        var assembly = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName("Dynamic." + def.Name), AssemblyBuilderAccess.Run);

                        var module = assembly.DefineDynamicModule("DontPanic.ContainerModule", true);
                        var classBuilder = module.DefineType(def.Name, TypeAttributes.Class, typeof(ContainerBase), new Type[] { });

                        // create properties
                        CreateProperties(classBuilder, def);

                        result = classBuilder.CreateType();                        

                        _types.Add(key, result);
                    }
                    else
                        result = _types[key];
                }
            }
            else
                result = _types[key];

            return result;
        }

        private static void CreateProperties(TypeBuilder classBuilder, ContainerDefinition def)
        {
            foreach (var column in def.Properties)
            {
                var field = classBuilder.DefineField("_" + column.Name, column.Type, FieldAttributes.Private | FieldAttributes.HasDefault);

                var prop = classBuilder.DefineProperty(column.Name, PropertyAttributes.HasDefault, column.Type, new Type[] { });

                MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                // create getter
                var getDef = classBuilder.DefineMethod("get_" + column.Name, attributes, column.Type, Type.EmptyTypes);
                var getIL = getDef.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, field as FieldInfo);
                getIL.Emit(OpCodes.Ret);

                prop.SetGetMethod(getDef);

                // create setter
                var setDef = classBuilder.DefineMethod("set_" + column.Name, attributes, null, new Type[] { column.Type } );
                var setIL = setDef.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, field as FieldInfo);
                setIL.Emit(OpCodes.Ret);

                prop.SetSetMethod(setDef);
            }
        }
        
    }

    public class ContainerDefinition
    {
        public string Name { get; set; }
        public ContainerPropDefinition[] Properties { get; set; }
    }

    public class ContainerPropDefinition
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
