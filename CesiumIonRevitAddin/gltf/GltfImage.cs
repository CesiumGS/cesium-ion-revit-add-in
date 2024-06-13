using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfImage
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
