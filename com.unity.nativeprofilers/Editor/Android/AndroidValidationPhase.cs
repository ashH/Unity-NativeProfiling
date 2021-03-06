﻿using System;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace Unity.NativeProfiling
{
    public class AndroidValidationPhase : WizardPhase
    {
        public AndroidValidationPhase() : base("Unity project setup")
        {
        }

        public override void Update(VisualElement root)
        {
            base.Update(root);

            var content = root.Q("content");
            var table = AddTable(content);
            MakeRow(table, "Active target - Android", () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android); });
            MakeRow(table, "Gradle Export", () => EditorUserBuildSettings.exportAsGoogleAndroidProject, () => { EditorUserBuildSettings.exportAsGoogleAndroidProject = true; });
            MakeRow(table, "Minification mode", () => EditorUserBuildSettings.androidDebugMinification == AndroidMinification.Proguard, () => { EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Proguard; });
            MakeRow(table, "Development mode", () => EditorUserBuildSettings.development == false, () => { EditorUserBuildSettings.development = false; });
            MakeRow(table, "Scripting Backend", () => PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP, () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); });
            MakeRow(table, "Internet permissions", () => PlayerSettings.Android.forceInternetPermission, () => { PlayerSettings.Android.forceInternetPermission = true; });
            MakeRow(table, "Force SD Card permissions", () => PlayerSettings.Android.forceSDCardPermission, () => { PlayerSettings.Android.forceSDCardPermission = true; });
            MakeRow(table, "Installation location - external", () => PlayerSettings.Android.preferredInstallLocation == AndroidPreferredInstallLocation.PreferExternal, () => { PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal; });
#if UNITY_2017_3_OR_NEWER
            MakeRow(table, "Limit to ARM v7 or 64 targets", () => ((PlayerSettings.Android.targetArchitectures & (AndroidArchitecture.ARMv7|AndroidArchitecture.ARM64)) != 0) && ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.X86) == 0), () => { PlayerSettings.Android.targetArchitectures &= ~AndroidArchitecture.X86; if ((PlayerSettings.Android.targetArchitectures & (AndroidArchitecture.ARMv7|AndroidArchitecture.ARM64)) == 0) PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7; });
#else
            MakeRow(table, "Limit to ARM v7 target", () => { return PlayerSettings.Android.targetDevice == AndroidTargetDevice.ARMv7; }, () => { PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7; } );
#endif
#if UNITY_2018_3_OR_NEWER
            MakeRow(table, "Stripping level", () => PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Low, () => { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low); });
#else
            MakeRow(table, "Stripping level", () => PlayerSettings.strippingLevel == StrippingLevel.Disabled, () => { PlayerSettings.strippingLevel = StrippingLevel.Disabled; } );
#endif
            MakeRow(table, "Engine code stripping", () => !PlayerSettings.stripEngineCode, () => { PlayerSettings.stripEngineCode = false; });
        }

        private VisualElement MakeRow(VisualElement table, string name, Func<bool> check, Action fix)
        {
            var rowRoot = AddTableRow(table);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("nameLabel");

            var statusGroup = new VisualElement();
            statusGroup.AddToClassList("stretchContent");

            var statusLabel = new Label("Good");
            var statusFixButton = new Button(null);

            statusLabel.name = "status";
            statusLabel.style.positionType = PositionType.Absolute;
            statusFixButton.text = "Fix";
            statusFixButton.name = "statusFix";
            statusFixButton.style.positionType = PositionType.Absolute;
            statusFixButton.AddToClassList("compactButton");
            statusFixButton.clickable.clicked += () => {
                fix();
                UpdateStatus(check, statusGroup);
            };

            statusGroup.Add(statusLabel);
            statusGroup.Add(statusFixButton);

            rowRoot.Add(nameLabel);
            rowRoot.Add(statusGroup);

            UpdateStatus(check, statusGroup);

            return rowRoot;
        }

        private void UpdateStatus(Func<bool> check, VisualElement root)
        {
            var status = check();
            root.Q("status").visible = status;
            root.Q("statusFix").visible = !status;
        }
    }
}