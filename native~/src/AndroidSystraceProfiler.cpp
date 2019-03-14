#include "ProfilerPlugin.h"
#include <string.h>
#include <android/log.h>
#include "AndroidSystrace.h"

#define  LOG_TAG    "Unity"
#define  LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)

static bool s_isCapturing = false;
static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;
static const UnityProfilerMarkerDesc* s_DefaultMarkerDesc = NULL;

static void UNITY_INTERFACE_API CreateMarkerCallback(const UnityProfilerMarkerDesc* eventDesc, void* userData);
static void UNITY_INTERFACE_API EventCallback(const UnityProfilerMarkerDesc* eventDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData);
static void UNITY_INTERFACE_API FrameCallback(void* userData);


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_isCapturing = false;
    s_DefaultMarkerDesc = NULL;

    // Init Systrace API
    // It's safe even if it fails to init. we'll end up with just FrameCallback, as SystraceIsEnabled will always return false
    SystraceInit();

    // Register with Unity profiler interface
    s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
    s_UnityProfilerCallbacks->RegisterFrameCallback(&FrameCallback, NULL);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    if (s_UnityProfilerCallbacks)
    {
        s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, EventCallback, NULL);
        s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(CreateMarkerCallback, NULL);
        s_UnityProfilerCallbacks->UnregisterFrameCallback(FrameCallback, NULL);
        s_UnityProfilerCallbacks = NULL;
    }

    s_isCapturing = false;
    s_DefaultMarkerDesc = NULL;

    SystraceTerm();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerBegin(const char *name)
{
    SystraceMarkerBegin(name);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerEnd()
{
    SystraceMarkerEnd();
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

    s_UnityProfilerCallbacks->RegisterMarkerEventCallback(eventDesc, EventCallback, NULL);
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
                SystraceMarkerBegin(S_TEMP_BUFFER);
            }
            else
            {
                SystraceMarkerBegin(eventDesc->name);
            }

            break;
        }
        case kUnityProfilerMarkerEventTypeEnd:
        {
            SystraceMarkerEnd();
            break;
        }
    }
}

static void UNITY_INTERFACE_API FrameCallback(void* userData)
{
    bool isCapturing = SystraceIsEnabled();
    if (isCapturing != s_isCapturing)
    {
        s_isCapturing = isCapturing;
        if (isCapturing)
        {
            LOGD("NativeProfilingTools: Starting systrace");
            s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(CreateMarkerCallback, NULL);
        }
        else
        {
            LOGD("NativeProfilingTools: Stopping systrace");
            s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(CreateMarkerCallback, NULL);
            s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, EventCallback, NULL);
        }
    }
}
