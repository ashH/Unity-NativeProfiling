using System.Collections.Generic;

namespace Unity.NativeProfiling
{
    public class VTuneAmplifierIntegration : NativeTool
    {
        public string Name
        {
            get { return "VTune Amplifier"; }
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
