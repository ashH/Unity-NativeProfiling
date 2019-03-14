#include <string.h>
#include "ProfilerPlugin.h"

void UTF16TOANSI(const void *src, size_t srcSize, char* dst, size_t dstSize)
{
    size_t length = srcSize / sizeof(uint16_t);
    if (length >= dstSize)
        length = dstSize - 1;

    const uint16_t* srcU16 = static_cast<const uint16_t*>(src);
    for (int i = 0; i < length; ++i)
        dst[i] = static_cast<char>(srcU16[i]);

    dst[length] = 0;
}
