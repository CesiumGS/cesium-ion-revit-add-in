using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CesiumIonRevitAddin.CesiumIonClient;
using CesiumIonRevitAddin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CesiumIonRevitAddin.Forms;

namespace CesiumIonRevitAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalApplication : IExternalApplication
    {
        private const string RIBBONTAB = "Cesium";
        private const string CESIUM_ION_PANEL = "Cesium ion";
        private const string TILES_PANEL = "3D Tiles";
        private const string ABOUT_PANEL = "About";

        private static readonly string addInPath = Assembly.GetAssembly(typeof(ExternalApplication)).Location;
        private static readonly string buttonIconsFolder = Path.GetDirectoryName(addInPath) + "\\Images\\";

        // Store an instance of ExternalApplication
        private static ExternalApplication _instance;

        // Declare the buttons to control their enabled/disabled state
        private PushButton pushButtonConnect;
        private PushButton pushButtonSignOut;
        private PushButton pushButtonUpload;
        private PushButton pushButtonExportDisk;
        private PushButton pushButtonAbout;

        public Result OnStartup(UIControlledApplication application)
        {
            // Store this instance in a static field
            _instance = this;

            // Create Ribbon Tab
            CreateRibbonTab(application, RIBBONTAB);

            // Create or find Cesium ion panel
            RibbonPanel panelCesiumIon = FindOrCreatePanel(application, CESIUM_ION_PANEL);

            // Create Connect button
            PushButtonData pushButtonDataConnect =
                new PushButtonData("ConnectToIon", "Connect", addInPath, "CesiumIonRevitAddin.ConnectToIon")
                {
                    LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute))
                };
            pushButtonDataConnect.ToolTip = "Connects to Cesium ion";
            pushButtonConnect = panelCesiumIon.AddItem(pushButtonDataConnect) as PushButton;

            // Create Sign Out button
            PushButtonData pushButtonDataSignOut =
                new PushButtonData("SignOut", "Sign Out", addInPath, "CesiumIonRevitAddin.Disconnect")
                {
                    LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute))
                };
            pushButtonDataSignOut.ToolTip = "Signs out from Cesium ion";
            pushButtonSignOut = panelCesiumIon.AddItem(pushButtonDataSignOut) as PushButton;

            // Create or find 3D Tiles panel
            RibbonPanel panel3DTiles = FindOrCreatePanel(application, TILES_PANEL);

            // Create Upload button
            PushButtonData pushButtonDataUpload =
                new PushButtonData("ExportToIon", "Upload", addInPath, "CesiumIonRevitAddin.ExportToIon")
                {
                    LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute))
                };
            pushButtonDataUpload.ToolTip = "Uploads the current 3D View to Cesium ion";
            pushButtonUpload = panel3DTiles.AddItem(pushButtonDataUpload) as PushButton;

            // Create Export to Disk button
            PushButtonData pushButtonDataExportDisk =
                new PushButtonData("ExportToDisk", "Export", addInPath, "CesiumIonRevitAddin.ExportToDisk")
                {
                    LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute))
                };
            pushButtonDataExportDisk.ToolTip = "Exports the current 3D View into a 3D Tiles tileset on disk";
            pushButtonExportDisk = panel3DTiles.AddItem(pushButtonDataExportDisk) as PushButton;

            // Create or find About panel
            RibbonPanel panelAbout = FindOrCreatePanel(application, ABOUT_PANEL);

            // Create About button
            PushButtonData pushButtonDataAbout = new PushButtonData("About", "About", addInPath, "CesiumIonRevitAddin.AboutUs")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute))
            };
            pushButtonDataAbout.ToolTip = "Find out more about Cesium and 3D Tiles";
            pushButtonAbout = panelAbout.AddItem(pushButtonDataAbout) as PushButton;

            // Initially update button states based on connection status
            UpdateButtonStates();

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        // Method to enable/disable buttons based on connection status
        public void UpdateButtonStates()
        {
            bool isConnected = CesiumIonRevitAddin.CesiumIonClient.Connection.IsConnected();

            pushButtonConnect.Enabled = !isConnected;      // Enable Connect if not connected
            pushButtonSignOut.Enabled = isConnected;       // Enable Sign Out if connected
            pushButtonUpload.Enabled = isConnected;        // Enable Upload if connected
            pushButtonExportDisk.Enabled = isConnected;    // Enable Export to Disk if connected
        }

        // Static method to allow access to UpdateButtonStates from other commands
        public static void RefreshButtonStates()
        {
            _instance?.UpdateButtonStates();
        }

        // Helper to find or create a panel by name
        private RibbonPanel FindOrCreatePanel(UIControlledApplication application, string panelName)
        {
            var panels = application.GetRibbonPanels(RIBBONTAB);
            foreach (var existingPanel in panels)
            {
                if (existingPanel.Name.Equals(panelName))
                {
                    return existingPanel;
                }
            }

            return application.CreateRibbonPanel(RIBBONTAB, panelName);
        }

        // Helper to create the Ribbon Tab if it doesn't exist
        private static void CreateRibbonTab(UIControlledApplication application, string ribbonTabName)
        {
            Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
            var tab = ribbon.FindTab(ribbonTabName);

            if (tab == null)
            {
                application.CreateRibbonTab(ribbonTabName);
            }
        }
    }





    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportToDisk : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Get the export view
            View3D exportView = IonExportUtils.GetExportView(doc.ActiveView);
            if (exportView == null)
                return Result.Cancelled;

            // Get export preferences from the user
            Preferences preferences = IonExportUtils.GetUserPreferences(doc);
            if (preferences == null)
                return Result.Cancelled;

            // Get the save location from the user
            string savePath = IonExportUtils.GetSavePath(doc, preferences);
            if (savePath == null)
                return Result.Cancelled;

            preferences.OutputPath = savePath;
            preferences.ionExport = false;

            // Export the intermediate format 
            Result exportResult = IonExportUtils.ExportIntermediateFormat(exportView, preferences);
            if (exportResult != Result.Succeeded)
            {
                IonExportUtils.Cleanup(preferences);
                return exportResult;
            }

            // If we don't export to ion, we execute the tiler locally
            TilerExportUtils.RunTiler(preferences.JsonPath);

            // Move the .3dtiles to the final location
            if (preferences.Export3DTilesDB)
            {
                File.Copy(preferences.Temp3DTilesPath, preferences.OutputPath, overwrite: true);
                File.Delete(preferences.Temp3DTilesPath);
            }

            // Clean up the export contents
            IonExportUtils.Cleanup(preferences);

            // As the export has been successful, save out the changes
            // TODO: Decide if this should happen in a failed export
            IonExportUtils.SaveUserPreferences(doc, preferences);

            TaskDialog.Show("Export Complete", "View exported to 3D Tiles");

            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportToIon : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!Connection.IsConnected())
            {
                TaskDialog.Show("Not Connected", "Please connect to Cesium ion before uploading");
                return Result.Failed;
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Get the export view
            View3D exportView = IonExportUtils.GetExportView(doc.ActiveView);
            if (exportView == null)
                return Result.Cancelled;

            // Get export preferences from the user
            Preferences preferences = IonExportUtils.GetUserPreferences(doc);
            if (preferences == null)
                return Result.Cancelled;

            // Provide a spoof path for the export
            preferences.OutputPath = Path.Combine(Path.GetTempPath(), "cesium", "ion_export.3dtiles");
            preferences.ionExport = true;

            // Export the intermediate format 
            Result exportResult = IonExportUtils.ExportIntermediateFormat(exportView, preferences);
            if (exportResult != Result.Succeeded)
            {
                IonExportUtils.Cleanup(preferences);
                return exportResult;
            }

            // Zip the export before uploading
            string zipPath = Path.Combine(Path.GetTempPath(), "cesium", "upload.zip");

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // Specify the folder that you want to zip
            string folderToZip = preferences.TempDirectory;

            // Create a new zip file (overwrites if already exists)
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Get all files from the specified folder
                foreach (string filePath in Directory.GetFiles(folderToZip))
                {
                    // Ignore tileset.json
                    if (filePath.EndsWith("tileset.json"))
                        continue;

                    // Add each file to the zip archive
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                }
            }

            // Clean up the export contents
            IonExportUtils.Cleanup(preferences);

            string assetName = Path.GetFileName(doc.PathName);
            string assetDesc = IonExportUtils.GetProjectInformationAsString(doc);

            // The upload dialog handles the upload process
            using (IonUploadDialog ionUploadDialog = new IonUploadDialog(zipPath, assetName, assetDesc, $"EPSG:{preferences.EpsgCode}"))
            {
                ionUploadDialog.ShowDialog();
            }

            // Remove the zip
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // As the export has been successful, save out the changes
            // TODO: Decide if this should happen in a failed export
            IonExportUtils.SaveUserPreferences(doc, preferences);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConnectToIon : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            using (IonConnectDialog ionConnectDialog = new IonConnectDialog())
            {
                ionConnectDialog.ShowDialog();
            }

            // Update button states after connecting
            ExternalApplication.RefreshButtonStates();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Disconnect : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            CesiumIonClient.Connection.Disconnect();

            // Update button states after disconnecting
            ExternalApplication.RefreshButtonStates();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class AboutUs : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                string url = "https://www.cesium.com";

                // Use the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", message);
                return Result.Failed;
            }
        }
    }
}