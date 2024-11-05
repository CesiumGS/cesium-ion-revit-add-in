# Developer Guide

Autodesk Revit runs only on Windows, therefore the following documentation is for Windows only.

## Project Overview

The project consists of the following components:

- Individual .NET projects for each version of Autodesk Revit.
- A shared project containing the majority of add-in code used by the above projects.

This guide covers how to run and build in Visual Studio.

## Prerequisites

- **Visual Studio 2022** - Install it from https://visualstudio.microsoft.com/downloads. 
- **.NET Framework 4.8 and .NET 8** - These can be installed using the Visual Studio installer.  We recommend having the .NET desktop development workload installed.

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


### Debugging

When using Revit AddinManager and a Debug build of the add-in, you can attach the Visual Studio debugger to the revit.exe process for proper debugging workflows.

## Creating a Bundle

The following steps can be used to create a bundle that supports multiple versions of Revit.  This is also the content that gets uploaded to the Autodesk App Store for new releases.

1. Switch the Solution **Configuration** to **Release** in Visual Studio.
2. Right click on the Solution and choose **Rebuild**.
3. Execute the powershell script [bundle.ps1](/AddinBundle/bundle.ps1)
4. All Release builds will be copied into the [/AddinBundle/CesiumIonRevitAddin.bundle](/AddinBundle/CesiumIonRevitAddin.bundle) folder, which can be copied into `C:\ProgramData\Autodesk\ApplicationPlugins` 