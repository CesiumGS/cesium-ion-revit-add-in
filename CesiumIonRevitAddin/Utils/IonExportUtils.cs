using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CesiumIonRevitAddin.Forms;
using CesiumIonRevitAddin.Gltf;
using System.IO;
using System.Windows.Forms;


namespace CesiumIonRevitAddin.Utils
{
    public class IonExportUtils
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
    }
}
