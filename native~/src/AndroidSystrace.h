#pragma once

void SystraceInit();
void SystraceTerm();

bool SystraceIsEnabled();

void SystraceMarkerEnd();
void SystraceMarkerBegin(const char *name);
