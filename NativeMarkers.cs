using System.Runtime.InteropServices;

namespace Unity.Profiling
{
    public class NativeProfiler
    {
#if ENABLE_IL2CPP
        [DllImport("__Internal")]
        public static extern void TraceInit();

        [DllImport("__Internal")]
        public static extern void TraceBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("__Internal")]
        public static extern void TraceEnd();
#else
        public static void TraceInit()
        {
        }

        public static void TraceBegin(string str)
        {
            UnityEngine.Debug.Log("S|" + str);
        }

        public static void TraceEnd()
        {
            UnityEngine.Debug.Log("E");
        }
#endif
}
}