SETLOCAL enabledelayedexpansion
@ECHO off

ECHO :: Killing 3ds Max process if running
TASKKILL /F /t /IM 3dsmax.exe >nul 2>&1
ping 1.1.1.1 -n 2 >nul

SET max_version=%3
rem SET dll_name=RedHaloM2B_%max_version%.dll
SET dll_name=RedHaloM2B.dll
SET source_dir=%1



IF %2=="Debug" GOTO OnDebug
IF %2=="Release" GOTO OnRelease

:OnDebug
rem SET dest_dir="C:\Program Files\Autodesk\3ds Max 2023\bin\assemblies"
SET dest_dir_package="D:\APP\RedHaloM2B_Lib\Boundle\Contents\bin"
SET dest_dir="C:\ProgramData\Autodesk\ApplicationPlugins\Max2Blender\Contents\bin"
GOTO CopyFiles

:OnRelease
rem SET dest_dir=D:\APP\DLL
SET dest_dir_package="D:\APP\RedHaloM2B_Lib\Boundle\Contents\bin"
SET dest_dir="C:\ProgramData\Autodesk\ApplicationPlugins\Max2Blender\Contents\bin"
GOTO CopyFiles

:CopyFiles
ECHO :: Copying plug-in files
ECHO :: From: %source_dir%
ECHO :: To: %dest_dir%

if exist %dest_dir%\%max_version%\%dll_name% del /f /q %dest_dir%\%max_version%\%dll_name%
if exist %dest_dir_package%\%max_version%\%dll_name% del /f /q %dest_dir_package%\%max_version%\%dll_name%
COPY %source_dir%\RedHaloM2B.dll %dest_dir%\%max_version%\%dll_name%
COPY %source_dir%\RedHaloM2B.dll %dest_dir_package%\%max_version%\%dll_name%

:Close
PAUSE
EXIT
