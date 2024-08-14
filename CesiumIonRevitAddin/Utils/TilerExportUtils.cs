using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace CesiumIonRevitAddin.Utils
{
    internal class TilerExportUtils
    {
        public static void WriteTilerJson(string jsonPath, Preferences preferences)
        {

            string inputPath = Path.GetFileName(preferences.OutputPath);
            string outputPath = Path.GetFileNameWithoutExtension(preferences.OutputPath) + "/tileset.json";

            var jsonObject = new JObject
            {
                ["input"] = new JObject
                {
                    ["path"] = inputPath
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
                jsonObject["input"]["crs"] = "EPSG:" + preferences.EpsgCode;
            
            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

            File.WriteAllText(jsonPath, jsonString);
        }
    }
}
