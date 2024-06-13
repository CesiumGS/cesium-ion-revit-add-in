using Newtonsoft.Json;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfPbr
    {
        [JsonProperty("baseColorFactor")]
        public List<float> BaseColorFactor { get; set; }

        [JsonProperty("metallicFactor")]
        public float MetallicFactor { get; set; }

        [JsonProperty("roughnessFactor")]
        public float RoughnessFactor { get; set; }

        [JsonProperty("baseColorTexture")]
        public GltfTextureInfo BaseColorTexture { get; set; }
    }
}
