using System;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements;

namespace Unity.NativeProfiling
{
    public class AndroidDeviceCheckPhase : WizardPhase
    {
        private AndroidADB m_Adb = new AndroidADB();
        private Dictionary<string, AndroidDeviceInfo> m_Devices = new Dictionary<string, AndroidDeviceInfo>();

        public AndroidDeviceCheckPhase() : base("Phone status check")
        {
        }

        public override void Update(VisualElement root)
        {
            base.Update(root);

            var content = root.Q("content");
            foreach (var dev in m_Devices)
                GenrateDeviceTable(content, dev.Value);

            var refreshBtn = new Button(null);
            refreshBtn.text = "Refresh";
            refreshBtn.clickable.clicked += () => { Refresh(); Update(root); };
            content.Add(refreshBtn);
        }

        private void GenrateDeviceTable(VisualElement root, AndroidDeviceInfo dev)
        {
            var table = AddTable(root);

            // Device name
            var row = AddTableRow(table);
            var nameLabel = new Label(dev.Model);
            nameLabel.AddToClassList("h3");
            row.Add(nameLabel);

            // Device API Level - 26 is minimum required
            MakeRow(table, "SDK Version", dev.GetProperty("ro.build.version.sdk"), Int32.Parse(dev.GetProperty("ro.build.version.sdk")) >= 26 ? "Good" : "Failed");
            // Check that we can execute 'su' command
            MakeRow(table, "Is Rooted", dev.IsRooted.ToString(), dev.IsRooted ? "Good" : "");
            // Check '/proc/sys/kernel/perf_event_paranoid' for perf mode
            MakeRow(table, "Perf enabled", dev.PerfLevel.ToString(), dev.PerfLevel == 3 ? "Disabled" : (dev.PerfLevel == -1 ? "Full access" : "Limited"));
            // Check property 'security.perf_harden', is access to perf hardened
            MakeRow(table, "Perf access hardened", dev.GetProperty("security.perf_harden"), Int32.Parse(dev.GetProperty("security.perf_harden")) == 0 ? "Good" : "Failed");
            // Check SE Linux default permission mode
            MakeRow(table, "Kernel security policy", dev.KernelPolicy, dev.KernelPolicy.ToLower() == "permissive" ? "Good" : "Failed");
            // Check is kernel function names namespace available
            MakeRow(table, "Kernel namespace", dev.GetProperty("kernel.kptr_restrict"), dev.GetProperty("kernel.kptr_restrict") == "1" ? "Restricted" : "Visible");
        }

        private void MakeRow(VisualElement root, string name, string val, string status)
        {
            var row = AddTableRow(root);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("nameLabel");
            row.Add(nameLabel);

            var valueLabel = new Label(val);
            valueLabel.AddToClassList("valueLabel");
            row.Add(valueLabel);

            var statusLabel = new Label(status);
            row.Add(statusLabel);
        }

        private void Refresh()
        {
            m_Devices.Clear();

            var list = m_Adb.RetrieveConnectDevicesIDs();
            foreach (var i in list)
                m_Devices[i] = m_Adb.RetriveDeviceInfo(i);
        }
    }
}