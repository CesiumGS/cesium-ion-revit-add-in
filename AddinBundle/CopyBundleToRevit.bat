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

setlocal

rem Set the source folder to the path of the script directory
set "sourceFolder=%~dp0CesiumIonRevitAddin.bundle"
set "destinationFolder=C:\ProgramData\Autodesk\ApplicationPlugins\CesiumIonRevitAddin.bundle"

rem Use robocopy to mirror the source folder to the destination
robocopy "%sourceFolder%" "%destinationFolder%" /MIR /R:3 /W:5 /NFL

echo Mirroring complete from %sourceFolder% to %destinationFolder%.

pause
endlocal
