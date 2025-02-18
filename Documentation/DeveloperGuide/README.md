# Developer Guide

Autodesk Revit runs only on Windows, therefore the following documentation is for Windows only.

## Project Overview

The project consists of the following components:

- Individual .NET projects for each version of Autodesk Revit.
- A shared project containing the majority of add-in code used by the above projects.

This guide covers how to run and build in Visual Studio.

## Prerequisites

- **Visual Studio 2022** - Install it from https://visualstudio.microsoft.com/downloads. 
- **.NET Framework 4.8 and .NET 8** - These can be installed using the Visual Studio installer.  We recommend having ".NET desktop development" under the "Workloads" tab installed.

## Clone the Repository

```sh
git clone git@github.com:CesiumGS/cesium-ion-revit-add-in.git
```

## Building

### Visual Studio 2022

Open `CesiumIonRevitAddin.sln` in Visual Studio 2022 and build the solution.

Binaries for each project will be located in the individual project folders in either `bin\Debug` or `bin\Release` depending on the build configuration used.  


## Installing

Open the project folder specific to the version of Revit you want to test with.  

Copy the contents of the `bin\Debug` or `bin\Release` folder into `C:\ProgramData\Autodesk\Revit\Addins` under the specific version of Revit you're using.

When opening Revit, you may see a Security - Unsigned Add-In dialog appear.  This is advising you that Revit cannot verify the publisher and confirming if you want to load the add-in.  Click **Always Load** or **Load Once** to use the add-in with Revit.

## Developing

### Test changes without restarting Revit

When developing, we use [Revit AddinManager](https://github.com/chuongmep/RevitAddInManager) to test changes to builds without needing to restart Revit.  

Open Autodesk Revit with Revit AddinManager installed.

Navigate to **Add-Ins > Add-in Manager > Add-in Manager (Manual Mode)**

With the **Load Command** tab selected, click the **Load* button at the bottom right of the interface.

Select a **CesiumIonRevitAddin.dll** from your most recent build.

You will see the different commands in the Add-in available in the interface.  Select one and click the **Run** button.  This will execute the command, just as you would if you clicked the button on the UI.

If you make a change and produce a new build, the Revit AddinManager will utilise the latest changes to the .dll.

**Tips**

- Use Add-In Manager (Manual Mode, Faceless) to re-run the previous command without needing to navigate the UI
- Click Show/Hide Panel (Debug Trace Events) to display a debug console that prints `Debug.Writeline()` output.

### Testing with local files

Debug builds feature an **Export 3D Tiles to Disk** option in the ribbon.  This can export glTF to disk, which can then be used to produce 3D Tiles without uploading to Cesium ion.

By default, a glTF and tileset.json will be exported through this process.  If you have a local tiler, the add-in can be configured to automatically call tilers.exe and convert the glTF into 3D Tiles.  The glTF remains on the system after conversion for debugging purposes.

To configure a tiler, add a `CESIUM_TILER_PATH` environment variable to your system, with a value that represents an absolute path to your `tilers.exe`.

### Debugging

When using Revit AddinManager and a Debug build of the add-in, you can attach the Visual Studio debugger to the revit.exe process for proper debugging workflows.

## Creating a Bundle

The following steps can be used to create a bundle that supports multiple versions of Revit.  This is also the content that gets uploaded to the Autodesk App Store for new releases.

1. Ensure there are no modified or untracked files in [/AddinBundle/CesiumIonRevitAddin.bundle](/AddinBundle/CesiumIonRevitAddin.bundle).
2. Switch the Solution **Configuration** to **Release** in Visual Studio.
3. Right click on the Solution and choose **Rebuild**.
4. Execute the batch file [BundleDebug.bat](/AddinBundle/BundleDebug.bat) or [BundleRelease.bat](/AddinBundle/BundleRelease.bat) depending on which configuration you want to bundle
5. All builds from individual projects will be copied into the [/AddinBundle/CesiumIonRevitAddin.bundle](/AddinBundle/CesiumIonRevitAddin.bundle) folder
6. To test the bundle with Revit, execute the batch file [CopyBundleToRevit.bat](/AddinBundle/CopyBundleToRevit.bat).  This will copy the bundle to `C:\ProgramData\Autodesk\ApplicationPlugins\CesiumIonRevitAddin.bundle`, which will make the add-in available to all currently installed and supported versions of Revit.  NOTE: This will override any existing bundle in the same location
7. The bundle can be removed from Revit using [RemoveBundleFromRevit.bat](/AddinBundle/RemoveBundleFromRevit.bat).
8. The final bundle can be packaged into an installer and uploaded to the Autodesk App Store for new releases (see the [Release Guide](/Documentation/ReleaseGuide/README.md))

