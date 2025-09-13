SETLOCAL enabledelayedexpansion
@ECHO off

SET max_version=%3
SET dll_name=RedHaloM2B_%max_version%.dll
SET source_dir=%1

IF %2=="Debug" GOTO OnDebug
IF %2=="Release" GOTO OnRelease

:OnDebug
SET dest_dir="C:\Program Files\Autodesk\3ds Max 2023\bin\assemblies"
rem SET dest_dir=D:\APP\DLL
GOTO CopyFiles

:OnRelease
rem SET dest_dir=D:\APP\DLL
SET dest_dir="C:\Program Files\Autodesk\3ds Max 2023\bin\assemblies"
rem "%max_location%bin\assemblies"
GOTO CopyFiles

:CopyFiles
ECHO :: Copying plug-in files
ECHO :: From: %source_dir%
ECHO :: To: %dest_dir%

if exist %dest_dir%\%dll_name% del /f /q %dest_dir%\%dll_name%
COPY %source_dir%\RedHaloM2B.dll %dest_dir%\%dll_name%

:Close
PAUSE
EXIT
