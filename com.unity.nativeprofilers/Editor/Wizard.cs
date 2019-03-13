using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Unity.NativeProfiling
{
    /*
     * Wizard setup with multiple phases
     */

    public class WizardPhase
    {
        private int phaseId = 0;
        private string m_Name = "";
        private VisualTreeAsset m_PhaseTemplate;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public WizardPhase(string _name)
        {
            m_Name = _name;
            m_PhaseTemplate = Resources.Load<VisualTreeAsset>("wizardphase-template");
        }

        public void SetPhase(int _id)
        {
            phaseId = _id;
        }

        public virtual void Update(VisualElement root)
        {
            root.Clear();
            m_PhaseTemplate.CloneTree(root, null);

            root.Q<Label>("header").text = m_Name;
            root.Q<Label>("phase").text = phaseId.ToString();
        }

        protected VisualElement AddTable(VisualElement root)
        {
            var table = new VisualElement();
            table.AddToClassList("wizardTable");
            table.AddToClassList("verticalGroup");
            root.Add(table);
            return table;
        }

        protected VisualElement AddTableRow(VisualElement root)
        {
            var separator = new VisualElement();
            separator.name = "separator";
            separator.AddToClassList("horizontalSeparator");
            root.Add(separator);

            var rowGroup = new VisualElement();
            rowGroup.AddToClassList("wizardTableRow");
            rowGroup.AddToClassList("horizontalGroup");
            root.Add(rowGroup);

            return rowGroup;
        }
    }

    public class TextWizardPhase : WizardPhase
    {
        private string m_Link;
        private string m_Description;

        public TextWizardPhase(string _name, string _description, string _link = null) : base (_name)
        {
            m_Link = _link;
            m_Description = _description;
        }

        public override void Update(VisualElement root)
        {
            base.Update(root);

            var descrText = new Label();
            descrText.text = m_Description;
            root.Q("content").Add(descrText);

            if (m_Link != null)
            {
                var linkText = new Label();
                linkText.text = "Instructions link";
                linkText.AddToClassList("link");
                linkText.RegisterCallback<MouseDownEvent>((clickEvent) => { Application.OpenURL(m_Link); });
                root.Q("content").Add(linkText);
            }
        }
    }

    public interface Wizard
    {
        string Name { get; }
        string[] RequiredPackages { get; }

        IEnumerable<WizardPhase> GetPhases();
    }
}