using System;

#if UNITY_2019_1_OR_NEWER
using UnityEditor.Android;
#endif

namespace Unity.NativeProfiling
{
    public class AndroidDeviceInfo
    {
        public readonly string Id;
        public readonly string Model;
        public readonly bool IsRooted;
        public readonly int PerfLevel;
        public readonly string KernelPolicy;
        public readonly Func<string, string> GetProperty;

        public AndroidDeviceInfo(AndroidADB adb, string deviceId)
        {
#if UNITY_2019_1_OR_NEWER
            var deviceInfo = new AndroidDevice(adb.GetADB(), deviceId);

            Id = deviceInfo.Id;
            Model = deviceInfo.Model;

            GetProperty = (string id) =>
            {
                return deviceInfo.Properties[id];
            };
#else
            var asm = AndroidUtils.GetUnityEditorAndroidAssembly();
            var refType = asm.GetType("UnityEditor.Android.AndroidDevice");
            var deviceInfo = Activator.CreateInstance(refType, new object[] { adb.GetADB(), deviceId });

            Id = PropertyAccessor<string>(deviceInfo, "Id");
            Model = PropertyAccessor<string>(deviceInfo, "Model");

            GetProperty = (string id) =>
            {
                var table = deviceInfo.GetType().GetProperty("Properties").GetValue(deviceInfo, null);
                return (string)table.GetType().GetProperty("Item").GetValue(table, new object[] { id });
            };

#endif

            PerfLevel = GetValueFileKey(adb, "/proc/sys/kernel/perf_event_paranoid", 3);
            KernelPolicy = adb.Run(new[] { "shell", "getenforce" }, "Enforcing");
            try
            {
                IsRooted = adb.Run(new[] { "shell", "su", "-c", "id" }, "Permission denied").Contains("uid=0(");
            }
            catch
            {
                IsRooted = false;
            }
        }


        private T PropertyAccessor<T>(object deviceInfo, string id)
        {
            return (T)deviceInfo.GetType().GetProperty(id).GetValue(deviceInfo, null);
        }

        private int GetValueFileKey(AndroidADB adb, string key, int defValue)
        {
            var perfStr = adb.Run(new[] { "shell", "cat", key }, defValue.ToString());
            try
            {
                return Int32.Parse(perfStr);
            }
            catch {}

            return defValue;
        }
    }
}
