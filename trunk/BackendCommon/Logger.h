#ifndef LOGGER_H
#define LOGGER_H
#pragma once

#include <windows.h>
#include <cstdio>

class Logger
{
public:
	Logger();
	~Logger();

	void WriteEvent(const char* category, const char* message, ...);
	void WriteEvent(const char* category, const wchar_t* message, ...);

	static const char* INFO;
	static const char* WARNING;
	static const char* FAIL;

private:
	wchar_t m_path[MAX_PATH + 32];
	FILE* m_file;
};

#endif
