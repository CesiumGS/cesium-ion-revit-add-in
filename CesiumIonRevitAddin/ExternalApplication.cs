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
        private const string RIBBONPANEL = "Export";
        private static readonly string addInPath = Assembly.GetAssembly(typeof(ExternalApplication)).Location;
        private static readonly string buttonIconsFolder = Path.GetDirectoryName(addInPath) + "\\Images\\";

        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbonTab(application, RIBBONTAB);

            RibbonPanel panel = null;
            List<RibbonPanel> ribbonPanels = application.GetRibbonPanels();
            foreach (var existingPanel in ribbonPanels)
            {
                var name = existingPanel.Name;
                if (name.Equals(RIBBONPANEL))
                {
                    panel = existingPanel;
                    break;
                }
            }

            if (panel == null)
            {
                panel = application.CreateRibbonPanel(RIBBONTAB, RIBBONPANEL);
            }

            ContextualHelp contexHelp = new Autodesk.Revit.UI.ContextualHelp(Autodesk.Revit.UI.ContextualHelpType.Url, "www.cesium.com");
            PushButtonData pushButtonData =
                new Autodesk.Revit.UI.PushButtonData("push Button Name", "Export to 3D Tiles", addInPath, "CesiumIonRevitAddin.ExportCommand")
                {
                    LargeImage = new BitmapImage(new System.Uri(Path.Combine(buttonIconsFolder, "logo.png"), System.UriKind.Absolute))
                };
            pushButtonData.SetContextualHelp(contexHelp);
            pushButtonData.ToolTip = "Exports the current 3D View into a 3D Tiles tileset";
            // TODO: Add LongDescription if needed
            //pushButtonData.LongDescription = "Exports the current 3D View into a 3D Tiles tileset.";
            panel.AddItem(pushButtonData);

#pragma warning disable S125
            /*
             * Remove for now until ion integration is complete
             * 
            var ionConnectionButton = new PushButtonData("ion Connection", "ion Connection", addInPath, "CesiumIonRevitAddin.ConnectToIon");
            ionConnectionButton.LargeImage = new BitmapImage(new System.Uri(Path.Combine(buttonIconsFolder, "logo.png"), System.UriKind.Absolute));
            ionConnectionButton.ToolTip = "Connect to ion";
            ionConnectionButton.SetContextualHelp(contexHelp);
            ionConnectionButton.LongDescription = "Connect to ion";
            panel.AddItem(ionConnectionButton);

            var uploadButton = new PushButtonData("Upload File", "Upload File", addInPath, "CesiumIonRevitAddin.UploadFile");
            uploadButton.LargeImage = new BitmapImage(new System.Uri(Path.Combine(buttonIconsFolder, "logo.png"), System.UriKind.Absolute));
            uploadButton.ToolTip = "Upload file to ion";
            uploadButton.SetContextualHelp(contexHelp);
            uploadButton.LongDescription = "Upload to ion";
            panel.AddItem(uploadButton);
            */
#pragma warning restore S125

            // look for RibbonPanel, or create it if not already created
            RibbonPanel panelAbout = null;
            foreach (var existingPanel in application.GetRibbonPanels())
            {
                if (existingPanel.Name.Equals("About"))
                {
                    panelAbout = existingPanel;
                    break;
                }
            }

            if (panelAbout == null)
            {
                panelAbout = application.CreateRibbonPanel(RIBBONTAB, "About");
            }

            var pushDataButtonAbout = new PushButtonData("About", "About", addInPath, "CesiumIonRevitAddin.AboutUs");
            pushDataButtonAbout.LargeImage = new BitmapImage(new System.Uri(System.IO.Path.Combine(buttonIconsFolder, "logo.png"), System.UriKind.Absolute));
            pushDataButtonAbout.ToolTip = "Find out more about Cesium and 3D Tiles";
            pushDataButtonAbout.SetContextualHelp(contexHelp);
            // TODO: Add LongDescription if needed
            //pushDataButtonAbout.LongDescription = "Find out more about Cesium and 3D Tiles";
            panelAbout.AddItem(pushDataButtonAbout);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

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

            // Spawn the async task and wait for it to complete
            try
            {
                Task uploadTask = Connection.Upload(zipPath, Path.GetFileName(exportView.Document.PathName), "desc", "attr", "GLTF", "3D_MODEL");

                // Block the main thread until the async upload completes
                uploadTask.Wait(); // or uploadTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Upload Error", $"An error occurred during upload: {ex.Message}");
                return Result.Failed;
            }

            // Remove the zip
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // As the export has been successful, save out the changes
            // TODO: Decide if this should happen in a failed export
            IonExportUtils.SaveUserPreferences(doc, preferences);

            TaskDialog.Show("Export Complete", "View exported to Cesium ion");

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

            Debug.WriteLine("Signed out");

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