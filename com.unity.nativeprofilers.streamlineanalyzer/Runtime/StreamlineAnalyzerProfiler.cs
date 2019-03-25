using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

namespace Unity.NativeProfiling
{
    /*
    public class StreamlineAnalyzerProfiler : INativeProfiler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Startup()
        {
            NativeProfiler.RegisterProfiler(new StreamlineAnalyzerProfiler());
        }

        public void BeginMarker(string name)
        {
            SimpleMarkerBegin(name);
        }

        public void EndMarker()
        {
            SimpleMarkerEnd();
        }


        [DllImport("streamlineanalyzer")]
        private static extern void SimpleMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("streamlineanalyzer")]
        private static extern void SimpleMarkerEnd();
    }
    */
}