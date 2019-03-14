#include "ProfilerPlugin.h"
#include <sys/kdebug_signpost.h>

static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;
static const UnityProfilerMarkerDesc* s_DefaultMarkerDesc = NULL;

//https://stackoverflow.com/questions/19017843/alternative-to-dtsendsignalflag-to-identify-key-events-in-instruments/39416673#39416673
//https://medium.com/@gerogerber/points-of-interest-profiling-unity-with-xcode-instruments-cd2634ce704c

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
    s_UnityProfilerCallbacks->RegisterFrameCallback(&FrameCallback, NULL);
    s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&CreateMarkerCallback, NULL);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_UnityProfilerCallbacks->UnregisterFrameCallback(&FrameCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&CreateMarkerCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &EventCallback, NULL);
    s_UnityProfilerCallbacks = NULL;

    s_DefaultMarkerDesc = NULL;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerBegin(const char *name)
{
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SimpleMarkerEnd()
{
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
                //kdebug_signpost_start(SignPostCode.download.rawValue, UInt(index), 0, 0, SignPostColor.orange.rawValue)
            }
            else
            {
                //kdebug_signpost_start(SignPostCode.download.rawValue, UInt(index), 0, 0, SignPostColor.orange.rawValue)
            }

            break;
        }
        case kUnityProfilerMarkerEventTypeEnd:
        {
            //kdebug_signpost_end(SignPostCode.download.rawValue, UInt(index), 0, 0, SignPostColor.orange.rawValue)
            break;
        }
    }
}

static void UNITY_INTERFACE_API FrameCallback(void* userData)
{
    //kdebug_signpost(SignPostCode.download.rawValue, UInt(index), 0, 0, SignPostColor.orange.rawValue)
}
