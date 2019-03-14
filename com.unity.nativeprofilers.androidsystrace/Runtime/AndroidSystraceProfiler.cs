using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

namespace Unity.NativeProfiling
{
    public class AndroidSystraceProfiler : INativeProfiler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Startup()
        {
            NativeProfiler.RegisterProfiler(new AndroidSystraceProfiler());
        }

        public void BeginMarker(string name)
        {
            SimpleMarkerEnd(name);
        }

        public void EndMarker()
        {
            SimpleMarkerEnd();
        }


        [DllImport("androidstudio")]
        private static extern void SimpleMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("androidstudio")]
        private static extern void SimpleMarkerEnd();
    }
}