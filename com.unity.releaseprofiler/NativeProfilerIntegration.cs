using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

namespace Unity.Profiling
{
    public class NativeProfilerIntegration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static private void Integrate()
        {
            NativeProfiler.TraceInit();

            var loop = PlayerLoop.GetDefaultPlayerLoop();

            loop = PatchSystem(loop, 0, loop.updateFunction);

            PlayerLoop.SetPlayerLoop(loop);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint PlayerLoopDelegate();

        static private int m_LastLevel = 0;

        static private void TraceMarker(int level, string name)
        {
            for (int i = level; i <= m_LastLevel; i++)
                NativeProfiler.TraceEnd();

            NativeProfiler.TraceBegin(level.ToString() + "|" + name);
            m_LastLevel = level;
        }

        static private PlayerLoopSystem PatchSystem(PlayerLoopSystem system, int level, IntPtr nullFnc)
        {
            PlayerLoopDelegate systemDelegate = null;
            if (system.updateFunction.ToInt64() != 0)
            {
                var intPtr = Marshal.ReadIntPtr(system.updateFunction);
                if (intPtr.ToInt64() != 0)
                    systemDelegate = (PlayerLoopDelegate)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(PlayerLoopDelegate));
            }

            var type = system.type;
            system.updateDelegate = () => { TraceMarker(level, type.Name); if (systemDelegate != null) systemDelegate(); };
            system.updateFunction = nullFnc;

            if (system.subSystemList == null)
                return system;

            for (int i = 0; i < system.subSystemList.Length; i++)
                system.subSystemList[i] = PatchSystem(system.subSystemList[i], level + 1, nullFnc);

            return system;
        }
    }
}