// Example of how of what the Emitter will generate for a contract

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DontPanic.Helpers;

namespace BasicTests
{

    public interface IContractExample
    {
        string TestMe(string input);
    }

    public class ContractExampleProxy : ServiceBase, IContractExample
    {
        public string TestMe(string input)
        {            
            return Factory.CallMethod<IContractExample>(
                "TestMe", new object[] { input }, null) as string;
        }
    }

    /*
 
.method public final hidebysig newslot virtual 
    instance string TestMe (
        string input
    ) cil managed 
{
    .locals init (
        [0] string CS$1$0000,
        [1] object[] CS$0$0001
    )

    IL_0000: nop
    IL_0001: ldarg.0
    IL_0002: call instance class [DontPanic.ServiceHelpers]DontPanic.Helpers.IProxyFactory [DontPanic.ServiceHelpers]DontPanic.Helpers.ServiceBase::get_Factory()
    IL_0007: ldstr "TestMe"
    IL_000c: ldc.i4.1
    IL_000d: newarr [mscorlib]System.Object
    IL_0012: stloc.1
    IL_0013: ldloc.1
    IL_0014: ldc.i4.0
    IL_0015: ldarg.1
    IL_0016: stelem.ref
    IL_0017: ldloc.1
    IL_0018: callvirt instance object [DontPanic.ServiceHelpers]DontPanic.Helpers.IProxyFactory::CallMethod<class BasicTests.IContractExample>(string,  object[])
    IL_001d: isinst [mscorlib]System.String
    IL_0022: stloc.0
    IL_0023: br.s IL_0025

    IL_0025: ldloc.0
    IL_0026: ret
}     
     */
}
