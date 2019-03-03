#pragma once

#include <android/log.h>
#include "Unity/IUnityInterface.h"

#define  LOG_TAG    "Unity"
#define  LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)

#ifdef __cplusplus
extern "C" {
#endif

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceInit();
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceTerm();
bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceIsEnabled();
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceMarkerBegin(const char *name);
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API TraceMarkerEnd();

#ifdef __cplusplus
}
#endif
