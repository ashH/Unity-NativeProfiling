using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;

namespace Unity.NativeProfiling
{
    public class StreamlineAnalyzerIntegration : Wizard
    {
        public string Name
        {
            get { return "Arm Streamline Analyzer"; }
        }

        public string[] RequiredPackages
        {
            get { return null; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return new AndroidValidationPhase();
//            yield return new BuildPostprocessPhase();
            yield return new AndroidDeviceCheckPhase();
            yield return new TextWizardPhase("", "");
        }
    }
}
