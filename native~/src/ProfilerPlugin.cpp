#include <string.h>
#include "Unity/IUnityInterface.h"
#include "Unity/IUnityProfilerCallbacks.h"
#include "TraceApi.h"

class AndroidSystraceProfiler
{
private:
    bool m_isCapturing;
    IUnityProfilerCallbacks* m_UnityProfilerCallbacks;
    const UnityProfilerMarkerDesc* m_DefaultMarkerDesc;

public:
    AndroidSystraceProfiler(IUnityInterfaces* unityInterfaces)
        : m_isCapturing(false)
        , m_UnityProfilerCallbacks(0)
        , m_DefaultMarkerDesc(0)
    {
        TraceInit();

        if (TraceIsEnabled())
        {
            LOGD("NativeProfilingTools: Enabling Unity systrace");
            m_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
            m_UnityProfilerCallbacks->RegisterFrameCallback(&FrameCallback, this);
        }
        else
        {
            LOGD("NativeProfilingTools: Unity systrace disabled. Failed to init native tracing API");
        }
    }

    ~AndroidSystraceProfiler()
    {
        if (m_UnityProfilerCallbacks)
        {
            m_UnityProfilerCallbacks->UnregisterFrameCallback(FrameCallback, this);
            m_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(CreateEventCallback, this);
            m_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, EventCallback, this);
            m_UnityProfilerCallbacks = 0;
        }

        TraceTerm();
    }

private:
    static void UNITY_INTERFACE_API CreateEventCallback(const UnityProfilerMarkerDesc* eventDesc, void* userData)
    {
        AndroidSystraceProfiler* this__ = static_cast<AndroidSystraceProfiler*>(userData);

        if (this__->m_DefaultMarkerDesc == NULL)
        {
            if (strcmp(eventDesc->name, "Profiler.Default") == 0)
                this__->m_DefaultMarkerDesc = eventDesc;
        }

        this__->m_UnityProfilerCallbacks->RegisterMarkerEventCallback(eventDesc, EventCallback, this__);
    }

    static void UNITY_INTERFACE_API EventCallback(const UnityProfilerMarkerDesc* eventDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
    {
        AndroidSystraceProfiler* this__ = static_cast<AndroidSystraceProfiler*>(userData);

        switch (eventType)
        {
            case kUnityProfilerMarkerEventTypeBegin:
            {
                if (eventDataCount > 1 && eventDesc == this__->m_DefaultMarkerDesc)
                {
                    // Profiler.Default marker emits UTF16 string as the second metadata parameter.
                    // For simplicity we slice UTF16 data to char.
                    TraceMarkerBegin(UTF16TOANSI(eventData[1].ptr, eventData[1].size));
                }
                else
                {
                    TraceMarkerBegin(eventDesc->name);
                }

                break;
            }
            case kUnityProfilerMarkerEventTypeEnd:
            {
                TraceMarkerEnd();
                break;
            }
        }
    }

    static void UNITY_INTERFACE_API FrameCallback(void* userData)
    {
        AndroidSystraceProfiler* this__ = static_cast<AndroidSystraceProfiler*>(userData);

        bool isCapturing = TraceIsEnabled();
        if (isCapturing != this__->m_isCapturing)
        {
            this__->m_isCapturing = isCapturing;
            if (isCapturing)
            {
                LOGD("NativeProfilingTools: Starting systrace");
                this__->m_UnityProfilerCallbacks->RegisterCreateMarkerCallback(CreateEventCallback, this__);
            }
            else
            {
                LOGD("NativeProfilingTools: Stopping systrace");
                this__->m_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(CreateEventCallback, this__);
                this__->m_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, EventCallback, this__);
            }
        }
    }

    static char* UTF16TOANSI(const void *ptr, size_t size)
    {
        static const int S_TEMP_SIZE = 255;
        static char S_TEMP_BUFFER[S_TEMP_SIZE + 1];

        size_t length = size / sizeof(uint16_t);
        if (length > S_TEMP_SIZE)
            length = S_TEMP_SIZE;

        const uint16_t* srcU16 = static_cast<const uint16_t*>(ptr);
        for (int i = 0; i < length; ++i)
            S_TEMP_BUFFER[i] = static_cast<char>(srcU16[i]);
        S_TEMP_BUFFER[length + 1] = 0;

        return static_cast<char*>(S_TEMP_BUFFER);
    }
};

static AndroidSystraceProfiler* s_AndroidSystraceProfiler = 0;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_AndroidSystraceProfiler = new AndroidSystraceProfiler(unityInterfaces);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    if (s_AndroidSystraceProfiler != 0)
        delete s_AndroidSystraceProfiler;

    s_AndroidSystraceProfiler = 0;
}
