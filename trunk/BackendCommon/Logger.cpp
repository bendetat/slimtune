#include "stdafx.h"
#include "Logger.h"
#include <ShlObj.h>

const char* Logger::INFO = "INFO";
const char* Logger::WARNING = "WARNING";
const char* Logger::FAIL = "FAIL";

Logger::Logger()
{
	SHGetFolderPath(NULL, CSIDL_PERSONAL, NULL, SHGFP_TYPE_CURRENT, m_path);
	wcscat_s(m_path, MAX_PATH, L"\\SlimTuneCLR-log.txt");
	m_file = _wfopen(m_path, L"wt");
}

Logger::~Logger()
{
	if(m_file)
	{
		fflush(m_file);
		fclose(m_file);
	}
}

void Logger::WriteEvent(const char* category, const char* message, ...)
{
	va_list args;
	va_start(args, message);

	char messageBuffer[2048];
	vsprintf_s(messageBuffer, 2048, message, args);
	char buffer[2080];
	sprintf_s(buffer, 2080, "%s:\t\t%s\n", category, messageBuffer);

	if(m_file)
	{
		fputs(buffer, m_file);
		fflush(m_file);
	}
}

void Logger::WriteEvent(const char* category, const wchar_t* message, ...)
{
	va_list args;
	va_start(args, message);

	wchar_t messageBuffer[2048];
	vswprintf_s(messageBuffer, 2048, message, args);
	char buffer[32];
	sprintf_s(buffer, 32, "%s:\t\t", category);

	if(m_file)
	{
		fputs(buffer, m_file);
		fputws(messageBuffer, m_file);
		fputs("\n", m_file);
		fflush(m_file);
	}
}

