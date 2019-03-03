using System.Runtime.InteropServices;

namespace Unity.Profiling
{
    public class StreamlineAnalyzer
    {
        [DllImport("libstreamlineanalyzer")]
        public static extern void TraceInit();

        [DllImport("libstreamlineanalyzer")]
        public static extern void TraceTerm();

        [DllImport("libstreamlineanalyzer")]
        public static extern bool TraceIsEnabled();

        [DllImport("libstreamlineanalyzer")]
        public static extern void TraceMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("libstreamlineanalyzer")]
        public static extern void TraceMarkerEnd();
    }
}