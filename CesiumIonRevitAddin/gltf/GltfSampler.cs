using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfSampler
    {
        [JsonProperty("magFilter")]
        public int? MagFilter { get; set; } = 9729; // LINEAR

        [JsonProperty("minFilter")]
        public int? MinFilter { get; set; } = 9987; // LINEAR

        [JsonProperty("wrapS")]
        public int WrapS { get; set; } = 10497;  // REPEAT

        [JsonProperty("wrapT")]
        public int WrapT { get; set; } = 10497;
    }
}
