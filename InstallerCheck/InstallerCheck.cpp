// InstallerCheck.cpp: Definiert den Einstiegspunkt für die Konsolenanwendung.
// Copyright 2019 Digital-Programming

#define _AFXDLL

#include "stdafx.h"
#include <iostream>
#include <fstream>
#include <afxcoll.h>
#include <pathcch.h>
#include <string> 
#include <vector>
#include <sstream>
#include <algorithm>
#include <iterator>
#include <locale>
#include <codecvt>
#include <atlconv.h>
#include "resource.h"

using namespace std;

#define	KEY		_T("SOFTWARE\\Microsoft\\.NETFramework\\policy")
#define	KEY4	_T("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full")
#define	KEY_LEN	256
#define VERSION	255
char minimumVersion[VERSION] = "4.6";
const char* downloadLink = "https://www.microsoft.com/en-US/download/details.aspx?id=56116";
wstring fwVersion;

vector<string> split(const string& s, const char delimiter)
{
	vector<string> tokens;
	string token;
	istringstream tokenStream(s);
	while (getline(tokenStream, token, delimiter))
	{
		tokens.push_back(token);
	}
	return tokens;
}

bool CompareFWVersions(char* existing, char* required)
{
	vector<int> existingVersions;
	vector<int> requiredVersions;
	for (string version : split(existing, '.'))
		existingVersions.push_back(atoi(version.c_str()));
	for (string version : split(required, '.'))
		requiredVersions.push_back(atoi(version.c_str()));
	while (existingVersions.size() < requiredVersions.size())
		existingVersions.push_back(0);
	while (requiredVersions.size() < existingVersions.size())
		requiredVersions.push_back(0);

	bool result = false;
	for (int i = 0; i < existingVersions.size(); i++)
	{
		if (existingVersions[i] < requiredVersions[i])
			break;
		else if (existingVersions[i] > requiredVersions[i])
		{
			result = true;
			break;
		}
		else if (existingVersions[i] == requiredVersions[i] && i + 1 == existingVersions.size())
		{
			result = true;
			break;
		}
	}
	return result;
}

LONG GetStringRegKey(HKEY hKey, const wstring &strValueName, wstring &strValue, const wstring &strDefaultValue)
{
	strValue = strDefaultValue;
	WCHAR szBuffer[512];
	DWORD dwBufferSize = sizeof(szBuffer);
	ULONG nError;
	nError = RegQueryValueExW(hKey, strValueName.c_str(), 0, NULL, (LPBYTE)szBuffer, &dwBufferSize);
	if (ERROR_SUCCESS == nError)
	{
		strValue = szBuffer;
	}
	return nError;
}

bool CheckRegistryKeyExistance()
{
	HKEY hKey;
	DWORD dwIndex = 0;
	DWORD cbName = KEY_LEN;
	TCHAR sz[KEY_LEN];
	bool result = false;

	try
	{
		if (ERROR_SUCCESS == ::RegOpenKeyEx(HKEY_LOCAL_MACHINE, KEY, 0, KEY_READ, &hKey))
		{
			// Iterate through all subkeys
			while ((ERROR_NO_MORE_ITEMS != ::RegEnumKeyEx(hKey, dwIndex, sz, &cbName, NULL, NULL, NULL, NULL)))
			{
				if (!_tcsnicmp(sz, _T("V"), 1))
					fwVersion = sz + 1;
				dwIndex++;
				cbName = KEY_LEN;
			}
			::RegCloseKey(hKey);

			char fwVersionChars[VERSION + 1];
			wcstombs(fwVersionChars, fwVersion.c_str(), wcslen(fwVersion.c_str()) + 1);
			if (CompareFWVersions(fwVersionChars, _strdup("4.0")))
			{
				HKEY key;
				if (ERROR_SUCCESS == RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full", 0, KEY_READ, &key))
				{
					wstring strValueOfBinDir;
					GetStringRegKey(key, L"Version", strValueOfBinDir, L"4.0");

					wcout << L"Framework version found: " << strValueOfBinDir << L"\n";
					fwVersion = strValueOfBinDir;
				}
			}
			else
				wcout << L"Framework version found: " << fwVersion << L"\n";
			result = true;
		}
		else
			result = false;
		return result;
	}
	catch (...)
	{
		return false;
	}
}

void RunApplication(const char* path)
{
	printf("Now starting %s...\n", path);
	try
	{
		WinExec(path, SW_SHOW);
	}
	catch (...)
	{
		printf("Couldn't run %s\n", path);
	}
}

int main(int argc, char* argv[])
{
	SetConsoleTitle(_T("SLCB Script-Browser Framework Check"));
	printf("Checking .NET Framework before launching installer...\n");
	char basePath[256];
	_fullpath(basePath, argv[0], sizeof(basePath));
	string executionDirectory = string(basePath).substr(0, string(basePath).find_last_of("/\\"));

	// Check if Framework key exists and get the FW version if yes
	if (CheckRegistryKeyExistance())
	{
		using convert_type = codecvt_utf8<wchar_t>;
		wstring_convert<convert_type, wchar_t> converter;

		string converted_str = converter.to_bytes(fwVersion);
		// Compare versions
		if (CompareFWVersions(_strdup(converted_str.c_str()), minimumVersion))
		{
			HRSRC rc = ::FindResource(GetModuleHandle(NULL), MAKEINTRESOURCE(IDR_INSTALLER1), L"Installer");
			HGLOBAL rcData = ::LoadResource(GetModuleHandle(NULL), rc);
			DWORD size = ::SizeofResource(GetModuleHandle(NULL), rc);

			const byte* data = static_cast<const byte*>(::LockResource(rcData));
			FILE* file = fopen((executionDirectory + "\\Script-Browser Installer.exe").c_str(), "wb");
			fwrite(data, 1, size, file);
			fclose(file);

			RunApplication((executionDirectory + "\\Script-Browser Installer.exe").c_str());
			exit(-1);
		}
		else
		{
			// Minimum FW requirement not met
			wcout << L"\n===ATTENTION===\n\nThe installed .NET Framework (" << fwVersion << L") is not suitable with the application!\n";
			printf("Please install the Version %s or higher of the Microsoft .NET Framework!\n", minimumVersion);
			printf("Download: %s\n\n", downloadLink);
		}
	}
	else
	{
		// No FW exists
		printf("\n===ATTENTION===\n\n");
		printf("Please install the Version %s or higher of the Microsoft .NET Framework!\n", minimumVersion);
		printf("Download: %s\n\n", downloadLink);
	}

	system("pause");
	exit(-2);
    return 0;
}

