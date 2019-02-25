using System.Runtime.InteropServices;

public class NativeProfiler
{
    [DllImport("__Internal")]
    private static extern int BeginSample([MarshalAs(UnmanagedType.LPWStr)]string str);

    [DllImport("__Internal")]
    private static extern int EndSample();

    [DllImport("__Internal")]
    private static extern int Event([MarshalAs(UnmanagedType.LPWStr)]string str);
}
