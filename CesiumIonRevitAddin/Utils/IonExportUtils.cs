using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CesiumIonRevitAddin.Forms;
using CesiumIonRevitAddin.Gltf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (view.GetType().Name != "View3D")
            {
                Autodesk.Revit.UI.TaskDialog.Show("Wrong View", "You must be in a 3D view to export");
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
                    return param.AsElementId().Value.ToString();
                default:
                    return string.Empty;
            }
        }
    }
}
