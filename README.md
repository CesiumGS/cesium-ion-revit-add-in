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

The repository requires two additional repositories to be cloned alongside it.  Post build scripts will extract built files from these repositories and add them to the Revit Add-in build folder.

You should clone the following repositories into the same parent folder
```
git clone --recurse-submodules git@github.com:CesiumGS/cesium-ion-revit-add-in.git
```
```
git clone --recurse-submodules git@github.com:CesiumGS/cesium-native-ion-wrapper.git
```
```
git clone --recurse-submodules -b web-ifc git@github.com:CesiumGS/tilers.git
```

After cloning, your folder structure should look like this

![image](https://github.com/user-attachments/assets/4cbb7a1b-7e75-4806-bacd-4b826f349653)

Alternatively, you can place pre-built versions of `cesium-native-ion-wrapper` and `tilers` at the following locations.  The post build scripts will look for files in these locations and copy them across to the add-in

- Cesium Native Ion Wrapper Debug - `cesium-native-ion-wrapper\build\CesiumNativeIonWrapper\Debug`
- Cesium Native Ion Wrapper Release - `cesium-native-ion-wrapper\build\CesiumNativeIonWrapper\Release`
- Tilers Debug - `tilers\build\bin\Debug`
- Tilers Release - `tilers\build\bin\Release`

# Building the Repo

`cesium-ion-revit-add-in` and `tilers` must be built first.  Build them by following their respective build documentation, and choose the same build target (eg. Debug, Release) that you're using for `cesium-ion-revit-add-in`

With the addin repo cloned, open `cesium-ion-revit-add-in\CesiumIonRevitAddin.sln` and build. The addin DLL and all files from the Native wrapper will be in `cesium-ion-revit-add-in\CesiumIonRevitAddin\bin\Debug` or `cesium-ion-revit-add-in\CesiumIonRevitAddin\bin\Release` depending on build configuration.

A successful build will see `tilers` (green) and `cesium-ion-revit-add-in` (orange) content alongside the add-in .dll

![image](https://github.com/user-attachments/assets/de6480d4-3714-49c5-be1e-629fe6a2aa46)


# Developing

For development, we use [Revit AddinManager](https://github.com/chuongmep/RevitAddInManager). It allows for reloading the addin after code changes without restarting Revit.


The `CesiumIonRevit.addin` manifest file is also included in the repo if you wish to install more traditionally by [adding the manifest to a directory Revit looks in](https://help.autodesk.com/view/RVT/2024/ENU/?guid=Revit_API_Revit_API_Developers_Guide_Introduction_Add_In_Integration_Add_in_Registration_html) such as `C:\ProgramData\Autodesk\Revit\Addins\Revit 2024` or `C:\Users<user>\AppData\Roaming\Autodesk\Revit\Addins\Revit 2024\`.
