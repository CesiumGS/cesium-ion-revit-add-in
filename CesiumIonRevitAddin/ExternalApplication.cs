using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CesiumIonRevitAddin.CesiumIonClient;
using CesiumIonRevitAddin.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
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
        private const string SUPPORT_PANEL = "Support";

        private static readonly string addInPath = Assembly.GetAssembly(typeof(ExternalApplication)).Location;
        private static readonly string buttonIconsFolder = Path.Combine(Path.GetDirectoryName(addInPath), "Images");
        private static readonly string fontAwesomeFolder = Path.Combine(buttonIconsFolder, "FontAwesome");

        // Store an instance of ExternalApplication
        private static ExternalApplication _instance;

        // Declare the buttons to control their enabled/disabled state
        private PushButton pushButtonConnect;
        private PushButton pushButtonSignOut;
        private PushButton pushButtonUpload;
        private PushButton pushButtonExportDisk;

        public Result OnStartup(UIControlledApplication application)
        {
            // Store this instance in a static field
            _instance = this;

            // Create Ribbon Tab
            CreateRibbonTab(application, RIBBONTAB);

            // Create Connect button
            var pushButtonDataConnect = new PushButtonData("ConnectToIon", "Connect", addInPath, "CesiumIonRevitAddin.ConnectToIon")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "right-to-bracket-solid.png"), UriKind.Absolute)),
                ToolTip = "Connects to Cesium ion or Cesium ion Self Hosted.",
                LongDescription = "The Connect function allows you to establish a connection between Autodesk Revit and Cesium ion, either through the cloud-based Cesium ion service or a locally hosted instance of Cesium ion. This connection enables seamless integration of 3D models and geospatial data into Cesium’s platform, allowing you to visualize, manage, and share your architectural and geospatial models in a high-performance, interactive 3D environment."
            };

            // Create Sign Out button
            var pushButtonDataSignOut = new PushButtonData("SignOut", "Sign Out", addInPath, "CesiumIonRevitAddin.Disconnect")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "right-from-bracket-solid.png"), UriKind.Absolute)),
                ToolTip = "Signs out from the current Cesium ion server.",
                LongDescription = "The Sign Out function allows you to log out of your current Cesium ion session. This action disconnects your Autodesk Revit session from the Cesium ion server, ensuring that any ongoing operations requiring a Cesium ion connection (such as uploading models or accessing hosted assets) are terminated. After signing out, you’ll need to sign in again to re-establish a connection."
            };
            
            // Create Upload button
            var pushButtonDataUpload = new PushButtonData("ExportToIon", "Upload", addInPath, "CesiumIonRevitAddin.ExportToIon")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "cloud-arrow-up-solid.png"), UriKind.Absolute)),
                ToolTip = "Uploads the current 3D View to Cesium ion.",
                LongDescription = "The Upload function enables you to send your current 3D View directly to Cesium ion. Your 3D model will be stored on Cesium ion’s platform. This operation creates a cloud-hosted version of your model, making it accessible from anywhere and optimizing it for use in Cesium-based applications or visualizations."
            };

            // Create Export button
            var pushButtonDataExportDisk = new PushButtonData("ExportToDisk", "Export", addInPath, "CesiumIonRevitAddin.ExportToDisk")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "file-export-solid.png"), UriKind.Absolute)),
                ToolTip = "Exports the current 3D View into a 3D Tiles tileset on disk.",
                LongDescription = "The Export function allows you to export your current 3D view or model in Autodesk Revit as a 3D Tiles tileset. 3D Tiles is an open format optimized for streaming and rendering large-scale 3D geospatial data, widely supported by Cesium’s platform. The export process creates a folder containing all the necessary tileset data, which can then be stored locally on disk, used for offline access, or integrated into other 3D visualization environments."
            };

            // Create Learn button
            var pushButtonDataLearn = new PushButtonData("Learn", "Learn", addInPath, "CesiumIonRevitAddin.LearningContent")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "book-open-reader-solid.png"), UriKind.Absolute)),
                ToolTip = "Opens Cesium tutorials and learning resources.",
                LongDescription = "The Learn function provides quick access to Cesium’s comprehensive collection of tutorials, documentation, and educational materials. From beginner to expert, these structured resources and tutorials will help you understand how to integrate and visualize 3D models, geospatial data, and more."
            };

            // Create Help button
            var pushButtonDataHelp = new PushButtonData("Help", "Help", addInPath, "CesiumIonRevitAddin.CommunityForum")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "circle-question.png"), UriKind.Absolute)),
                ToolTip = "Search for existing questions or ask a new question on the Cesium Community Forum.",
                LongDescription = "The Help function allows you to access the Cesium Community Forum. It is an active community of developers and geospatial professionals who share their knowledge, troubleshoot problems, and discuss best practices. If you can’t find an existing answer to your question, you can post a new inquiry to get help from the Cesium community or Cesium support team."
            };

            // Create help
            string helpFilePath = Path.Combine(Path.GetDirectoryName(addInPath), "help.html");
            ContextualHelp help = new ContextualHelp(ContextualHelpType.Url, helpFilePath);

            // Add help to buttons - NOTE: This must be done before adding to panels
            pushButtonDataConnect.SetContextualHelp(help);
            pushButtonDataSignOut.SetContextualHelp(help);
            pushButtonDataUpload.SetContextualHelp(help);

            // Add to Cesium tab
            RibbonPanel panelCesiumIon = FindOrCreatePanel(application, CESIUM_ION_PANEL);
            RibbonPanel panel3DTiles = FindOrCreatePanel(application, TILES_PANEL);
            RibbonPanel panelSupport = FindOrCreatePanel(application, SUPPORT_PANEL);

            pushButtonConnect = panelCesiumIon.AddItem(pushButtonDataConnect) as PushButton;
            pushButtonSignOut = panelCesiumIon.AddItem(pushButtonDataSignOut) as PushButton;
            pushButtonUpload = panel3DTiles.AddItem(pushButtonDataUpload) as PushButton;
