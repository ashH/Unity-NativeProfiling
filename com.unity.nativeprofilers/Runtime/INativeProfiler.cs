namespace Unity.NativeProfiling
{
    public interface INativeProfiler
    {
        void Init();
        void Term();

        void BeginMarker(string name);
        void EndMarker();
    }
}