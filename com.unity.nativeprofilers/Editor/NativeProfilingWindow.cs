using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.PackageManager;


// Tasks:
// - Save last selected tool
// - Colors for enabled/disabled post-processor
// - Streamline Analyzer - same as AS integration, just w/o export (build APK) and with different plugin
// Streamline doesn't need post-processor, as it can load symbold from separate files
// - VTune integration - development/release, with plugin and ScriptablePlayerLoop class (depends on build type, as DEFINE, to exclude in Development build)
// - MGD - vulkan/ogl - can I include pre-build .so with plugin?

namespace Unity.NativeProfiling
{
    public class NativeProfilingWindow : EditorWindow
    {
        private Button m_ToolSelector;
        private List<VisualElement> m_Phases = new List<VisualElement>();

        private Wizard m_ActiveTool = null;
        private Wizard[] m_Tools = {
            new AndroidStudioIntegration(),
            new StreamlineAnalyzerIntegration(),
            new SnapdragonProfilerIntegration(),
            new VTuneAmplifierIntegration()
        };


        [MenuItem("Window/Analysis/Profiling Tools")]
        public static void ShowWindow()
        {
            var wnd = EditorWindow.GetWindow(typeof(NativeProfilingWindow)) as NativeProfilingWindow;
            wnd.titleContent = new GUIContent("Native profiling wizard");
            wnd.minSize = new Vector2(200, 300);
        }

        private void OnEnable()
        {
            var root = this.GetRootVisualContainer();
            root.style.flexDirection = FlexDirection.Row;

            root.AddStyleSheetPath("nativeprofiling-style");
            var template = Resources.Load<VisualTreeAsset>("nativeprofiling-template");
            template.CloneTree(root, null);

            m_ToolSelector = root.Q("toolSelector").Q<Button>("selector");
            m_ToolSelector.clickable.clicked += OnToolSelectorMouseDown;

            SetActiveTool(null);
        }

        private void OnToolSelectorMouseDown()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), m_ActiveTool == null, () => { SetActiveTool(null); });
            foreach (var tool in m_Tools)
            {
                menu.AddItem(new GUIContent(tool.Name), m_ActiveTool == tool, () => { SetActiveTool(tool); });
            }

            // Show dropdown menu
            var menuPosition = new Vector2(0, m_ToolSelector.layout.height);
            menuPosition = m_ToolSelector.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            menu.DropDown(menuRect);
        }

        private void SetActiveTool(Wizard wizard)
        {
            // Clear old wizard UI
            foreach (var phase in m_Phases)
            {
                phase.RemoveFromHierarchy();
            }
            m_Phases.Clear();

            // Remove packages
            if ((m_ActiveTool != null) && (m_ActiveTool.RequiredPackages != null))
            {
                foreach(var pkg in m_ActiveTool.RequiredPackages)
                    Client.Remove(pkg);
            }

            // Set active tool
            m_ActiveTool = wizard;
            m_ToolSelector.text = (m_ActiveTool == null ? "None" : m_ActiveTool.Name) + " ▾";

            if (m_ActiveTool == null)
                return;

            // Add packages
            if (m_ActiveTool.RequiredPackages != null)
            {
                foreach (var pkg in m_ActiveTool.RequiredPackages)
                    Client.Add(pkg);
            }

            // Generate UI for wizard phases
            var root = this.GetRootVisualContainer().Q("phasesView");
            if (m_ActiveTool.GetPhases() != null)
            {
                int counter = 2;
                foreach (var phase in m_ActiveTool.GetPhases())
                {
                    var phaseGroup = new VisualElement();
                    phaseGroup.AddToClassList("wizardGroup");
                    phaseGroup.AddToClassList("horizontalGroup");
                    phase.SetPhase(counter);
                    phase.Update(phaseGroup);

                    counter++;

                    root.Add(phaseGroup);
                    m_Phases.Add(phaseGroup);
                }
            }
        }
    }
}
