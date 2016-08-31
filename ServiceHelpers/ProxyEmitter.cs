using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.Security.Cryptography;

namespace DontPanic.Helpers
{
    public static class ProxyEmitter
    {
        private static object _lockObject = new object();
        private static Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public static Type EmitProxy(string proxyMethodName, Type interfaceType, string endpointOverrideAddress)
        {
            Type result = null;

            string className = null;
            var key = interfaceType.FullName + "#" + proxyMethodName;
            // Add hashed endpoint override address to the key to keep a cached version for each endpoint address
            if (!string.IsNullOrEmpty(endpointOverrideAddress))
            {
                var endpointAddressHash = GetMD5(endpointOverrideAddress);
                key += "#" + endpointAddressHash;
                className = interfaceType.Name + "Proxy" + endpointAddressHash;
            }
            else
            {
                className = interfaceType.Name + "Proxy";
            }

            if (!_types.ContainsKey(key))
            {
                lock (_lockObject)
                {
                    if (!_types.ContainsKey(key))
                    {
                        // setup dynamic assembly, module, and class
                        var assembly = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName("Dynamic." + interfaceType.Name), AssemblyBuilderAccess.Run);
                        var module = assembly.DefineDynamicModule("DontPanic.ProxyModule", true);
                        
                        var classBuilder = module.DefineType(className, TypeAttributes.Class, typeof(object), new Type[] { interfaceType });

                        // add a facctory field to proxy
                        var factory = classBuilder.DefineField("Factory", typeof(IProxyFactory), FieldAttributes.Public);

                        // implement inteface
                        ImplementInterface(interfaceType, interfaceType, classBuilder, proxyMethodName, factory, endpointOverrideAddress);

                        // if interface inherits from other interfaces
                        var baseInterfaces = interfaceType.GetInterfaces();
                        foreach (var baseInterface in baseInterfaces)
                        {
                            ImplementInterface(interfaceType, baseInterface, classBuilder, proxyMethodName, factory, endpointOverrideAddress);
                        }

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

        private static void ImplementInterface(Type rootInterface, Type interfaceType, TypeBuilder classBuilder, string proxyMethodName, FieldBuilder factory, string endpointOverrideAddress)
        {
            if (interfaceType.GetProperties() != null && interfaceType.GetProperties().Count() > 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "Unable to create dynamic proxy for interface {0}. WCF interfaces (contracts) cannot have properties.",
                    interfaceType.Name));
            }

            var methods = interfaceType.GetMethods();
            foreach (var interfaceMethod in methods)
            {
                GenerateMethod(rootInterface, interfaceType, classBuilder, interfaceMethod, factory, proxyMethodName, endpointOverrideAddress);
            }
        }

        private static void GenerateMethod(Type rootInterface, Type interfaceType, TypeBuilder classBuilder, MethodInfo interfaceMethod, FieldBuilder factory, string proxyMethodName, string endpointOverrideAddress)
        {
            var methodParameters = interfaceMethod.GetParameters().Select(s => s.ParameterType).ToArray();
            var methodDef = classBuilder.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                interfaceMethod.ReturnType, methodParameters);

            // because we implement an interface with this method we need to mark it as overriding
            classBuilder.DefineMethodOverride(methodDef, interfaceMethod);

            // generate IL for the method
            GenerateMethodBody(rootInterface, interfaceType, classBuilder, interfaceMethod, factory, methodDef, proxyMethodName, endpointOverrideAddress);
        }

        private static void GenerateMethodBody(Type rootInterface, Type interfaceType, TypeBuilder classBuider, MethodInfo interfaceMethod, FieldBuilder factory,
            MethodBuilder methodDef, string proxyMethodName, string endpointOverrideAddress)
        {
            var proxyMethod = typeof(IProxyFactory).GetMethod(proxyMethodName);
            var genericProxyMethod = proxyMethod.MakeGenericMethod(new Type[] { rootInterface });
            var parameters = interfaceMethod.GetParameters();
            var gen = methodDef.GetILGenerator();

            // declare locals
            gen.DeclareLocal(typeof(string));
            gen.DeclareLocal(typeof(object[]));
            gen.DeclareLocal(typeof(string));

            // create array
            gen.Emit(OpCodes.Ldc_I4_S, parameters.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            gen.Emit(OpCodes.Stloc_1);

            // create parameters
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];

                // reload array
                gen.Emit(OpCodes.Ldloc_1); // load array
                gen.Emit(OpCodes.Ldc_I4_S, i); // set position in array
                gen.Emit(OpCodes.Ldarg_S, i + 1); // load argument passed into this method
                // box if necessary
                if (p.ParameterType.IsValueType)
                    gen.Emit(OpCodes.Box, p.ParameterType);
                // stick into array element                
                gen.Emit(OpCodes.Stelem_Ref);
            }

            // call method
            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(OpCodes.Ldfld, factory); // factory property
            gen.Emit(OpCodes.Ldstr, interfaceMethod.Name);
            gen.Emit(OpCodes.Ldloc_1); // load array to use as argument
            // Load endpoint address to pass, or null if it is empty
            if (endpointOverrideAddress == null)
            {
                gen.Emit(OpCodes.Ldnull);
            }
            else
            {
                gen.Emit(OpCodes.Ldstr, endpointOverrideAddress);

            }
            gen.Emit(OpCodes.Callvirt, genericProxyMethod); // call the "CallMethod"

            // return
            if (interfaceMethod.ReturnType != typeof(void))
            {
                if (interfaceMethod.ReturnType.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox_Any, interfaceMethod.ReturnType);
                }
                else
                    gen.Emit(OpCodes.Isinst, interfaceMethod.ReturnType);
                gen.Emit(OpCodes.Ret);
            }
            else
            {
                gen.Emit(OpCodes.Pop);
                gen.Emit(OpCodes.Ret);
            }
        }

        private static string GetMD5(string text)
        {
            var UE = new UnicodeEncoding();
            var message = UE.GetBytes(text);

            var hashString = new MD5CryptoServiceProvider();
            var hex = string.Empty;

            var hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }
    }
}
