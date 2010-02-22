; Copyright (c) 2009 SlimDX Group
;
; Permission is hereby granted, free of charge, to any person obtaining a copy
; of this software and associated documentation files (the "Software"), to deal
; in the Software without restriction, including without limitation the rights
; to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
; copies of the Software, and to permit persons to whom the Software is
; furnished to do so, subject to the following conditions:
;
; The above copyright notice and this permission notice shall be included in
; all copies or substantial portions of the Software.
;
; THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
; IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
; FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
; AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
; LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
; OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
; THE SOFTWARE.

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={72513408-FD69-404F-9FDB-2DB6A7DCDE84}
AppName=SlimTune Profiler
AppVerName=SlimTune Profiler v0.2.0
AppPublisher=SlimDX Group
AppPublisherURL=http://www.slimtune.com/
AppSupportURL=http://www.slimtune.com/
AppUpdatesURL=http://www.slimtune.com/
DefaultDirName={pf}\SlimTune Profiler
DefaultGroupName=SlimTune Profiler
LicenseFile=..\ExtraFiles\License.rtf
OutputDir=.
OutputBaseFilename=SlimTune-0.2.0
Compression=lzma
SolidCompression=yes
UsePreviousAppDir=yes
MinVersion=0,5.01
VersionInfoVersion=0.2.0.0
VersionInfoCompany=SlimDX Group
VersionInfoCopyright=Copyright © SlimDX Group 2010
VersionInfoProductName=SlimTune Profiler v0.2.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\ExtraFiles\x86\vcredist_x86.exe"; DestDir: "{tmp}"; Flags: ignoreversion; AfterInstall: InstallVCRedist('x86')
Source: "..\ExtraFiles\x64\vcredist_x64.exe"; DestDir: "{tmp}"; Flags: ignoreversion; Check: IsWin64; AfterInstall: InstallVCRedist('x64')

Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\Backends\x86\SlimTuneCLR.dll"; DestDir: "{app}\Backends\x86"; Flags: 32bit regserver ignoreversion
Source: "..\publish\Backends\x64\SlimTuneCLR.dll"; DestDir: "{app}\Backends\x64"; Check: IsWin64; Flags: 64bit regserver ignoreversion

Source: "..\publish\Backends\*"; DestDir: "{app}\Backends"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\publish\Plugins\*"; DestDir: "{app}\Plugins"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\publish\Api\*"; DestDir: "{app}\Api"; Flags: ignoreversion recursesubdirs createallsubdirs

Source: "..\ExtraFiles\x86\SSCERuntime-ENU-x86.msi"; DestDir: "{tmp}"; Flags: ignoreversion

[Registry]
Root: HKLM; Subkey: "Software\SlimDX Group"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\SlimDX Group\SlimTune"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\SlimDX Group\SlimTune"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"
Root: HKLM; Subkey: "Software\SlimDX Group\SlimTune"; ValueType: string; ValueName: "PluginsPath"; ValueData: "{app}\Plugins"

[Icons]
Name: "{group}\SlimTune Profiler"; Filename: "{app}\SlimTuneUI.exe"
Name: "{group}\{cm:UninstallProgram,SlimTune Profiler}"; Filename: "{uninstallexe}"

[Tasks]
Name: Firewall; Description: "Open TCP subnet ports 3000 - 3001 in Windows Firewall"; GroupDescription: "Firewall:"; MinVersion: 0,5.01.2600sp2;

[Run]
Filename: "msiexec.exe"; Parameters: "/passive /i ""{tmp}\SSCERuntime-ENU-x86.msi"""; StatusMsg: "Installing SQL Server Compact Edition"
Filename: "{sys}\netsh.exe"; Parameters: "firewall add portopening TCP 3000 ""SlimTune Profiler (3000)"" ENABLE SUBNET"; Flags: runhidden; Tasks: Firewall;
Filename: "{sys}\netsh.exe"; Parameters: "firewall add portopening TCP 3001 ""SlimTune Profiler (3001)"" ENABLE SUBNET"; Flags: runhidden; Tasks: Firewall;

Filename: "{app}\SlimTuneUI.exe"; Description: "{cm:LaunchProgram,SlimTune Profiler}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\netsh.exe"; Parameters: "firewall delete portopening TCP 3000"; Flags: runhidden; MinVersion:0,5.01.2600sp2; Tasks: Firewall;
Filename: "{sys}\netsh.exe"; Parameters: "firewall delete portopening TCP 3001"; Flags: runhidden; MinVersion:0,5.01.2600sp2; Tasks: Firewall;

[Code]
function IsX64: Boolean;
begin
  Result := IsWin64 and (ProcessorArchitecture = paX64);
end;

function InitializeSetup(): Boolean;
var
  NetFrameWorkInstalled : Boolean;
begin
  NetFrameWorkInstalled := RegKeyExists(HKLM,'SOFTWARE\Microsoft\.NETFramework\policy\v2.0');
  if NetFrameWorkInstalled then
  begin
    Result := true;
  end else
  begin
    MsgBox('This setup requires the .NET Framework 2.0 or later. Please download and install the .NET Framework and run this setup again.',
      mbConfirmation, MB_OK);
    Result:=false;
  end;
end;

procedure InstallVCRedist(arch: String);
var
  TempDir:      String;
  RedistPath:   String;
  ReturnCode:   Integer;
begin
  TempDir := ExpandConstant('{tmp}');
  RedistPath := TempDir + '\vcredist_' + arch + '.exe';

  Exec(RedistPath, '/qb!', TempDir, SW_SHOW, ewWaitUntilTerminated, ReturnCode)
end;
