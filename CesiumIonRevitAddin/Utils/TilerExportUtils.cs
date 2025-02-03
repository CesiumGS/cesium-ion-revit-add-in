using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;


namespace CesiumIonRevitAddin.Utils
{
    internal static class TilerExportUtils
    {
        public static void WriteTilerJson(Preferences preferences)
        {

            string inputPath = preferences.GltfPath;
            string outputPath = preferences.OutputPath;

            // Export to a subfolder if not using a .3dtiles DB
            if (!preferences.Export3DTilesDB)
            {
                outputPath = Path.Combine(preferences.OutputDirectory, Path.GetFileNameWithoutExtension(outputPath), "tileset.json");
            }
            else
            {
                outputPath = preferences.Temp3DTilesPath;
            }

            // Generate Relative paths to the JSON
            Uri jsonDirectoryUri = new Uri(Path.GetDirectoryName(preferences.JsonPath) + Path.DirectorySeparatorChar);
            Uri inputPathUri = new Uri(inputPath);
            Uri outputPathUri = new Uri(outputPath);

            inputPathUri = jsonDirectoryUri.MakeRelativeUri(inputPathUri);
            outputPathUri = jsonDirectoryUri.MakeRelativeUri(outputPathUri);

            // Convert the URI to a relative path
            outputPath = Uri.UnescapeDataString(outputPathUri.ToString()).Replace('/', Path.DirectorySeparatorChar);

            var jsonObject = new JObject
            {
                ["input"] = new JObject
                {
                    ["path"] = inputPathUri
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
                        ["enableInstancing"] = preferences.IonInstancing,
                    }
                },
                ["gzip"] = true,
                ["gltf"] = new JObject
                {
                    ["geometricCompression"] = "MESHOPT",
                    ["colorTextureCompression"] = "KTX2"
                }
            };

            // Only add the CRS information if it exists
            if (preferences.EpsgCode != "" && preferences.SharedCoordinates)
            {
                jsonObject["input"]["coordinateSystem"] = new JObject();
                jsonObject["input"]["coordinateSystem"]["crs"] = "EPSG:" + preferences.EpsgCode;
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
            {
                throw new FileNotFoundException("Tiler executable not found at: " + exePath);
            }

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
            string envPath = Environment.GetEnvironmentVariable("CESIUM_TILER_PATH");
            if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
            {
                return envPath;
            }
            return string.Empty;
        }
    }
}