#if DEBUG
            pushButtonExportDisk = panel3DTiles.AddItem(pushButtonDataExportDisk) as PushButton;
#endif
            panelSupport.AddItem(pushButtonDataLearn);
            panelSupport.AddItem(pushButtonDataHelp);

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
            pushButtonConnect.Visible = !isConnected;      // Show Connect if not connected
            pushButtonSignOut.Enabled = isConnected;       // Enable Sign Out if connected
            pushButtonSignOut.Visible = isConnected;       // Show Sign Out if connected
            pushButtonUpload.Enabled = isConnected;        // Enable Upload if connected
        }

        // Static method to allow access to UpdateButtonStates from other commands
        public static void RefreshButtonStates()
        {
            _instance?.UpdateButtonStates();
        }

        // Helper to find or create a panel by name
        private static RibbonPanel FindOrCreatePanel(UIControlledApplication application, string panelName)
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

#if DEBUG
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
            {
                return Result.Cancelled;
            }

            Preferences preferences = IonExportUtils.GetUserPreferences(doc);
            if (preferences == null)
            {
                return Result.Cancelled;
            }

            string savePath = IonExportUtils.GetSavePath(doc, preferences);
            if (savePath == null)
            {
                return Result.Cancelled;
            }

            preferences.OutputPath = savePath;
            preferences.ionExport = false;

            IonExportUtils.SaveUserPreferences(doc, preferences);

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

            TaskDialog.Show("Export Complete", "View exported to 3D Tiles");

            return Result.Succeeded;
        }
    }
#endif

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

            IonExportUtils.SaveUserPreferences(doc, preferences);

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

            // Only supply an EPSG code if the user has shared coordinates enabled
            string inputCrs = preferences.EpsgCode != "" && preferences.SharedCoordinates ? $"EPSG:{preferences.EpsgCode}" : "";

            // The upload dialog handles the upload process
            using (var ionUploadDialog = new IonUploadDialog(zipPath, assetName, assetDesc, inputCrs))
            {
                ionUploadDialog.ShowDialog();
            }

            // Remove the zip
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConnectToIon : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            using (var ionConnectDialog = new IonConnectDialog())
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
    public class LearningContent : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                string url = "https://cesium.com/learn/";

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

    [Transaction(TransactionMode.Manual)]
    public class CommunityForum : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                string url = "https://community.cesium.com/";

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