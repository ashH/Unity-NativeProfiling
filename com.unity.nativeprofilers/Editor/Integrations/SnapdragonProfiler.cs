using System.Collections.Generic;

namespace Unity.NativeProfiling
{
    public class SnapdragonProfilerIntegration : Wizard
    {
        public string Name
        {
            get { return "Snapdragon Profiler"; }
        }

        public string[] RequiredPackages
        {
            get { return null; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return null;
        }
    }
}
