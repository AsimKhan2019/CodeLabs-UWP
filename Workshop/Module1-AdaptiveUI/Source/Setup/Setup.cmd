@ECHO off
%~d0

CD "%~dp0"
ECHO Install Visual Studio 2015 Code Snippets for the module:
ECHO -------------------------------------------------------------------------------
CALL .\Scripts\InstallCodeSnippets.cmd
ECHO Done!
ECHO Create desktop shortcut for the workshop folders:
ECHO -------------------------------------------------------------------------------
CALL .\Scripts\CreateShortcut.cmd
ECHO Done!
ECHO.
ECHO *******************************************************************************
ECHO.

@PAUSE
