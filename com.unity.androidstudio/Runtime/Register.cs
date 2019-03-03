using System.Runtime.InteropServices;

namespace Unity.Profiling
{
    public class AndroidStudioSystrace
    {
        [DllImport("libandroidstudio")]
        public static extern void TraceInit();

        [DllImport("libandroidstudio")]
        public static extern void TraceTerm();

        [DllImport("libandroidstudio")]
        public static extern bool TraceIsEnabled();

        [DllImport("libandroidstudio")]
        public static extern void TraceMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("libandroidstudio")]
        public static extern void TraceMarkerEnd();
    }
}