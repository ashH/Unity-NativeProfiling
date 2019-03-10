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
            if (!AndroidSystraceProfiler.TraceIsEnabled())
                return;

            NativeProfiler.RegisterProfiler(new AndroidSystraceProfiler());
        }

        public void Init()
        {
            TraceInit();
        }

        public void Term()
        {
            TraceTerm();
        }

        public void BeginMarker(string name)
        {
            TraceMarkerBegin(name);
        }

        public void EndMarker()
        {
            TraceMarkerEnd();
        }


        [DllImport("androidstudio")]
        private static extern void TraceInit();

        [DllImport("androidstudio")]
        private static extern void TraceTerm();

        [DllImport("androidstudio")]
        private static extern bool TraceIsEnabled();

        [DllImport("androidstudio")]
        private static extern void TraceMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("androidstudio")]
        private static extern void TraceMarkerEnd();
    }
}