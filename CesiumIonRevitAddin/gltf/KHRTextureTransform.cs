using Newtonsoft.Json;
using CesiumIonRevitAddin.Export;

namespace CesiumIonRevitAddin.Gltf
{
    internal class KHRTextureTransform
    {
        [JsonProperty("offset")]
        public XY? Offset { get; set; }
        [JsonProperty("rotation")]
        public double? Rotation { get; set; }
        [JsonProperty("scale")]
        public XY? Scale { get; set; }
    }
}
