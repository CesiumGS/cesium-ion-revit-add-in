using Newtonsoft.Json;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfNode
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mesh")]
        public int? Mesh { get; set; } = null;
        [JsonProperty("rotation")]
        public List<double> Rotation { get; set; }
        [JsonProperty("scale")]
        public List<double> Scale { get; set; }
        [JsonProperty("translation")]
        public List<float> Translation { get; set; }
        [JsonProperty("children")]
        public List<int> Children { get; set; }
        [JsonProperty("extras")]
        public GltfExtras Extras { get; set; }
        // TODO: property
        public GltfExtensions extensions = new GltfExtensions();
    }
}
