using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CesiumIonRevitAddin.Forms;
using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

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
    public class ExportCommand : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var view = commandData.Application.ActiveUIDocument.Document.ActiveView;
            if (view.GetType().Name != "View3D")
            {
                Autodesk.Revit.UI.TaskDialog.Show("Wrong View", "You must be in a 3D view to export");
                return Result.Succeeded;
            }
            View3D exportView = (View3D)view;

            // Load preferences for this document, if it exists
            Preferences preferences;
            string preferencesPath = Preferences.GetPreferencesPathForProject(exportView.Document.PathName);
            bool existingPreferences = File.Exists(preferencesPath);
            if (existingPreferences && exportView.Document.PathName != "")
            {
                preferences = Preferences.LoadFromFile(preferencesPath);
            }
            else
            {
                preferences = new Preferences();
            }

            // Display the export preferences dialog
            using (ExportDialog exportDialog = new ExportDialog(ref preferences))
            {
                exportDialog.ShowDialog();
                if (exportDialog.DialogResult != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancelled;
                }
            }

            // Display the Safe File Dialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "3D Tiles (*.3dtiles)|*.3dtiles";
                saveFileDialog.FilterIndex = 1;

                // Load the initial directory and filename from the preferences
                if (existingPreferences)
                {
                    saveFileDialog.FileName = preferences.OutputFilename;
                    saveFileDialog.InitialDirectory = preferences.OutputDirectory;
                }
                else
                {
                    saveFileDialog.RestoreDirectory = true;
                    saveFileDialog.FileName = Path.GetFileNameWithoutExtension(exportView.Document.PathName);
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    preferences.OutputPath = saveFileDialog.FileName;
                }
                else
                {
                    return Result.Cancelled;
                }
            }

            var exportContext = new GltfExportContext(exportView.Document, preferences);
            var exporter = new CustomExporter(exportView.Document, exportContext)
            {
                ShouldStopOnError = false,
                IncludeGeometricObjects = false
            };

            exporter.Export(exportView);

            // Execute the tiler
            TilerExportUtils.RunTiler(preferences.JsonPath);

            // Move the .3dtiles to the final location
            if (preferences.Export3DTilesDB)
            {
                File.Copy(preferences.Temp3DTilesPath, preferences.OutputPath, overwrite: true);
                File.Delete(preferences.Temp3DTilesPath);
            }

            // Remove the temp glTF directory
            if (!preferences.KeepGltf)
            {
                Directory.Delete(preferences.TempDirectory, true);
            }

            // Write out the updated preferences for this document
            if (exportView.Document.PathName != "")
            {
                preferences.SaveToFile(preferencesPath);
            }

            TaskDialog.Show("Export Complete", "View exported to 3D Tiles");

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConnectToIon : Autodesk.Revit.UI.IExternalCommand
    {
        [DllImport("CesiumNativeIonWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void printLog();
        [DllImport("CesiumNativeIonWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void initializeAndAuthenticate();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // var ionConnectionForm = new IonConnectionForm();
            // ionConnectionForm.ShowDialog();
            // Autodesk.Revit.UI.TaskDialog.Show("xxxx", "Connecting to Cesium ion");
            initializeAndAuthenticate();
            // printLog();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UploadFile : Autodesk.Revit.UI.IExternalCommand
    {
        [DllImport("CesiumNativeIonWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void upload();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            upload();
            return Result.Succeeded;
        }
    };

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