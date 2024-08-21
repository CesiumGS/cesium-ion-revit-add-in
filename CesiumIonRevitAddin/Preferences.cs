using System;
using System.IO;
using CesiumIonRevitAddin.Utils;
using Newtonsoft.Json;

namespace CesiumIonRevitAddin
{
    public class Preferences
    {
        public bool Materials { get; set; } = true;
        public bool Textures { get; set; } = true;
        public bool Normals { get; set; } = true;
        public bool Links { get; set; } = false;
        public bool Levels { get; set; }
        public bool Properties { get; set; }
        public bool RelocateTo0 { get; set; }
        public bool FlipAxis { get; set; } = true;
        public bool Instancing { get; set; } = true;
        public bool TrueNorth { get; set; } = true;
        public bool SharedCoordinates { get; set; } = true;
        public string EpsgCode { get; set; } = "";
        public int MaxTextureSize { get; set; } = 2048;
        public bool KeepGltf { get; } = false;
        public bool Export3DTilesDB { get; } = true;

        // TODO: needed?
        //#if REVIT2019 || REVIT2020\
        //        DisplayUnitType units;
        //#else
        //        ForgeTypeId units;
        //
        //#endif

        public string OutputPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tileset.3dtiles";
        public string OutputDirectory
        {
            get
            {
                return Path.GetDirectoryName(OutputPath);
            }
        }
        public string OutputFilename
        {
            get
            {
                return Path.GetFileName(OutputPath);
            }
        }
        public string TempDirectory
        {
            get
            {
                return Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(OutputPath) + "_temp");
            }
        }
        public string JsonPath
        {
            get
            {
                return Path.Combine(TempDirectory, "tileset.json");
            }
        }
        public string BinPath
        {
            get
            {
                return Path.Combine(TempDirectory, "tileset.bin");
            }
        }
        public string GltfPath
        {
            get
            {
                return Path.Combine(TempDirectory, "tileset.gltf");
            }
        }
        public string Temp3DTilesPath
        {
            get
            {
                return Path.Combine(TempDirectory, OutputFilename);
            }
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static Preferences FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Preferences>(json);
        }

        public void SaveToFile(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, ToJson());
        }

        public static Preferences LoadFromFile(string filePath)
        {
            return FromJson(File.ReadAllText(filePath));
        }

        public static string GetPreferencesFolder()
        {
            return Path.Combine(Util.GetAddinUserDataFolder(), "preferences");
        }

        public static string GetPreferencesPathForProject(string projectPath)
        {
            return Path.Combine(GetPreferencesFolder(), Path.GetFileNameWithoutExtension(projectPath) + ".json");
        }
    }
}
