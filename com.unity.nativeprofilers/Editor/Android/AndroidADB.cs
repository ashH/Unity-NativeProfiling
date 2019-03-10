using System;
using System.Linq;
using System.Collections.Generic;

#if UNITY_2019_1_OR_NEWER
using UnityEditor.Android;
#endif

namespace Unity.NativeProfiling
{
    public class AndroidADB
    {
        public List<string> RetrieveConnectDevicesIDs()
        {
            var deviceIds = new List<string>();

            var adbOutput = Run(new[] { "devices" }, "Unable to list connected devices. ");
            foreach (var line in adbOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()))
            {
                if (line.EndsWith("device"))
                {
                    var deviceId = line.Substring(0, line.IndexOf('\t'));
                    deviceIds.Add(deviceId);
                }
            }

            return deviceIds;
        }

        public AndroidDeviceInfo RetriveDeviceInfo(string id)
        {
            return new AndroidDeviceInfo(this, id);
        }

#if UNITY_2019_1_OR_NEWER
        private ADB m_Adb = null;

        public string Run(string[] command, string errorMsg)
        {
            return GetADB().Run(command, errorMsg);
        }

        public ADB GetADB()
        {
            if (m_Adb == null)
                m_Adb = ADB.GetInstance();

            return m_Adb;
        }
#else
        private object m_Adb;

        public string Run(string[] command, string errorMsg)
        {
            var adb = GetADB();
            System.Reflection.MethodInfo method = adb.GetType().GetMethod("Run", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return (string)method.Invoke(adb, new object[] { command, null, errorMsg });
        }

        public object GetADB()
        {
            if (m_Adb == null)
            {
                var asm = AndroidUtils.GetUnityEditorAndroidAssembly();
                var javaTools = AndroidUtils.GetJavaToolsInstance(asm);
                var sdkTools = AndroidUtils.GetAndroidSDKToolsInstance(asm, javaTools);
                m_Adb = AndroidUtils.InstantiateADB(asm, sdkTools);
            }

            return m_Adb;
        }
#endif
    }
}