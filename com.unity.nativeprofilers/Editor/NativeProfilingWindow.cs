using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.PackageManager;


// Tasks:
// - m_ActiveTool - should NativeTool instead of string id
// - Android project props needs to be moved out of Android Studio prep
// - Streamline Analyzer - same as AS integration, just w/o export (build APK) and with different plugin
// - VTune integration - development/release, with plugin and ScriptablePlayerLoop class (depends on build type, as DEFINE, to exclude in Development build)
// - MGD - vulkan/ogl - can I include pre-build .so with plugin?


namespace Unity.NativeProfiling
{
    public class NativeProfilingWindow : EditorWindow
    {
        [MenuItem("Window/Analysis/Profiling Tools")]
        public static void ShowWindow()
        {
            var wnd = EditorWindow.GetWindow(typeof(NativeProfilingWindow)) as NativeProfilingWindow;
            wnd.titleContent = new GUIContent("Native profiling wizard");
            wnd.minSize = new Vector2(200, 300);
        }

        private string m_ActiveTool = "";
        private NativeTool[] m_Tools = {
            new AndroidStudioIntegration(),
            new StreamlineAnalyzerIntegration(),
            new SnapdragonProfilerIntegration(),
            new VTuneAmplifierIntegration()
        };

        private Button m_ToolSelector;

        private void OnEnable()
        {
            var root = this.GetRootVisualContainer();
            root.style.flexDirection = FlexDirection.Row;

            root.AddStyleSheetPath("nativeprofiling-style");
            var template = Resources.Load<VisualTreeAsset>("nativeprofiling-template");
            template.CloneTree(root, null);

            m_ToolSelector = root.Q("group-1").Q<Button>("selector");
            m_ToolSelector.clickable.clickedWithEventInfo += OnToolSelectorMouseDown;

            SetActiveTool("");
        }

        private void OnToolSelectorMouseDown(EventBase evt)
        {
            if (evt.propagationPhase != PropagationPhase.AtTarget)
                return;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), m_ActiveTool == "", () => { SetActiveTool(""); });
            foreach (var tool in m_Tools)
            {
                menu.AddItem(new GUIContent(tool.Name), m_ActiveTool == tool.Name, () => { SetActiveTool(tool.Name); });
            }

            var root = this.GetRootVisualContainer();
            var menuPosition = new Vector2(0, m_ToolSelector.layout.height);
            menuPosition = m_ToolSelector.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            menu.DropDown(menuRect);
        }

        private void SetActiveTool(string toolId)
        {
            // Remove packages
            var prevActiveTool = GetActiveTool();
            if ((prevActiveTool != null) && (prevActiveTool.RequiredPackages != null))
            {
                foreach(var pkg in prevActiveTool.RequiredPackages)
                    Client.Remove(pkg);
            }

            // Set active tool
            m_ActiveTool = toolId;
            m_ToolSelector.text = (m_ActiveTool == "" ? "None" : m_ActiveTool) + " ▾";

            var activeTool = GetActiveTool();

            // Add packages
            if ((activeTool != null) && (activeTool.RequiredPackages != null))
            {
                foreach (var pkg in activeTool.RequiredPackages)
                    Client.Add(pkg);
            }

            // Update UI
            var root = this.GetRootVisualContainer();
            IEnumerator<NativeToolPhase> buildParams = activeTool != null ? activeTool.GetPhases().GetEnumerator() : null;
            for (int i = 2; i < 6; ++i)
            {
                var group = root.Q("group-" + i);

                if ((buildParams != null) && (buildParams.MoveNext()))
                {
                    group.visible = true;
                    group.Q<Label>("header").text = buildParams.Current.name;

                    var content = group.Q("content");
                    content.Clear();
                    buildParams.Current.BuildUI(content);
                }
                else
                {
                    var content = group.Q("content");
                    content.Clear();
                    group.visible = false;
                }
            }
        }

        private NativeTool GetActiveTool()
        {
            foreach (var tool in m_Tools)
            {
                if (tool.Name == m_ActiveTool)
                    return tool;
            }

            return null;
        }
    }
}
