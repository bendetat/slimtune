#include "stdafx.h"
#include "Logger.h"
#include <ShlObj.h>

const char* Logger::INFO = "INFO";
const char* Logger::WARNING = "WARNING";
const char* Logger::FAIL = "FAIL";

Logger::Logger()
{
	SHGetFolderPath(NULL, CSIDL_PERSONAL, NULL, SHGFP_TYPE_CURRENT, m_path);
	wcscat(m_path, L"\\SlimTune-CLR-log.txt");
	m_file = _wfopen(m_path, L"wt");
}

Logger::~Logger()
{
	fflush(m_file);
	fclose(m_file);
}

void Logger::WriteEvent(const char* category, const char* message)
{
	char buffer[512];
	sprintf(buffer, "%s:\t\t%s\n", category, message);
	fputs(buffer, m_file);
	fflush(m_file);
}

