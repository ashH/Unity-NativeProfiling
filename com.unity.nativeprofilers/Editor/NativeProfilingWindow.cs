using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

//UnityEditor.PackageManager 
//Client.Add/Remove
// https://docs.unity3d.com/ScriptReference/PackageManager.Client.html

namespace Unity.NativeProfiling
{
    public class NativeProfilingWindow : EditorWindow
    {
        [MenuItem("Window/Analysis/Profiling Tools")]
        public static void ShowWindow()
        {
            var wnd = EditorWindow.GetWindow(typeof(NativeProfilingWindow)) as NativeProfilingWindow;
            wnd.titleContent = new GUIContent("DataGrid");
            wnd.minSize = new Vector2(200, 300);
        }

        private string m_ActiveTool = "";
        private NativeTool[] m_Tools = {
            new AndroidStudioIntegration()
        };

        private void OnEnable()
        {
            var root = this.GetRootVisualContainer();
            root.style.flexDirection = FlexDirection.Row;

            root.AddStyleSheetPath("nativeprofiling-style");
            var template = Resources.Load<VisualTreeAsset>("nativeprofiling-template");
            template.CloneTree(root, null);

            var toolSelector = root.Q("group-1").Q<Button>("selector");
            toolSelector.text = "Android";
            toolSelector.RegisterCallback<MouseDownEvent>((mouseEvent) => {});

            UpdateToolsUI();
        }

        private void UpdateToolsUI()
        {
            var root = this.GetRootVisualContainer();

            var buildParams = m_Tools[0].GetPhases().GetEnumerator();
            for (int i = 2; i < 6; ++i)
            {
                var group = root.Q("group-" + i);

                if (buildParams.MoveNext())
                {
                    group.visible = true;
                    group.Q<Label>("header").text = buildParams.Current.name;
                    buildParams.Current.BuildUI(group.Q("content"));
                }
                else
                {
                    group.visible = false;
                }
            }
        }
    }
}
