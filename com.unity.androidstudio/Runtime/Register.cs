using System.Runtime.InteropServices;

namespace Unity.Profiling
{
    public class AndroidStudioSystrace
    {
        [DllImport("androidstudio")]
        public static extern void TraceInit();

        [DllImport("androidstudio")]
        public static extern void TraceTerm();

        [DllImport("androidstudio")]
        public static extern bool TraceIsEnabled();

        [DllImport("androidstudio")]
        public static extern void TraceMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("androidstudio")]
        public static extern void TraceMarkerEnd();
    }
}