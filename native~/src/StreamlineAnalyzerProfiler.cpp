#include "ProfilerPlugin.h"
#include <string.h>
#include <android/log.h>
#include "Arm/streamline_annotate.h"

#define  LOG_TAG    "Unity"
#define  LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)

static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;
static const UnityProfilerMarkerDesc* s_DefaultMarkerDesc = NULL;

static void UNITY_INTERFACE_API CreateMarkerCallback(const UnityProfilerMarkerDesc* eventDesc, void* userData);
static void UNITY_INTERFACE_API EventCallback(const UnityProfilerMarkerDesc* eventDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData);
static void UNITY_INTERFACE_API FrameCallback(void* userData);


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    ANNOTATE_SETUP;
    LOGD("NativeProfilingTools: Init Streamlineanalyzer");

    s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
    s_UnityProfilerCallbacks->RegisterFrameCallback(&FrameCallback, NULL);
    s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&CreateMarkerCallback, NULL);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &EventCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&CreateMarkerCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterFrameCallback(&FrameCallback, NULL);
    s_UnityProfilerCallbacks = NULL;

    s_DefaultMarkerDesc = NULL;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerBegin(const char *name)
{
    ANNOTATE(name);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerEnd()
{
    ANNOTATE_END();
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// IUnityProfilerCallbacks callbacks implementation
static void UNITY_INTERFACE_API CreateMarkerCallback(const UnityProfilerMarkerDesc* eventDesc, void* userData)
{
    if (s_DefaultMarkerDesc == NULL)
    {
        if (strcmp(eventDesc->name, "Profiler.Default") == 0)
            s_DefaultMarkerDesc = eventDesc;
    }

    s_UnityProfilerCallbacks->RegisterMarkerEventCallback(eventDesc, &EventCallback, NULL);
}

static void UNITY_INTERFACE_API EventCallback(const UnityProfilerMarkerDesc* eventDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
{
    switch (eventType)
    {
        case kUnityProfilerMarkerEventTypeBegin:
        {
            if (eventDataCount > 1 && eventDesc == s_DefaultMarkerDesc)
            {
                // Profiler.Default marker emits UTF16 string as the second metadata parameter.
                // For simplicity we slice UTF16 data to char.
                char S_TEMP_BUFFER[256];
                UTF16TOANSI(eventData[1].ptr, eventData[1].size, S_TEMP_BUFFER, sizeof(S_TEMP_BUFFER));
                ANNOTATE(S_TEMP_BUFFER);
            }
            else
            {
                ANNOTATE(eventDesc->name);
            }

            break;
        }
        case kUnityProfilerMarkerEventTypeEnd:
        {
            ANNOTATE_END();
            break;
        }
    }
}

static void UNITY_INTERFACE_API FrameCallback(void* userData)
{
    ANNOTATE_MARKER_STR("Frame");
}
