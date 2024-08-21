using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Autodesk.Revit.DB;


namespace CesiumIonRevitAddin.Utils
{
    internal class TilerExportUtils
    {
        public static void WriteTilerJson(Preferences preferences)
        {

            string outputPath = preferences.OutputPath;

            // Export to a subfolder if not using a .3dtiles DB
            if (!preferences.Export3DTilesDB)
            {
                outputPath = Path.Combine(preferences.OutputDirectory, Path.GetFileNameWithoutExtension(outputPath), "tileset.json");
            }

            var jsonObject = new JObject
            {
                ["input"] = new JObject
                {
                    ["path"] = preferences.GltfPath
                },
                ["output"] = new JObject
                {
                    ["path"] = outputPath
                },
                ["overwrite"] = true,
                ["pipeline"] = new JObject
                {
                    ["type"] = "DESIGN_TILER",
                    ["designTiler"] = new JObject
                    {
                        ["flattenClassHierarchy"] = true,
                        ["flattenObjectHierarchy"] = true,
                        ["separatePropertyTables"] = true
                    }
                }
            };

            // Only add the CRS information if it exists
            if (preferences.EpsgCode != "" && preferences.SharedCoordinates)
            {
                jsonObject["input"]["crs"] = "EPSG:" + preferences.EpsgCode;
            }

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

            File.WriteAllText(preferences.JsonPath, jsonString);
        }

        public static void RunTiler(string jsonPath)
        {
            Logger.Instance.Log("Running tiler");

            // Find the tiler executable
            string exePath = GetTilerLocation();
            if (!File.Exists(exePath))
                throw new FileNotFoundException("Tiler executable not found at: " + exePath);

            // Define the arguments
            string arguments = $"--config \"{jsonPath}\"";
            string workingDirectory = Path.GetDirectoryName(jsonPath);

            // Create the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,    // Tiler needs cwd to match the json file if relative paths are used
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,    // Required for redirection
                CreateNoWindow = true   // Hide the command window
            };

            try
            {
                // Start the tiler process
                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();
                    int exitCode = process.ExitCode;

                    // Wait for the process to exit
                    process.WaitForExit();

                    // Capture the output and error
                    Logger.Instance.Log($"Tiler output: {output}");
                    Logger.Instance.Log($"Tiler errors: {errors}");
                    Logger.Instance.Log($"Tiler exit code: {exitCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("An error occurred while trying to execute the tiler:");
                Logger.Instance.Log(ex.Message);
            }
        }

        public static string GetTilerLocation()
        {
            return Util.GetAddinFolder() + "\\tiler\\tilers.exe";
        }
    }
}
