using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CesiumIonRevitAddin.CesiumIonClient;
using CesiumIonRevitAddin.Forms;
using CesiumIonRevitAddin.Gltf;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;


namespace CesiumIonRevitAddin.Utils
{
    internal static class IonExportUtils
    {
        public static Preferences GetUserPreferences(Document doc)
        {
            // Load preferences for this document, if it exists
            Preferences preferences;
            string preferencesPath = Preferences.GetPreferencesPathForProject(doc.PathName);
            bool existingPreferences = File.Exists(preferencesPath);
            if (existingPreferences && doc.PathName != "")
                preferences = Preferences.LoadFromFile(preferencesPath);
            else
                preferences = new Preferences();

            // Use the existing filename for the first export instead of the default tileset.3dtiles
            if (!existingPreferences && doc.PathName != "")
            {
                string docFileName = Path.GetFileNameWithoutExtension(doc.PathName);
                preferences.OutputPath = Path.Combine(preferences.OutputDirectory, docFileName + ".3dtiles");
            }

            // Display the export preferences dialog
            using (ExportDialog exportDialog = new ExportDialog(ref preferences))
            {
                exportDialog.ShowDialog();
                if (exportDialog.DialogResult != DialogResult.OK)
                    return null;
            }
            return preferences;
        }

        public static Preferences SaveUserPreferences(Document doc, Preferences preferences)
        {
            // Write out the updated preferences for this document
            if (doc.PathName != "")
                preferences.SaveToFile(Preferences.GetPreferencesPathForProject(doc.PathName));
            return preferences;
        }

        public static View3D GetExportView(Autodesk.Revit.DB.View view)
        {
            if (view.Document.IsFamilyDocument)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Families Not Supported", "Families cannot be uploaded to Cesium ion. Please open a project document and try again.");
                return null;
            }

            if (view.GetType().Name != "View3D")
            {
                Autodesk.Revit.UI.TaskDialog.Show("3D View Required", "A 3D view is required to upload to Cesium ion. Please switch to a 3D view and try again.");
                return null;
            }
            return (View3D)view;
        }
        public static string GetSavePath(Document doc, Preferences preferences)
        {
            // Display the Save File Dialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "3D Tiles (*.3dtiles)|*.3dtiles";
                saveFileDialog.FilterIndex = 1;

                // Load the initial directory and filename from the preferences
                if (preferences.ionExport)
                {
                    saveFileDialog.RestoreDirectory = true;
                    saveFileDialog.FileName = Path.GetFileNameWithoutExtension(doc.PathName);
                }
                else
                {
                    saveFileDialog.FileName = preferences.OutputFilename;
                    saveFileDialog.InitialDirectory = preferences.OutputDirectory;
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return saveFileDialog.FileName;
                }
                else
                {
                    return null;
                }
            }
        }

        public static Result ExportIntermediateFormat(View3D view, Preferences preferences)
        {
            var exportContext = new GltfExportContext(view.Document, preferences);
            var exporter = new CustomExporter(view.Document, exportContext)
            {
                ShouldStopOnError = false,
                IncludeGeometricObjects = false
            };

            exporter.Export(view);

            return Result.Succeeded;
        }

        public static void Cleanup(Preferences preferences)
        {
            // Remove the temp glTF directory
            if (!preferences.KeepGltf)
            {
                Directory.Delete(preferences.TempDirectory, true);
            }
        }

        public static string GetProjectInformationAsString(Document doc)
        {
            Element projectInfoElement = doc.ProjectInformation;

            if (projectInfoElement == null)
                return "";

            Dictionary<string, string> projectInfoDict = new Dictionary<string, string>();

            // Loop through the parameters of the project information element
            foreach (Parameter param in projectInfoElement.Parameters)
            {
                string paramName = param.Definition.Name;
                string paramValue = GetParameterValue(param);

                // Only add to dictionary if the value is not empty or null
                if (!string.IsNullOrWhiteSpace(paramValue))
                {
                    projectInfoDict[paramName] = paramValue;
                }
            }

            // Sort the dictionary by key
            var sortedProjectInfo = projectInfoDict.OrderBy(kv => kv.Key);

            StringBuilder projectInfoBuilder = new StringBuilder();

            foreach (var kv in sortedProjectInfo)
            {
                projectInfoBuilder.AppendLine($"{kv.Key}: {kv.Value}");
            }

            // Return the formatted and sorted string
            return projectInfoBuilder.ToString();
        }

        public static string GetParameterValue(Parameter param)
        {
            if (param == null)
                return null;

            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.AsString();
                case StorageType.Integer:
                    return param.AsInteger().ToString();
                case StorageType.Double:
                    return param.AsDouble().ToString();
                case StorageType.ElementId:
                    return Util.GetElementIdAsLong(param.AsElementId()).ToString();
                default:
                    return string.Empty;
            }
        }

        public static void ConfigureClient(UIApplication app)
        {
            string fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            string revitInfo = $"Autodesk Revit {app.Application.SubVersionNumber}";
            string view = app.ActiveUIDocument.ActiveView.Name;
            string project = app.ActiveUIDocument.Document.Title;

            string santizedView = string.IsNullOrWhiteSpace(view) ? "UnknownView" : view;
            string sanitizedProject = string.IsNullOrWhiteSpace(project) ? "UnknownProject" : project;
            string projectInfo = $"{sanitizedProject}:{santizedView}";

            Connection.ConfigureClient("Cesium ion for Autodesk Revit", fileVersionInfo, revitInfo, projectInfo);
        }
    }
}
