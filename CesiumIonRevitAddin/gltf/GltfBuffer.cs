using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfBuffer
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("byteLength")]
        public ulong ByteLength { get; set; }
    }
}
