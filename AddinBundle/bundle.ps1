$contentsFolder = ".\CesiumIonRevitAddin.bundle\Contents"
$versions = @(2022, 2023, 2024, 2025)

foreach ($version in $versions) {
    $sourcePath = "..\CesiumIonRevitAddin_$version\bin\Release"
    $destinationPath = Join-Path -Path $contentsFolder -ChildPath $version

    # Use robocopy to mirror each release folder to the content folder
    Write-Host "Copying files from $sourcePath to $destinationPath using Robocopy..."
    $robocopyArgs = @(
        "$sourcePath",
        "$destinationPath",
        "/IS",
        "/IT",
        "/MIR",
        "/E",                       # Copy all subdirectories, including empty ones
        "/R:3",                     # Retry 3 times on failed copies
        "/W:5",                     # Wait 5 seconds between retries
        "/NFL"                      # Reduce logging
    )

    Start-Process -FilePath "robocopy.exe" -ArgumentList $robocopyArgs -Wait -NoNewWindow

    Write-Host "Bundling complete for version $version!"
}
