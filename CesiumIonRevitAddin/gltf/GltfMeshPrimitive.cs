using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfMeshPrimitive
    {
        [JsonProperty("attributes")]
        public GltfAttribute Attributes { get; set; } = new GltfAttribute();

        [JsonProperty("indices")]
        public int Indices { get; set; }

        [JsonProperty("material")]
        public int? Material { get; set; } = null;
    }
}
