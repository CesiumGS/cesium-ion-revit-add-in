using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public GltfExtensions Extensions { get; set; } = new GltfExtensions();
    }
}
