using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfTexture
    {
        [JsonProperty("source")]
        public int Source { get; set; }

        [JsonProperty("sampler")]
        public int Sampler { get; set; }
    }
}
