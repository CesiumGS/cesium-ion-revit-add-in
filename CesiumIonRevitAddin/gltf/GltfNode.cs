using Newtonsoft.Json;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfNode
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mesh")]
        public int? Mesh { get; set; } = null;

        [JsonProperty("matrix")]
        public List<double> Matrix { get; set; }

        [JsonProperty("rotation")]
        public List<double> Rotation { get; set; }
        [JsonProperty("scale")]
        public List<double> Scale { get; set; }
        [JsonProperty("translation")]
        public List<float> Translation { get; set; }

        [JsonProperty("children")]
        public List<int> Children { get; set; }
        [JsonProperty("extensions")]
        public GltfExtensions Extensions { get; set; } = null;
    }
}
