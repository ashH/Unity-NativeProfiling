using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

public class NativeProfilerIntegration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private void Integrate()
    {
        var loop = PlayerLoop.GetDefaultPlayerLoop();

        Debug.Log("Root");
        loop.updateDelegate = () => { Debug.Log("Root"); };
        loop = PatchSystem(loop, 1, loop.updateFunction);

        PlayerLoop.SetPlayerLoop(loop);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint PlayerLoopDelegate();

    private PlayerLoopSystem PatchSystem(PlayerLoopSystem system, int level, IntPtr nullFnc)
    {
        if (system.subSystemList == null)
            return system;

        for (int i = 0; i < system.subSystemList.Length; i++)
        {
            var type = system.subSystemList[i].type;
            Debug.Log("L: " + level + " - " + type);

            PlayerLoopDelegate systemDelegate = null;
            if (system.subSystemList[i].updateFunction.ToInt32() != 0)
            {
                var val = Marshal.ReadInt64(system.subSystemList[i].updateFunction);
                if (val != 0)
                {
                    IntPtr intPtr = new IntPtr(val);
                    systemDelegate = (PlayerLoopDelegate)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(PlayerLoopDelegate));
                }
            }

            if (systemDelegate != null)
            {
                system.subSystemList[i].updateDelegate = () => { Debug.Log("L: " + level + " - " + type); systemDelegate(); };
                system.subSystemList[i].updateFunction = nullFnc;
            }

            system.subSystemList[i] = PatchSystem(system.subSystemList[i], level + 1, nullFnc);
        }

        return system;
    }
}
