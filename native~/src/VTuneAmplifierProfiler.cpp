#include <string.h>
#include "ProfilerPlugin.h"

#undef UNICODE
#undef _UNICODE
#include "Intel/ittnotify.h"

static bool s_ShouldEndFrame = false;
static __itt_domain* s_UnityDomain = NULL;
static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;
static const UnityProfilerMarkerDesc* s_DefaultMarkerDesc = NULL;

static void UNITY_INTERFACE_API CreateMarkerCallback(const UnityProfilerMarkerDesc* eventDesc, void* userData);
static void UNITY_INTERFACE_API EventCallback(const UnityProfilerMarkerDesc* eventDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData);
static void UNITY_INTERFACE_API FrameCallback(void* userData);


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    // VTune expects only one domain per app, although plugins lifetime is the same as the app
    if (s_UnityDomain == NULL)
        s_UnityDomain = __itt_domain_create("com.unity.nativeprofilers.vtune");

    s_DefaultMarkerDesc = NULL;

    s_ShouldEndFrame = false;
    s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
    s_UnityProfilerCallbacks->RegisterFrameCallback(&FrameCallback, NULL);
    s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&CreateMarkerCallback, NULL);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &EventCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&CreateMarkerCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterFrameCallback(&FrameCallback, NULL);
    s_DefaultMarkerDesc = NULL;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerBegin(const char *name)
{
    __itt_string_handle* handle = __itt_string_handle_create(name);
    __itt_task_begin(s_UnityDomain, __itt_null, __itt_null, handle);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerEnd()
{
    __itt_task_end(s_UnityDomain);
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

    __itt_string_handle* handle = __itt_string_handle_create(eventDesc->name);
    s_UnityProfilerCallbacks->RegisterMarkerEventCallback(eventDesc, &EventCallback, handle);
}

static void UNITY_INTERFACE_API EventCallback(const UnityProfilerMarkerDesc* eventDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
{
    __itt_string_handle* handle = static_cast<__itt_string_handle*>(userData);
    
    switch (eventType)
    {
        case kUnityProfilerMarkerEventTypeBegin:
            if (eventDataCount > 2 && eventDesc != NULL && eventDesc == s_DefaultMarkerDesc)
            {
                char S_TEMP_BUFFER[256];
                UTF16TOANSI(eventData[1].ptr, eventData[1].size, S_TEMP_BUFFER, sizeof(S_TEMP_BUFFER));

                // Override handle
                handle = __itt_string_handle_create(S_TEMP_BUFFER);
            }

            __itt_task_begin(s_UnityDomain, __itt_null, __itt_null, handle);
            break;
        case kUnityProfilerMarkerEventTypeEnd:
            __itt_task_end(s_UnityDomain);
            break;
    }
}

static void UNITY_INTERFACE_API FrameCallback(void* userData)
{
    if (s_ShouldEndFrame)
        __itt_frame_end_v3(s_UnityDomain, NULL);
    else
       s_ShouldEndFrame = true;

    __itt_frame_begin_v3(s_UnityDomain, NULL);
}
