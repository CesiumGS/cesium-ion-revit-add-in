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
        private PushButton pushButtonLearn;
        private PushButton pushButtonHelp;

        private PushButton pushButtonConnectAddin;
        private PushButton pushButtonSignOutAddin;
        private PushButton pushButtonUploadAddin;
        private PushButton pushButtonExportDiskAddin;
        private PushButton pushButtonLearnAddin;
        private PushButton pushButtonHelpAddin;

        public Result OnStartup(UIControlledApplication application)
        {
            // Store this instance in a static field
            _instance = this;

            // Create Ribbon Tab
            CreateRibbonTab(application, RIBBONTAB);

            // Create Connect button
            PushButtonData pushButtonDataConnect = new PushButtonData("ConnectToIon", "Connect", addInPath, "CesiumIonRevitAddin.ConnectToIon")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "right-to-bracket-solid.png"), UriKind.Absolute))
            };
            pushButtonDataConnect.ToolTip = "Connects to Cesium ion or Cesium ion Self Hosted";
            
            // Create Sign Out button
            PushButtonData pushButtonDataSignOut = new PushButtonData("SignOut", "Sign Out", addInPath, "CesiumIonRevitAddin.Disconnect")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "right-from-bracket-solid.png"), UriKind.Absolute))
            };
            pushButtonDataSignOut.ToolTip = "Signs out from the current Cesium ion server";
           
            // Create Upload button
            PushButtonData pushButtonDataUpload = new PushButtonData("ExportToIon", "Upload", addInPath, "CesiumIonRevitAddin.ExportToIon")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "cloud-arrow-up-solid.png"), UriKind.Absolute))
            };
            pushButtonDataUpload.ToolTip = "Uploads the current 3D View to Cesium ion";

            // Create Export button
            PushButtonData pushButtonDataExportDisk = new PushButtonData("ExportToDisk", "Export", addInPath, "CesiumIonRevitAddin.ExportToDisk")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "file-export-solid.png"), UriKind.Absolute))
            };
            pushButtonDataExportDisk.ToolTip = "Exports the current 3D View into a 3D Tiles tileset on disk";

            // Create Learn button
            PushButtonData pushButtonDataLearn = new PushButtonData("Learn", "Learn", addInPath, "CesiumIonRevitAddin.LearningContent")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "book-open-reader-solid.png"), UriKind.Absolute))
            };
            pushButtonDataLearn.ToolTip = "Open Cesium tutorials and learning resources";

            // Create Help button
            PushButtonData pushButtonDataHelp = new PushButtonData("Help", "Help", addInPath, "CesiumIonRevitAddin.CommunityForum")
            {
                LargeImage = new BitmapImage(new Uri(Path.Combine(fontAwesomeFolder, "handshake-solid.png"), UriKind.Absolute))
            };
            pushButtonDataHelp.ToolTip = "Search for existing questions or ask a new question on the Cesium Community Forum";

            // Add to Cesium tab
            RibbonPanel panelCesiumIon = FindOrCreatePanel(application, CESIUM_ION_PANEL);
            RibbonPanel panel3DTiles = FindOrCreatePanel(application, TILES_PANEL);
            RibbonPanel panelSupport = FindOrCreatePanel(application, SUPPORT_PANEL);

            pushButtonConnect = panelCesiumIon.AddItem(pushButtonDataConnect) as PushButton;
            pushButtonSignOut = panelCesiumIon.AddItem(pushButtonDataSignOut) as PushButton;
            pushButtonUpload = panel3DTiles.AddItem(pushButtonDataUpload) as PushButton;
            pushButtonExportDisk = panel3DTiles.AddItem(pushButtonDataExportDisk) as PushButton;
            pushButtonLearn = panelSupport.AddItem(pushButtonDataLearn) as PushButton;
            pushButtonHelp = panelSupport.AddItem(pushButtonDataHelp) as PushButton;

            // Add to Add-in tab
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Cesium");

            // Create main pulldown button
            PulldownButtonData pullDownButtonData = new PulldownButtonData("CesiumButton", "Cesium");
            pullDownButtonData.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute));
            PulldownButton pullDownButtonCesium = ribbonPanel.AddItem(pullDownButtonData) as PulldownButton;

            pushButtonConnectAddin = pullDownButtonCesium.AddPushButton(pushButtonDataConnect);
            pushButtonSignOutAddin = pullDownButtonCesium.AddPushButton(pushButtonDataSignOut);
            pullDownButtonCesium.AddSeparator();
            pushButtonUploadAddin = pullDownButtonCesium.AddPushButton(pushButtonDataUpload);
            pushButtonExportDiskAddin = pullDownButtonCesium.AddPushButton(pushButtonDataExportDisk);
            pullDownButtonCesium.AddSeparator();
            pushButtonLearnAddin = pullDownButtonCesium.AddPushButton(pushButtonDataLearn);
            pushButtonHelpAddin = pullDownButtonCesium.AddPushButton(pushButtonDataHelp);

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
            pushButtonExportDisk.Enabled = isConnected;    // Enable Export to Disk if connected

            pushButtonConnectAddin.Enabled = !isConnected;      // Enable Connect if not connected
            pushButtonConnectAddin.Visible = !isConnected;      // Show Connect if not connected
            pushButtonSignOutAddin.Enabled = isConnected;       // Enable Sign Out if connected
            pushButtonSignOutAddin.Visible = isConnected;       // Show Sign Out if connected
            pushButtonUploadAddin.Enabled = isConnected;        // Enable Upload if connected
            pushButtonExportDiskAddin.Enabled = isConnected;    // Enable Export to Disk if connected
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

            // Only supply an EPSG code if the user has shared coordinates enabled
            string inputCrs = preferences.EpsgCode != "" && preferences.SharedCoordinates ? $"EPSG:{preferences.EpsgCode}" : "";

            // The upload dialog handles the upload process
            using (IonUploadDialog ionUploadDialog = new IonUploadDialog(zipPath, assetName, assetDesc, inputCrs))
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