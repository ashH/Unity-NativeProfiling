using System.Runtime.InteropServices;

namespace Unity.Profiling
{
    public class StreamlineAnalyzer
    {
        [DllImport("streamlineanalyzer")]
        public static extern void TraceInit();

        [DllImport("streamlineanalyzer")]
        public static extern void TraceTerm();

        [DllImport("streamlineanalyzer")]
        public static extern bool TraceIsEnabled();

        [DllImport("streamlineanalyzer")]
        public static extern void TraceMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("streamlineanalyzer")]
        public static extern void TraceMarkerEnd();
    }
}