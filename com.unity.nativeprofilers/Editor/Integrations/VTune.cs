using System.Collections.Generic;

namespace Unity.NativeProfiling
{
    public class VTuneAmplifierIntegration : Wizard
    {
        public string Name
        {
            get { return "VTune Amplifier"; }
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
