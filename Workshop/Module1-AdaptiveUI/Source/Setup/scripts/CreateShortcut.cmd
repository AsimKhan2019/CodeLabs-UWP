@echo off
%~d0
cd "%~dp0"

echo.
echo =======================================================
echo Create desktop shortcut for the workshop module folders
echo =======================================================
echo.

IF EXIST %WINDIR%\SysWow64 (
set powerShellDir=%WINDIR%\SysWow64\windowspowershell\v1.0
) ELSE (
set powerShellDir=%WINDIR%\system32\windowspowershell\v1.0
)

call %powerShellDir%\powershell.exe -Command Set-ExecutionPolicy unrestricted

call %powerShellDir%\powershell.exe -Command "&'.\CreateShortcut.ps1'"

