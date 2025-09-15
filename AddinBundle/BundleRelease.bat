@echo off
setlocal

rem Set the base directory to the location of this script
set "baseDir=%~dp0"
set "parentDir=%baseDir%.."
set "contentsFolder=%baseDir%CesiumIonRevitAddin.bundle\Contents"
set "versions=2022 2023 2024 2025 2026"

rem Loop through each version number
for %%v in (%versions%) do (
    rem Correct the path to reference the parent directory of the script location
    robocopy "%parentDir%\CesiumIonRevitAddin_%%v\bin\Release" "%contentsFolder%\%%v" /IS /IT /MIR /E /R:3 /W:5 /NFL
    echo Bundling complete for version %%v!
)

pause
endlocal
