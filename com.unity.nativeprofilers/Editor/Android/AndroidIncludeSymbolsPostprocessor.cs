using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_2_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Unity.NativeProfiling
{
#if UNITY_2018_2_OR_NEWER
    public class AndroidIncludeSymbolsPostprocessor : IPostprocessBuildWithReport
#else
    public class AndroidIncludeSymbolsPostprocessor : IPostprocessBuild
#endif
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

#if UNITY_2018_2_OR_NEWER
        public void OnPostprocessBuild(BuildReport report)
        {
            PatchBuild(report.summary.platform, report.summary.outputPath);
        }
#else
        public void OnPostprocessBuild(BuildTarget target, string outputPath)
        {
            PatchBuild(target, outputPath);
        }
#endif

        protected void PatchBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.Android)
                return;
//            if (!EditorPrefs.GetBool(AndroidProfilingWindow.kAndroidDebugInfoPostprocessorKey, false))
//                return;

            Debug.Log("Build: post-processing your build for profiling. Disable it for non-profiling builds!");

            // Target folder
            var buildPath = Path.Combine(path, PlayerSettings.productName);

            // Default to ARMv7 and not ARM64, as 64-bit support doesn't exist prior to 2017 LTS
            bool buildingARMv7 = true;
            bool buildingARM64 = false;
#if UNITY_2017_3_OR_NEWER
            buildingARMv7 = ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) != 0);
            buildingARM64 = ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0);
#endif
            var armv7aFolder = "armeabi-v7a";
            var armv64Folder = "arm64-v8a";

            // Copy unstripped libil2cpp from staging area
            var projectPath = Directory.GetCurrentDirectory();
            var libIl2cppTarget = "src/main/jniLibs/{0}/libil2cpp.so";
            var libIl2cppSource = "Temp/StagingArea/symbols/{0}/libil2cpp.so.debug";
            if (buildingARMv7)
            {
                var targetPath = Path.Combine(buildPath, String.Format(libIl2cppTarget, armv7aFolder));
                var sourcePath = String.Format(libIl2cppSource, armv7aFolder);
                CopyFile(new string[] { projectPath }, sourcePath, targetPath);
            }
            if (buildingARM64)
            {
                var targetPath = Path.Combine(buildPath, String.Format(libIl2cppTarget, armv64Folder));
                var sourcePath = String.Format(libIl2cppSource, armv64Folder);
                CopyFile(new string[] { projectPath }, sourcePath, targetPath);
            }

            // Copy unstripped libunity.so from Unity Editor folder
            // Source folder can be different, depends on platform and they way Unity was built
            var pbeLocations = new string[]
            {
            Directory.GetParent(EditorApplication.applicationPath).ToString(),
            EditorApplication.applicationPath,
            EditorApplication.applicationContentsPath,
            };

            var libUnityTarget = "src/main/jniLibs/{0}/libunity.so";
            var libUnitySource = "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Development/Libs/{0}/libunity.so";
            if (buildingARMv7)
            {
                var targetPath = Path.Combine(buildPath, String.Format(libUnityTarget, armv7aFolder));
                var sourcePath = String.Format(libUnitySource, armv7aFolder);
                CopyFile(pbeLocations, sourcePath, targetPath);
            }
            if (buildingARM64)
            {
                var targetPath = Path.Combine(buildPath, String.Format(libUnityTarget, armv64Folder));
                var sourcePath = String.Format(libUnitySource, armv64Folder);
                CopyFile(pbeLocations, sourcePath, targetPath);
            }
        }

        private bool CopyFile(string[] baseSrc, string src, string dst)
        {
            bool success = false;
            foreach (var i in baseSrc)
            {
                try
                {
                    File.Copy(Path.Combine(i, src), dst, true);
                    success = true;
                    break;
                }
                catch (Exception)
                {
                }
            }

            if (!success)
            {
                Debug.LogError(string.Format("Failed to copy {0} -> {1}", src, dst));
                return false;
            }

            return true;
        }
    }
}