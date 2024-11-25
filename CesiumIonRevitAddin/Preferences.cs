using CesiumIonRevitAddin.Utils;
using Newtonsoft.Json;
using System;
using System.IO;

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
        public bool Instancing { get; set; } = false;
        public bool TrueNorth { get; set; } = true;
        public bool SharedCoordinates { get; set; } = true;
        public string EpsgCode { get; set; } = "";
        public int MaxTextureSize { get; set; } = 2048;
#if DEBUG
        public bool KeepGltf { get; } = true;
#else
        public bool KeepGltf { get; } = false;
#endif
        public bool Export3DTilesDB { get; } = true;
        public bool ExportMetadata { get; set; } = true;
        public string OutputPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tileset.3dtiles";
        public string OutputDirectory => Path.GetDirectoryName(OutputPath);
        public string OutputFilename => Path.GetFileName(OutputPath);
        public string TempDirectory => Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(OutputPath) + "_temp");
        public string JsonPath => Path.Combine(TempDirectory, "tileset.json");
        public string BinPath => Path.Combine(TempDirectory, "tileset.bin");
        public string GltfPath => Path.Combine(TempDirectory, "tileset.gltf");
        public string Temp3DTilesPath => Path.Combine(TempDirectory, OutputFilename);
        public bool ionExport { get; set; } = false;

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static Preferences FromJson(string json) => JsonConvert.DeserializeObject<Preferences>(json);

        public void SaveToFile(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, ToJson());
        }

        public static Preferences LoadFromFile(string filePath) => FromJson(File.ReadAllText(filePath));

        public static string GetPreferencesFolder() => Path.Combine(Util.GetAddinUserDataFolder(), "preferences");

        public static string GetPreferencesPathForProject(string projectPath) => Path.Combine(GetPreferencesFolder(), Path.GetFileNameWithoutExtension(projectPath) + ".json");
    }
}
