using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

namespace Unity.NativeProfiling
{
    public class StreamlineAnalyzerProfiler : INativeProfiler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Startup()
        {
            if (!StreamlineAnalyzerProfiler.TraceIsEnabled())
                return;

            NativeProfiler.RegisterProfiler(new StreamlineAnalyzerProfiler());
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


        [DllImport("streamlineanalyzer")]
        private static extern void TraceInit();

        [DllImport("streamlineanalyzer")]
        private static extern void TraceTerm();

        [DllImport("streamlineanalyzer")]
        private static extern bool TraceIsEnabled();

        [DllImport("streamlineanalyzer")]
        private static extern void TraceMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("streamlineanalyzer")]
        private static extern void TraceMarkerEnd();
    }
}