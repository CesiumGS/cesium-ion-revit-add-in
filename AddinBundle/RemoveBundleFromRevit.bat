@echo off
:: Check if the script is running with administrative privileges
net session >nul 2>nul
if %errorlevel% neq 0 (
    echo This script requires administrator privileges.
    echo.
    echo Relaunching as Administrator...
    echo.
    :: Relaunch the script with admin privileges using PowerShell
    powershell -Command "Start-Process cmd -ArgumentList '/c, %~s0' -Verb runAs"
    exit /b
)

setlocal enabledelayedexpansion

set "folder=C:\ProgramData\Autodesk\ApplicationPlugins\CesiumIonRevitAddin.bundle"

if exist "%folder%" (
	echo Deleting: %folder%
	
	rmdir /s /q "%folder%"
	
) else (
	echo %folder% does not exist
)
	
pause
endlocal