This repository will build the Cesium Ion Revit Addin. 

#  Prerequisites

This repository is Windows-only.

## Revit 2024

Install Revit 2024.

Use the default settings. If you change the installation directory, the Visual Studio references to the Autodesk DLLs will need to be updated.

## Visual Studio Professional

Install [Visual Studio Professional 2022](https://visualstudio.microsoft.com/downloads).

After installation, add the .NET Framework 4.8 targeting pack:

![image](https://github.com/user-attachments/assets/e98dee83-46b8-400d-acf6-5e5b4615fccd)

## RevitLookup (Optional, but highly recommended)

Install [RevitLookup](https://github.com/jeremytammik/RevitLookup). This will allow you to query the API and objects in the scene for faster debugging and development.

# Clone the Repo 

It is recommended to clone the repo into the same parent directory as [cesium-native-ion-wrapper](https://github.com/CesiumGS/cesium-native-ion-wrapper). 

(It isn't necessary to clone or build the [cesium-native-ion-wrapper](https://github.com/CesiumGS/cesium-native-ion-wrapper) repo. As an alternative, you can download the latest wrapper release and place the contents in `cesium-native-ion-wrapper\build\CesiumNativeIonWrapper\Debug`.
The build process for the Revit Addin simply looks in this relative folder and copies the contents over to bundle the addin.)

After changing to the appropriate directory, run the following:
```
git clone git@github.com:CesiumGS/cesium-ion-revit-add-in.git
```
After cloning, your directory should have these two folders in it:

![image](https://github.com/user-attachments/assets/5e47a21b-a4e5-4173-9fb5-d4f927875d9a)

With the addin repo cloned, open `CesiumIonRevitAddin.sln` and build. The addin DLL and all files from the Native wrapper will be in `cesium-ion-revit-add-in\CesiumIonRevitAddin\bin\Debug`

# Referencing the Design Tiler

The addin currently uses the Design Tiler to produce .3dtiles files at the end of the export process.

The addin expects the tiler to be present in `CesiumIonRevitAddin\bin\Debug\Tiler\tilers.exe` or `CesiumIonRevitAddin\bin\Release\Tiler\tilers.exe`

You will need to clone, build and place the Design Tiler at one of these locations.

# Developing

For development, we use [Revit AddinManager](https://github.com/chuongmep/RevitAddInManager). It allows for reloading the addin after code changes without restarting Revit.


The `CesiumIonRevit.addin` manifest file is also included in the repo if you wish to install more traditionally by [adding the manifest to a directory Revit looks in](https://help.autodesk.com/view/RVT/2024/ENU/?guid=Revit_API_Revit_API_Developers_Guide_Introduction_Add_In_Integration_Add_in_Registration_html) such as `C:\ProgramData\Autodesk\Revit\Addins\Revit 2024` or `C:\Users<user>\AppData\Roaming\Autodesk\Revit\Addins\Revit 2024\`.
