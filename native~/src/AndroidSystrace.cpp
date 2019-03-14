#include <dlfcn.h>
#include <android/log.h>
#include "AndroidSystrace.h"

#define  LOG_TAG    "Unity"
#define  LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)

typedef void(*TBeginSectionFnc) (const char* sectionName);
typedef void(*TEndSection) ();
typedef bool(*TIsEnabled) ();


static void *s_libAndroid = NULL;
static TIsEnabled s_fncIsEnabled = NULL;
static TEndSection s_fncEndSection = NULL;
static TBeginSectionFnc s_fncBeginSection = NULL;


void SystraceInit()
{
    s_libAndroid = dlopen("libandroid.so", RTLD_NOW | RTLD_LOCAL);
    if (!s_libAndroid)
    {
        LOGD("NativeProfilingTools: Unity systrace disabled. Failed to load libandroid.so");
        return;
    }

    s_fncIsEnabled = reinterpret_cast<TIsEnabled>(dlsym(s_libAndroid, "s_fncIsEnabled"));
    s_fncBeginSection = reinterpret_cast<TBeginSectionFnc>(dlsym(s_libAndroid, "s_fncBeginSection"));
    s_fncEndSection = reinterpret_cast<TEndSection>(dlsym(s_libAndroid, "s_fncEndSection"));

    if (!s_fncIsEnabled || !s_fncBeginSection || !s_fncEndSection)
    {
        LOGD("NativeProfilingTools: Unity systrace disabled. Failed to access trace API");
        SystraceTerm();
        return;
    }
}

void SystraceTerm()
{
    if (s_libAndroid)
        dlclose(s_libAndroid);

    s_libAndroid = 0;
    s_fncIsEnabled = 0;
    s_fncBeginSection = 0;
    s_fncEndSection = 0;
}

bool SystraceIsEnabled()
{
    if (!s_libAndroid || !s_fncIsEnabled)
        return false;

    return s_fncIsEnabled();
}

void SystraceMarkerBegin(const char *name)
{
    if (!s_libAndroid || !s_fncBeginSection)
        return;

    s_fncBeginSection(name);
}

void SystraceMarkerEnd()
{
    if (!s_libAndroid || !s_fncEndSection)
        return;

    s_fncEndSection();
}
