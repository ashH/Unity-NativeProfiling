using System.Collections.Generic;

namespace Unity.NativeProfiling
{
    public class SnapdragonProfilerIntegration : NativeTool
    {
        public string Name
        {
            get { return "Snapdragon Profiler"; }
        }

        public string[] RequiredPackages
        {
            get { return null; }
        }

        public IEnumerable<NativeToolPhase> GetPhases()
        {
            yield return null;
        }
    }
}
