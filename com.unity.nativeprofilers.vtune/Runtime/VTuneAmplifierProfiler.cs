using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

namespace Unity.NativeProfiling
{
    /*
    public class VTuneAmplifierProfiler : INativeProfiler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Startup()
        {
            NativeProfiler.RegisterProfiler(new VTuneAmplifierProfiler());
        }

        public void BeginMarker(string name)
        {
            SimpleMarkerBegin(name);
        }

        public void EndMarker()
        {
            SimpleMarkerEnd();
        }


        [DllImport("vtuneamplifier")]
        private static extern void SimpleMarkerBegin([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("vtuneamplifier")]
        private static extern void SimpleMarkerEnd();
    }
    */
}