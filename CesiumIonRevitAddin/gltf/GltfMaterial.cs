using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfMaterial
    {
        [JsonProperty("alphaMode")]
        public string AlphaMode { get; set; }

        [JsonProperty("alphaCutoff")]
        public float? AlphaCutoff { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pbrMetallicRoughness")]
        public GltfPbr PbrMetallicRoughness { get; set; }

        [JsonProperty("doubleSided")]
        public bool DoubleSided { get; set; }

        [JsonProperty("extensions")]
        public GltfExtensions Extensions { get; set; } = null;
    }
}
