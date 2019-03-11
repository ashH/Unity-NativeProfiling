using System;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace Unity.NativeProfiling
{
    public interface NativeToolPhase
    {
        string name { get; }

        void BuildUI(VisualElement root);
    }

    public class ValidationCollectionPhase : NativeToolPhase
    {
        public struct ValidationParam
        {
            public string Name;
            public Func<bool> Check;
            public Action Fix;

            public ValidationParam(string _name, Func<bool> _check, Action _fix)
            {
                Name = _name;
                Check = _check;
                Fix = _fix;
            }
        }

        private string m_Name;
        private ValidationParam[] m_ValidationParams;

        public ValidationCollectionPhase(string _name, ValidationParam[] _validationParams)
        {
            m_Name = _name;
            m_ValidationParams = _validationParams;
        }

        public string name
        {
            get
            {
                return m_Name;
            }
        }

        public void BuildUI(VisualElement root)
        {
            foreach (var i in m_ValidationParams)
            {
                var nameLabel = new Label(i.Name);
                nameLabel.AddToClassList("name-label");

                var statusGroup = new VisualElement();
                var statusLabel = new Label("Good");
                var statusFixButton = new Button();

                statusLabel.name = "status";
                statusLabel.style.positionType = PositionType.Absolute;
                statusFixButton.text = "Fix";
                statusFixButton.name = "status-fix";
                statusFixButton.style.positionType = PositionType.Absolute;
                statusFixButton.AddToClassList("compact-button");
                statusFixButton.clickable.clicked += () => { i.Fix(); UpdateStatus(i, statusGroup); };

                statusGroup.Add(statusLabel);
                statusGroup.Add(statusFixButton);

                var separator = new VisualElement();
                separator.AddToClassList("horizontal-separator");
                root.Add(separator);

                var paramGroup = new VisualElement();
                paramGroup.AddToClassList("vertical-group");
                paramGroup.Add(nameLabel);
                paramGroup.Add(statusGroup);
                root.Add(paramGroup);

                UpdateStatus(i, statusGroup);
            }
        }

        private void UpdateStatus(ValidationParam param, VisualElement root)
        {
            var status = param.Check();
            root.Q("status").visible = status;
            root.Q("status-fix").visible = !status;
        }
    }
    public class InstructionPhase : NativeToolPhase
    {
        public string name
        {
            get
            {
                return "Instructions";
            }
        }

        public void BuildUI(VisualElement root)
        {
            var label = new Label("Here goes instruction set with link to document");
            label.AddToClassList("link");
            root.Add(label);
        }
    }

    public interface NativeTool
    {
        string Name { get; }
        string[] RequiredPackages { get; }

        IEnumerable<NativeToolPhase> GetPhases();
    }
}