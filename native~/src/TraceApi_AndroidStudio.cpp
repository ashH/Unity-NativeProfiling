#include <dlfcn.h>
#include "TraceApi.h"

typedef void(*fp_ATrace_beginSection) (const char* sectionName);
typedef void(*fp_ATrace_endSection) ();
typedef bool(*fp_ATrace_isEnabled) ();

static void *s_libAndroid = 0;
static fp_ATrace_beginSection ATrace_beginSection = 0;
static fp_ATrace_endSection ATrace_endSection = 0;
static fp_ATrace_isEnabled ATrace_isEnabled = 0;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceInit()
{
    s_libAndroid = dlopen("libandroid.so", RTLD_NOW | RTLD_LOCAL);
    if (!s_libAndroid)
    {
        LOGD("NativeProfilingTools: Unity systrace disabled. Failed to load libandroid.so");
        return;
    }

    ATrace_isEnabled = reinterpret_cast<fp_ATrace_isEnabled>(dlsym(s_libAndroid, "ATrace_isEnabled"));
    ATrace_beginSection = reinterpret_cast<fp_ATrace_beginSection>(dlsym(s_libAndroid, "ATrace_beginSection"));
    ATrace_endSection = reinterpret_cast<fp_ATrace_endSection>(dlsym(s_libAndroid, "ATrace_endSection"));

    if (!ATrace_isEnabled || !ATrace_beginSection || !ATrace_endSection)
    {
        LOGD("NativeProfilingTools: Unity systrace disabled. Failed to access trace API");
        TraceTerm();
        return;
    }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceTerm()
{
    if (s_libAndroid)
        dlclose(s_libAndroid);

    s_libAndroid = 0;
    ATrace_isEnabled = 0;
    ATrace_beginSection = 0;
    ATrace_endSection = 0;
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceIsEnabled()
{
    if (!s_libAndroid || !ATrace_isEnabled)
        return false;

    return ATrace_isEnabled();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceMarkerBegin(const char *name)
{
    if (!s_libAndroid || !ATrace_beginSection)
        return;

    ATrace_beginSection(name);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceMarkerEnd()
{
    if (!s_libAndroid || !ATrace_endSection)
        return;

    ATrace_endSection();
}
