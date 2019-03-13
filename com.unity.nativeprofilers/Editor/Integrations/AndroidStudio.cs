using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Experimental.UIElements;

namespace Unity.NativeProfiling
{
    public class AndroidStudioIntegration : Wizard
    {
        public static readonly string kAndroidDebugInfoPostprocessorKey = "AndroidDebugInfoPostprocessorEnabled";

        public string Name
        {
            get { return "Android Studio"; }
        }

        public string[] RequiredPackages
        {
            get { return null; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return new AndroidValidationPhase();
            yield return new BuildPostprocessPhase();
#if UNITY_2018_2_OR_NEWER
            yield return new AndroidDeviceCheckPhase();
#endif
            yield return new TextWizardPhase("Instructions", 
                "Open exported Gradle project in Android Studio and start profiler.\nFor Android Studio setup and guides click on _this link_", 
                "https://docs.google.com/document/d/17WJQZyT4PSSumEZvyvDlpAfC0qZER_vRqmkhrelU6k4/edit?usp=sharing");

        }

        private class BuildPostprocessPhase : WizardPhase
        {
            public BuildPostprocessPhase() : base("Build post processor")
            {
            }

            public override void Update(VisualElement root)
            {
                base.Update(root);

                var enablePP = new Button(null);
                enablePP.clickable.clicked += () => { UpdateStatus(!EditorPrefs.GetBool(kAndroidDebugInfoPostprocessorKey, false), enablePP); };
                root.Q("content").Add(enablePP);

                UpdateStatus(EditorPrefs.GetBool(kAndroidDebugInfoPostprocessorKey, false), enablePP);
            }

            private void UpdateStatus(bool status, Button button)
            {
                EditorPrefs.SetBool(kAndroidDebugInfoPostprocessorKey, status);
                button.text = status ? "Disabled" : "Enabled";
            }
        }
    }
}
