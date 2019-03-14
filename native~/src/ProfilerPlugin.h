#pragma once

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityProfilerCallbacks.h"

// Internal function to crunch UTF16 to ANSI
void UTF16TOANSI(const void *src, size_t srcSize, char* dst, size_t dstSize);
