#include "TraceApi.h"
#include "Arm/streamline_annotate.h"

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceInit()
{
    ANNOTATE_SETUP;
    LOGD("NativeProfilingTools: Init Streamlineanalyzer");
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceTerm()
{
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceIsEnabled()
{
    return true;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceMarkerBegin(const char *name)
{
    ANNOTATE(name);
    LOGD("B: %s", name);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceMarkerEnd()
{
    ANNOTATE_END();
    LOGD("E");
}
